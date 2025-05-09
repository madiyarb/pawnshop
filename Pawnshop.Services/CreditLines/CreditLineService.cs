using ContractNs = Pawnshop.Data.Models.Contracts;
using Pawnshop.AccountingCore.Models;
using Pawnshop.Core.Exceptions;
using Pawnshop.Core;
using Pawnshop.Data.Access;
using Pawnshop.Data.Models.Contracts.Actions;
using Pawnshop.Data.Models.Contracts.Expenses;
using Pawnshop.Data.Models.Contracts.Inscriptions;
using Pawnshop.Data.Models.CreditLines;
using Pawnshop.Services.Calculation;
using Pawnshop.Services.Contracts;
using Pawnshop.Services.Models.Calculation;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System;
using System.Collections.Concurrent;

namespace Pawnshop.Services.CreditLines
{
    public sealed class CreditLineService : ICreditLineService
    {
        private readonly IContractActionPrepaymentService _contractActionPrepaymentService;
        private readonly IContractActionRowBuilder _contractActionRowBuilder;
        private readonly IContractActionService _contractActionService;
        private readonly IContractDutyService _contractDutyService;
        private readonly IContractPaymentService _contractPaymentService;
        private readonly IContractService _contractService;
        private readonly CreditLineRepository _creditLineRepository;
        private readonly GroupRepository _groupRepository;
        private readonly LoanPercentRepository _loanPercentRepository;
        private readonly ContractCreditLineAdditionalLimitsRepository _contractCreditLineAdditionalLimitsRepository;
        private readonly ContractActionRepository _contractActionRepository;

        public CreditLineService(
            IContractActionPrepaymentService contractActionPrepaymentService,
            IContractActionRowBuilder contractActionRowBuilder,
            IContractActionService contractActionService,
            IContractDutyService contractDutyService,
            IContractPaymentService contractPaymentService,
            IContractService contractService,
            CreditLineRepository creditLineRepository,
            GroupRepository groupRepository,
            LoanPercentRepository loanPercentRepository,
            ContractCreditLineAdditionalLimitsRepository contractCreditLineAdditionalLimitsRepository,
            ContractActionRepository contractActionRepository)
        {
            _contractActionPrepaymentService = contractActionPrepaymentService;
            _contractActionRowBuilder = contractActionRowBuilder;
            _contractActionService = contractActionService;
            _contractDutyService = contractDutyService;
            _contractPaymentService = contractPaymentService;
            _contractService = contractService;
            _creditLineRepository = creditLineRepository;
            _groupRepository = groupRepository;
            _loanPercentRepository = loanPercentRepository;
            _contractCreditLineAdditionalLimitsRepository = contractCreditLineAdditionalLimitsRepository;
            _contractActionRepository = contractActionRepository;
        }


        public async Task<CheckForOpenTrancheModel> CheckForOpenTranche(int creditLineId, ContractNs.Contract creditLine)
        {
            creditLine ??= _contractService.GetOnlyContract(creditLineId, true);

            var product = await _loanPercentRepository.GetOnlyAsync(creditLine.SettingId.Value);

            var creditLineLimit = _contractService.GetCreditLineLimit(creditLine.Id).Result;
            creditLineLimit = Math.Truncate(creditLineLimit);

            if (product.LoanCostFrom > creditLineLimit)
                return new CheckForOpenTrancheModel
                {
                    IsCanOpen = false,
                    Message = $"Лимит кредитной линии [{creditLineId}] меньше минимальной суммы продукта, {product.LoanCostFrom:0,0.##} > {creditLineLimit:0,0.##}!",
                };

            var remainingPaymentsCount = GetRemainingPaymentsCount(creditLineId, creditLine);

            if (remainingPaymentsCount < product.ContractPeriodFrom.Value)
                return new CheckForOpenTrancheModel
                {
                    IsCanOpen = false,
                    Message = $"Количество оставшихся платежей по кредитной линии [{creditLineId}] меньше минимального количества платежей по продукту, {product.ContractPeriodFrom.Value} > {remainingPaymentsCount}!",
                };

            var tranches = await _contractService.GetAllSignedTranches(creditLineId);
            var firstCurrentPaymentDate = tranches.Min(x => x.NextPaymentDate);

            if (firstCurrentPaymentDate.HasValue && firstCurrentPaymentDate.Value.Date < DateTime.Today)
                return new CheckForOpenTrancheModel
                {
                    IsCanOpen = false,
                    Message = $"По кредитной линии [{creditLineId}] имеются просроченные платежи!",
                };

            return new CheckForOpenTrancheModel
            {
                CountPayment = remainingPaymentsCount,
                IsCanOpen = true,
                Message = $"Кредитная линия [{creditLineId}] доступна для открытия траншей.",
            };
        }

        public async Task<decimal> GetAmountForCurrentlyPayment(int creditLineId, DateTime? accrualDate = null)
        {
            decimal amount = 0;
            decimal extraExpensesCost = 0;
            List<int> contractIds = _creditLineRepository.GetContractListByCreditLineId(creditLineId);
            var creditLineBalance = await _creditLineRepository.GetBalancesByContractIdsAsync(new List<int> { creditLineId });
            foreach (var contractId in contractIds)
            {
                var contract = _contractService.Get(contractId);
                if (!contract.IsOffBalance)
                {
                    bool hasInscription = contract.InscriptionId.HasValue && contract.Inscription != null &&
                                          contract.Inscription.Status != InscriptionStatus.Denied;
                    _contractActionRowBuilder.Init(contract, date: accrualDate, balanceAccountsOnly: hasInscription);
                    amount += _contractActionRowBuilder.DisplayAmountWithoutExpenses;
                }

                if (_contractActionRowBuilder.ExtraExpensesCost > 0)
                    extraExpensesCost = _contractActionRowBuilder.ExtraExpensesCost;
            }

            if (extraExpensesCost > 0)
                amount += extraExpensesCost;

            if (creditLineBalance.FirstOrDefault() != null)
            {
                if (creditLineBalance.FirstOrDefault().PrepaymentBalance > 0)
                {
                    amount -= creditLineBalance.FirstOrDefault().PrepaymentBalance;
                }
            }


            return amount;
        }

        public async Task<CreditLineBalance> GetCurrentlyDebtForCreditLine(int creditLineId, List<int> selectedContracts = null, DateTime? date = null)
        {
            List<int> contractIds = _creditLineRepository.GetContractListByCreditLineId(creditLineId)
                .Where(id => selectedContracts == null || selectedContracts.Contains(id))
                .ToList();//берем только необходимые договора заодно проверяем относится ли присланный номер к этой кл
            contractIds.Add(creditLineId);
            IList<ContractBalance> contractBalances = (await _creditLineRepository.GetBalancesByContractIdsAsync(new List<int>(contractIds), date))
                .Where(contractBalance => contractBalance.IsOffBalance == false).ToList();

            CreditLineBalance creditLineBalance = new CreditLineBalance
            {
                ContractId = creditLineId,
                ContractsBalances = contractBalances.ToList(),
                SummaryAccountAmount = contractBalances.Sum(contract => contract.AccountAmount),
                SummaryProfitAmount = contractBalances.Sum(contract => contract.ProfitAmount),
                SummaryCurrentDebt = contractBalances.Sum(contract => contract.CurrentDebt),
                SummaryExpenseAmount = contractBalances.Sum(contract => contract.ExpenseAmount),
                SummaryOverdueAccountAmount = contractBalances.Sum(contract => contract.OverdueAccountAmount),
                SummaryOverdueProfitAmount = contractBalances.Sum(contract => contract.OverdueProfitAmount),
                SummaryPenyAmount = contractBalances.Sum(contract => contract.PenyAmount),
                SummaryTotalAcountAmount = contractBalances.Sum(contract => contract.TotalAcountAmount),
                SummaryTotalProfitAmount = contractBalances.Sum(contract => contract.TotalProfitAmount),
                SummaryPrepaymentBalance = contractBalances.Where(contract => contract.ContractId == creditLineId).Sum(contract => contract.PrepaymentBalance),
                SummaryTotalRepaymentAmount = contractBalances.Sum(contract => contract.TotalRepaymentAmount),
                SummaryTotalRedemptionAmount = contractBalances.Sum(contract => contract.TotalRedemptionAmount),
                SummaryProfitOffBalance = contractBalances.Sum(contract => contract.ProfitOffBalance),
                SummaryOverdueProfitOffBalance = contractBalances.Sum(contract => contract.OverdueProfitOffBalance),
                SummaryPenyAccountOffBalance = contractBalances.Sum(contract => contract.PenyAccountOffBalance),
                SummaryPenyProfitOffBalance = contractBalances.Sum(contract => contract.PenyProfitOffBalance),
                SummaryRepaymentAccountAmount = contractBalances.Sum(contract => contract.RepaymentAccountAmount),
                SummaryPenyAccount = contractBalances.Sum(contract => contract.PenyAccount),
                SummaryPenyProfit = contractBalances.Sum(contract => contract.PenyProfit),
                SummaryDefermentProfit = contractBalances.Sum(contract => contract.DefermentProfit),
                SummaryAmortizedProfit = contractBalances.Sum(contract => contract.AmortizedProfit),
                SummaryAmortizedDebtPenalty = contractBalances.Sum(contract => contract.AmortizedPenyAccount),
                SummaryAmortizedLoanPenalty = contractBalances.Sum(contract => contract.AmortizedPenyProfit)
            };
            return creditLineBalance;

        }

        public RefillableAccountsInfo GetExpensesPaymentSum(decimal summaryExpenseAmount, decimal amount, int creditLineId)
        {
            if (amount >= summaryExpenseAmount)
            {
                return new RefillableAccountsInfo("Дополнительные расходы", summaryExpenseAmount, false); // Можем погасить все доп расходы
            }

            var contractDutyModel = new ContractDutyCheckModel
            {
                ActionType = ContractActionType.Payment,
                ContractId = creditLineId,
                Cost = amount,
                Date = DateTime.Now,
                PayTypeId = 2 //TODO Get Default payment Type id 
            };

            ContractDuty contractDuty = _contractDutyService.GetContractDuty(contractDutyModel);
            List<ContractExpense> payableExtraExpenses = contractDuty.ExtraContractExpenses.OrderBy(expense => expense.TotalCost).ToList(); // Возможно мы можем погасить какой то из доп расходов? 

            if (payableExtraExpenses.Count == 0)
            {
                return new RefillableAccountsInfo("Дополнительные расходы", 0, true);
            }

            decimal howManyCanPay = 0;
            for (int i = 0; i < payableExtraExpenses.Count; i++)
            {
                if (amount >= payableExtraExpenses[i].TotalCost)
                {
                    amount -= payableExtraExpenses[i].TotalCost;
                    howManyCanPay += payableExtraExpenses[i].TotalCost;
                }
                else
                {
                    return new RefillableAccountsInfo("Дополнительные расходы", howManyCanPay, true);
                }
            }

            throw new PawnshopApplicationException(
                "Не сходятся суммы по оплате доп расходов из GetContractDuty и переданный в метод");
        }

        public int GetRemainingPaymentsCount(int creditLineId, ContractNs.Contract creditLine)
        {
            var nextPaymentDate = _contractService.GetNextPaymentDateByCreditLineId(creditLineId);

            if (!nextPaymentDate.HasValue)
                nextPaymentDate = DateTime.Today.AddMonths(1);

            if (creditLine == null)
                creditLine = _contractService.GetOnlyContract(creditLineId);

            int remainingPaymentsCount = 0;
            bool finish = false;

            while (!finish)
            {
                if (nextPaymentDate <= creditLine.MaturityDate)
                {
                    remainingPaymentsCount++;
                    nextPaymentDate = nextPaymentDate.Value.AddMonths(1);
                }
                else
                {
                    finish = true;
                }
            }

            return remainingPaymentsCount;
        }

        public ContractAction MovePrepayment(int creditLineId, int contractId, decimal value, int authorId, int branchId, string note = null,
            ContractAction action = null, bool autoApprove = false, DateTime? date = null)
        {
            if (date == null)
            {
                date = DateTime.Now;
            }
            var branch = _groupRepository.Get(branchId);

            var prepaymentModel = new MovePrepayment
            {
                SourceContractId = creditLineId,
                Date = date.Value,
                Amount = value,
                RecipientContractId = contractId,
                Note = note ?? new string("Перемещение дс между кредитной линии и договорами для оплаты или выкупа договоров")
            };
            return _contractActionPrepaymentService.MovePrepayment(prepaymentModel, authorId, branch, action, autoApprove);
        }

        public ContractAction MovePrepaymentFromTrancheToCreditLine(int creditLineId, int contractId, decimal value,
            int authorId, int branchId, string note = null,
            ContractAction action = null, bool autoApprove = false, DateTime? date = null)
        {
            if (date == null)
            {
                date = DateTime.Now;
            }
            var branch = _groupRepository.Get(branchId);

            var prepaymentModel = new MovePrepayment
            {
                SourceContractId = contractId,
                Date = date.Value,
                Amount = value,
                RecipientContractId = creditLineId,
                Note = note ?? new string("Перемещение дс между кредитной линии и договорами для оплаты или выкупа договоров")
            };
            return _contractActionPrepaymentService.MovePrepayment(prepaymentModel, authorId, branch, action, autoApprove);
        }

        public ContractAction PayExtraExpenses(int creditLineId, decimal amount, DateTime date, int payTypeId, int authorId, int branchId, bool autoApprove)
        {
            var contractDutyModel = new ContractDutyCheckModel
            {
                ActionType = ContractActionType.Payment,
                ContractId = creditLineId,
                Cost = amount,
                Date = date,
                PayTypeId = payTypeId
            };

            ContractDuty contractDuty = _contractDutyService.GetContractDuty(contractDutyModel);
            decimal payableExtraExpensesCost = 0;
            List<ContractExpense> payableExtraExpenses = contractDuty.ExtraContractExpenses;

            if (payableExtraExpenses.Count > 0)
                payableExtraExpensesCost = contractDuty.ExtraContractExpenses.Sum(e => e.TotalCost);

            List<ContractActionRow> contractActionRows = contractDuty.Rows;
            var contractActionAmountTypeDict = new Dictionary<AmountType, decimal>();
            foreach (ContractActionRow contractActionRow in contractActionRows)
                contractActionAmountTypeDict[contractActionRow.PaymentType] = contractActionRow.Cost;

            decimal actionRowsCost = contractActionAmountTypeDict.Values.Sum();
            decimal expensesAndActionRowsCost = payableExtraExpensesCost + actionRowsCost;
            List<int> payableExtraExpenseIds = payableExtraExpenses.Select(e => e.Id).ToList();

            ContractAction expensePaymentAction = new ContractAction()
            {
                ContractId = creditLineId,
                CreateDate = date,
                Data = new ContractActionData(),
                Date = date,
                AuthorId = authorId,
                PayTypeId = payTypeId,
                TotalCost = actionRowsCost,
                ExtraExpensesIds = payableExtraExpenseIds,
                Cost = expensesAndActionRowsCost,
                Rows = contractDuty.Rows.ToArray(),
                Reason = contractDuty.Reason,
                ActionType = ContractActionType.Payment,
                Discount = contractDuty.Discount,
                ExtraExpensesCost = payableExtraExpensesCost,
            };

            var contractAction = _contractPaymentService.PaymentWithReturnContractAction(expensePaymentAction, branchId, authorId,
                forceExpensePrepaymentReturn: true, autoApprove: autoApprove);

            return contractAction;


        }

        public async Task<bool> UnconfirmedActionExists(List<int> contractIds)
        {
            foreach (var contractId in contractIds)
            {
                if (await _contractActionService.IncopleteActionExists(contractId))
                    return true;
            }
            return false;
        }

        public async Task<decimal> GetLimitForInsuranceByContractId(int contractId)
        {
            var position = _contractService.GetPositionsByContractId(contractId).FirstOrDefault();
            return await GetLimitForInsuranceByPosition(position.EstimatedCost);
        }

        public async Task<decimal> GetLimitForInsuranceByPosition(decimal estimatedCost)
        {
            var applicationAdditionalLimit = await GetLimitPersentForInsurance(estimatedCost);

            var additionalLimitCost = Math.Min(estimatedCost * applicationAdditionalLimit / 100, Constants.MAX_ADDITIONAL_SUM);
            return additionalLimitCost;
        }

        public async Task<decimal> GetLimitPersentForInsurance(decimal estimatedCost)
        {
            return await _contractCreditLineAdditionalLimitsRepository.GetLimitBySum(estimatedCost);
        }

        public async Task<bool> IncompleteActionExistsAsync(List<int> contractIds)
        {
            var tasks = new List<Task>();
            var incompleteActionsList = new ConcurrentBag<ContractAction>();

            foreach (var contractId in contractIds)
            {
                tasks.Add(Task.Run(async () =>
                {
                    var incompleteActions = await _contractActionRepository.GetIncompleteActions(contractId);

                    foreach (var item in incompleteActions)
                    {
                        incompleteActionsList.Add(item);
                    }
                }));
            }

            Task.WaitAll(tasks.ToArray());

            return incompleteActionsList.Count > 0;
        }

        public async Task<bool> HasExpenses(List<int> contractIds)
        {
            var tasks = new List<Task>();
            var expensesResults = new ConcurrentBag<bool>();

            foreach (var contractId in contractIds)
            {
                tasks.Add(Task.Run(async () =>
                {
                    var hasExpenses = await _contractService.HasExpenses(contractId);
                    expensesResults.Add(hasExpenses);
                }));
            }

            Task.WaitAll(tasks.ToArray());

            return expensesResults.Any(x => x);
        }
    }
}
