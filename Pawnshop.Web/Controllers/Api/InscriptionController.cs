using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Pawnshop.AccountingCore.Models;
using Pawnshop.Core.Exceptions;
using Pawnshop.Core.Queries;
using Pawnshop.Core;
using Pawnshop.Data.Access.AccountingCore;
using Pawnshop.Data.Access;
using Pawnshop.Data.Models.Audit;
using Pawnshop.Data.Models.CashOrders;
using Pawnshop.Data.Models.Contracts.Discounts;
using Pawnshop.Data.Models.Contracts.Inscriptions;
using Pawnshop.Data.Models.Contracts;
using Pawnshop.Services.AbsOnline;
using Pawnshop.Services.AccountingCore;
using Pawnshop.Services.Contracts;
using Pawnshop.Services.Models.List;
using Pawnshop.Web.Engine.Audit;
using Pawnshop.Web.Engine.Middleware;
using Pawnshop.Web.Engine;
using Pawnshop.Web.Models.Inscription;
using System.Collections.Generic;
using System.Linq;
using System;
using Pawnshop.Services.CreditLines;

namespace Pawnshop.Web.Controllers.Api
{
    [Authorize(Permissions.InscriptionView)]
    public class InscriptionController : Controller
    {

        private readonly ISessionContext _sessionContext;
        private readonly EventLog _eventLog;
        private readonly IInscriptionService _inscriptionService;
        private readonly IContractService _contractService;
        private readonly ContractDiscountRepository _contractDiscountRepository;
        private readonly ICashOrderService _cashOrderService;
        private readonly BranchContext _branchContext;
        private readonly AccountRecordRepository _accountRecordRepository;
        private readonly IAccountRecordService _accountRecordService;
        private readonly CashOrderRepository _cashOrderRepository;
        private readonly IBusinessOperationSettingService _businessOperationSettingService;
        private readonly IAbsOnlineService _absOnlineService;

        public InscriptionController(
            ISessionContext sessionContext,
            EventLog eventLog, ICashOrderService cashOrderService,
            IInscriptionService inscriptionService,
            IContractService contractService,
            ContractDiscountRepository contractDiscountRepository,
            BranchContext branchContext, AccountRecordRepository accountRecordRepository,
            IAccountRecordService accountRecordService,
            CashOrderRepository cashOrderRepository,
            IBusinessOperationSettingService businessOperationSettingService,
            IAbsOnlineService absOnlineService
            )
        {
            _sessionContext = sessionContext;
            _eventLog = eventLog;
            _inscriptionService = inscriptionService;
            _contractService = contractService;
            _contractDiscountRepository = contractDiscountRepository;
            _cashOrderService = cashOrderService;
            _branchContext = branchContext;
            _accountRecordRepository = accountRecordRepository;
            _accountRecordService = accountRecordService;
            _cashOrderRepository = cashOrderRepository;
            _businessOperationSettingService = businessOperationSettingService;
            _absOnlineService = absOnlineService;
        }

        [HttpPost]
        public ListModel<Inscription> List([FromBody] ListQuery listQuery) => _inscriptionService.List(listQuery);

        [HttpPost]
        public Inscription Card([FromBody] int id)
        {
            if (id <= 0) throw new ArgumentOutOfRangeException(nameof(id));
            var result = _inscriptionService.Get(id);
            if (result == null) throw new InvalidOperationException();
            return result;
        }

        [HttpPost, Authorize(Permissions.InscriptionManage)]
        [Event(EventCode.InscriptionSaved, EventMode = EventMode.Request, EntityType = EntityType.Contract)]
        public Inscription Save([FromBody] Inscription inscription)
        {
            ModelState.Validate();
            if (inscription.Id > 0)
                throw new PawnshopApplicationException("Исполнительная надпись уже была сохранена ранее");

            inscription.AuthorId = _sessionContext.UserId;
            inscription.CreateDate = DateTime.Now;
            inscription.Status = InscriptionStatus.New;
            inscription.Actions = new List<InscriptionAction>
            {
                new InscriptionAction()
                {
                    ActionType = InscriptionActionType.Сreation,
                    AuthorId = _sessionContext.UserId,
                    CreateDate = DateTime.Now,
                    Date = inscription.Date,
                    Rows = inscription.Rows
                }
            };
            var contract = _contractService.Get(inscription.ContractId);
            if (contract == null)
                throw new PawnshopApplicationException($"Договор {nameof(contract)} не найден");

            CheckForDiscountsExistance(contract);
            contract.IsOffBalance = true;
            _contractService.Save(contract);
            _inscriptionService.Save(inscription);

            _absOnlineService.SendNotificationCreditLineChangedAsync(contract.Id, contract).Wait();

            return inscription;
        }

        [HttpPost, Authorize(Permissions.InscriptionManage)]
        [Event(EventCode.InscriptionApproved, EventMode = EventMode.Request, EntityType = EntityType.Contract)]
        public IActionResult Approve([FromBody] InscriptionModel inscription)
        {
            ModelState.Validate();
            if (inscription.Id <= 0) throw new ArgumentException(nameof(inscription.Id));

            var oldInscription = _inscriptionService.Get(inscription.Id);
            if (oldInscription.Status >= InscriptionStatus.Approved)
                throw new PawnshopApplicationException("Исполнительная надпись уже была утверждена ранее");

            var orders = _inscriptionService.WriteOffBalance(inscription.ContractId, inscription.RowsToWriteOff, DateTime.Now);

            using (var transaction = _inscriptionService.BeginInscriptionTransaction())
            {
                InscriptionAction action = new InscriptionAction()
                {
                    ActionType = InscriptionActionType.Confirm,
                    AuthorId = _sessionContext.UserId,
                    CreateDate = DateTime.Now,
                    Date = inscription.Date,
                    InscriptionId = oldInscription.Id
                };

                UpdateActionRows(orders, inscription.Rows);
                action.Rows = inscription.Rows;

                _inscriptionService.InsertAction(action);
                transaction.Commit();
            }

            oldInscription.Status = InscriptionStatus.Approved;
            oldInscription.TotalCost = inscription.TotalCost;
            _inscriptionService.Save(oldInscription);

            _absOnlineService.SendNotificationCreditLineChangedAsync(inscription.ContractId).Wait();

            return Ok();
        }

        [HttpPost, Authorize(Permissions.InscriptionManage)]
        [Event(EventCode.InscriptionDenied, EventMode = EventMode.Request, EntityType = EntityType.Contract)]
        public IActionResult Deny([FromBody] InscriptionModel inscription, [FromServices] ICreditLineService creditLineService)
        {
            ModelState.Validate();
            if (inscription.Id <= 0) throw new ArgumentException(nameof(inscription.Id));

            var oldInscription = _inscriptionService.Get(inscription.Id);

            if (oldInscription.Status >= InscriptionStatus.Denied)
                throw new PawnshopApplicationException("Исполнительная надпись уже была отозвана или исполнена");
            using (var transaction = _inscriptionService.BeginInscriptionTransaction())
            {
                InscriptionAction action = new InscriptionAction()
                {
                    ActionType = InscriptionActionType.Withdraw,
                    AuthorId = _sessionContext.UserId,
                    CreateDate = DateTime.Now,
                    Date = DateTime.Now,
                    InscriptionId = inscription.Id
                };

                oldInscription.Status = InscriptionStatus.Denied;
                var contract = _contractService.Get(inscription.ContractId);
                if (contract == null)
                    throw new PawnshopApplicationException("Договор {inscription.ContractId} не найден");

                contract.IsOffBalance = false;
                _contractService.Save(contract);

                if (inscription.RowsToWriteOff.Any())
                {
                    var orders = _inscriptionService.RestoreOnBalance(contract, DateTime.Now, inscription.RowsToWriteOff);
                    var rows = inscription.RowsToWriteOff.Select(t => new InscriptionRow() { Cost = t.Cost, PaymentType = t.PaymentType }).Distinct().ToList();
                    UpdateActionRows(orders, rows);

                    action.Rows = rows;
                }

                _inscriptionService.RestoreOrders(oldInscription, contract, InscriptionActionType.Confirm);

                _inscriptionService.InsertAction(action);
                _inscriptionService.Save(oldInscription);
                transaction.Commit();
            }
            _absOnlineService.SendNotificationCreditLineChangedAsync(inscription.ContractId).Wait();
            var contractForMovePrepayment = _contractService.Get(inscription.ContractId);
            if (contractForMovePrepayment.ContractClass == ContractClass.Tranche && contractForMovePrepayment.CreditLineId != null)
            {

                var balance = creditLineService.GetCurrentlyDebtForCreditLine(contractForMovePrepayment.CreditLineId.Value).Result;
                var prepaymentBalanceOnTranche = balance.ContractsBalances
                    .SingleOrDefault(contr => contr.ContractId == contractForMovePrepayment.Id)
                    .PrepaymentBalance;
                if (prepaymentBalanceOnTranche > 0)
                {
                    using (var transaction = _inscriptionService.BeginInscriptionTransaction())
                    {
                        creditLineService.MovePrepaymentFromTrancheToCreditLine(contractForMovePrepayment.CreditLineId.Value,
                            contractForMovePrepayment.Id, prepaymentBalanceOnTranche,
                            _sessionContext.UserId, contractForMovePrepayment.BranchId, autoApprove: true);
                        transaction.Commit();
                    }

                }
            }

            return Ok();
        }

        [HttpPost, Authorize(Permissions.InscriptionManage)]
        [Event(EventCode.InscriptionExecuted, EventMode = EventMode.Request, EntityType = EntityType.Contract)]
        public IActionResult Execute([FromBody] Inscription inscription)
        {
            ModelState.Validate();
            if (inscription.Id <= 0) throw new ArgumentException(nameof(inscription.Id));
            var oldInscription = _inscriptionService.Get(inscription.Id);
            if (oldInscription.Status >= InscriptionStatus.Denied)
                throw new PawnshopApplicationException("Исполнительная надпись не может быть исполнена");
            using (var transaction = _inscriptionService.BeginInscriptionTransaction())
            {
                InscriptionAction action = new InscriptionAction()
                {
                    ActionType = InscriptionActionType.Execution,
                    AuthorId = _sessionContext.UserId,
                    CreateDate = DateTime.Now,
                    Date = DateTime.Now,
                    InscriptionId = inscription.Id
                };
                _inscriptionService.InsertAction(action);
                transaction.Commit();
            }

            inscription.Status = InscriptionStatus.Executed;
            _inscriptionService.Save(inscription);

            _absOnlineService.SendNotificationCreditLineChangedAsync(inscription.ContractId).Wait();

            return Ok();
        }

        [HttpPost]
        [Authorize(Permissions.InscriptionManage)]
        public void Reverse([FromBody] InscriptionAction action)
        {
            ModelState.Validate();

            if (action == null) throw new ArgumentNullException(nameof(action));
            if (action.InscriptionId <= 0) throw new ArgumentException();
            if (action.Date.Date != DateTime.Today && !_sessionContext.ForSupport)
            {
                throw new PawnshopApplicationException("Данное действие отмене не подлежит.");
            }

            var inscription = _inscriptionService.Get(action.InscriptionId);
            var contract = _contractService.Get(inscription.ContractId);
            if (contract == null)
                throw new PawnshopApplicationException($"Договор {inscription.ContractId} не найден");


            using (var transaction = _inscriptionService.BeginInscriptionTransaction())
            {
                switch (action.ActionType)
                {
                    case InscriptionActionType.Сreation:
                        _inscriptionService.Delete(inscription.Id);
                        contract.IsOffBalance = false;
                        _contractService.Save(contract);
                        _inscriptionService.RestoreOnBalance(contract, DateTime.Now);
                        break;
                    case InscriptionActionType.Confirm:
                        var restoredAction = _inscriptionService.RestoreOrders(inscription, contract, InscriptionActionType.Confirm);

                        if (restoredAction != null)
                        {
                            decimal totalCost = Math.Round(restoredAction.Rows.Sum(t => t.Cost));
                            inscription.TotalCost = totalCost;
                        }

                        _inscriptionService.DeleteAction(action.Id);
                        inscription.Status = InscriptionStatus.New;
                        _inscriptionService.Save(inscription);
                        break;
                    case InscriptionActionType.Withdraw:
                        CheckForDiscountsExistance(contract);
                        inscription.Status = inscription.Actions.Exists(x => x.ActionType == InscriptionActionType.Confirm) ? InscriptionStatus.Approved : InscriptionStatus.New;
                        _inscriptionService.RestoreOrders(inscription, contract, InscriptionActionType.Withdraw);

                        var confirmAction = inscription.Actions.FirstOrDefault(t => t.ActionType == InscriptionActionType.Confirm);

                        if (confirmAction != null)
                        {
                            inscription.Rows = confirmAction.Rows;
                            var rowsToWriteOff = _inscriptionService.GetInscriptionRows(inscription, DateTime.Now);
                            var orders = _inscriptionService.WriteOffBalance(contract.Id, rowsToWriteOff, DateTime.Now);
                            UpdateActionRows(orders, confirmAction.Rows);
                            _inscriptionService.SaveActionRows(confirmAction);
                        }

                        contract.IsOffBalance = true;
                        _contractService.Save(contract);

                        _inscriptionService.DeleteAction(action.Id);
                        _inscriptionService.Save(inscription);
                        break;
                    case InscriptionActionType.Execution:
                        inscription.Status =
                            inscription.Actions.Exists(x => x.ActionType == InscriptionActionType.Confirm)
                                ? InscriptionStatus.Approved
                                : InscriptionStatus.New;
                        _inscriptionService.DeleteAction(action.Id);
                        _inscriptionService.Save(inscription);
                        break;
                }

                transaction.Commit();
            }

            _absOnlineService.SendNotificationCreditLineChangedAsync(contract.Id, contract).Wait();

            _eventLog.Log(EventCode.InscriptionActionCanceled, EventStatus.Success, EntityType.Contract,
                inscription.ContractId, null, null);

            _absOnlineService.SendNotificationCreditLineChangedAsync(contract.Id, contract).Wait();
        }

        [HttpPost, Authorize(Permissions.InscriptionManage)]
        [Event(EventCode.InscriptionDeleted, EventMode = EventMode.Request, EntityType = EntityType.Client)]
        public IActionResult Delete([FromBody] InscriptionAction action)
        {
            ModelState.Validate();
            var recalculateBalanceAccountDict = new Dictionary<int, (int, DateTime)>();
            if (action == null) throw new ArgumentNullException(nameof(action));
            if (action.InscriptionId <= 0) throw new ArgumentException();
            if (action.Date.Date != DateTime.Today && !_sessionContext.ForSupport)
            {
                throw new PawnshopApplicationException("Данное действие отмене не подлежит.");
            }

            int authorId = _sessionContext.UserId;
            int branchId = _branchContext.Branch.Id;
            var inscription = _inscriptionService.Get(action.InscriptionId);
            var contract = _contractService.Get(inscription.ContractId);
            if (contract == null)
                throw new PawnshopApplicationException($"Договор {inscription.ContractId} не найден");

            using (var transaction = _inscriptionService.BeginInscriptionTransaction())
            {
                var recalculateAccountsDict = new Dictionary<int, DateTime>();
                switch (action.ActionType)
                {
                    case InscriptionActionType.Сreation:
                        _inscriptionService.Delete(inscription.Id);
                        contract.IsOffBalance = false;
                        _contractService.Save(contract);
                        _inscriptionService.RestoreOnBalance(contract, DateTime.Now);
                        break;
                    case InscriptionActionType.Confirm:
                        IDictionary<int, (int, DateTime)> actionsWithDatesDict = _inscriptionService.DeleteOrders(inscription, contract, InscriptionActionType.Confirm, authorId, branchId);
                        if (actionsWithDatesDict == null)
                            throw new PawnshopApplicationException(
                                $"Ожидалось что {nameof(_inscriptionService)}.{nameof(_inscriptionService.DeleteOrders)} не вернет null");

                        foreach ((int accountId, (int accountRecordId, DateTime accountDate)) in actionsWithDatesDict)
                        {
                            if (recalculateBalanceAccountDict.ContainsKey(accountId))
                            {
                                (int _, DateTime date) = recalculateBalanceAccountDict[accountId];
                                if (date < accountDate)
                                    continue;
                            }

                            recalculateBalanceAccountDict[accountId] = (accountRecordId, accountDate);
                        }

                        decimal totalCost = Math.Round(action.Rows.Sum(t => t.Cost));
                        inscription.TotalCost = totalCost;

                        _inscriptionService.DeleteAction(action.Id);
                        inscription.Status = InscriptionStatus.New;
                        _inscriptionService.Save(inscription);
                        break;
                    case InscriptionActionType.Withdraw:
                        CheckForDiscountsExistance(contract);
                        inscription.Status = inscription.Actions.Exists(x => x.ActionType == InscriptionActionType.Confirm) ? InscriptionStatus.Approved : InscriptionStatus.New;
                        var confirmAction = inscription.Actions.FirstOrDefault(t => t.ActionType == InscriptionActionType.Confirm);
                        IDictionary<int, (int, DateTime)> actionsWithDatesDictWithdraw = _inscriptionService.DeleteOrders(inscription, contract, InscriptionActionType.Withdraw, authorId, branchId);
                        if (actionsWithDatesDictWithdraw == null)
                            throw new PawnshopApplicationException(
                                $"Ожидалось что {nameof(_inscriptionService)}.{nameof(_inscriptionService.DeleteOrders)} не вернет null");

                        foreach ((int accountId, (int accountRecordId, DateTime accountDate)) in actionsWithDatesDictWithdraw)
                        {
                            if (recalculateBalanceAccountDict.ContainsKey(accountId))
                            {
                                (int _, DateTime date) = recalculateBalanceAccountDict[accountId];
                                if (date < accountDate)
                                    continue;
                            }

                            recalculateBalanceAccountDict[accountId] = (accountRecordId, accountDate);
                        }

                        if (confirmAction != null)
                        {
                            inscription.Rows = confirmAction.Rows;
                            foreach (InscriptionRow inscriptionRow in inscription.Rows)
                            {
                                if (inscriptionRow.OrderId.HasValue)
                                {
                                    CashOrder cashOrder = _cashOrderService.GetAsync(inscriptionRow.OrderId.Value).Result;
                                    if (cashOrder == null)
                                        throw new PawnshopApplicationException($"Кассовый ордер {inscriptionRow.OrderId.Value} не найден");

                                    CashOrder stornoCashOrder = _cashOrderRepository.GetOrderByStornoId(cashOrder.Id);
                                    if (stornoCashOrder == null)
                                    {
                                        throw new PawnshopApplicationException($"Кассовый ордер {inscriptionRow.OrderId.Value} должен иметь сторнирующий его ордер");
                                    }

                                    IDictionary<int, (int, DateTime)> confirmActionsWithDatesDictWithdraw =
                                        _cashOrderService.Delete(stornoCashOrder.Id, authorId, branchId);
                                    if (confirmActionsWithDatesDictWithdraw == null)
                                        throw new PawnshopApplicationException(
                                            $"Ожидалось что {nameof(_inscriptionService)}.{nameof(_cashOrderService.Delete)} не вернет null");

                                    foreach ((int accountId, (int accountRecordId, DateTime accountDate)) in confirmActionsWithDatesDictWithdraw)
                                    {
                                        if (recalculateBalanceAccountDict.ContainsKey(accountId))
                                        {
                                            (int _, DateTime date) = recalculateBalanceAccountDict[accountId];
                                            if (date < accountDate)
                                                continue;
                                        }

                                        recalculateBalanceAccountDict[accountId] = (accountRecordId, accountDate);
                                    }
                                }
                            }
                        }

                        contract.IsOffBalance = true;
                        _contractService.Save(contract);

                        _inscriptionService.DeleteAction(action.Id);
                        _inscriptionService.Save(inscription);
                        break;
                    case InscriptionActionType.Execution:
                        inscription.Status =
                            inscription.Actions.Exists(x => x.ActionType == InscriptionActionType.Confirm)
                                ? InscriptionStatus.Approved
                                : InscriptionStatus.New;
                        _inscriptionService.DeleteAction(action.Id);
                        _inscriptionService.Save(inscription);
                        break;
                }

                foreach ((int accountId, (int accountRecordId, DateTime date)) in recalculateBalanceAccountDict)
                {
                    DateTime? recalculateDate = null;
                    AccountRecord accountRecordBeforeDate = _accountRecordRepository.GetLastRecordByAccountIdAndEndDate(accountId, accountRecordId, date);
                    if (accountRecordBeforeDate != null)
                        recalculateDate = accountRecordBeforeDate.Date;

                    _accountRecordService.RecalculateBalanceOnAccount(accountId, beginDate: recalculateDate);
                }

                _eventLog.Log(EventCode.InscriptionActionCanceled, EventStatus.Success, EntityType.Contract,
                    inscription.ContractId, null, null);
                transaction.Commit();
            }

            _absOnlineService.SendNotificationCreditLineChangedAsync(contract.Id, contract).Wait();

            return Ok();
        }

        [HttpPost, Authorize(Permissions.InscriptionManage)]
        public List<InscriptionRow> GetInscriptionRows([FromBody] Inscription inscription) => _inscriptionService.GetInscriptionRows(inscription, DateTime.Now);

        private void UpdateActionRows(IEnumerable<CashOrder> orders, List<InscriptionRow> rows)
        {
            if (orders.Any())
                foreach (var order in orders)
                {
                    var amountType = _businessOperationSettingService.Get(order.BusinessOperationSettingId.Value).AmountType;
                    var row = rows.FirstOrDefault(t => t.PaymentType == amountType && t.OrderId is null);
                    if (row != null)
                        row.OrderId = order.Id;
                    else
                        rows.Add(new InscriptionRow()
                        {
                            Cost = order.OrderCost,
                            PaymentType = amountType
                        });

                }
        }

        private void CheckForDiscountsExistance(Contract contract)
        {
            if (contract == null)
                throw new ArgumentNullException(nameof(contract));

            List<ContractDiscount> contractDiscounts = _contractDiscountRepository.GetByContractId(contract.Id);
            if (contractDiscounts == null)
                throw new PawnshopApplicationException($"Ожидалось что {nameof(contractDiscounts)} не будет null");

            List<ContractDiscount> activeContractDiscounts =
                contractDiscounts.Where(d => d.Status == ContractDiscountStatus.Accepted).ToList();

            if (activeContractDiscounts.Count > 0)
            {
                List<ContractDiscount> activeNonTypicalContractDiscounts = activeContractDiscounts.Where(d => !d.IsTypical).ToList();
                if (activeNonTypicalContractDiscounts.Count > 0)
                    throw new PawnshopApplicationException(
                        $"По данному договору({contract.ContractNumber}) существуют скидки по сумме, перед созданием исполнительной надписи отмените все скидки");
            }
        }
    }
}