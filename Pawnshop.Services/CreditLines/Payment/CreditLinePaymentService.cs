using Pawnshop.Core.Exceptions;
using Pawnshop.Data.Access;
using Pawnshop.Data.Models.CashOrders;
using Pawnshop.Data.Models.Contracts.Actions;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Pawnshop.Data.Models.Collection;
using Pawnshop.Services.AccountingCore;
using Pawnshop.Services.Contracts;
using Pawnshop.Services.Models.Calculation;
using Pawnshop.Services.Calculation;
using Pawnshop.Services.Collection;
using Pawnshop.Services.LegalCollection;
using Pawnshop.Services.LegalCollection.Inerfaces;

namespace Pawnshop.Services.CreditLines.Payment
{
    public sealed class CreditLinePaymentService : ICreditLinePaymentService
    {
        private readonly ICreditLineService _creditLineService;
        private readonly IContractActionService _contractActionService;
        private readonly IContractActionPrepaymentService _contractActionPrepaymentService;
        private readonly ContractRepository _contractRepository;
        private readonly IContractDutyService _contractDutyService;
        private readonly IContractPaymentService _contractPaymentService;
        private readonly ContractDiscountRepository _contractDiscountRepository;
        private readonly AccountRepository _accountRepository;
        private readonly ICashOrderService _cashOrderService;
        private readonly GroupRepository _groupRepository;
        private readonly ICollectionService _collectionService;
        private readonly ILegalCollectionCloseService _legalCollectionCloseService;
        
        
        public CreditLinePaymentService(ICreditLineService creditLineService, 
            ContractRepository contractRepository,
            IContractActionPrepaymentService contractActionPrepaymentService,
            IContractActionService contractActionService,
            IContractDutyService contractDutyService,
            IContractPaymentService contractPaymentService,
            ContractDiscountRepository contractDiscountRepository,
            AccountRepository accountRepository, 
            CashOrderService cashOrderService,
            ICollectionService collectionService,
            GroupRepository groupRepository,
            ILegalCollectionCloseService legalCollectionCloseService)
        {
            _creditLineService = creditLineService;
            _contractRepository = contractRepository;
            _contractActionPrepaymentService = contractActionPrepaymentService;
            _contractActionService = contractActionService;
            _contractDutyService = contractDutyService;
            _contractPaymentService = contractPaymentService;
            _contractDiscountRepository = contractDiscountRepository;
            _accountRepository = accountRepository;
            _cashOrderService = cashOrderService;
            _groupRepository = groupRepository;
            _legalCollectionCloseService = legalCollectionCloseService;
            _collectionService = collectionService;
        }
        
        public async Task<int?> TransferPrepaymentAndPayment(int creditLineId, int authorId, int payTypeId, int branchId,
            DateTime? date = null, string note = null, decimal amount = 0, bool autoApprove = false)
        {
            if (date == null)
                date = DateTime.Now;

            var balance = await _creditLineService.GetCurrentlyDebtForCreditLine(creditLineId, date: date);

            decimal prepaymentBalance = balance.SummaryPrepaymentBalance;
            decimal fundsFromClient = 0;

            if (amount == 0)
            {
                amount = prepaymentBalance;
            }
            if (amount > prepaymentBalance)
            {
                fundsFromClient = amount - prepaymentBalance;
                fundsFromClient = Math.Ceiling(fundsFromClient);
            }

            if (fundsFromClient > 0 && branchId == 530 && payTypeId == 2)// TODO get from find 
            {
                throw new PawnshopApplicationException("Внеcение денег на аванс договора через кассу из филиала Tas Online недоступно.");
            }

            Dictionary<int, decimal> contractsSum = (await GetCreditLineAccountBalancesDistribution(creditLineId: creditLineId, amount: amount, date: date))
                .CreditLineTransfers.ToDictionary(k => k.ContractId, v => v.Amount);

            if (await _creditLineService.UnconfirmedActionExists(contractsSum.Keys.ToList()))
                throw new PawnshopApplicationException("В договоре имеется неподтвержденное действие");

            OrderStatus orderStatus = OrderStatus.WaitingForApprove;
            if (autoApprove)
            {
                orderStatus = OrderStatus.Approved;
            }


            ContractAction parentAction = null;
            ContractAction childAction = null;
            
            var contractIds = new List<int>();
            
            using (IDbTransaction transaction = _contractRepository.BeginTransaction())
            {
                #region Пополняем аванс КЛ

                if (fundsFromClient > 0)
                {
                    ContractAction prepaymentAction = _contractActionPrepaymentService.Exec(creditLineId, fundsFromClient, payTypeId,
                        branchId, authorId, date: date.Value,
                        orderStatus: orderStatus); // Пополняем аванс с КЛ
                    parentAction = prepaymentAction;
                }

                #endregion

                #region Оплачиваем доп расход с аванса КЛ

                if (contractsSum[creditLineId] > 0)
                {
                    childAction = _creditLineService.PayExtraExpenses(creditLineId, contractsSum[creditLineId], date.Value, payTypeId,
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
                             .Where(sum => sum.Key != creditLineId))
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

                    childAction = await PaymentOnContract(contractId: sum.Key,
                        value: sum.Value,
                        payTypeId: payTypeId,
                        parentAction: parentAction,
                        authorId: authorId,
                        branchId: branchId,
                        autoApprove, date: date);
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

                    contractIds.Add(sum.Key);
                }

                #endregion

                if (autoApprove && parentAction != null)
                {
                    var branch = _groupRepository.Get(branchId);
                    var orders = await _cashOrderService.CheckOrdersForConfirmation(parentAction.Id);
                    var relatedActions = orders.Item2;
                    await _cashOrderService.ChangeStatusForOrders(relatedActions, OrderStatus.Approved,
                        userId: 1, branch: branch, false);
                }

                transaction.Commit();
                if (contractIds.Any())
                {
                    foreach (var contrId in contractIds)
                    {
                        await _legalCollectionCloseService.CloseAsync(contrId);
                    }
                }
                
                if (parentAction != null)
                    return parentAction.Id;

                return null;
            }
        }


        public async Task<CreditLineAccountBalancesDistribution> GetCreditLineAccountBalancesDistribution(
    int creditLineId, decimal amount = 0, DateTime? date = null)
        {
            var creditLineBalance = await _creditLineService.GetCurrentlyDebtForCreditLine(creditLineId, null, date: date);

            List<int> contractIds = creditLineBalance.ContractsBalances
                .Select(contractBalance => contractBalance.ContractId).ToList();
            if (date == null)
            {
                date = DateTime.Now;
            }

            var discounts = _contractDiscountRepository.GetByContractIds(contractIds)
                .Where(discount => discount.BeginDate <= date && discount.EndDate >= date && discount.ContractActionId == null);
            var discountsDictionary = discounts.ToDictionary(discount => discount.ContractId, discount => discount);
            Dictionary<int, CreditLineTransfer> сreditLineTransfers = new Dictionary<int, CreditLineTransfer>();

            decimal discountAmount = 0;

            foreach (var contract in creditLineBalance.ContractsBalances)
            {
                сreditLineTransfers.Add(contract.ContractId, new CreditLineTransfer
                {
                    Amount = 0,
                    ContractId = contract.ContractId,
                    ContractNumber = contract.ContractNumber,
                    RefillableAccounts = new List<RefillableAccountsInfo>()
                });
            }
            decimal availableFunds = amount;

            decimal afterPaymentPrepaymentBalance = creditLineBalance.SummaryPrepaymentBalance - amount;//сколько останется после текущей задолжности
            if (afterPaymentPrepaymentBalance < 0)// если меньше 0 то баланс будет 0
                afterPaymentPrepaymentBalance = 0;

            decimal paymentAmountFromClient = amount - creditLineBalance.SummaryPrepaymentBalance;//Сколько надо взять с клиента
            if (paymentAmountFromClient < 0) // Если на авансе больше то 0 
                paymentAmountFromClient = 0;

            if (paymentAmountFromClient > creditLineBalance.SummaryCurrentDebt)
            {
                paymentAmountFromClient =
                    creditLineBalance.SummaryCurrentDebt - creditLineBalance.SummaryPrepaymentBalance;
            }

            if (paymentAmountFromClient < 0)
            {
                paymentAmountFromClient = 0;
            }


            decimal totalDue = amount - creditLineBalance.SummaryExpenseAmount;

            if (totalDue < 0)
            {
                totalDue = 0;
            }

            if (totalDue > creditLineBalance.SummaryCurrentDebt)
            {
                totalDue = creditLineBalance.SummaryCurrentDebt;
            }

            CreditLineAccountBalancesDistribution creditLineAccountBalances =
                new CreditLineAccountBalancesDistribution
                {
                    ContractBalances = creditLineBalance.ContractsBalances,
                    ContractId = creditLineBalance.ContractId,
                    SummaryCurrentDebt = creditLineBalance.SummaryCurrentDebt,
                    SummaryExpenseAmount = creditLineBalance.SummaryExpenseAmount,
                    SummaryPrepaymentBalance = creditLineBalance.SummaryPrepaymentBalance,
                    AfterPaymentPrepaymentBalance = afterPaymentPrepaymentBalance,
                    PaymentAmountFromClient = paymentAmountFromClient,
                    TotalDue = totalDue,
                    TotalCost = totalDue
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
                || creditLineBalance.SummaryProfitOffBalance > 0)// какой то из договоров передан в ЧСИ на нем работает только выкуп со снятием исполнительной надписи 
            {
                throw new PawnshopApplicationException("Оплата невозможна один из договоров передан в ЧСИ");
            }

            if (creditLineBalance.SummaryExpenseAmount > 0)
            {
                var expenses =
                    _creditLineService.GetExpensesPaymentSum(creditLineBalance.SummaryExpenseAmount, availableFunds, creditLineId);

                if (expenses.PartialPayment)
                {
                    if (expenses.Amount != 0)//чет все таки погасить можем
                    {
                        сreditLineTransfers[creditLineId].RefillableAccounts.Add(expenses);
                        сreditLineTransfers[creditLineId].Amount = expenses.Amount;
                        creditLineAccountBalances.CreditLineTransfers = сreditLineTransfers.Values.ToList();
                    }
                    creditLineAccountBalances.CreditLineTransfers = сreditLineTransfers.Values.ToList();
                    return creditLineAccountBalances;
                }
                сreditLineTransfers[creditLineId].RefillableAccounts.Add(expenses);
                creditLineAccountBalances.CreditLineTransfers = сreditLineTransfers.Values.ToList();
                сreditLineTransfers[creditLineId].Amount = expenses.Amount;
                availableFunds -= expenses.Amount;
            }


            // todo: скидки для амортизированных счетов
            // пока не доделали, в планах сделать

            /*foreach (var balance in creditLineBalance.ContractsBalances.Where(contract => contract.ContractId != creditLineId
                         && contract.DefermentProfitPayment > 0)) // Отсроченное вознаграждение (начисленное)
            {
                decimal tempBalance = balance.DefermentProfitPayment;
                // todo: скидки для амортизированных счетов

                сreditLineTransfers[balance.ContractId].RefillableAccounts
                    .Add(new RefillableAccountsInfo("Отсроченное вознаграждение", availableFunds, tempBalance));
                сreditLineTransfers[balance.ContractId].Amount +=
                    сreditLineTransfers[balance.ContractId].RefillableAccounts.Last().Amount;

                if (сreditLineTransfers[balance.ContractId].RefillableAccounts.Last().PartialPayment)
                {
                    creditLineAccountBalances.CreditLineTransfers = сreditLineTransfers.Values.ToList();
                    return creditLineAccountBalances;
                }
                availableFunds -= сreditLineTransfers[balance.ContractId].RefillableAccounts.Last().Amount;
            }

            foreach (var balance in creditLineBalance.ContractsBalances.Where(contract => contract.ContractId != creditLineId
                         && contract.AmortizedLoanPayment > 0)) // Амортизированное вознаграждение (начисленное)
            {
                decimal tempBalance = balance.AmortizedLoanPayment;
                // todo: скидки для амортизированных счетов

                сreditLineTransfers[balance.ContractId].RefillableAccounts
                    .Add(new RefillableAccountsInfo("Амортизированное вознаграждение", availableFunds, tempBalance));
                сreditLineTransfers[balance.ContractId].Amount +=
                    сreditLineTransfers[balance.ContractId].RefillableAccounts.Last().Amount;

                if (сreditLineTransfers[balance.ContractId].RefillableAccounts.Last().PartialPayment)
                {
                    creditLineAccountBalances.CreditLineTransfers = сreditLineTransfers.Values.ToList();
                    return creditLineAccountBalances;
                }
                availableFunds -= сreditLineTransfers[balance.ContractId].RefillableAccounts.Last().Amount;
            }

            foreach (var balance in creditLineBalance.ContractsBalances.Where(contract => contract.ContractId != creditLineId
                         && contract.AmortizedDebtPenaltyPayment > 0)) // Амортизированая пеня на долг просроченный (начисленное)
            {
                decimal tempBalance = balance.AmortizedDebtPenaltyPayment;
                // todo: скидки для амортизированных счетов

                сreditLineTransfers[balance.ContractId].RefillableAccounts
                    .Add(new RefillableAccountsInfo("Амортизированая пеня на долг просроченный", availableFunds, tempBalance));
                сreditLineTransfers[balance.ContractId].Amount +=
                    сreditLineTransfers[balance.ContractId].RefillableAccounts.Last().Amount;

                if (сreditLineTransfers[balance.ContractId].RefillableAccounts.Last().PartialPayment)
                {
                    creditLineAccountBalances.CreditLineTransfers = сreditLineTransfers.Values.ToList();
                    return creditLineAccountBalances;
                }
                availableFunds -= сreditLineTransfers[balance.ContractId].RefillableAccounts.Last().Amount;
            }

            foreach (var balance in creditLineBalance.ContractsBalances.Where(contract => contract.ContractId != creditLineId
                         && contract.AmortizedLoanPenaltyPayment > 0)) // Амортизированая пеня на проценты просроченные (начисленное)
            {
                decimal tempBalance = balance.AmortizedLoanPenaltyPayment;
                // todo: скидки для амортизированных счетов

                сreditLineTransfers[balance.ContractId].RefillableAccounts
                    .Add(new RefillableAccountsInfo("Амортизированая пеня на проценты просроченные", availableFunds, tempBalance));
                сreditLineTransfers[balance.ContractId].Amount +=
                    сreditLineTransfers[balance.ContractId].RefillableAccounts.Last().Amount;

                if (сreditLineTransfers[balance.ContractId].RefillableAccounts.Last().PartialPayment)
                {
                    creditLineAccountBalances.CreditLineTransfers = сreditLineTransfers.Values.ToList();
                    return creditLineAccountBalances;
                }
                availableFunds -= сreditLineTransfers[balance.ContractId].RefillableAccounts.Last().Amount;
            }*/

            foreach (var balance in creditLineBalance.ContractsBalances.Where(contract => contract.ContractId != creditLineId
                         && contract.OverdueAccountAmount > 0)) //Просроченный ОД
            {
                сreditLineTransfers[balance.ContractId].RefillableAccounts
                    .Add(new RefillableAccountsInfo("Просроченный Основной Долг", availableFunds, balance.OverdueAccountAmount));
                сreditLineTransfers[balance.ContractId].Amount +=
                    сreditLineTransfers[balance.ContractId].RefillableAccounts.Last().Amount;
                if (сreditLineTransfers[balance.ContractId].RefillableAccounts.Last().PartialPayment)
                {
                    creditLineAccountBalances.CreditLineTransfers = сreditLineTransfers.Values.ToList();
                    return creditLineAccountBalances;
                }
                availableFunds -= сreditLineTransfers[balance.ContractId].RefillableAccounts.Last().Amount;
            }

            foreach (var balance in creditLineBalance.ContractsBalances.Where(contract => contract.ContractId != creditLineId
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
                    .Add(new RefillableAccountsInfo("Просроченные проценты", availableFunds, tempBalance));
                сreditLineTransfers[balance.ContractId].Amount +=
                    сreditLineTransfers[balance.ContractId].RefillableAccounts.Last().Amount;

                if (сreditLineTransfers[balance.ContractId].RefillableAccounts.Last().PartialPayment)
                {
                    creditLineAccountBalances.CreditLineTransfers = сreditLineTransfers.Values.ToList();
                    if (discountsDictionary.Count > 0)
                    {
                        return ChangeAmountsWithDiscounts(creditLineAccountBalances, discountAmount, amount);
                    }
                    return creditLineAccountBalances;
                }
                availableFunds -= сreditLineTransfers[balance.ContractId].RefillableAccounts.Last().Amount;
            }

            foreach (var balance in creditLineBalance.ContractsBalances.Where(contract => contract.ContractId != creditLineId
                         && contract.RepaymentAccountAmount > 0)) //текущий ОД
            {
                сreditLineTransfers[balance.ContractId].RefillableAccounts
                    .Add(new RefillableAccountsInfo("Текущий основной долг", availableFunds, balance.RepaymentAccountAmount));
                сreditLineTransfers[balance.ContractId].Amount +=
                    сreditLineTransfers[balance.ContractId].RefillableAccounts.Last().Amount;
                if (сreditLineTransfers[balance.ContractId].RefillableAccounts.Last().PartialPayment)
                {
                    creditLineAccountBalances.CreditLineTransfers = сreditLineTransfers.Values.ToList();
                    if (discountsDictionary.Count > 0)
                    {
                        return ChangeAmountsWithDiscounts(creditLineAccountBalances, discountAmount, amount);
                    }
                    return creditLineAccountBalances;
                }
                availableFunds -= сreditLineTransfers[balance.ContractId].RefillableAccounts.Last().Amount;
            }

            foreach (var balance in creditLineBalance.ContractsBalances.Where(contract => contract.ContractId != creditLineId
                         && contract.RepaymentProfitAmount > 0 )) //текущие %
            {

                decimal tempBalance = balance.RepaymentProfitAmount;
                if (discountsDictionary.ContainsKey(balance.ContractId))
                {
                    tempBalance = balance.RepaymentProfitAmount;// Нужно оплатить с учетом скидки
                    tempBalance -= discountsDictionary[balance.ContractId].PercentDiscountSum;// Нужно оплатить с учетом скидки
                    discountAmount += discountsDictionary[balance.ContractId].PercentDiscountSum;
                    creditLineAccountBalances.Discount.Message += $"По договору {balance.ContractId} - проценты: {balance.RepaymentProfitAmount} \n";
                    creditLineAccountBalances.Discount.Message +=
                        $"По договору {balance.ContractId} - списаны проценты: {discountsDictionary[balance.ContractId].PercentDiscountSum} \n";
                }
                сreditLineTransfers[balance.ContractId].RefillableAccounts
                    .Add(new RefillableAccountsInfo("Текущие проценты", availableFunds, tempBalance));
                сreditLineTransfers[balance.ContractId].Amount +=
                    сreditLineTransfers[balance.ContractId].RefillableAccounts.Last().Amount;

                if (сreditLineTransfers[balance.ContractId].RefillableAccounts.Last().PartialPayment)
                {
                    creditLineAccountBalances.CreditLineTransfers = сreditLineTransfers.Values.ToList();
                    if (discountsDictionary.Count > 0)
                    {
                        return ChangeAmountsWithDiscounts(creditLineAccountBalances, discountAmount, amount);
                    }
                    return creditLineAccountBalances;
                }
                availableFunds -= сreditLineTransfers[balance.ContractId].RefillableAccounts.Last().Amount;
            }

            foreach (var balance in creditLineBalance.ContractsBalances.Where(contract => contract.ContractId != creditLineId
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
                    .Add(new RefillableAccountsInfo("Пеня основной долг", availableFunds, tempBalance));
                сreditLineTransfers[balance.ContractId].Amount +=
                    сreditLineTransfers[balance.ContractId].RefillableAccounts.Last().Amount;

                if (сreditLineTransfers[balance.ContractId].RefillableAccounts.Last().PartialPayment)
                {
                    creditLineAccountBalances.CreditLineTransfers = сreditLineTransfers.Values.ToList();
                    if (discountsDictionary.Count > 0)
                    {
                        return ChangeAmountsWithDiscounts(creditLineAccountBalances, discountAmount, amount);
                    }
                    return creditLineAccountBalances;
                }
                availableFunds -= сreditLineTransfers[balance.ContractId].RefillableAccounts.Last().Amount;
            }

            foreach (var balance in creditLineBalance.ContractsBalances.Where(contract => contract.ContractId != creditLineId
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
                    .Add(new RefillableAccountsInfo("Пеня проценты", availableFunds, tempBalance));
                сreditLineTransfers[balance.ContractId].Amount +=
                    сreditLineTransfers[balance.ContractId].RefillableAccounts.Last().Amount;

                if (сreditLineTransfers[balance.ContractId].RefillableAccounts.Last().PartialPayment)
                {
                    creditLineAccountBalances.CreditLineTransfers = сreditLineTransfers.Values.ToList();
                    if (discountsDictionary.Count > 0)
                    {
                        return ChangeAmountsWithDiscounts(creditLineAccountBalances, discountAmount, amount);
                    }
                    return creditLineAccountBalances;
                }
                availableFunds -= сreditLineTransfers[balance.ContractId].RefillableAccounts.Last().Amount;
            }

            creditLineAccountBalances.CreditLineTransfers = сreditLineTransfers.Values.ToList();
            if (discountsDictionary.Count > 0)
            {
                return ChangeAmountsWithDiscounts(creditLineAccountBalances, discountAmount, amount);
            }
            return creditLineAccountBalances;
        }

        public async Task<ContractAction> PaymentOnContract(int contractId, decimal value, int payTypeId, ContractAction parentAction, int authorId,
            int branchId, bool autoApprove = false, DateTime? date = null)
        {
            if (date == null)
            {
                date = DateTime.Now;
            }
            var contractDutyModel = new ContractDutyCheckModel
            {
                ActionType = ContractActionType.Payment,
                ContractId = contractId,
                Cost = value,
                Date = date.Value,
                PayTypeId = payTypeId
            };
            ContractDuty contractDuty = _contractDutyService.GetContractDuty(contractDutyModel);
            List<int> payableExtraExpenseIds = new List<int>();

            ContractAction action = new ContractAction()
            {
                ContractId = contractId,
                CreateDate = date.Value,
                Data = new ContractActionData(),
                Date = date.Value,
                AuthorId = authorId,
                PayTypeId = payTypeId,
                TotalCost = value,
                ExtraExpensesIds = payableExtraExpenseIds,
                Cost = value, //
                Rows = contractDuty.Rows.ToArray(),
                Reason = contractDuty.Reason,
                ActionType = ContractActionType.Payment,
                Discount = contractDuty.Discount,
                ExtraExpensesCost = 0,
                ParentAction = parentAction
            };
            return _contractPaymentService.PaymentWithReturnContractAction(action, branchId, authorId,
                forceExpensePrepaymentReturn: autoApprove, autoApprove: autoApprove);
        }

        private CreditLineAccountBalancesDistribution ChangeAmountsWithDiscounts(
            CreditLineAccountBalancesDistribution balancesDistribution, decimal discountAmount, decimal amount)
        {
            //balancesDistribution.SummaryCurrentDebt -= discountAmount;
            //balancesDistribution.PaymentAmountFromClient -= discountAmount;
            //decimal afterPaymentPrepaymentBalance = amount - balancesDistribution.SummaryCurrentDebt;//сколько останется после текущей задолжности
            //if (afterPaymentPrepaymentBalance < 0)// если меньше 0 то баланс будет 0
            //    afterPaymentPrepaymentBalance = 0;
            //balancesDistribution.AfterPaymentPrepaymentBalance = afterPaymentPrepaymentBalance;
            //balancesDistribution.TotalDue -= discountAmount;
            //balancesDistribution.TotalCost = balancesDistribution.TotalDue;
            balancesDistribution.PaymentAmountFromClient =
                balancesDistribution.CreditLineTransfers.Sum(transfer => transfer.Amount);
            return balancesDistribution;
        }

    }
}
