using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Pawnshop.AccountingCore.Models;
using Pawnshop.Core;
using Pawnshop.Core.Exceptions;
using Pawnshop.Core.Extensions;
using Pawnshop.Core.Queries;
using Pawnshop.Data.Access;
using Pawnshop.Data.Access.AccountingCore;
using Pawnshop.Data.Access.Auction.Interfaces;
using Pawnshop.Data.Models.Auction;
using Pawnshop.Data.Models.Auction.HttpRequestModels;
using Pawnshop.Data.Models.Audit;
using Pawnshop.Data.Models.CashOrders;
using Pawnshop.Data.Models.Collection;
using Pawnshop.Data.Models.Contracts;
using Pawnshop.Data.Models.Contracts.Actions;
using Pawnshop.Data.Models.Files;
using Pawnshop.Data.Models.Insurances;
using Pawnshop.Data.Models.Membership;
using Pawnshop.Data.Models.MobileApp.HardCollection.Commands;
using Pawnshop.Services;
using Pawnshop.Services.AccountingCore;
using Pawnshop.Services.Auction.HttpServices.Interfaces;
using Pawnshop.Services.Auction.Interfaces;
using Pawnshop.Services.Collection;
using Pawnshop.Services.Contracts;
using Pawnshop.Services.Crm;
using Pawnshop.Services.Expenses;
using Pawnshop.Services.Integrations.UKassa;
using Pawnshop.Services.LegalCollection.Inerfaces;
using Pawnshop.Services.Models.Filters;
using Pawnshop.Services.Models.List;
using Pawnshop.Services.Positions;
using Pawnshop.Services.Reports;
using Pawnshop.Services.TasOnline;
using Pawnshop.Services.TMF;
using Pawnshop.Web.Engine;
using Pawnshop.Web.Engine.Export;
using Pawnshop.Web.Engine.Middleware;
using Pawnshop.Web.Engine.Services.Interfaces;
using Pawnshop.Services.Storage;
using Pawnshop.Services.ApplicationsOnline;
using Pawnshop.Services.ApplicationOnlineRefinances;
using Pawnshop.Services.CashOrderRemittances;
using Pawnshop.Services.Refinance;
using Pawnshop.Services.Models.Clients;
using Pawnshop.Services.Clients;
using Pawnshop.Services.Remittances;

namespace Pawnshop.Web.Controllers.Api
{
    [Authorize(Permissions.CashOrderView)]
    public class CashOrderController : Controller
    {
        private readonly ICashOrderService _service;
        private readonly CashOrdersExcelBuilder _excelBuilder;
        private readonly CashOrderRepository _cashOrderRepository;
        private readonly AccountRecordRepository _accountRecordRepository;
        private readonly IStorage _storage;
        private readonly BranchContext _branchContext;
        private readonly ISessionContext _sessionContext;
        private readonly IAccountRecordService _accountRecordService;
        private readonly IContractService _contractService;
        private readonly IBusinessOperationService _businessOperationService;
        private readonly IContractActionService _contractActionService;
        private readonly ICollectionService _collectionService;

        private readonly IContractActionProlongService _contractActionProlongService;
        private readonly IContractActionBuyoutService _contractActionBuyoutService;

        private readonly IUKassaService _uKassaService;
        private readonly IContractActionSellingService _contractSellingService;
        private readonly IContractActionOperationService _contractActionOperationService;
        private readonly IContractPaymentService _contractPaymentService;

        private readonly ContractRepository _contractRepository;
        private readonly TasOnlineRequestService _tasOnlineRequestService;
        private readonly ITMFRequestService _tMFRequestService;

        private readonly IContractExpenseOperationService _contractExpenseOperationService;

        private readonly IContractExpenseService _contractExpenseService;
        private readonly ContractExpenseRowRepository _contractExpenseRowRepository;
        private readonly IExpenseService _expenseService;
        private readonly ILogger<CashOrderController> _logger;
        private readonly UserRepository _userRepository;
        private readonly IContractPaymentScheduleService _contractPaymentScheduleService;

        private readonly IBalanceSheetReportService _balanceSheetReportService;
        private readonly IPositionEstimateHistoryService _positionEstimateHistoryService;
        private readonly IPositionSubjectService _positionSubjectService;
        private readonly IEventLog _eventLog;
        private readonly ICrmPaymentService _crmPaymentService;
        private readonly ILegalCollectionCloseService _closeLegalCaseService;
        private readonly ILegalCollectionNotificationService _legalCollectionNotificationService;
        private readonly IClientExpenseService _clientExpenseService;
        private readonly IAuctionPaymentRepository _auctionPaymentRepository;
        private readonly IAuctionOperationHttpService _auctionOperationHttpService;
        private readonly ICancelAuctionOperationService _cancelAuction;
        private readonly ICashOrderRemittanceService _orderRemittanceService;
        private readonly IRemittanceService _remittanceService;
        private readonly IBusinessOperationSettingService _businessOperationSettingService;
        
        public CashOrderController(ICashOrderService service,
            CashOrdersExcelBuilder excelBuilder,
            IStorage storage,
            BranchContext branchContext,
            ISessionContext sessionContext,
            CashOrderRepository cashOrderRepository,
            IAccountRecordService accountRecordService,
            AccountRecordRepository accountRecordRepository,
            IContractService contractService,
            IBusinessOperationService businessOperationService,
            IContractActionService contractActionService,
            ICollectionService collectionService,
            IContractActionProlongService contractActionProlongService,
            IContractActionBuyoutService contractActionBuyoutService,
            IUKassaService uKassaService,
            IContractActionSellingService contractSellingService,
            IContractActionOperationService contractActionOperationService,
            IContractPaymentService contractPaymentService,
            ContractRepository contractRepository,
            TasOnlineRequestService tasOnlineRequestService,
            ITMFRequestService tMFRequestService,
            IContractExpenseOperationService contractExpenseOperationService,
            IContractExpenseService contractExpenseService,
            ContractExpenseRowRepository contractExpenseRowRepository,
            IExpenseService expenseService,
            ILogger<CashOrderController> logger,
            UserRepository userRepository,
            IContractPaymentScheduleService contractPaymentScheduleService,
            IBalanceSheetReportService balanceSheetReportService,
            IPositionSubjectService positionSubjectService,
            IPositionEstimateHistoryService positionEstimateHistoryService,
            IEventLog eventLog,
            ICrmPaymentService crmPaymentService,
            ILegalCollectionCloseService closeLegalCaseService,
            ILegalCollectionNotificationService legalCollectionNotificationService,
            IClientExpenseService clientExpenseService,
            IAuctionPaymentRepository auctionPaymentRepository,
            IAuctionOperationHttpService auctionOperationHttpService,
            IRemittanceService remittanceService,
            ICashOrderRemittanceService orderRemittanceService,
            IBusinessOperationSettingService businessOperationSettingService,
            ICancelAuctionOperationService cancelAuction)
        {
            _service = service;
            _excelBuilder = excelBuilder;
            _storage = storage;
            _branchContext = branchContext;
            _sessionContext = sessionContext;
            _cashOrderRepository = cashOrderRepository;
            _accountRecordRepository = accountRecordRepository;
            _accountRecordService = accountRecordService;
            _contractService = contractService;
            _businessOperationService = businessOperationService;
            _contractActionService = contractActionService;
            _collectionService = collectionService;
            _contractActionBuyoutService = contractActionBuyoutService;
            _contractActionProlongService = contractActionProlongService;
            _uKassaService = uKassaService;
            _contractSellingService = contractSellingService;
            _contractActionOperationService = contractActionOperationService;
            _contractPaymentService = contractPaymentService;
            _contractRepository = contractRepository;
            _tasOnlineRequestService = tasOnlineRequestService;
            _contractExpenseOperationService = contractExpenseOperationService;
            _contractExpenseService = contractExpenseService;
            _contractExpenseRowRepository = contractExpenseRowRepository;
            _expenseService = expenseService;
            _logger = logger;
            _userRepository = userRepository;
            _tMFRequestService = tMFRequestService;
            _contractPaymentScheduleService = contractPaymentScheduleService;
            _balanceSheetReportService = balanceSheetReportService;
            _positionSubjectService = positionSubjectService;
            _positionEstimateHistoryService = positionEstimateHistoryService;
            _eventLog = eventLog;
            _crmPaymentService = crmPaymentService;
            _closeLegalCaseService = closeLegalCaseService;
            _legalCollectionNotificationService = legalCollectionNotificationService;
            _clientExpenseService = clientExpenseService;
            _auctionPaymentRepository = auctionPaymentRepository;
            _auctionOperationHttpService = auctionOperationHttpService;
            _remittanceService = remittanceService;
            _orderRemittanceService = orderRemittanceService;
            _businessOperationSettingService = businessOperationSettingService;
            _cancelAuction = cancelAuction;
        }

        [HttpPost("/api/cashOrder/list")]
        public ListModel<CashOrder> List(
            [FromBody] ListQueryModel<CashOrderFilter> listQuery)
        {
            if (listQuery == null)
                listQuery = new ListQueryModel<CashOrderFilter>();
            if (listQuery.Model == null)
                listQuery.Model = new CashOrderFilter();
            if (!listQuery.Model.OwnerId.HasValue || listQuery.Model.OwnerId.Value == 0)
            {
                listQuery.Model.OwnerId = _branchContext.Branch.Id;
            }

            if (listQuery.Model.EndDate.HasValue)
            {
                listQuery.Model.EndDate = listQuery.Model.EndDate.Value.Date.AddHours(23).AddMinutes(59).AddSeconds(59);
            }
            var cashOrders = _service.List(listQuery);

            bool isCashOrderFounderNoteView = _sessionContext.HasPermission(Permissions.CashOrderNoteManage);

            if (!isCashOrderFounderNoteView)
            {
                foreach (CashOrder cashOrder in cashOrders.List)
                {
                    cashOrder.Note = string.Empty;
                }
            }
            return cashOrders;
        }

        [HttpPost("/api/cashOrder/balanceSheetReportList")]
        public async Task<ListModel<CashOrder>> BalanceSheetReportList([FromBody] ReportGenerateQuery query)
        {
            var cashOrders = await _balanceSheetReportService.GetBalanceSheetReportList(query);

            return cashOrders;
        }

        [HttpPost("/api/cashOrder/card")]
        public async Task<CashOrder> Card([FromBody] int id)
        {
            if (id <= 0)
                throw new ArgumentOutOfRangeException(nameof(id));

            var cashOrder = await _service.GetAsync(id);
            if (cashOrder == null)
                throw new InvalidOperationException();

            bool isCashOrderFounderNoteView = _sessionContext.HasPermission(Permissions.CashOrderNoteManage);

            if (!isCashOrderFounderNoteView)
            {
                cashOrder.Note = string.Empty;
            }

            cashOrder.RelationCount = _service.RelationCount(id);
            return cashOrder;
        }

        [HttpPost("/api/cashOrder/copy"), Authorize(Permissions.CashOrderManage)]
        public async Task<CashOrder> Copy([FromBody] int id)
        {
            if (id <= 0)
                throw new ArgumentOutOfRangeException(nameof(id));

            var cashOrder = await _service.GetAsync(id);
            if (cashOrder == null)
                throw new InvalidOperationException();

            cashOrder.Id = 0;
            cashOrder.OrderNumber = string.Empty;
            cashOrder.OrderDate = DateTime.Now;
            cashOrder.RegDate = DateTime.Now;
            cashOrder.OwnerId = 0;
            cashOrder.BranchId = 0;
            cashOrder.AuthorId = 0;
            return cashOrder;
        }

        [HttpPost("/api/cashOrder/save"), Authorize(Permissions.CashOrderManage)]
        [Event(EventCode.CashOrderSaved, EventMode = EventMode.Response, EntityType = EntityType.CashOrder, IncludeFails = true)]
        public CashOrder Save([FromBody] CashOrder cashOrder)
        {
            if (cashOrder == null)
                throw new NullReferenceException("Ошибка получения данных, объект не был распознан");

            if (cashOrder.Id == 0)
            {
                cashOrder.RegDate = DateTime.Now;
                cashOrder.OwnerId = _branchContext.Branch.Id;
                cashOrder.BranchId = _branchContext.Branch.Id;
                cashOrder.AuthorId = _sessionContext.UserId;
                var bo = _businessOperationService.GetAsync(cashOrder.BusinessOperationId.Value).Result;
                cashOrder.ApproveStatus = bo.OrdersCreateStatus.HasValue ? (OrderStatus)bo.OrdersCreateStatus.Value : 0;
            };

            ModelState.Clear();
            TryValidateModel(cashOrder);
            ModelState.Validate();

            if (cashOrder.Id > 0)
            {
                var savedCashOrder = _service.GetAsync(cashOrder.Id).Result;

                bool isCashOrderFounderNoteView = _sessionContext.HasPermission(Permissions.CashOrderNoteManage);

                if (isCashOrderFounderNoteView)
                {
                    savedCashOrder.Note = cashOrder.Note;
                }
                else
                {
                    cashOrder.Note = string.Empty;
                }

                if (cashOrder.ClientId != savedCashOrder.ClientId || cashOrder.UserId != savedCashOrder.UserId)
                {
                    var authorId = _sessionContext.UserId;
                    var requestLog = new
                    {
                        OldClientId = savedCashOrder.ClientId,
                        NewClientId = cashOrder.ClientId,
                        OldUserId = savedCashOrder.UserId,
                        NewUserId = cashOrder.UserId,
                        AuthorId = authorId
                    };
                    string requestLogJson = JsonConvert.SerializeObject(requestLog);

                    _eventLog.Log(EventCode.CashOrderUpdated, EventStatus.Success, EntityType.CashOrder,
                        savedCashOrder.Id, requestLogJson, null, userId: authorId);
                }
                savedCashOrder.ClientId = cashOrder.ClientId;
                savedCashOrder.UserId = cashOrder.UserId;
                using (var transaction = _service.BeginCashOrderTransaction())
                {
                    _service.Save(savedCashOrder);

                    transaction.Commit();
                }
            }
            if (cashOrder.Id == 0)
            {
                using (var transaction = _service.BeginCashOrderTransaction())
                {
                    _service.Register(cashOrder, _branchContext.Branch);

                    transaction.Commit();
                }
            }
            return cashOrder;
        }

        [HttpPost("/api/cashOrder/reverse"), Authorize(Permissions.CashOrderManage)]
        [Event(EventCode.CashOrderDeleted, EventMode = EventMode.All, EntityType = EntityType.CashOrder, IncludeFails = true)]
        public async Task<IActionResult> Reverse([FromBody] int id)
        {
            if (id <= 0)
                throw new ArgumentOutOfRangeException(nameof(id));

            int authorId = _sessionContext.UserId;
            Group branch = _branchContext.Branch;
            var cashOrder = await _service.GetAsync(id);
            if (cashOrder == null)
                throw new PawnshopApplicationException($"Кассовый ордер {id} не найден");

            if (cashOrder.StornoId.HasValue)
                throw new PawnshopApplicationException("Нельзя сторнировать сторнирующую проводку");

            if (!cashOrder.CreatedToday && !_sessionContext.ForSupport)
                throw new PawnshopApplicationException("Удалять можно только кассовые ордеры за сегодняшний день");

            var auctionPayments = await _auctionPaymentRepository.GetByCashOrderIdAsync(cashOrder.Id);
            var auctionPaymentExists = !auctionPayments.IsNullOrEmpty();

            int relationsCount = _service.RelationCount(id);
            if (relationsCount > 0 && !auctionPaymentExists)
                throw new PawnshopApplicationException("Невозможно удалить кассовый ордер, так как он привязан к другим документам");

            if (auctionPaymentExists)
            {
                await _cancelAuction.CancelAsync(cashOrder);
                return Ok();
            }

            using (IDbTransaction transaction = _cashOrderRepository.BeginTransaction())
            {
                _service.Cancel(cashOrder, authorId, branch);
                await _service.DeleteCashOrderPrintLanguageForOrder(cashOrder);
                transaction.Commit();
                _uKassaService.FinishRequests(new List<int> { cashOrder.Id });
            }
            return Ok();
        }

        [HttpPost("/api/cashOrder/delete"), Authorize(Permissions.CashOrderManage)]
        [Event(EventCode.CashOrderDeleted, EventMode = EventMode.All, EntityType = EntityType.CashOrder, IncludeFails = true)]
        public async Task<IActionResult> Delete([FromBody] int id)
        {
            if (id <= 0)
                throw new ArgumentOutOfRangeException(nameof(id));

            int authorId = _sessionContext.UserId;
            int branchId = _branchContext.Branch.Id;
            var cashOrder = await _service.GetAsync(id);
            if (cashOrder == null)
                throw new PawnshopApplicationException($"Кассовый ордер {id} не найден");

            if (cashOrder.StornoId.HasValue)
                throw new PawnshopApplicationException("Нельзя удалить сторнирующую проводку");

            if (!cashOrder.CreatedToday && !_sessionContext.ForSupport)
                throw new PawnshopApplicationException("Удалять можно только кассовые ордеры за сегодняшний день");

            int relationsCount = _service.RelationCount(id);
            if (relationsCount > 0)
                throw new PawnshopApplicationException("Невозможно удалить кассовый ордер, так как он привязан к другим документам");

            using (IDbTransaction transaction = _cashOrderRepository.BeginTransaction())
            {
                IDictionary<int, (int, DateTime)> recalculateBalanceAccountDict = _service.Delete(id, authorId, branchId);
                if (recalculateBalanceAccountDict == null)
                    throw new PawnshopApplicationException($"Ожидалось что {nameof(_service)}." +
                        $"{nameof(_service.Delete)} не будет null");

                await _service.DeleteCashOrderPrintLanguageForOrder(cashOrder);

                foreach ((int accountId, (int accountRecordId, DateTime date)) in recalculateBalanceAccountDict)
                {
                    DateTime? recalculateDate = null;
                    AccountRecord accountRecordBeforeDate = _accountRecordRepository.GetLastRecordByAccountIdAndEndDate(accountId, accountRecordId, date);
                    if (accountRecordBeforeDate != null)
                        recalculateDate = accountRecordBeforeDate.Date;

                    _accountRecordService.RecalculateBalanceOnAccount(accountId, beginDate: recalculateDate);
                }
                
                if (cashOrder.AuctionRequestId.HasValue)
                {
                    await _cancelAuction.CancelAsync(cashOrder);
                }

                transaction.Commit();
                var list = new List<int>();
                var stornedOrder = _cashOrderRepository.GetOrderByStornoId(cashOrder.Id);
                if (stornedOrder != null)
                    list.Add(stornedOrder.Id);
                _uKassaService.FinishRequests(list);
                if (cashOrder.AuctionRequestId.HasValue)
                {
                    await _auctionOperationHttpService.Reject(cashOrder.AuctionRequestId.GetValueOrDefault());
                }
            }
            return Ok();
        }

        [HttpPost("/api/cashOrder/undoDelete"), Authorize(Permissions.CashOrderManage)]
        [Event(EventCode.CashOrderRecovery, EventMode = EventMode.All, EntityType = EntityType.CashOrder, IncludeFails = true)]
        public async Task<IActionResult> UndoDelete([FromBody] int id)
        {
            if (id <= 0)
                throw new ArgumentOutOfRangeException(nameof(id));

            var cashOrder = await _service.GetAsync(id);
            if (cashOrder == null)
                throw new InvalidOperationException();
            if (!cashOrder.CreatedToday && !_sessionContext.ForSupport)
                throw new PawnshopApplicationException("Восстанавливать можно только кассовые ордера за сегодняшний день");

            _service.UndoDelete(id);
            return Ok();
        }

        [HttpPost("/api/cashOrder/approve")]
        [Event(EventCode.CashOrderApproved, EventMode = EventMode.All, EntityType = EntityType.CashOrder, IncludeFails = true)]
        public async Task<CashOrder> Approve(
            [FromBody] CashOrderApproveInputStructure cashOrderApproveStructure,
            [FromServices] IMediator mediator,
            [FromServices] IApplicationOnlineService applicationOnlineService,
            [FromServices] IApplicationOnlineRefinancesService applicationOnlineRefinancesService,
            [FromServices] IRefinanceBuyOutService refinanceBuyOutService)
        {
            if (_sessionContext.Permissions.Where(x => x == Permissions.CashOrderCashTransaction).FirstOrDefault() == null
                && _sessionContext.Permissions.Where(x => x == Permissions.CashOrderApprove).FirstOrDefault() == null)
            {
                throw new PawnshopApplicationException("У вас недостаточно прав на согласование кассового ордера.");
            }

            if (cashOrderApproveStructure.OrderId <= 0)
                throw new ArgumentOutOfRangeException(nameof(cashOrderApproveStructure.OrderId));

            if (cashOrderApproveStructure.LanguageId <= 0)
                throw new ArgumentOutOfRangeException(nameof(cashOrderApproveStructure.LanguageId));

            CashOrder cashOrder = await _service.GetAsync(cashOrderApproveStructure.OrderId);

            if (cashOrder.OrderDate.Date != DateTime.Today.Date && !_sessionContext.ForSupport)
            {
                throw new PawnshopApplicationException("Данный ордер не может быть подтвержден. Обратитесь к администратору.");
            }

            if (cashOrder.OperationId.HasValue)
            {
                throw new PawnshopApplicationException("Нельзя изменять статус кассового ордера, привязанного к платежной операции!");
            }

            if (cashOrder.ApproveStatus == OrderStatus.WaitingForConfirmation)
            {
                throw new PawnshopApplicationException("Данный ордер должен быть согласован");
            }

            var contractIdsForCloseLegalCase = new List<int>();
            using var transaction = _service.BeginCashOrderTransaction();
            {
                var relatedActionsByOrder = await _contractActionService.GetRelatedContractActionsByOrder(cashOrder);
                var signAction = await _contractActionService.GetSignAction(relatedActionsByOrder);

                if (signAction != null && signAction.Status == ContractActionStatus.Await)
                {
                    var refinanceResult = await applicationOnlineRefinancesService.RefinanceAllAssociatedContracts(signAction.ContractId);

                    if (!refinanceResult)
                    {
                        transaction.Rollback();

                        throw new PawnshopApplicationException("Ошибка рефинансирования займов: Не удалось рефинансировать займы!");
                    }
                }

                await _service.ChangeLanguageForRelatedCashOrder(cashOrder, cashOrderApproveStructure.LanguageId);
                await _service.ChangeStatusForRelatedOrders(cashOrder, OrderStatus.Approved, _sessionContext.UserId, _branchContext.Branch, _sessionContext.ForSupport);

                if (cashOrder.ContractActionId.HasValue)
                {
                    Contract contract = _contractService.Get(cashOrder.ContractId.Value);
                    var relatedActions = await _contractActionService.GetRelatedContractActionsByOrder(cashOrder);
                    foreach (var contractActionId in relatedActions.OrderBy(x => x))
                    {
                        var relatedAction = _contractActionService.GetAsync(contractActionId).Result;
                        if (relatedAction != null && relatedAction.Status.HasValue && relatedAction.Status != ContractActionStatus.Await)
                            continue;

                        relatedAction.Status = ContractActionStatus.Approved;
                        _contractActionService.Save(relatedAction);
                        switch (relatedAction.ActionType)
                        {
                            case ContractActionType.Selling:
                                {
                                    var close = new CollectionClose()
                                    {
                                        ContractId = relatedAction.ContractId,
                                        ActionId = relatedAction.Id
                                    };
                                    contractIdsForCloseLegalCase.Add(close.ContractId);
                                    var isClosed = _collectionService.CloseContractCollection(close);
                                    if (isClosed)
                                        await mediator.Send(new SendClosedContractCommand() { ContractId = relatedAction.ContractId });
                                    else
                                        await mediator.Send(new SendContractOnlyCommand() { ContractId = relatedAction.ContractId });

                                    break;
                                }
                            case ContractActionType.Prolong:
                                {
                                    _contractActionProlongService.ExecOnApprove(relatedAction, _sessionContext.UserId);

                                    var close = new CollectionClose()
                                    {
                                        ContractId = relatedAction.ContractId,
                                        ActionId = relatedAction.Id
                                    };
                                    contractIdsForCloseLegalCase.Add(close.ContractId);
                                    var isClosed = _collectionService.CloseContractCollection(close);
                                    if (isClosed)
                                        await mediator.Send(new SendClosedContractCommand() { ContractId = relatedAction.ContractId });
                                    else
                                        await mediator.Send(new SendContractOnlyCommand() { ContractId = relatedAction.ContractId });

                                    break;
                                }
                            case ContractActionType.Sign:
                                {
                                    if (!_contractService.IsKDNPassedForOffline(relatedAction.ContractId))
                                    {
                                        throw new PawnshopApplicationException("КДН не пройден!");
                                    }
                                    contract = _contractService.Get(relatedAction.ContractId);
                                    if (contract != null && contract.Status != ContractStatus.AwaitForOrderApprove)
                                        throw new PawnshopApplicationException($"Договор {contract.ContractNumber} не находится в состоянии 'Ожидает подтверждения кассового ордера' для подтверждения проводок");

                                    await applicationOnlineService.CheckEncumbranceRegisteredForCashIssue(contract.Id);

                                    if (contract != null && contract.Status == ContractStatus.AwaitForOrderApprove)
                                    {
                                        _contractService.ContractStatusUpdate(contract.Id, ContractStatus.Signed);
                                        if (!contract.PartialPaymentParentId.HasValue)
                                        {
                                            var setting = _contractService.GetSettingForContract(contract);

                                            if (setting.IsInsuranceAvailable)
                                            {
                                                InsurancePoliceRequest latestPoliceRequest = null;
                                                int contractId = contract.Id;

                                                if (relatedAction.ParentActionId != null)
                                                {
                                                    var parentAction = _contractActionService.GetAsync(relatedAction.ParentActionId.Value).Result;
                                                    var parentActionContract = _contractService.GetOnlyContract(parentAction.ContractId);

                                                    if (parentActionContract.ContractClass == ContractClass.Credit
                                                        || parentActionContract.ParentId.HasValue
                                                        || parentActionContract.ClosedParentId.HasValue)
                                                        contractId = parentAction.ContractId;
                                                }
                                            }
                                        }

                                        if (relatedAction.ExpenseId.HasValue)
                                        {
                                            var expense = await _contractExpenseService.GetAsync(relatedAction.ExpenseId.Value);
                                            var expenseType = _expenseService.Get(expense.ExpenseId);
                                            if (expense != null && !expenseType.ExtraExpense)
                                            {
                                                expense.IsPayed = true;
                                                _contractExpenseService.Save(expense);
                                            }
                                        }
                                        await _positionEstimateHistoryService.SavePositionEstimateToHistoryForContract(contract);
                                        await _positionSubjectService.SavePositionSubjectsToHistoryForContract(contract);
                                        await applicationOnlineService.SendInsurance(contract.Id);

                                        var refinanceBuyOutResult = await refinanceBuyOutService.BuyOutAllRefinancedContractsForApplicationsOnline(contract.Id);

                                        if (!refinanceBuyOutResult)
                                        {
                                            transaction.Rollback();

                                            throw new PawnshopApplicationException("Ошибка выкупа займов: Не удалось выкупить займы!");
                                        }

                                        if (contract.ContractClass == ContractClass.Tranche && contract.LoanPeriod != Math.Abs((contract.MaturityDate.Date - contract.ContractDate.Date).Days))
                                        {
                                            contract.LoanPeriod = Math.Abs((contract.MaturityDate.Date - contract.ContractDate.Date).Days);
                                            _contractRepository.UpdateLoanPeriod(contract);
                                        }

                                        var expence = _clientExpenseService.Get(contract.ClientId);
                                        if (expence != null)
                                        {
                                            expence.AllLoan = null;
                                            var expenseDto = new ClientExpenseDto()
                                            {
                                                AvgPaymentToday = expence.AvgPaymentToday,
                                                Family = expence.Family,
                                                Housing = expence.Housing,
                                                Loan = expence.Loan,
                                                Other = expence.Other,
                                                Vehicle = expence.Vehicle,
                                                AllLoan = expence.AllLoan,
                                            };

                                            _clientExpenseService.Save(contract.ClientId, expenseDto);
                                        }
                                    }
                                    break;
                                }
                            case ContractActionType.Buyout:
                            case ContractActionType.BuyoutRestructuringCred:
                                {
                                    if (relatedAction.SellingId.HasValue)
                                    {
                                        relatedAction.isFromSelling = true;
                                    }
                                    await _contractActionBuyoutService.ExecuteOnApprove(relatedAction, _sessionContext.UserId, _branchContext.Branch.Id, null);
                                    if (relatedAction.SellingId.HasValue)
                                    {
                                        contract = _contractService.Get(relatedAction.ContractId);
                                        contract.Status = ContractStatus.Disposed;
                                        _contractService.Save(contract);
                                        var selling = _contractSellingService.GetSelling(relatedAction.SellingId.Value);
                                        selling.Status = Data.Models.Sellings.SellingStatus.Sold;
                                        _contractSellingService.Save(selling);
                                    }
                                    if (relatedAction.ExpenseId.HasValue)
                                    {
                                        var expense = await _contractExpenseService.GetAsync(relatedAction.ExpenseId.Value);
                                        var expenseType = _expenseService.Get(expense.ExpenseId);
                                        if (expense != null && !expenseType.ExtraExpense)
                                        {
                                            expense.IsPayed = true;
                                            _contractExpenseService.Save(expense);
                                        }
                                    }

                                    var close = new CollectionClose()
                                    {
                                        ContractId = relatedAction.ContractId,
                                        ActionId = relatedAction.Id
                                    };
                                    contractIdsForCloseLegalCase.Add(close.ContractId);
                                    var isClosed = _collectionService.CloseContractCollection(close);
                                    if (isClosed)
                                        await mediator.Send(new SendClosedContractCommand() { ContractId = relatedAction.ContractId });
                                    else
                                        await mediator.Send(new SendContractOnlyCommand() { ContractId = relatedAction.ContractId });

                                    break;
                                }
                            case ContractActionType.Payment:
                                {
                                    _contractPaymentService.ExecuteOnApprove(relatedAction, _branchContext.Branch.Id, _sessionContext.UserId, forceExpensePrepaymentReturn: false);

                                    var close = new CollectionClose()
                                    {
                                        ContractId = relatedAction.ContractId,
                                        ActionId = relatedAction.Id
                                    };
                                    contractIdsForCloseLegalCase.Add(close.ContractId);
                                    var isClosed = _collectionService.CloseContractCollection(close);
                                    if (isClosed)
                                        await mediator.Send(new SendClosedContractCommand() { ContractId = relatedAction.ContractId });
                                    else
                                        await mediator.Send(new SendContractOnlyCommand() { ContractId = relatedAction.ContractId });

                                    break;
                                }
                            case ContractActionType.Prepayment:
                                {
                                    if (relatedAction.IsInitialFee.HasValue && relatedAction.IsInitialFee.Value)
                                    {
                                        decimal depoMerchantBalance = _contractService.GetDepoMerchantBalance(contract.Id, relatedAction.Date);
                                        if (contract.RequiredInitialFee <= depoMerchantBalance)
                                        {
                                            // перезагружаем договор
                                            contract = _contractRepository.Get(contract.Id);
                                            if (contract == null)
                                                throw new PawnshopApplicationException($"Договор {contract.Id} не найден");

                                            contract.Status = ContractStatus.PositionRegistration;
                                            contract.PayedInitialFee = depoMerchantBalance;
                                            _contractRepository.Update(contract);
                                        }
                                        await mediator.Send(new SendContractOnlyCommand() { ContractId = relatedAction.ContractId });
                                    }
                                    await _legalCollectionNotificationService
                                        .SendPrepaymentReceivedToLegalCase(relatedAction.ContractId, _sessionContext.UserId);
                                    break;
                                }
                            case ContractActionType.PrepaymentReturn:
                                {
                                    if (contract.Status < ContractStatus.Signed)
                                        contract.Status = ContractStatus.Canceled;
                                    _contractRepository.Update(contract);
                                    break;
                                }
                            case ContractActionType.PartialPayment:
                                {
                                    await _contractPaymentScheduleService.UpdateContractPaymentScheduleHistoryStatus(relatedAction.ContractId, relatedAction.Id, 20);
                                    await _contractPaymentScheduleService.UpdateActionIdForPartialPayment(relatedAction.Id, relatedAction.Date, relatedAction.ContractId);
                                    var penaltyCost = relatedAction.Rows.Where(x => (x.PaymentType == AmountType.DebtPenalty || x.PaymentType == AmountType.LoanPenalty) && x.DebitAccountId != null).Sum(r => r.Cost);
                                    await _contractPaymentScheduleService.UpdateActionIdForPartialPaymentUnpaid(relatedAction.Id, relatedAction.Date, relatedAction.ContractId, penaltyCost, isEndPeriod: contract.PercentPaymentType == PercentPaymentType.EndPeriod);

                                    if (contract.PercentPaymentType == PercentPaymentType.EndPeriod)
                                    {
                                        var unpaid = await _contractPaymentScheduleService.GetUnpaidSchedule(contract.Id);
                                        if (unpaid != null)
                                        {
                                            contract.MaturityDate = unpaid.Date;
                                        }
                                        _contractRepository.Update(contract);
                                    }
                                    else
                                    {
                                        var nextPaymentDate = await _contractPaymentScheduleService.GetNextPaymentSchedule(contract.Id);
                                        if (nextPaymentDate != null)
                                        {
                                            contract.NextPaymentDate = nextPaymentDate.Date;
                                        }
                                        _contractRepository.Update(contract);
                                    }

                                    var close = new CollectionClose()
                                    {
                                        ContractId = relatedAction.ContractId,
                                        ActionId = relatedAction.Id
                                    };
                                    contractIdsForCloseLegalCase.Add(close.ContractId);
                                    var isClosed = _collectionService.CloseContractCollection(close);
                                    if (isClosed)
                                        await mediator.Send(new SendClosedContractCommand() { ContractId = relatedAction.ContractId });
                                    else
                                        await mediator.Send(new SendContractOnlyCommand() { ContractId = relatedAction.ContractId });


                                    _crmPaymentService.Enqueue(contract);
                                    break;
                                }
                        }
                    }
                }
                else
                {
                    await ApproveAuctionCashOrders(cashOrder, languageId: cashOrderApproveStructure.LanguageId);
                    ApproveOrder(_sessionContext.UserId, _branchContext.Branch, ref cashOrder, languageId: cashOrderApproveStructure.LanguageId);

                    if (cashOrder.TasOnlinePaymentId != null)
                    {
                        _tasOnlineRequestService.PrepareAndSendRequest(cashOrder);
                    }
                    if (cashOrder.TMFPaymentId != null)
                    {
                        _tMFRequestService.PrepareAndSendRequest(cashOrder);
                    }
                }

                transaction.Commit();
            }
            if (contractIdsForCloseLegalCase.Any())
            {
                foreach (var contractId in contractIdsForCloseLegalCase)
                {
                    await _closeLegalCaseService.CloseAsync(contractId);
                }
            }

            await ApproveInAuction(cashOrder);
            
            var orderIds = new List<int>();
            if (cashOrder.ContractActionId.HasValue)
                orderIds = await _service.GetAllRelatedOrdersByContractActionId(cashOrder.ContractActionId.Value);
            else
                orderIds.Add(cashOrder.Id);
            _uKassaService.FinishRequests(orderIds);
            return cashOrder;
        }

        [HttpPost("/api/cashOrder/confirm")]
        [Event(EventCode.CashOrderConfirmed, EventMode = EventMode.All, EntityType = EntityType.CashOrder, IncludeFails = true)]
        public async Task<CashOrder> Confirm([FromBody] int id)
        {
            if (_sessionContext.Permissions.Where(x => x == Permissions.CashOrderCashTransaction).FirstOrDefault() == null
                && _sessionContext.Permissions.Where(x => x == Permissions.CashOrderConfirm).FirstOrDefault() == null)
            {
                throw new PawnshopApplicationException("У вас недостаточно прав на согласование кассового ордера.");
            }

            if (id <= 0)
                throw new ArgumentOutOfRangeException(nameof(id));

            var cashOrder = await _service.GetAsync(id);

            if (cashOrder.OrderDate.Date != DateTime.Today.Date && !_sessionContext.ForSupport)
            {
                throw new PawnshopApplicationException("Данный ордер не может быть согласован. Обратитесь к администратору.");
            }

            if (cashOrder.OperationId.HasValue)
            {
                throw new PawnshopApplicationException("Нельзя изменять статус кассового ордера, привязанного к платежной операции!");
            }

            if (cashOrder.ApproveStatus == OrderStatus.WaitingForConfirmation)
            {
                var contractExpenseRowOrderANDrelatedOrdersList = await _service.GetRelatedContractExpenseOrders(cashOrder.Id);

                //проверяем если есть кассовые (10,20,60) ордера
                var cashOrderExists = false;
                if (cashOrder.OrderType == OrderType.CashIn || cashOrder.OrderType == OrderType.CashOut || cashOrder.OrderType == OrderType.Payment)
                {
                    cashOrderExists = true;
                }
                foreach (var order in contractExpenseRowOrderANDrelatedOrdersList.Item2)
                {
                    if (!order.ContractActionId.HasValue && (
                    order.OrderType == OrderType.CashIn || order.OrderType == OrderType.CashOut || order.OrderType == OrderType.Payment))
                    {
                        cashOrderExists = true;
                    }
                }

                using (var transaction = _service.BeginCashOrderTransaction())
                {
                    //если есть кассовые (10,20,60) ордера, то отправляем на апрув
                    if (cashOrderExists)
                    {
                        cashOrder.ApproveStatus = OrderStatus.WaitingForApprove;
                        cashOrder.ApprovedId = _sessionContext.UserId;
                        cashOrder.Approved = _userRepository.Get(_sessionContext.UserId);
                        cashOrder.ApproveDate = DateTime.Now;
                        await _service.ChangeStatusForRelatedOrders(cashOrder, OrderStatus.WaitingForApprove, _sessionContext.UserId, _branchContext.Branch, _sessionContext.ForSupport);
                        _service.Register(cashOrder, _branchContext.Branch);

                        foreach (var order in contractExpenseRowOrderANDrelatedOrdersList.Item2)
                        {
                            if (order.DeleteDate.HasValue)
                                throw new PawnshopApplicationException($"Зависимый кассовый ордер {order.Id} является удаленным, нельзя его подтвердить");

                            if (order.ApproveStatus == OrderStatus.Prohibited)
                                throw new PawnshopApplicationException($"Зависимый Кассовый ордер {order.Id} является отклоненным, нельзя его подтвердить");

                            order.ApproveStatus = OrderStatus.WaitingForApprove;
                            await _service.ChangeStatusForRelatedOrders(order, OrderStatus.WaitingForApprove, _sessionContext.UserId, _branchContext.Branch, _sessionContext.ForSupport);
                            _service.Register(order, _branchContext.Branch);
                        }
                    }
                    else //если нет кассовых ордеров то автоматом апрувим после согласования
                    {
                        ApproveOrder(_sessionContext.UserId, _branchContext.Branch, ref cashOrder);
                    }
                    transaction.Commit();
                    _uKassaService.FinishRequests(contractExpenseRowOrderANDrelatedOrdersList.Item2.Select(x => x.Id).ToList());
                }
            }
            return cashOrder;
        }

        [HttpPost("/api/cashOrder/prohibit")]
        [Event(EventCode.CashOrderProhibited, EventMode = EventMode.Response, EntityType = EntityType.CashOrder, IncludeFails = true)]
        public async Task<CashOrder> Prohibit([FromBody] int id)
        {
            if (_sessionContext.Permissions.Where(x => x == Permissions.CashOrderCashTransaction).FirstOrDefault() == null
                && _sessionContext.Permissions.Where(x => x == Permissions.CashOrderApprove).FirstOrDefault() == null)
            {
                throw new PawnshopApplicationException("У вас недостаточно прав на согласование кассового ордера.");
            }

            if (id <= 0)
                throw new ArgumentOutOfRangeException(nameof(id));

            var cashOrder = await _service.GetAsync(id);

            if (cashOrder.ContractActionId.HasValue)
            {
                var action = _contractActionService.GetAsync(cashOrder.ContractActionId.Value).Result;
                if (!action.Status.HasValue || action.Status == ContractActionStatus.Approved)
                    throw new PawnshopApplicationException("Действие не требует подтверждения или уже подтверждено");

                using var transaction = _service.BeginCashOrderTransaction();
                {
                    await _contractActionOperationService.Cancel(action.Id, _sessionContext.UserId, _branchContext.Branch.Id, false, true);
                    transaction.Commit();
                }
            }
            else
            {
                if (cashOrder.OrderDate.Date != DateTime.Today.Date && !_sessionContext.ForSupport)
                {
                    throw new PawnshopApplicationException("Данный ордер не может быть отклонен. Обратитесь к администратору.");
                }

                if (cashOrder.OperationId.HasValue)
                {
                    throw new PawnshopApplicationException("Нельзя изменять статус кассового ордера, привязанного к платежной операции!");
                }

                using (var transaction = _service.BeginCashOrderTransaction())
                {
                    cashOrder.ApprovedId = _sessionContext.UserId;
                    cashOrder.ApproveStatus = OrderStatus.Prohibited;
                    cashOrder.ApproveDate = DateTime.Now;
                    _service.Register(cashOrder, _branchContext.Branch);

                    var contractExpenseRowOrderANDrelatedOrdersList = await _service.GetRelatedContractExpenseOrders(cashOrder.Id);
                    foreach (var order in contractExpenseRowOrderANDrelatedOrdersList.Item2)
                    {
                        if (order.DeleteDate.HasValue)
                            throw new PawnshopApplicationException($"Зависимый кассовый ордер {order.Id} является удаленным, нельзя его отклонить");

                        if (order.ApproveStatus == OrderStatus.Approved)
                            throw new PawnshopApplicationException($"Зависимый кассовый ордер {order.Id} является подтвержденным, нельзя его отклонить");

                        order.ApprovedId = _sessionContext.UserId;
                        order.ApproveStatus = OrderStatus.Prohibited;
                        order.ApproveDate = DateTime.Now;
                        _service.Register(order, _branchContext.Branch);

                        var expenseRow = _contractExpenseRowRepository.Get(contractExpenseRowOrderANDrelatedOrdersList.Item1.ContractExpenseRowId);
                        if (expenseRow != null)
                            _contractExpenseOperationService.DeleteWithOrders(expenseRow.ContractExpenseId, _sessionContext.UserId, _branchContext.Branch.Id);
                    }
                    
                    transaction.Commit();
                    if (cashOrder.AuctionRequestId.HasValue)
                    {
                        await _auctionOperationHttpService.Reject(cashOrder.AuctionRequestId.GetValueOrDefault());
                    }
                }
            }

            return cashOrder;
        }

        [HttpPost("/api/cashOrder/export")]
        public async Task<IActionResult> Export([FromBody] List<CashOrder> cashOrders)
        {
            using (var stream = _excelBuilder.Build(cashOrders))
            {
                var fileName = await _storage.Save(stream, ContainerName.Temp, "export.xlsx");
                string contentType;
                new FileExtensionContentTypeProvider().TryGetContentType(fileName, out contentType);

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

        [HttpPost("/api/cashOrder/find")]
        public async Task<CashOrder> Find([FromBody] CashOrderFilter query) => await Task.Run(() => _service.Find(query));

        [HttpGet("/api/cashOrder/getfiscalchecks")]
        public async Task<IActionResult> GetFiscalChecks(int OrderId)
        {
            var cashOrder = await _service.GetAsync(OrderId);
            if (cashOrder == null)
                return NotFound();
            var relatedActions = await _contractActionService.GetRelatedContractActionsByOrder(cashOrder);
            var list = new List<int>();
            if (relatedActions.Count > 0)
            {
                foreach (var contractActionId in relatedActions.OrderBy(x => x))
                {
                    list.AddRange(_cashOrderRepository.GetOrderIdsForFiscalCheck(contractActionId));
                }
            }
            else
            {
                list.Add(OrderId);
            }
            return Ok(list);
        }

        [HttpGet("/api/cashOrder/getrelatedorders")]
        public async Task<IActionResult> GetRelatedOrders(int OrderId)
        {
            List<CashOrder> result = new List<CashOrder>();
            var cashOrder = await _service.GetAsync(OrderId);
            if (cashOrder == null)
                return NotFound();
            var relatedActions = await _contractActionService.GetRelatedContractActionsByOrder(cashOrder);
            if (relatedActions.Count > 0)
            {
                foreach (var contractActionId in relatedActions.OrderBy(x => x))
                {
                    result.AddRange(_cashOrderRepository.GetAllCashOrdersByContractActionId(contractActionId));
                }
            }
            else
            {
                result.Add(cashOrder);
            }
            return Ok(result);
        }

        private void ApproveOrder(int userId, Group branch, ref CashOrder cashOrder, int? languageId = null)
        {
            if (cashOrder.ApproveStatus == OrderStatus.Approved) return;
            
            cashOrder.ApprovedId = userId;
            cashOrder.Approved = _userRepository.Get(userId);
            cashOrder.ApproveStatus = OrderStatus.Approved;
            cashOrder.ApproveDate = DateTime.Now;
            _service.Register(cashOrder, branch);
            _service.SetLanguageIfNecessary(cashOrder, languageId);

            var contractExpenseRowOrderANDrelatedOrdersList = _service.GetRelatedContractExpenseOrders(cashOrder.Id).Result;
            foreach (var order in contractExpenseRowOrderANDrelatedOrdersList.Item2)
            {
                if (order.DeleteDate.HasValue)
                    throw new PawnshopApplicationException($"Зависимый кассовый ордер {order.Id} является удаленным, нельзя его подтвердить");

                if (order.ApproveStatus == OrderStatus.Prohibited)
                    throw new PawnshopApplicationException($"Зависимый Кассовый ордер {order.Id} является отклоненным, нельзя его подтвердить");

                order.ApprovedId = userId;
                order.ApproveStatus = OrderStatus.Approved;
                order.ApproveDate = DateTime.Now;
                _service.Register(order, branch);
                _service.SetLanguageIfNecessary(cashOrder, languageId);
                var expenseRow = _contractExpenseRowRepository.Get(contractExpenseRowOrderANDrelatedOrdersList.Item1.ContractExpenseRowId);
                var expense = _contractExpenseService.GetAsync(expenseRow.ContractExpenseId).Result;
                var expenseType = _expenseService.Get(expense.ExpenseId);
                if (expense != null && !expenseType.ExtraExpense)
                {
                    expense.IsPayed = true;
                    _contractExpenseService.Save(expense);
                }
            }
        }
        
        private async Task ApproveAuctionCashOrders(CashOrder cashOrder, int? languageId = null)
        {
            if (!cashOrder.AuctionRequestId.HasValue) return;

            IEnumerable<AuctionPayment> auctionPayments = await _auctionPaymentRepository.GetByRequestIdAsync(cashOrder.AuctionRequestId.Value);
            if (auctionPayments.IsNullOrEmpty()) return;

            var auctionExpensesBOS = _businessOperationSettingService.GetByCode(Constants.ACCOUNT_SETTING_EXPENSE);
            if (cashOrder.BusinessOperationSettingId == auctionExpensesBOS.Id)
            {
                await ApproveRemittance(cashOrder.Id);
            }

            ApproveOrder(_sessionContext.UserId, _branchContext.Branch, ref cashOrder, languageId);
            
            var cashOrderIds = auctionPayments
                .Select(payment => payment.CashOrderId)
                .Where(id => id != cashOrder.Id)
                .Distinct()
                .ToList();

            if (!cashOrderIds.Any())
            {
                await ApproveRemittance(cashOrder.Id);
                return;
            }

            foreach (var cashOrderId in cashOrderIds)
            {
                var relatedOrder = await _service.GetAsync(cashOrderId);
                if (relatedOrder == null) continue;

                ApproveOrder(_sessionContext.UserId, _branchContext.Branch, ref relatedOrder, languageId);
            }
            
            await ApproveRemittance(cashOrder.Id);
        }

        private async Task ApproveInAuction(CashOrder cashOrder)
        {
            if (!cashOrder.AuctionRequestId.HasValue) return;
            
            await _auctionOperationHttpService.Approve(new ApproveAuctionCommand
                {
                    RequestId = (Guid)cashOrder.AuctionRequestId,
                    AuthorName = _sessionContext.UserName
                }
            );
        }
        
        private async Task ApproveRemittance(int cashOrderId)
        {
            var cashOrderRemittance = await _orderRemittanceService.GetByCashOrderId(cashOrderId);
            if (cashOrderRemittance is null) return;
            
            var remittance = await _remittanceService.GetAsync(cashOrderRemittance.RemittanceId);
            if (remittance is null || remittance.Status == RemittanceStatusType.Received) return;
            
            _remittanceService.Accept(remittance.Id, _sessionContext.UserId);
        }
    }
}