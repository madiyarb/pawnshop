using Pawnshop.Core.Exceptions;
using Pawnshop.Core;
using Pawnshop.Data.Access;
using Pawnshop.Data.Models.CashOrders;
using Pawnshop.Data.Models.Contracts.Actions;
using Pawnshop.Services.Calculation;
using Pawnshop.Services.Contracts.PartialPayment;
using Pawnshop.Services.Contracts;
using Pawnshop.Services.CreditLines.Payment;
using Pawnshop.Services.Models.Calculation;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System;

namespace Pawnshop.Services.CreditLines.PartialPayment
{
    public sealed class CreditLinePartialPaymentService : ICreditLinePartialPaymentService
    {
        private readonly ICreditLineService _creditLineService;
        private readonly IContractActionService _contractActionService;
        private readonly IContractActionPrepaymentService _contractActionPrepaymentService;
        private readonly ContractRepository _contractRepository;
        private readonly IContractService _contractService;
        private readonly IContractDutyService _contractDutyService;
        private readonly IContractActionPartialPaymentService _contractActionPartialPaymentService;
        private readonly ICreditLinePaymentService _creditLinePaymentService;
        private readonly ContractDiscountRepository _contractDiscountRepository;
        private readonly ApplicationRepository _applicationRepository;
        private readonly ApplicationDetailsRepository _applicationDetailsRepository;
        public CreditLinePartialPaymentService(
            ICreditLineService creditLineService, 
            ContractRepository contractRepository,
            IContractActionPrepaymentService 
            contractActionPrepaymentService, 
            IContractActionService contractActionService, 
            IContractService contractService, 
            IContractDutyService contractDutyService,
            IContractActionPartialPaymentService 
            contractActionPartialPaymentService, 
            ICreditLinePaymentService creditLinePaymentService,
            ContractDiscountRepository contractDiscountRepository, 
            ApplicationRepository applicationRepository, 
            ApplicationDetailsRepository applicationDetailsRepository)
        {
            _creditLineService = creditLineService;
            _contractRepository = contractRepository;
            _contractActionPrepaymentService = contractActionPrepaymentService;
            _contractActionService = contractActionService;
            _contractService = contractService;
            _contractDutyService = contractDutyService;
            _contractActionPartialPaymentService = contractActionPartialPaymentService;
            _creditLinePaymentService = creditLinePaymentService;
            _contractDiscountRepository = contractDiscountRepository;
            _applicationRepository = applicationRepository;
            _applicationDetailsRepository = applicationDetailsRepository;
        }

        public async Task<CreditLineAccountBalancesDistributionForPartialPayment> GetCreditLineAccountBalancesDistribution(
            int creditLineId, int partialPaymentContractId, decimal amount, DateTime? date = null)
        {
            bool enoughFundsForPayment = false;
            bool enoughFundsForBuyOut = false;
            var creditLineBalance = await _creditLineService.GetCurrentlyDebtForCreditLine(creditLineId, null, date: date);
            Dictionary<int, CreditLineTransferPartialPayment> сreditLineTransfers = new Dictionary<int, CreditLineTransferPartialPayment>();
            List<int> contractIds = creditLineBalance.ContractsBalances
                .Select(contractBalance => contractBalance.ContractId).ToList();
            if (date == null)
            {
                date = DateTime.Now;
            }

            var discounts = _contractDiscountRepository.GetByContractIds(contractIds)
                .Where(discount => discount.BeginDate <= date && discount.EndDate >= date && discount.ContractActionId == null);
            var discountsDictionary = discounts.ToDictionary(discount => discount.ContractId, discount => discount);
            decimal availableFunds = amount;
            decimal discountAmount = 0;
            decimal afterPaymentPrepaymentBalance = (decimal)0.0;

            if (creditLineBalance.SummaryPrepaymentBalance > amount)//сколько останется после чдп на авансе
            {
                afterPaymentPrepaymentBalance = creditLineBalance.SummaryPrepaymentBalance - amount;
            }

            var partialPaymentContractBalance = creditLineBalance.ContractsBalances.FirstOrDefault(contract => contract.ContractId == partialPaymentContractId);

            decimal currentProfit = 0;
            if (partialPaymentContractBalance != null)// Сумма текущих процентов 
            {
                currentProfit = partialPaymentContractBalance.ProfitAmount + partialPaymentContractBalance.AmortizedProfit + partialPaymentContractBalance.DefermentProfit;
            }

            decimal partialPaymentAmount = 0; //текущие начисленные %
            if (amount > creditLineBalance.SummaryCurrentDebt + partialPaymentContractBalance.AmortizedPenyAccount + partialPaymentContractBalance.AmortizedPenyProfit)
            {
                partialPaymentAmount = amount - creditLineBalance.SummaryCurrentDebt - currentProfit - partialPaymentContractBalance.AmortizedPenyAccount - partialPaymentContractBalance.AmortizedPenyProfit;
                if (partialPaymentAmount < 0)
                    partialPaymentAmount = 0;
            }

            decimal paymentAmountFromClient = amount - creditLineBalance.SummaryPrepaymentBalance;//Сколько надо взять с клиента
            if (paymentAmountFromClient < 0) // Если на авансе больше то 0 
                paymentAmountFromClient = 0;

            decimal totalDue = amount - creditLineBalance.SummaryExpenseAmount;

            if (totalDue < 0)
            {
                totalDue = 0;
            }

            decimal mainDebtAfterPayment = 0;
            if (partialPaymentContractBalance != null)
            {
                mainDebtAfterPayment = partialPaymentContractBalance.AccountAmount - partialPaymentAmount;
            }


            foreach (var contract in creditLineBalance.ContractsBalances)
            {
                сreditLineTransfers.Add(contract.ContractId, new CreditLineTransferPartialPayment
                {
                    Amount = 0,
                    ContractId = contract.ContractId,
                    ContractNumber = contract.ContractNumber,
                    IsCreditLine = contract.ContractId == creditLineId,
                    RefillableAccounts = new List<RefillableAccountsInfo>(),
                    DebtAfterPayment = creditLineBalance
                        .ContractsBalances.FirstOrDefault(contr => contr.ContractId == contract.ContractId).AccountAmount
                });
            }

            CreditLineAccountBalancesDistributionForPartialPayment creditLineAccountBalances =
                new CreditLineAccountBalancesDistributionForPartialPayment
                {
                    ContractBalances = creditLineBalance.ContractsBalances,
                    ContractId = creditLineBalance.ContractId,
                    SummaryCurrentDebt = creditLineBalance.SummaryCurrentDebt,
                    SummaryExpenseAmount = creditLineBalance.SummaryExpenseAmount,
                    SummaryPrepaymentBalance = creditLineBalance.SummaryPrepaymentBalance,
                    AfterPaymentPrepaymentBalance = afterPaymentPrepaymentBalance,
                    PartialPaymentAmount = partialPaymentAmount,
                    PaymentAmountFromClient = paymentAmountFromClient,
                    TotalDue = totalDue,
                    TotalCost = totalDue,
                    CurrentProfit = currentProfit,
                    DebtAfterPayment = mainDebtAfterPayment,
                    EnoughFundsForPayment = amount > creditLineBalance.SummaryCurrentDebt,
                    EnoughFundsForBuyOut = false
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

            if (amount == 0)
            {
                creditLineAccountBalances.CreditLineTransfers = сreditLineTransfers.Values.ToList();
                return creditLineAccountBalances;
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
                    return creditLineAccountBalances;
                }
                сreditLineTransfers[creditLineId].RefillableAccounts.Add(expenses);
                creditLineAccountBalances.CreditLineTransfers = сreditLineTransfers.Values.ToList();
                сreditLineTransfers[creditLineId].Amount = expenses.Amount;
                availableFunds -= expenses.Amount;
            }

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
                         && contract.RepaymentAccountAmount > 0 && contract.ContractId != partialPaymentContractId)) //текущий ОД
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
                         && contract.RepaymentProfitAmount > 0 && contract.ContractId != partialPaymentContractId)) //текущие %
            {
                сreditLineTransfers[balance.ContractId].RefillableAccounts
                    .Add(new RefillableAccountsInfo("Текущие проценты", availableFunds, balance.RepaymentProfitAmount));
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
            creditLineAccountBalances.EnoughFundsForPayment = true;
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

            foreach (var balance in creditLineBalance.ContractsBalances.Where(contract => contract.ContractId == partialPaymentContractId))
            {
                сreditLineTransfers[balance.ContractId].PaymentAmount = сreditLineTransfers[balance.ContractId].Amount;
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
                    .Add(new RefillableAccountsInfo("Вознаграждение", availableFunds, tempBalance));
                сreditLineTransfers[balance.ContractId].Amount +=
                    сreditLineTransfers[balance.ContractId].RefillableAccounts.Last().Amount;

                creditLineAccountBalances.CurrentProfit =
                    сreditLineTransfers[balance.ContractId].RefillableAccounts.Last().Amount;
                if (сreditLineTransfers[balance.ContractId].RefillableAccounts.Last().PartialPayment)
                {
                    creditLineAccountBalances.CreditLineTransfers = сreditLineTransfers.Values.ToList();
                    return creditLineAccountBalances;
                }
                availableFunds -= сreditLineTransfers[balance.ContractId].RefillableAccounts.Last().Amount;
            }

            foreach (var balance in creditLineBalance.ContractsBalances.Where(contract => contract.ContractId != creditLineId
                         && contract.AmortizedProfit > 0 && contract.ContractId == partialPaymentContractId)) // Амортизированное вознаграждение (для чдп начисляемый счет и амортизированный)
            {
                сreditLineTransfers[balance.ContractId].RefillableAccounts
                    .Add(new RefillableAccountsInfo("Амортизированное вознаграждение", availableFunds, balance.AmortizedProfit));
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
               && contract.DefermentProfit > 0 && contract.ContractId == partialPaymentContractId)) // Отсроченное вознаграждение (для чдп начисляемый счет и амортизированный)
            {
                сreditLineTransfers[balance.ContractId].RefillableAccounts
                    .Add(new RefillableAccountsInfo("Отсроченное вознаграждение", availableFunds, balance.DefermentProfit));
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
                         && contract.AmortizedPenyAccount > 0 && contract.ContractId == partialPaymentContractId)) // Амортизированая пеня на долг просроченный (для чдп начисляемый счет и амортизированный)
            {
                сreditLineTransfers[balance.ContractId].RefillableAccounts
                    .Add(new RefillableAccountsInfo("Амортизированая пеня на долг просроченный", availableFunds, balance.AmortizedPenyAccount));
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
                         && contract.AmortizedPenyProfit > 0 && contract.ContractId == partialPaymentContractId)) // Амортизированая пеня на проценты просроченные (для чдп начисляемый счет и амортизированный)
            {
                сreditLineTransfers[balance.ContractId].RefillableAccounts
                    .Add(new RefillableAccountsInfo("Амортизированая пеня на проценты просроченные", availableFunds, balance.AmortizedPenyProfit));
                сreditLineTransfers[balance.ContractId].Amount +=
                    сreditLineTransfers[balance.ContractId].RefillableAccounts.Last().Amount;

                if (сreditLineTransfers[balance.ContractId].RefillableAccounts.Last().PartialPayment)
                {
                    creditLineAccountBalances.CreditLineTransfers = сreditLineTransfers.Values.ToList();
                    return creditLineAccountBalances;
                }
                availableFunds -= сreditLineTransfers[balance.ContractId].RefillableAccounts.Last().Amount;
            }

            foreach (var balance in creditLineBalance.ContractsBalances.Where(contract => contract.ContractId == partialPaymentContractId))
            {
                сreditLineTransfers[balance.ContractId].RefillableAccounts
                    .Add(new RefillableAccountsInfo("Основной долг", availableFunds, balance.AccountAmount));
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
            creditLineAccountBalances.EnoughFundsForBuyOut = true;
            if (discountsDictionary.Count > 0)
            {
                return ChangeAmountsWithDiscounts(creditLineAccountBalances, discountAmount, amount);
            }
            return creditLineAccountBalances;
        }

        public async Task<int?> PartialPaymentAndTransfer(int creditLineId, int authorId, int partialPaymentContractId, int payTypeId,
            int branchId, decimal amount, DateTime? date = null, bool unsecuredContractSignNotallowed = false, bool changeCategory = false)
        {
            ContractAction partialPaymentAction = null;
            if (date == null)
                date = DateTime.Now;

            var partialPaymentBalance = await GetCreditLineAccountBalancesDistribution(creditLineId: creditLineId,
                amount: amount,
                partialPaymentContractId: partialPaymentContractId, date: date);
            decimal fundsFromClient = 0;

            fundsFromClient = amount - partialPaymentBalance.SummaryPrepaymentBalance;
            if (fundsFromClient < 0)
                fundsFromClient = 0;
            else
                fundsFromClient = Math.Ceiling(fundsFromClient);

            if (fundsFromClient > 0 && branchId == 530 && payTypeId == 2)
            {
                throw new PawnshopApplicationException("Внеcение денег на аванс договора через кассу из филиала Tas Online недоступно.");
            }

            if (!partialPaymentBalance.EnoughFundsForPayment)
                throw new PawnshopApplicationException("Денег не хватает на оплату текущей задолжности");

            if (partialPaymentBalance.SummaryCurrentDebt + 1000 > amount)
                throw new PawnshopApplicationException("Денег хватает на оплату текущей задолжности но частичное погашение меньше чем на 1000 тг невозможно");

            if (partialPaymentBalance.SummaryCurrentDebt > 0)
                throw new PawnshopApplicationException("Есть текущая задолжность сначало её необходимо погасить чтобы выполнить действие ЧДП");

            if (partialPaymentBalance.EnoughFundsForBuyOut)
                throw new PawnshopApplicationException(
                    "Денег хватает на выкуп, выполните его вместо оплаты текущей задолжности");


            Dictionary<int, decimal> contractsSum = partialPaymentBalance
                .CreditLineTransfers.ToDictionary(k => k.ContractId, v => v.Amount);

            if (await _creditLineService.UnconfirmedActionExists(contractsSum.Keys.ToList()))
                throw new PawnshopApplicationException("В договоре имеется неподтвержденное действие");

            OrderStatus orderStatus = OrderStatus.WaitingForApprove;

            ContractAction parentAction = null;
            ContractAction childAction = null;
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
                        authorId, branchId, false);
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
                             .Where(sum => sum.Key != creditLineId && sum.Key != partialPaymentContractId))
                {
                    childAction = _creditLineService.MovePrepayment(creditLineId: creditLineId,
                        contractId: sum.Key,
                        value: sum.Value,
                        authorId: authorId,
                        action: parentAction,
                        autoApprove: false,
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
                        autoApprove: false,
                        date: date);
                    parentAction.ChildActionId = childAction.Id;
                    _contractActionService.Save(parentAction);
                    childAction.ParentActionId = parentAction.Id;
                    _contractActionService.Save(childAction);
                    parentAction = childAction;
                }

                #endregion

                #region Ч.Д.П.
                childAction = _creditLineService.MovePrepayment(creditLineId: creditLineId,
                    contractId: partialPaymentContractId,
                    value: contractsSum[partialPaymentContractId],
                    authorId: authorId,
                    action: parentAction,
                    autoApprove: false,
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
                decimal mainDebtPaymenttAmount = 0;
                var mainDebtPaymentt = partialPaymentBalance
                    .CreditLineTransfers.FirstOrDefault(clt => clt.ContractId == partialPaymentContractId)
                    .RefillableAccounts.FirstOrDefault(refilableAccount => refilableAccount.Name == "Основной долг");

                if (mainDebtPaymentt != null)
                {
                    mainDebtPaymenttAmount = mainDebtPaymentt.Amount;
                }

                childAction = await PartialPayment(contractId: partialPaymentContractId, payTypeId: payTypeId, authorId: authorId, branchId: branchId,
                    cost: mainDebtPaymenttAmount,
                    totalCost: contractsSum[partialPaymentContractId], unsecuredContractSignNotAllowed: unsecuredContractSignNotallowed,
                    date: date);

                if (changeCategory)
                {
                    var creditLine = _contractService.GetOnlyContract(creditLineId);
                    if (creditLine != null && !creditLine.CreatedInOnline)
                    {
                        var totalDebt = _contractService.GetDebtInfoByCreditLine(creditLineId).PrincipalDebt;
                        var debt = (totalDebt - childAction.Cost);
                        var application = _applicationRepository.FindByContractId(partialPaymentContractId) ?? _applicationRepository.FindByContractId(creditLineId);
                        var lastTrancheApplication = _applicationRepository.FindByContractId(_contractRepository.GetAllSignedTranches(creditLineId).Result.LastOrDefault().Id);
                        if (lastTrancheApplication == null)
                            lastTrancheApplication = _applicationRepository.FindByContractId(creditLineId);

                        if (_contractService.GetContractSettings(creditLineId).IsInsuranceAdditionalLimitOn && !lastTrancheApplication.WithoutDriving)
                        {
                            var additionalLimit = await _creditLineService.GetLimitForInsuranceByContractId(creditLineId);

                            var appDetails = _applicationDetailsRepository.Find(new { ContractId = partialPaymentContractId }) ?? _applicationDetailsRepository.Find(new { ContractId = creditLineId });

                            var creditLineCategory = _contractRepository.GetContractPositions(creditLineId).Positions.FirstOrDefault().Category.Code;
                            bool isCategoryMotor = creditLineCategory == Constants.WITHOUT_DRIVE_RIGHT_CATEGORY_CODE;

                            _contractService.ChangeCategoryForCredilLineData(application, isCategoryMotor, appDetails.ProdKind, additionalLimit, debt, _contractRepository.GetOnlyContract(partialPaymentContractId).SettingId.Value);

                            if (application != null && debt <= application.LightCost.Value + additionalLimit)
                                ChangeCategory(childAction, creditLineId, debt, additionalLimit);
                            else
                                throw new PawnshopApplicationException("Нельзя сменить категорию, есть неоплаченный долг по кредиту");
                        }
                        else if (!lastTrancheApplication.WithoutDriving)
                        {
                            ChangeCategory(childAction, creditLineId, debt, 0);
                        }
                        else
                        {
                            throw new PawnshopApplicationException("Нельзя сменить категорию аналитики");
                        }
                    }
                }
                partialPaymentAction = childAction;
                if (parentAction != null)
                {
                    parentAction.ChildActionId = childAction.Id;
                    _contractActionService.Save(parentAction);
                    childAction.ParentActionId = parentAction.Id;
                    _contractActionService.Save(childAction);
                }
                #endregion
                transaction.Commit();
            }

            if (partialPaymentAction == null)
                return null;
            return partialPaymentAction.Id;
        }

        public async Task<ContractAction> PartialPayment(int contractId, int payTypeId, int authorId, int branchId, decimal cost,
            decimal totalCost, bool unsecuredContractSignNotAllowed, DateTime? date = null)
        {
            if (date == null)
            {
                date = DateTime.Now;
            }
            var partialPaymentCheckModel = new ContractDutyCheckModel
            {
                ActionType = ContractActionType.PartialPayment,
                ContractId = contractId,
                Date = date.Value,
                PayTypeId = payTypeId,
                Cost = cost
            };
            var contract = _contractService.Get(contractId);

            ContractDuty partialPaymentDuty = _contractDutyService.GetContractDuty(partialPaymentCheckModel);

            var partialPaymentContractAction = new ContractAction
            {
                ActionType = ContractActionType.PartialPayment,
                ContractId = contractId,
                CreateDate = date.Value,
                Date = partialPaymentDuty.Date,
                AuthorId = authorId,
                ExtraExpensesCost = partialPaymentDuty.ExtraExpensesCost,
                PayTypeId = payTypeId,
                Discount = partialPaymentDuty?.Discount,
                Rows = partialPaymentDuty.Rows.ToArray(),
                Cost = cost,
                TotalCost = totalCost,
                Reason = $"Досрочное погашение по договору {contract.ContractNumber} от {date.Value}"
            };

            return (await _contractActionPartialPaymentService.Exec(partialPaymentContractAction, authorId, branchId, unsecuredContractSignNotAllowed, forceExpensePrepaymentReturn: false));
        }

        public async Task<int?> PartialPaymentAndTransferByOnline(int creditLineId, int authorId, int partialPaymentContractId, int payTypeId,
            int branchId, decimal amount, DateTime? date = null, bool unsecuredContractSignNotallowed = false, bool changeCategory = false)
        {
            ContractAction partialPaymentAction = null;

            if (date == null)
                date = DateTime.Now;

            var partialPaymentBalance = await GetCreditLineAccountBalancesDistribution(creditLineId: creditLineId,
                amount: amount,
                partialPaymentContractId: partialPaymentContractId, date: date);

            decimal fundsFromClient = 0;

            fundsFromClient = amount - partialPaymentBalance.SummaryPrepaymentBalance;

            if (fundsFromClient < 0)
                fundsFromClient = 0;
            else
                fundsFromClient = Math.Ceiling(fundsFromClient);

            Dictionary<int, decimal> contractsSum = partialPaymentBalance
                .CreditLineTransfers.ToDictionary(k => k.ContractId, v => v.Amount);

            ContractAction parentAction = null;
            ContractAction childAction = null;

            using (IDbTransaction transaction = _contractRepository.BeginTransaction())
            {
                #region Ч.Д.П.
                childAction = _creditLineService.MovePrepayment(creditLineId: creditLineId,
                    contractId: partialPaymentContractId,
                    value: contractsSum[partialPaymentContractId],
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
                decimal mainDebtPaymenttAmount = 0;
                var mainDebtPaymentt = partialPaymentBalance
                    .CreditLineTransfers.FirstOrDefault(clt => clt.ContractId == partialPaymentContractId)
                    .RefillableAccounts.FirstOrDefault(refilableAccount => refilableAccount.Name == "Основной долг");

                if (mainDebtPaymentt != null)
                {
                    mainDebtPaymenttAmount = mainDebtPaymentt.Amount;
                }

                childAction = await PartialPayment(contractId: partialPaymentContractId, payTypeId: payTypeId, authorId: authorId, branchId: branchId,
                    cost: mainDebtPaymenttAmount,
                    totalCost: contractsSum[partialPaymentContractId], unsecuredContractSignNotAllowed: unsecuredContractSignNotallowed,
                    date: date);

                if (changeCategory)
                {
                    var creditLine = _contractService.GetOnlyContract(creditLineId);
                    if (creditLine != null && !creditLine.CreatedInOnline)
                    {
                        var totalDebt = _contractService.GetDebtInfoByCreditLine(creditLineId).PrincipalDebt;
                        var debt = (totalDebt - childAction.Cost);
                        var application = _applicationRepository.FindByContractId(partialPaymentContractId) ?? _applicationRepository.FindByContractId(creditLineId);
                        var lastTrancheApplication = _applicationRepository.FindByContractId(_contractRepository.GetAllSignedTranches(creditLineId).Result.LastOrDefault().Id);
                        if (lastTrancheApplication == null)
                            lastTrancheApplication = _applicationRepository.FindByContractId(creditLineId);

                        if (_contractService.GetContractSettings(creditLineId).IsInsuranceAdditionalLimitOn && !lastTrancheApplication.WithoutDriving)
                        {
                            var additionalLimit = await _creditLineService.GetLimitForInsuranceByContractId(creditLineId);

                            var appDetails = _applicationDetailsRepository.Find(new { ContractId = partialPaymentContractId }) ?? _applicationDetailsRepository.Find(new { ContractId = creditLineId });

                            var creditLineCategory = _contractRepository.GetContractPositions(creditLineId).Positions.FirstOrDefault().Category.Code;
                            bool isCategoryMotor = creditLineCategory == Constants.WITHOUT_DRIVE_RIGHT_CATEGORY_CODE;

                            _contractService.ChangeCategoryForCredilLineData(application, isCategoryMotor, appDetails.ProdKind, additionalLimit, debt, _contractRepository.GetOnlyContract(partialPaymentContractId).SettingId.Value);

                            if (application != null && debt <= application.LightCost.Value + additionalLimit)
                                ChangeCategory(childAction, creditLineId, debt, additionalLimit);
                            else
                                throw new PawnshopApplicationException("Нельзя сменить категорию, есть неоплаченный долг по кредиту");
                        }
                        else if (!lastTrancheApplication.WithoutDriving)
                        {
                            ChangeCategory(childAction, creditLineId, debt, 0);
                        }
                        else
                        {
                            throw new PawnshopApplicationException("Нельзя сменить категорию аналитики");
                        }
                    }
                }

                partialPaymentAction = childAction;
                if (parentAction != null)
                {
                    parentAction.ChildActionId = childAction.Id;
                    _contractActionService.Save(parentAction);
                    childAction.ParentActionId = parentAction.Id;
                    _contractActionService.Save(childAction);
                }
                #endregion

                transaction.Commit();
            }

            if (partialPaymentAction == null)
                return null;

            return partialPaymentAction.Id;
        }


        private CreditLineAccountBalancesDistributionForPartialPayment ChangeAmountsWithDiscounts(
            CreditLineAccountBalancesDistributionForPartialPayment balancesDistribution, decimal discountAmount, decimal amount)
        {
            //balancesDistribution.SummaryCurrentDebt -= discountAmount;
            //balancesDistribution.PaymentAmountFromClient -= discountAmount;
            //decimal afterPaymentPrepaymentBalance = amount - balancesDistribution.SummaryCurrentDebt;//сколько останется после текущей задолжности
            //if (afterPaymentPrepaymentBalance < 0)// если меньше 0 то баланс будет 0
            //    afterPaymentPrepaymentBalance = 0;
            //balancesDistribution.AfterPaymentPrepaymentBalance = afterPaymentPrepaymentBalance;
            //balancesDistribution.TotalDue -= discountAmount;
            //balancesDistribution.TotalCost = balancesDistribution.TotalDue;

            return balancesDistribution;
        }

        private bool ChangeCategory(ContractAction action, int contractId, decimal debt, decimal additionalLimit)
        {
            if (action.Data != null)
                action.Data.CategoryChanged = true;
            else
            {
                ContractActionData data = new ContractActionData();
                data.CategoryChanged = true;
                action.Data = data;
            }
            var contract = _contractService.Get(contractId);
            // LeftLoanCost из кредитной линии = 0, поэтому счимаем задолжность по всем траншам
            return _contractService.ChangeCategory(action, contract, debt, additionalLimit);
        }
    }
}
