using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Pawnshop.AccountingCore.Models;
using Pawnshop.Core;
using Pawnshop.Core.Exceptions;
using Pawnshop.Data.Access;
using Pawnshop.Data.Models.Audit;
using Pawnshop.Data.Models.CashOrders;
using Pawnshop.Data.Models.InnerNotifications;
using Pawnshop.Services;
using Pawnshop.Services.Remittances;
using Pawnshop.Web.Engine;
using Pawnshop.Web.Engine.Audit;
using Pawnshop.Web.Engine.Middleware;
using Pawnshop.Web.Models.CashOrder;
using Pawnshop.Web.Models.List;

namespace Pawnshop.Web.Controllers.Api
{
    [Authorize(Permissions.CashOrderView)]
    public class RemittanceController : Controller
    {
        private readonly RemittanceRepository _remittanceRepository;
        private readonly BranchContext _branchContext;
        private readonly ISessionContext _sessionContext;
        private readonly IRemittanceService _remittanceService;

        public RemittanceController(RemittanceRepository remittanceRepository,
                                    BranchContext  branchContext, ISessionContext sessionContext,
                                    IRemittanceService remittanceService)
        {
            _remittanceRepository = remittanceRepository;
            _branchContext = branchContext;
            _sessionContext = sessionContext;
            _remittanceService = remittanceService;
        }

        [HttpPost]
        public ListModel<Remittance> List([FromBody] ListQueryModel<RemittanceListQueryModel> listQuery)
        {
            if (listQuery == null) listQuery = new ListQueryModel<RemittanceListQueryModel>();
            if (listQuery.Model == null) listQuery.Model = new RemittanceListQueryModel();
            listQuery.Model.BranchId = _branchContext.Branch.Id;

            if (listQuery.Model.EndDate.HasValue)
            {
                listQuery.Model.EndDate = listQuery.Model.EndDate.Value.Date.AddHours(23).AddMinutes(59).AddSeconds(59);
            }

            return new ListModel<Remittance>
            {
                List = _remittanceRepository.List(listQuery, listQuery.Model),
                Count = _remittanceRepository.Count(listQuery, listQuery.Model)
            };
        }

        [HttpPost]
        public Remittance Card([FromBody] int id)
        {
            if (id <= 0) throw new ArgumentOutOfRangeException(nameof(id));

            var model = _remittanceRepository.Get(id);
            if (model == null) throw new InvalidOperationException();

            return model;
        }

        [HttpPost, Authorize(Permissions.CashOrderManage), ProducesResponseType(typeof(Remittance), 200)]
        [Event(EventCode.RemittanceSaved, EventMode = EventMode.Response, EntityType = EntityType.OnlinePayment)]
        public IActionResult Save([FromBody] Remittance model)
        {
            ModelState.Validate();
            if (model.Id == 0)
                model.SendBranchId = _branchContext.Branch.Id;

            int branchId = _branchContext.Branch.Id;
            int authorId = _sessionContext.UserId;
            if (model.SendBranchId != branchId) 
                throw new PawnshopApplicationException("Запрещено редактировать переводы созданные не в вашем филиале");

            Remittance remittance = null;
            if (model.Id > 0)
                remittance = _remittanceService.Update(model.Id, model.ReceiveBranchId, model.SendCost, model.Note, authorId, branchId);
            else
                remittance = _remittanceService.Register(branchId, model.ReceiveBranchId, model.SendCost, model.Note, authorId);

            return Ok(remittance);
        }

        [HttpPost, Authorize(Permissions.CashOrderManage)]
        [Event(EventCode.RemittanceDeleted, EventMode = EventMode.Response, EntityType = EntityType.OnlinePayment)]
        public IActionResult Delete([FromBody] int id)
        {
            int branchId = _branchContext.Branch.Id;
            _remittanceService.Delete(id, branchId);
            return Ok();
        }

        [HttpPost, Authorize(Permissions.CashOrderManage), ProducesResponseType(typeof(Remittance), 200)]
        [Event(EventCode.RemittanceReceived, EventMode = EventMode.Response, EntityType = EntityType.OnlinePayment)]
        public IActionResult Receive([FromBody] int id)
        {
            int authorId = _sessionContext.UserId;
            int branchId = _branchContext.Branch.Id;
            Remittance remittance = _remittanceService.Accept(id, authorId, branchId);
            return Ok(remittance);
        }

        [HttpPost, Authorize(Permissions.CashOrderManage)]
        [Event(EventCode.RemittanceReceiveCanceled, EventMode = EventMode.Response, EntityType = EntityType.OnlinePayment)]
        public async Task<IActionResult> ReceiveCancel([FromBody] int id)
        {
            int branchId = _branchContext.Branch.Id;
            int authorId = _sessionContext.UserId;
            await _remittanceService.CancelAcceptAsync(id, authorId, branchId);
            return Ok();
        }
    }
}
