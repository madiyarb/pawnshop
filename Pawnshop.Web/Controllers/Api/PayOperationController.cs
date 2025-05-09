using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.Extensions.Options;
using Pawnshop.AccountingCore.Models;
using Pawnshop.Core;
using Pawnshop.Core.Exceptions;
using Pawnshop.Core.Options;
using Pawnshop.Core.Queries;
using Pawnshop.Data.Access;
using Pawnshop.Data.Models._1c;
using Pawnshop.Data.Models.Audit;
using Pawnshop.Data.Models.CashOrders;
using Pawnshop.Data.Models.Contracts;
using Pawnshop.Data.Models.Contracts.Actions;
using Pawnshop.Data.Models.Files;
using Pawnshop.Data.Models.InnerNotifications;
using Pawnshop.Data.Models.PayOperations;
using Pawnshop.Services.AccountingCore;
using Pawnshop.Web.Engine;
using Pawnshop.Web.Engine.Audit;
using Pawnshop.Web.Engine.Calculation;
using Pawnshop.Web.Engine.Export;
using Pawnshop.Web.Engine.MessageSenders;
using Pawnshop.Web.Engine.Middleware;
using Pawnshop.Services.Storage;
using Pawnshop.Web.Models.List;
using Pawnshop.Web.Models.PayOperation;
using Pawnshop.Services.Contracts;

namespace Pawnshop.Web.Controllers.Api
{
    [Authorize(Permissions.PayOperationView)]
    public class PayOperationController : Controller
    {
        private readonly PayOperationRepository _repository;
        private readonly PayOperationActionRepository _payOperationActionRepository;
        private readonly ISessionContext _sessionContext;
        private readonly BranchContext _branchContext;
        private readonly PayOperationExcelBuilder _excelBuilder;
        private readonly IStorage _storage;
        private readonly ContractRepository _contractRepository;
        private readonly InnerNotificationRepository _innerNotificationRepository;
        private readonly CashOrderRepository _cashOrderRepository;
        private readonly ContractActionRepository _contractActionRepository;
        private readonly PayOperationQueryRepository _payOperationQueryRepository;
        private readonly IContractActionOperationService _contractActionOperationService;
        private readonly IContractService _contractService;

        private readonly string statusChangeError = "Невозможно изменить статус операции!";

        private ContractActionController _contractActionController;

        public PayOperationController(PayOperationRepository repository, ISessionContext sessionContext, BranchContext branchContext,
            PayOperationExcelBuilder excelBuilder, IStorage storage, ContractRepository contractRepository,
            InnerNotificationRepository innerNotificationRepository, PayOperationActionRepository payOperationActionRepository,
            CashOrderRepository cashOrderRepository, ContractActionRepository contractActionRepository, PayOperationQueryRepository payOperationQueryRepository, 
            ContractActionController contractActionController, IContractActionOperationService contractActionOperationService,
            IContractService contractService)
        {
            _repository = repository;
            _sessionContext = sessionContext;
            _branchContext = branchContext;
            _excelBuilder = excelBuilder;
            _storage = storage;
            _contractRepository = contractRepository;
            _innerNotificationRepository = innerNotificationRepository;
            _payOperationActionRepository = payOperationActionRepository;
            _cashOrderRepository = cashOrderRepository;
            _contractActionRepository = contractActionRepository;
            _payOperationQueryRepository = payOperationQueryRepository;
            _contractActionController = contractActionController;
            _contractActionOperationService = contractActionOperationService;
            _contractService = contractService;
        }

        [HttpPost]
        public ListModel<PayOperation> List([FromBody] ListQueryModel<PayOperationListQueryModel> listQuery)
        {
            if (listQuery == null) listQuery = new ListQueryModel<PayOperationListQueryModel>();
            if (listQuery.Model == null) listQuery.Model = new PayOperationListQueryModel();
            if (!listQuery.Model.BranchId.HasValue && !(listQuery.Model.BranchIds==null || listQuery.Model.BranchIds.Count > 0))
            {
                listQuery.Model.BranchId = _branchContext.Branch.Id;
            }

            if (listQuery.Model.EndDate.HasValue)
            {
                listQuery.Model.EndDate = listQuery.Model.EndDate.Value.Date.AddHours(23).AddMinutes(59).AddSeconds(59);
            }

            return new ListModel<PayOperation>
            {
                List = _repository.List(listQuery, listQuery.Model),
                Count = _repository.Count(listQuery, listQuery.Model)
            };
        }

        [HttpPost]
        public PayOperation Check([FromBody] PayOperationAction action)
        {
            if (action == null) throw new ArgumentNullException(nameof(action));
            if (action.OperationId <= 0) throw new ArgumentException(nameof(action.OperationId));
            
            var operation = _repository.Get(action.OperationId);

            if (operation.Status == PayOperationStatus.Repaired ||
                operation.Status == PayOperationStatus.Checked ||
                operation.Status == PayOperationStatus.Canceled ||
                operation.Status == PayOperationStatus.Executed)
            {
                throw new PawnshopApplicationException(statusChangeError);
            }

            if (!_contractService.IsKDNPassedForOffline((int)operation.ContractId))
            {
                throw new PawnshopApplicationException("КДН не пройден!");
            }

            operation.Status = PayOperationStatus.Checked;

            using (var transaction = _repository.BeginTransaction())
            {
                _repository.Update(operation);

                action.AuthorId = _sessionContext.UserId;
                action.CreateDate = DateTime.Now;
                if (action.Date == DateTime.MinValue)
                {
                    action.Date = DateTime.Now;
                }

                _payOperationActionRepository.Insert(action);

                //постановка в очередь на создание платежного поручения в 1С 
                if (operation.PayType.AccountantUploadRequired)
                    _payOperationQueryRepository.Insert(new PayOperationQuery
                    {
                        OperationId = operation.Id,
                        QueryType = QueryType.Upload,
                        Status = QueryStatus.Queued,
                        AuthorId = _sessionContext.UserId,
                        CreateDate = DateTime.Now
                    });

                transaction.Commit();
            }

            return operation;
        }

        [HttpPost]
        public PayOperation ReturnToRepair([FromBody] PayOperationAction action)
        {
            if (action == null) throw new ArgumentNullException(nameof(action));
            if (action.OperationId <= 0) throw new ArgumentException(nameof(action.OperationId));

            int authorId = _sessionContext.UserId;
            int branchId = _branchContext.Branch.Id;
            var operation = _repository.Get(action.OperationId);
            var contract = _contractRepository.Get(operation.ContractId.Value);

            if (operation.Status == PayOperationStatus.ReturnToRepair ||
                operation.Status == PayOperationStatus.Checked ||
                operation.Status == PayOperationStatus.Canceled ||
                operation.Status == PayOperationStatus.Executed)
            {
                throw new PawnshopApplicationException(statusChangeError);
            }
            
            operation.Status = PayOperationStatus.ReturnToRepair;

            ContractAction contractAction = null;
            if (operation.ActionId.HasValue)
            {
                contractAction = _contractActionRepository.Get(operation.ActionId.Value);
            }

            InnerNotification notification = new InnerNotification()
            {
                EntityId = (contractAction != null && contractAction.ParentActionId.HasValue) ? _contractActionRepository.Get(contractAction.ParentActionId.Value).ContractId : contract.Id,
                EntityType = EntityType.Contract,
                CreatedBy = _sessionContext.UserId,
                Status = InnerNotificationStatus.Sent,
                CreateDate = DateTime.Now,
                ReceiveBranchId = contract.BranchId,
                Message = $"Платежная операция №{operation.Number} не прошла проверку: {action.Note}"
            };

            using (var transaction = _repository.BeginTransaction())
            {
                _repository.Update(operation);
                action.AuthorId = _sessionContext.UserId;
                action.CreateDate = DateTime.Now;
                if (action.Date == DateTime.MinValue)
                {
                    action.Date = DateTime.Now;
                }

                if (contractAction != null)
                {
                    _contractActionOperationService.Cancel(contractAction.Id, authorId, branchId, false, false).Wait();
                }

                _payOperationActionRepository.Insert(action);
                _innerNotificationRepository.Insert(notification);

                transaction.Commit();
            }

            return operation;
        }

        [HttpPost]
        public PayOperation Execute([FromBody] PayOperationAction action)
        {
            if (action == null) throw new ArgumentNullException(nameof(action));
            if (action.OperationId <= 0) throw new ArgumentException(nameof(action.OperationId));

            var operation = _repository.Get(action.OperationId);
            var contract = _contractRepository.Get(operation.ContractId.Value);

            if (operation.Status == PayOperationStatus.AwaitForCheck ||
                operation.Status == PayOperationStatus.Canceled ||
                operation.Status == PayOperationStatus.ReturnToRepair ||
                operation.Status == PayOperationStatus.Executed)
            {
                throw new PawnshopApplicationException(statusChangeError);
            }

            operation.Status = PayOperationStatus.Executed;
            operation.ExecuteDate = DateTime.Now;

            action.AuthorId = _sessionContext.UserId;
            action.CreateDate = DateTime.Now;
            if (action.Date == DateTime.MinValue)
            {
                action.Date = DateTime.Now;
            }

            contract.ContractDate = action.Date;
            contract.Status = ContractStatus.Signed;

            using (var transaction = _repository.BeginTransaction())
            {
                operation.Orders.ForEach(order => {
                    order.ApprovedId = _sessionContext.UserId;
                    order.ApproveStatus = OrderStatus.Prohibited;
                    order.ApproveDate = DateTime.Now;

                    _cashOrderRepository.Update(order);
                });

                _repository.Update(operation);
                _contractRepository.Update(contract);
                _payOperationActionRepository.Insert(action);

                transaction.Commit();
            }

            return operation;
        }

        [HttpPost]
        public PayOperation Card([FromBody] int id)
        {
            if (id <= 0) throw new ArgumentOutOfRangeException(nameof(id));

            var model = _repository.Get(id);
            if (model == null) throw new InvalidOperationException();

            return model;
        }

        [HttpPost, Authorize(Permissions.PayOperationManage)]
        [Event(EventCode.PayOperationSaved, EventMode = EventMode.Response, EntityType = EntityType.PayOperation)]
        public PayOperation Save([FromBody] PayOperation model)
        {
            ModelState.Validate();
            if (model.Id <= 0) throw new ArgumentOutOfRangeException(nameof(model.Id));

            _repository.Update(model);
            return model;
        }

        [HttpPost, Authorize(Permissions.PayOperationManage)]
        [Event(EventCode.PayOperationDeleted, EventMode = EventMode.Request, EntityType = EntityType.PayOperation)]
        public IActionResult Delete([FromBody] int id)
        {
            if (id <= 0) throw new ArgumentOutOfRangeException(nameof(id));
            var count = _repository.RelationCount(id);
            if (count > 0)
            {
                throw new Exception("Невозможно удалить платежную операцию, так как она привязана к существующим действиям по договору");
            }

            _repository.Delete(id);
            return Ok();
        }

        [HttpPost]
        public async Task<IActionResult> Export([FromBody] List<PayOperation> operations)
        {
            using (var stream = _excelBuilder.Build(operations))
            {
                var fileName = await _storage.Save(stream, ContainerName.Temp, "export.xlsx");
                new FileExtensionContentTypeProvider().TryGetContentType(fileName, out var contentType);

                var fileRow = new FileRow
                {
                    CreateDate = DateTime.Now,
                    ContentType = contentType ?? "application/octet-stream",
                    FileName = fileName,
                    FilePath = fileName
                };
                return Ok(fileRow);
            }
        }
    }
}