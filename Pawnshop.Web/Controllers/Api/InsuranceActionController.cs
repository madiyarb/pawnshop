using System;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Pawnshop.AccountingCore.Models;
using Pawnshop.Core;
using Pawnshop.Core.Exceptions;
using Pawnshop.Data.Access;
using Pawnshop.Data.Models.Audit;
using Pawnshop.Data.Models.CashOrders;
using Pawnshop.Data.Models.Insurances;
using Pawnshop.Web.Engine;
using Pawnshop.Web.Engine.Audit;

namespace Pawnshop.Web.Controllers.Api
{
    public class InsuranceActionController : Controller
    {
        private readonly InsuranceActionRepository _actionRepository;
        private readonly InsuranceRepository _insuranceRepository;
        private readonly CashOrderRepository _orderRepository;
        private readonly CashOrderNumberCounterRepository _cashCounterRepository;
        private readonly BranchContext _branchContext;
        private readonly ISessionContext _sessionContext;
        private readonly EventLog _eventLog;

        public InsuranceActionController(InsuranceActionRepository actionRepository, 
                                         InsuranceRepository insuranceRepository, 
                                         CashOrderRepository orderRepository, 
                                         CashOrderNumberCounterRepository cashCounterRepository,
                                         BranchContext branchContext,
                                         ISessionContext sessionContext, 
                                         EventLog eventLog)
        {
            _actionRepository = actionRepository;
            _insuranceRepository = insuranceRepository;
            _orderRepository = orderRepository;
            _cashCounterRepository = cashCounterRepository;
            _branchContext = branchContext;
            _sessionContext = sessionContext;
            _eventLog = eventLog;
        }

        [HttpPost, Authorize(Permissions.InsuranceActionManage)]
        public InsuranceAction Sign([FromBody] InsuranceAction action)
        {
            if (action == null) throw new ArgumentNullException(nameof(action));
            if (action.InsuranceId <= 0) throw new ArgumentException();

            var insurance = _insuranceRepository.Get(action.InsuranceId);
            if (insurance.Status != InsuranceStatus.Draft) throw new InvalidOperationException();

            using (var transaction = _actionRepository.BeginTransaction())
            {
                action.ActionType = InsuranceActionType.Sign;
                action.ActionDate = DateTime.Now;
                action.AuthorId = _sessionContext.UserId;

                var debitAccountId = _branchContext.Configuration?.CashOrderSettings?.InsuranceSettings?.SignSettings?.DebitId ?? 0;
                var creditAccountId = _branchContext.Configuration?.CashOrderSettings?.InsuranceSettings?.SignSettings?.CreditId ?? 0;
                CreateOrder(insurance, action, OrderType.CashOut, debitAccountId, creditAccountId);

                ModelState.Clear();
                TryValidateModel(action);
                ModelState.Validate();

                insurance.Status = InsuranceStatus.Signed;
                _insuranceRepository.Update(insurance);
                _actionRepository.Insert(action);

                transaction.Commit();
            }

            return action;
        }

        [HttpPost]
        public void Cancel([FromBody] InsuranceAction action)
        {
            ModelState.Validate();

            if (action == null) throw new ArgumentNullException(nameof(action));
            if (action.InsuranceId <= 0) throw new ArgumentException();

            if (action.ActionDate.Date != DateTime.Today)
            {
                throw new PawnshopApplicationException("Данное действие отмене не подлежит.");
            }

            var insurance = _insuranceRepository.Get(action.InsuranceId);

            using (var transaction = _actionRepository.BeginTransaction())
            {
                switch (action.ActionType)
                {
                    case InsuranceActionType.Sign:
                        if (insurance.Status != InsuranceStatus.Signed) throw new InvalidOperationException();
                        insurance.Status = InsuranceStatus.Draft;
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }

                _orderRepository.Delete(action.OrderId);
                _eventLog.Log(EventCode.CashOrderDeleted, EventStatus.Success, EntityType.CashOrder, action.OrderId, null, null);
                _insuranceRepository.Update(insurance);
                _actionRepository.Delete(action.Id);

                transaction.Commit();
            }
        }

        private void CreateOrder(Insurance insurance, InsuranceAction action, OrderType orderType, int debitAccountId, int creditAccountId)
        {
            string code;
            switch (orderType)
            {
                case OrderType.CashIn:
                    code = _branchContext.Configuration.CashOrderSettings.CashInNumberCode;
                    break;
                case OrderType.CashOut:
                    code = _branchContext.Configuration.CashOrderSettings.CashOutNumberCode;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(orderType), orderType, null);
            }

            var order = new CashOrder
            {
                OrderType = orderType,
                ClientId = insurance.Contract.ClientId,
                DebitAccountId = debitAccountId,
                CreditAccountId = creditAccountId,
                OrderCost = insurance.InsuranceCost,
                OrderDate = action.ActionDate,
                Reason = GetReason(action.ActionType),
                RegDate = DateTime.Now,
                OwnerId = _branchContext.Branch.Id,
                BranchId = _branchContext.Branch.Id,
                AuthorId = _sessionContext.UserId,
                OrderNumber = _cashCounterRepository.Next(orderType, action.ActionDate.Year, _branchContext.Branch.Id, code)
            };

            if (order.DebitAccountId == 0 || order.CreditAccountId == 0)
            {
                throw new PawnshopApplicationException("Настройте счета дебет и кредит для страхового договора");
            }

            _orderRepository.Insert(order);
            _eventLog.Log(EventCode.CashOrderSaved, EventStatus.Success, EntityType.CashOrder, order.Id, null, null);

            action.OrderId = order.Id;
        }

        private string GetReason(InsuranceActionType actionType)
        {
            switch (actionType)
            {
                case InsuranceActionType.Sign:
                    return "Подписание страхового договора";
                default:
                    return string.Empty;
            }
        }
    }
}
