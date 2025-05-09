using System;
using Pawnshop.Core.Exceptions;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Pawnshop.Data.Models.CashOrders;
using Pawnshop.Data.Models.Contracts.Actions;
using Pawnshop.Data.Access;
using Pawnshop.Services.Calculation;
using Pawnshop.Services.Contracts;
using Pawnshop.Services.Domains;
using Pawnshop.AccountingCore.Models;
using Pawnshop.Core;
using Pawnshop.Data.Models.Contracts;
using Pawnshop.Services.CreditLines.Payment;
using Pawnshop.Services.Models.Calculation;
using Pawnshop.Data.Models.Contracts.Expenses;
using Pawnshop.Data.Models.Dictionaries;
using Pawnshop.Services.AccountingCore;
using Pawnshop.Services.Expenses;
using Pawnshop.Services.Collection;
using Pawnshop.Data.Models.Collection;

namespace Pawnshop.Services.CreditLines.Buyout
{
    public sealed class CreditLinesBuyoutService : ICreditLinesBuyoutService
    {
        private readonly ICreditLineService _creditLineService;
        private readonly IContractActionService _contractActionService;
        private readonly IContractActionPrepaymentService _contractActionPrepaymentService;
        private readonly ContractRepository _contractRepository;
        private readonly IContractDutyService _contractDutyService;
        private readonly IContractService _contractService;
        private readonly IDomainService _domainService;
        private readonly IContractActionBuyoutService _contractActionBuyoutService;
        private readonly ICreditLinePaymentService _creditLinePaymentService;
        private readonly IExpenseService _expenseService;
        private readonly ContractDiscountRepository _contractDiscountRepository;
        private readonly ICashOrderService _cashOrderService;
        private readonly GroupRepository _groupRepository;
        private readonly ICollectionService _collectionService;
        private readonly ClientDefermentRepository _clientDefermentRepository;

        public CreditLinesBuyoutService(IContractActionService contractActionService,
            IContractActionPrepaymentService contractActionPrepaymentService,
            ContractRepository contractRepository,
            ICreditLineService creditLineService,
            IContractDutyService contractDutyService,
            IContractService contractService,
            IDomainService domainService,
            IContractActionBuyoutService contractActionBuyoutService,
            ICreditLinePaymentService creditLinePaymentService,
            IExpenseService expenseService,
            ContractDiscountRepository contractDiscountRepository,
            CashOrderService cashOrderService,
            GroupRepository groupRepository,
            ICollectionService collectionService,
            ClientDefermentRepository clientDefermentRepository)
        {
            _contractActionService = contractActionService;
            _contractActionPrepaymentService = contractActionPrepaymentService;
            _contractRepository = contractRepository;
            _creditLineService = creditLineService;
            _contractDutyService = contractDutyService;
            _contractService = contractService;
            _domainService = domainService;
            _contractActionBuyoutService = contractActionBuyoutService;
            _creditLinePaymentService = creditLinePaymentService;
            _expenseService = expenseService;
            _contractDiscountRepository = contractDiscountRepository;
            _cashOrderService = cashOrderService;
            _groupRepository = groupRepository;
            _collectionService = collectionService;
            _clientDefermentRepository = clientDefermentRepository;
        }

        public async Task<CreditLineAccountBalancesDistributionForBuyOut> GetCreditLineAccountBalancesDistributionForBuyBack(
            int creditLineId, List<int> buyBackedContracts, int? expenseId = null, DateTime? date = null)
        {
            var creditLineBalance = await _creditLineService.GetCurrentlyDebtForCreditLine(creditLineId, date: date);
            var сreditLineTransfers = new Dictionary<int, CreditLineTransfer>();

            List<int> contractIds = creditLineBalance.ContractsBalances
                .Select(contractBalance => contractBalance.ContractId).ToList();
            if (date == null)
            {
                date = DateTime.Now;
            }
            var discounts = _contractDiscountRepository.GetByContractIds(contractIds)
                .Where(discount => discount.BeginDate <= date && discount.EndDate >= date && discount.ContractActionId == null);
            var discountsDictionary = discounts.ToDictionary(discount => discount.ContractId, discount => discount);
            decimal discountAmount = 0;

            foreach (var contract in creditLineBalance.ContractsBalances)
                сreditLineTransfers.Add(contract.ContractId, new CreditLineTransfer
                {
                    Amount = 0,
                    ContractId = contract.ContractId,
                    ContractNumber = contract.ContractNumber,
                    IsCreditLine = contract.ContractId == creditLineId,
                    RefillableAccounts = new List<RefillableAccountsInfo>()
                });

            var creditLineAccountBalances =
                new CreditLineAccountBalancesDistributionForBuyOut
                {
                    ContractBalances = creditLineBalance.ContractsBalances,
                    ContractId = creditLineBalance.ContractId,
                    SummaryCurrentDebt = creditLineBalance.SummaryCurrentDebt,
                    SummaryExpenseAmount = creditLineBalance.SummaryExpenseAmount,
                    SummaryPrepaymentBalance = creditLineBalance.SummaryPrepaymentBalance
                };

            if (discounts.Count() > 0)
            {
                creditLineAccountBalances.Discount = new Discount
                {
                    Message = "Подробная информация о скидках:\n   Персональная скидка по сумме: \n"
                };
            }

            if (creditLineBalance.SummaryOverdueProfitOffBalance > 0 ||
                creditLineBalance.SummaryPenyAccountOffBalance > 0 ||
                creditLineBalance.SummaryPenyProfitOffBalance > 0
                || creditLineBalance.SummaryProfitOffBalance >
                0) // какой то из договоров передан в ЧСИ на нем работает только выкуп со снятием исполнительной надписи 
                throw new PawnshopApplicationException("Оплата невозможна один из договоров передан в ЧСИ");

            if (creditLineBalance.SummaryExpenseAmount > 0)
            {
                сreditLineTransfers[creditLineId].RefillableAccounts.Add(
                    new RefillableAccountsInfo("Дополнительные расходы", creditLineBalance.SummaryExpenseAmount,
                        false));
                сreditLineTransfers[creditLineId].Amount = creditLineBalance.SummaryExpenseAmount;
            }

            if (expenseId != null)
            {
                var expense = _expenseService.Get(expenseId.Value);
                if (expense != null)
                {
                    сreditLineTransfers[creditLineId].RefillableAccounts.Add(
                        new RefillableAccountsInfo("Расходы, снятие обременение", expense.Cost,
                            false));
                    сreditLineTransfers[creditLineId].Amount += expense.Cost;
                    creditLineAccountBalances.SummaryExpenseAmount += expense.Cost;
                    creditLineAccountBalances.TotalDue += expense.Cost;
                    creditLineAccountBalances.ExpenseValue = expense.Cost;
                    creditLineAccountBalances.ExtraExpensesCost = expense.Cost;
                    creditLineAccountBalances.ContainExpense = true;
                }
            }

            foreach (var balance in creditLineBalance.ContractsBalances.Where(contract =>
                         contract.ContractId != creditLineId
                         && contract.OverdueAccountAmount > 0)) //Просроченный ОД
            {
                сreditLineTransfers[balance.ContractId].RefillableAccounts
                    .Add(new RefillableAccountsInfo("Просроченный Основной Долг", balance.OverdueAccountAmount, false));
                сreditLineTransfers[balance.ContractId].Amount +=
                    сreditLineTransfers[balance.ContractId].RefillableAccounts.Last().Amount;
            }

            foreach (var balance in creditLineBalance.ContractsBalances.Where(contract =>
                         contract.ContractId != creditLineId
                         && contract.OverdueProfitAmount > 0)) //Просроченные %
            {
                decimal tempBalance = balance.OverdueProfitAmount;// Нужно оплатить с учетом скидки
                if (discountsDictionary.ContainsKey(balance.ContractId))
                {
                    tempBalance -= discountsDictionary[balance.ContractId].OverduePercentDiscountSum;// Нужно оплатить с учетом скидки
                    discountAmount += discountsDictionary[balance.ContractId].OverduePercentDiscountSum;
                    creditLineAccountBalances.Discount.Message += $" По договору {balance.ContractId} - просроченное вознаграждение: {balance.OverdueProfitAmount} \n";
                    creditLineAccountBalances.Discount.Message +=
                        $"По договору {balance.ContractId} - просроченное вознаграждение списан на: {discountsDictionary[balance.ContractId].OverduePercentDiscountSum} \n";
                }

                сreditLineTransfers[balance.ContractId].RefillableAccounts
                    .Add(new RefillableAccountsInfo("Просроченные проценты", tempBalance, false));
                сreditLineTransfers[balance.ContractId].Amount +=
                    сreditLineTransfers[balance.ContractId].RefillableAccounts.Last().Amount;

            }

            //Амортизированое 
            foreach (var balance in creditLineBalance.ContractsBalances.Where(contract =>
                         contract.ContractId != creditLineId
                         && contract.AmortizedProfit > 0 && buyBackedContracts.Contains(contract.ContractId))) 
            {
                сreditLineTransfers[balance.ContractId].RefillableAccounts
                    .Add(new RefillableAccountsInfo("Амортизированное вознаграждение", balance.AmortizedProfit, false));
                сreditLineTransfers[balance.ContractId].Amount +=
                    сreditLineTransfers[balance.ContractId].RefillableAccounts.Last().Amount;
            }

            foreach (var balance in creditLineBalance.ContractsBalances.Where(contract =>
                         contract.ContractId != creditLineId
                         && contract.DefermentProfit > 0 && buyBackedContracts.Contains(contract.ContractId)))
            {
                сreditLineTransfers[balance.ContractId].RefillableAccounts
                    .Add(new RefillableAccountsInfo("Отсроченное вознаграждение", balance.DefermentProfit, false));
                сreditLineTransfers[balance.ContractId].Amount +=
                    сreditLineTransfers[balance.ContractId].RefillableAccounts.Last().Amount;
            }

            foreach (var balance in creditLineBalance.ContractsBalances.Where(contract =>
                         contract.ContractId != creditLineId
                         && contract.AmortizedPenyProfit > 0 && buyBackedContracts.Contains(contract.ContractId)))
            {
                сreditLineTransfers[balance.ContractId].RefillableAccounts
                    .Add(new RefillableAccountsInfo("Амортизированая пеня на проценты просроченные", balance.AmortizedPenyProfit, false));
                сreditLineTransfers[balance.ContractId].Amount +=
                    сreditLineTransfers[balance.ContractId].RefillableAccounts.Last().Amount;
            }

            foreach (var balance in creditLineBalance.ContractsBalances.Where(contract =>
                         contract.ContractId != creditLineId
                         && contract.AmortizedPenyAccount > 0 && buyBackedContracts.Contains(contract.ContractId)))
            {
                сreditLineTransfers[balance.ContractId].RefillableAccounts
                    .Add(new RefillableAccountsInfo("Амортизированая пеня на долг просроченный", balance.AmortizedPenyAccount, false));
                сreditLineTransfers[balance.ContractId].Amount +=
                    сreditLineTransfers[balance.ContractId].RefillableAccounts.Last().Amount;
            }

            foreach (var balance in creditLineBalance.ContractsBalances.Where(contract =>
                         contract.ContractId != creditLineId
                         && contract.RepaymentAccountAmount > 0 &&
                         !buyBackedContracts.Contains(contract.ContractId))) //текущий ОД
            {
                сreditLineTransfers[balance.ContractId].RefillableAccounts
                    .Add(new RefillableAccountsInfo("Текущий основной долг", balance.RepaymentAccountAmount, false));
                сreditLineTransfers[balance.ContractId].Amount +=
                    сreditLineTransfers[balance.ContractId].RefillableAccounts.Last().Amount;
            }

            foreach (var balance in creditLineBalance.ContractsBalances.Where(contract =>
                         contract.ContractId != creditLineId
                         && contract.RepaymentProfitAmount > 0 &&
                         !buyBackedContracts.Contains(contract.ContractId))) //текущие %
            {
                сreditLineTransfers[balance.ContractId].RefillableAccounts
                    .Add(new RefillableAccountsInfo("Текущие проценты", balance.RepaymentProfitAmount, false));
                сreditLineTransfers[balance.ContractId].Amount +=
                    сreditLineTransfers[balance.ContractId].RefillableAccounts.Last().Amount;
            }

            foreach (var balance in creditLineBalance.ContractsBalances.Where(contract =>
                         contract.ContractId != creditLineId
                         && contract.PenyAccount > 0)) //Пеня на од
            {
                decimal tempBalance = balance.PenyAccount;// Нужно оплатить с учетом скидки
                if (discountsDictionary.ContainsKey(balance.ContractId))
                {
                    tempBalance -= discountsDictionary[balance.ContractId].DebtPenaltyDiscountSum;// Нужно оплатить с учетом скидки
                    discountAmount += discountsDictionary[balance.ContractId].DebtPenaltyDiscountSum;
                    creditLineAccountBalances.Discount.Message += $" По договору {balance.ContractId} -  штраф/пеня на основной долг: {balance.PenyAccount} \n";
                    creditLineAccountBalances.Discount.Message +=
                        $"По договору {balance.ContractId} -  штраф/пеня на основной долг списан на {discountsDictionary[balance.ContractId].DebtPenaltyDiscountSum} \n";
                }

                сreditLineTransfers[balance.ContractId].RefillableAccounts
                    .Add(new RefillableAccountsInfo("Пеня основной долг", tempBalance, false));
                сreditLineTransfers[balance.ContractId].Amount +=
                    сreditLineTransfers[balance.ContractId].RefillableAccounts.Last().Amount;
            }

            foreach (var balance in creditLineBalance.ContractsBalances.Where(contract =>
                         contract.ContractId != creditLineId
                         && contract.PenyProfit > 0)) //Пеня на %
            {
                decimal tempBalance = balance.PenyProfit;// Нужно оплатить с учетом скидки
                if (discountsDictionary.ContainsKey(balance.ContractId))
                {
                    tempBalance -= discountsDictionary[balance.ContractId].PercentPenaltyDiscountSum;// Нужно оплатить с учетом скидки
                    discountAmount += discountsDictionary[balance.ContractId].PercentPenaltyDiscountSum;
                    creditLineAccountBalances.Discount.Message += $" По договору {balance.ContractId} - Штраф/пеня на вознаграждение/проценты: {balance.PenyProfit} \n";
                    creditLineAccountBalances.Discount.Message +=
                        $"По договору {balance.ContractId} -  Штраф/пеня на вознаграждение/проценты списан на: {discountsDictionary[balance.ContractId].PercentPenaltyDiscountSum} \n";
                }

                сreditLineTransfers[balance.ContractId].RefillableAccounts
                    .Add(new RefillableAccountsInfo("Пеня проценты", tempBalance, false));
                сreditLineTransfers[balance.ContractId].Amount +=
                    сreditLineTransfers[balance.ContractId].RefillableAccounts.Last().Amount;
            }


            foreach (var balance in creditLineBalance.ContractsBalances.Where(contract =>
                         buyBackedContracts.Contains(contract.ContractId)))
            {
                сreditLineTransfers[balance.ContractId].RefillableAccounts
                    .Add(new RefillableAccountsInfo("Основной долг", balance.AccountAmount, false));
                сreditLineTransfers[balance.ContractId].Amount +=
                    сreditLineTransfers[balance.ContractId].RefillableAccounts.Last().Amount;
                decimal tempBalance = balance.ProfitAmount;// Нужно оплатить с учетом скидки
                if (discountsDictionary.ContainsKey(balance.ContractId))
                {
                    tempBalance -= discountsDictionary[balance.ContractId].PercentDiscountSum;// Нужно оплатить с учетом скидки
                    discountAmount += discountsDictionary[balance.ContractId].PercentDiscountSum;
                    creditLineAccountBalances.Discount.Message += $"По договору {balance.ContractId} - проценты: {balance.ProfitAmount} \n";
                    creditLineAccountBalances.Discount.Message +=
                        $"По договору {balance.ContractId} - списаны проценты: {discountsDictionary[balance.ContractId].PercentDiscountSum} \n";

                }

                сreditLineTransfers[balance.ContractId].RefillableAccounts
                    .Add(new RefillableAccountsInfo("Вознаграждение", tempBalance, false));
                сreditLineTransfers[balance.ContractId].Amount +=
                    сreditLineTransfers[balance.ContractId].RefillableAccounts.Last().Amount;

            }

            creditLineAccountBalances.CreditLineTransfers = сreditLineTransfers.Values.ToList();
            var afterPaymentPrepaymentBalance = creditLineBalance.SummaryPrepaymentBalance -
                                                creditLineAccountBalances.CreditLineTransfers.Sum(
                                                    transfer => transfer.Amount);

            if (afterPaymentPrepaymentBalance < 0)
                afterPaymentPrepaymentBalance = 0;
            var totalDue = creditLineAccountBalances.CreditLineTransfers.Sum(
                transfer => transfer.Amount);
            var paymentAmountFromClient = totalDue - creditLineAccountBalances.SummaryPrepaymentBalance;
            if (paymentAmountFromClient < 0)
                paymentAmountFromClient = 0;
            creditLineAccountBalances.AfterPaymentPrepaymentBalance = afterPaymentPrepaymentBalance;
            creditLineAccountBalances.TotalDue = totalDue;
            creditLineAccountBalances.TotalCost = totalDue;
            creditLineAccountBalances.PaymentAmountFromClient = paymentAmountFromClient;

            return creditLineAccountBalances;
        }

        public async Task<int?> TransferPrepaymentAndBuyBack(int creditLineId, int authorId, int payTypeId, int branchId,
            List<int> buyBackedContracts, int buyoutReasonId,
            bool buyoutCreditLine = false, DateTime? date = null, string note = null, bool autoApprove = false, int? expenseId = null)
        {
            if (date == null)
                date = DateTime.Now;
            decimal fundsFromClient = 0;
            var balances = await GetCreditLineAccountBalancesDistributionForBuyBack(creditLineId, buyBackedContracts, expenseId, date: date);

            var amount = balances.CreditLineTransfers.Sum(transfer => transfer.Amount);
            if (amount == 0) amount = balances.SummaryPrepaymentBalance;
            if (amount > balances.SummaryPrepaymentBalance)
            {
                fundsFromClient = amount - balances.SummaryPrepaymentBalance;
                fundsFromClient = Math.Ceiling(fundsFromClient);
            }


            if (fundsFromClient > 0 && branchId == _groupRepository.Find(new { Name = "TSO" })?.Id && payTypeId == 2) 
                throw new PawnshopApplicationException(
                    "Вневение денег на аванс договора через кассу из филиала Tas Online недоступно.");

            var orderStatus = OrderStatus.WaitingForApprove;
            if (autoApprove) orderStatus = OrderStatus.Approved;

            var contractsSum = balances.CreditLineTransfers.ToDictionary(k => k.ContractId, v => v.Amount);

            if (await _creditLineService.UnconfirmedActionExists(contractsSum.Keys.ToList()))
                throw new PawnshopApplicationException("В договоре имеется неподтвержденное действие");

            ContractAction parentAction = null;
            ContractAction childAction = null;
            using (var transaction = _contractRepository.BeginTransaction())
            {
                #region Пополняем аванс КЛ

                if (fundsFromClient > 0)
                {
                    var prepaymentAction = _contractActionPrepaymentService.Exec(creditLineId,
                        fundsFromClient, payTypeId,
                        branchId, authorId, date.Value,
                        orderStatus: orderStatus); // Пополняем аванс с КЛ
                    parentAction = prepaymentAction;
                }

                #endregion

                #region Оплачиваем доп расход с аванса КЛ

                if (contractsSum[creditLineId] - balances.ExpenseValue > 0)
                {
                    //"Расходы, снятие обременение"
                    childAction = _creditLineService.PayExtraExpenses(creditLineId, contractsSum[creditLineId] - balances.ExpenseValue,
                        date.Value, payTypeId,
                        authorId, branchId, autoApprove);
                    if (parentAction != null)
                    {
                        parentAction.ChildActionId = childAction.Id;
                        _contractActionService.Save(parentAction);
                        childAction.ParentActionId = parentAction.Id;
                        _contractActionService.Save(childAction);
                    }

                    parentAction = childAction;
                }

                #endregion

                #region Переводим на авансы траншей и оплачиваем на них

                foreach (var sum in contractsSum.Where(sum => sum.Value > 0)
                             .Where(sum => sum.Key != creditLineId)
                             .Where(contract => !buyBackedContracts.Contains(contract.Key)))
                {
                    childAction = _creditLineService.MovePrepayment(creditLineId: creditLineId,
                        contractId: sum.Key,
                        value: sum.Value,
                        authorId: authorId,
                        action: parentAction,
                        autoApprove: autoApprove,
                        branchId: branchId,
                        date: date);
                    
                    if (parentAction != null)
                    {
                        parentAction.ChildActionId = childAction.Id;
                        _contractActionService.Save(parentAction);
                        childAction.ParentActionId = parentAction.Id;
                        _contractActionService.Save(childAction);
                    }

                    parentAction = childAction;

                    childAction = await _creditLinePaymentService.PaymentOnContract(contractId: sum.Key,
                        value: sum.Value,
                        payTypeId: payTypeId,
                        parentAction: parentAction,
                        authorId: authorId,
                        branchId: branchId,
                        autoApprove: autoApprove,
                        date: date);
                    parentAction.ChildActionId = childAction.Id;
                    _contractActionService.Save(parentAction);
                    childAction.ParentActionId = parentAction.Id;
                    _contractActionService.Save(childAction);
                    parentAction = childAction;

                    try
                    {
                        var collectionModel = new CollectionClose()
                        {
                            ContractId = sum.Key,
                            ActionId = parentAction.Id
                        };
                        _collectionService.CloseContractCollection(collectionModel);
                    }
                    catch { }
                }

                #endregion

                #region Выкупы

                var buyoutList = contractsSum.Where(sum => sum.Value > 0)
                    .Where(sum => sum.Key != creditLineId)
                    .Where(contract => buyBackedContracts.Contains(contract.Key)).ToList();

                for (var i = 0; i < buyoutList.Count; i++)
                {
                    var forceBuyOutCreditLine = false;
                    if (i == buyoutList.Count - 1)
                        forceBuyOutCreditLine = true;

                    childAction = _creditLineService.MovePrepayment(creditLineId: creditLineId,
                        contractId: buyoutList[i].Key,
                        value: buyoutList[i].Value,
                        authorId: authorId,
                        action: parentAction,
                        autoApprove: autoApprove,
                        branchId: branchId,
                        date: date);

                    if (parentAction != null)
                    {
                        parentAction.ChildActionId = childAction.Id;
                        _contractActionService.Save(parentAction);
                        childAction.ParentActionId = parentAction.Id;
                        _contractActionService.Save(childAction);
                    }

                    parentAction = childAction;

                    var buyoutActions = await BuyOut(contractId: buyoutList[i].Key,
                        creditLineId: creditLineId,
                        payTypeId: payTypeId,
                        parentAction: parentAction,
                        authorId: authorId,
                        branchId: branchId,
                        buyoutReasonId: buyoutReasonId,
                        autoApprove: autoApprove,
                        buyoutCreditLine: buyoutCreditLine,
                        forceBuyOutCreditLine: forceBuyOutCreditLine,
                        expenseId: expenseId,
                        date: date);
                    foreach (var action in buyoutActions.Where(action => action != null).OrderBy(action => action.Id))
                    {
                        childAction = action;
                        parentAction.ChildActionId = childAction.Id;
                        _contractActionService.Save(parentAction);
                        childAction.ParentActionId = parentAction.Id;
                        _contractActionService.Save(childAction);
                        parentAction = childAction;
                    }

                    try
                    {
                        var buyoutAction = buyoutActions.Where(action => action != null && action.ActionType == ContractActionType.Buyout
                            && action.ContractId == buyoutList[i].Key).FirstOrDefault();

                        var collectionModel = new CollectionClose()
                        {
                            ContractId = buyoutList[i].Key,
                            ActionId = buyoutAction.Id
                        };
                        _collectionService.CloseContractCollection(collectionModel);
                    }
                    catch { }
                }

                if (autoApprove) // Тут кассовые ордера подтверждаем 
                {
                    var branch = _groupRepository.Get(branchId);
                    var orders = await _cashOrderService.CheckOrdersForConfirmation(parentAction.Id);
                    var relatedActions = orders.Item2;
                    await _cashOrderService.ChangeStatusForOrders(relatedActions, OrderStatus.Approved,
                        userId: 1, branch: branch, false);
                }

                transaction.Commit();
            }

            if (parentAction != null)
            {
                return parentAction.Id;
            }

            return null;

            #endregion
        }

        public async Task<int?> TransferPrepaymentAndBuyOutOnline(int creditLineId, int authorId, int branchId, 
            List<int> buyBackedContracts)
        {
            DateTime date = DateTime.Now;
            decimal fundsFromClient = 0;
            int buyoutReasonId = _domainService
                .GetDomainValue(Constants.BUYOUT_REASON_CODE, Constants.BUYOUT_AUTOMATIC_BUYOUT).Id;
            int payTypeId = 2;
            bool buyoutCreditLine = false;

            var balances = await GetCreditLineAccountBalancesDistributionForBuyBack(creditLineId, buyBackedContracts);

            var amount = balances.CreditLineTransfers.Sum(transfer => transfer.Amount);
            if (amount == 0) amount = balances.SummaryPrepaymentBalance;
            if (amount > balances.SummaryPrepaymentBalance)
            {
                fundsFromClient = amount - balances.SummaryPrepaymentBalance;
                fundsFromClient = Math.Ceiling(fundsFromClient);
            }


            if (fundsFromClient > 0)
                throw new PawnshopApplicationException(
                    "При онлайн выкупе не предусмотрено внесение денег.");

            var contractsSum = balances.CreditLineTransfers.ToDictionary(k => k.ContractId, v => v.Amount);

            if (await _creditLineService.UnconfirmedActionExists(contractsSum.Keys.ToList()))
                throw new PawnshopApplicationException("В договоре имеется неподтвержденное действие");

            ContractAction parentAction = null;
            ContractAction childAction = null;
            using (var transaction = _contractRepository.BeginTransaction())
            {
                #region Выкупы

                var buyoutList = contractsSum.Where(sum => sum.Value > 0)
                    .Where(sum => sum.Key != creditLineId)
                    .Where(contract => buyBackedContracts.Contains(contract.Key)).ToList();

                for (var i = 0; i < buyoutList.Count; i++)
                {
                    bool forceBuyOutCreditLine = i == buyoutList.Count - 1;

                    childAction = _creditLineService.MovePrepayment(creditLineId: creditLineId,
                        contractId: buyoutList[i].Key,
                        value: buyoutList[i].Value,
                        authorId: authorId,
                        action: parentAction,
                        autoApprove: true,
                        branchId: branchId,
                        date: date);

                    if (parentAction != null)
                    {
                        parentAction.ChildActionId = childAction.Id;
                        _contractActionService.Save(parentAction);
                        childAction.ParentActionId = parentAction.Id;
                        _contractActionService.Save(childAction);
                    }

                    parentAction = childAction;

                    var buyoutActions = await BuyOut(contractId: buyoutList[i].Key,
                        creditLineId: creditLineId,
                        payTypeId: payTypeId,
                        parentAction: parentAction,
                        authorId: authorId,
                        branchId: branchId,
                        buyoutReasonId: buyoutReasonId,
                        autoApprove: true,
                        buyoutCreditLine: buyoutCreditLine,
                        forceBuyOutCreditLine: forceBuyOutCreditLine,
                        expenseId: null,
                        date: date);
                    foreach (var action in buyoutActions.Where(action => action != null).OrderBy(action => action.Id))
                    {
                        childAction = action;
                        parentAction.ChildActionId = childAction.Id;
                        _contractActionService.Save(parentAction);
                        childAction.ParentActionId = parentAction.Id;
                        _contractActionService.Save(childAction);
                        parentAction = childAction;
                    }

                    try
                    {
                        var buyoutAction = buyoutActions
                            .FirstOrDefault(action => action != null &&
                                                      action.ActionType == ContractActionType.Buyout
                                                      && action.ContractId == buyoutList[i].Key);

                        var collectionModel = new CollectionClose()
                        {
                            ContractId = buyoutList[i].Key,
                            ActionId = buyoutAction.Id
                        };
                        _collectionService.CloseContractCollection(collectionModel);
                    }
                    catch (Exception exception)
                    {
                        //_logger.LogError(exception, exception.Message);
                    }
                }

                #region Подтверждаем кассовые ордера
                var branch = _groupRepository.Get(branchId);
                var orders = await _cashOrderService.CheckOrdersForConfirmation(parentAction.Id);
                var relatedActions = orders.Item2;
                await _cashOrderService.ChangeStatusForOrders(relatedActions, OrderStatus.Approved,
                    userId: Constants.ADMINISTRATOR_IDENTITY, branch: branch, false);
                #endregion

                transaction.Commit();
            }

            if (parentAction != null)
            {
                return parentAction.Id;
            }

            return null;

            #endregion
        }

        public async Task<List<ContractAction>> BuyOut(int contractId,int creditLineId, int payTypeId,
            ContractAction parentAction, int authorId, int branchId, int? buyoutReasonId, bool buyoutCreditLine,
            bool autoApprove = false, bool forceBuyOutCreditLine = false, int? expenseId = null, DateTime? date = null)
        {
            if (date == null)
            {
                date = DateTime.Now;
            }
            Contract contract = _contractService.Get(contractId);
            ContractExpense contractExpense = null;
            if (expenseId != null)
            {
                Expense entity = _expenseService.Get(expenseId.Value);

                contractExpense = new ContractExpense
                {
                    Date = date.Value,
                    ExpenseId = expenseId.Value,
                    Expense = entity,
                    ContractId = creditLineId,
                    TotalCost = entity.Cost,
                    TotalLeft = entity.Cost,
                    Reason = $"{entity.Name} по договору займа № {creditLineId} от {date.Value}",
                    Name = entity.Name,
                    UserId = authorId,
                    IsPayed = true,
                };
            }
            var buyoutContractDutyCheckModel = new ContractDutyCheckModel
            {
                ActionType = ContractActionType.Buyout,
                ContractId = contractId,
                Date = date.Value,
                PayTypeId = payTypeId
            };
            
            var buyoutContractDuty = _contractDutyService.GetContractDuty(buyoutContractDutyCheckModel);
            if (buyoutContractDuty == null)
                throw new PawnshopApplicationException(
                    $"Ожидалось что {nameof(_contractDutyService)}.{nameof(_contractDutyService.GetContractDuty)} не вернет null, по действию выкупу");
            else if (buyoutContractDuty.Rows == null)
                throw new PawnshopApplicationException(
                    $"Ожидалось что {nameof(buyoutContractDuty)}.{nameof(buyoutContractDuty.Rows)} не будет null");

            var contractDate = contract.ContractDate;

            if (buyoutReasonId == null) buyoutReasonId = 0;

            var buyOutReasonCodeFromDatabase =
                _domainService.GetDomainValue(Constants.BUYOUT_REASON_CODE, buyoutReasonId.Value, false);
            string buyoutReason;
            if (buyOutReasonCodeFromDatabase == null)
            {
                buyoutReason = string.Format(Constants.REASON_AUTO_BUYOUT, contract.ContractNumber,
                    contractDate.ToString("dd.MM.yyyy"));
                buyoutReasonId = _domainService
                    .GetDomainValue(Constants.BUYOUT_REASON_CODE, Constants.BUYOUT_AUTOMATIC_BUYOUT).Id;
            }
            else
            {
                buyoutReason = string.Format(buyOutReasonCodeFromDatabase.Name, contract.ContractNumber,
                    contractDate.ToString("dd.MM.yyyy"));
                buyoutReasonId = buyOutReasonCodeFromDatabase.Id;
            }

            var buyoutActionRows = buyoutContractDuty.Rows.ToList();
            var buyoutAmountsDict = new Dictionary<AmountType, decimal>();
            foreach (var row in buyoutActionRows)
            {
                decimal amount;
                if (buyoutAmountsDict.TryGetValue(row.PaymentType, out amount))
                    if (row.Cost != amount)
                        throw new PawnshopApplicationException("Проводки по одинаковым типам не совпадают по сумме");

                buyoutAmountsDict[row.PaymentType] = row.Cost;
            }

            var buyoutActionRowsCost = buyoutAmountsDict.Count > 0 ? buyoutAmountsDict.Values.Sum() : 0;
            var buyoutContractAction = new ContractAction
            {
                ActionType = ContractActionType.Buyout,
                ContractId = contract.Id,
                CreateDate = date.Value,
                Date = buyoutContractDuty.Date,
                AuthorId = authorId,
                ExtraExpensesCost = buyoutContractDuty.ExtraExpensesCost,
                PayTypeId = payTypeId,
                Discount = buyoutContractDuty?.Discount,
                Rows = buyoutContractDuty.Rows.ToArray(),
                Cost = buyoutContractDuty.Cost,
                TotalCost = buyoutActionRowsCost,
                Reason = buyoutReason,
                ParentAction = parentAction,
                BuyoutReasonId = buyoutReasonId,
                BuyoutCreditLine = buyoutCreditLine,
            };

            return await _contractActionBuyoutService.ExecuteWithReturnContractAction(buyoutContractAction, authorId,
                branchId, true, autoApprove, null, forceBuyOutCreditLine, contractExpense);
        }
    }
}