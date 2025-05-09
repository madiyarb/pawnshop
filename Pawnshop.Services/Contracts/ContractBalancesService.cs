using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Pawnshop.Data.Access;
using Pawnshop.Data.Models.Contracts;
using Pawnshop.Data.Models.Contracts.Views;

namespace Pawnshop.Services.Contracts
{
    public sealed class ContractBalancesService : IContractBalancesService
    {
        private readonly ContractRepository _repository;
        private readonly IContractPaymentScheduleService _contractPaymentScheduleService;
        private readonly IContractService _contractService;
        private readonly CreditLineRepository _creditLineRepository;
        public ContractBalancesService(
            IContractPaymentScheduleService contractPaymentScheduleService,
            ContractRepository repository, IContractService contractService, CreditLineRepository creditLineRepository)
        {
            _contractPaymentScheduleService = contractPaymentScheduleService;
            _repository = repository;
            _contractService = contractService;
            _creditLineRepository = creditLineRepository;
        }

        public async Task<ContractBalanceOnlineView> GetContractBalanceOnline(Contract contract)
        {

            if (contract.ContractClass == ContractClass.CreditLine)
            {
                List<int> contractIds = _creditLineRepository.GetContractListByCreditLineId(contract.Id);
                var creditLineBalance = await _contractService.GetBalance(contract.Id);
                List<ContractBalanceOnlineView> balances = new List<ContractBalanceOnlineView>();
                foreach (var contractId in contractIds)
                {
                    var tranche = await _repository.GetOnlyContractAsync(contractId);
                    balances.Add(await GetTrancheOrCreditBalance(tranche));
                }

                return new ContractBalanceOnlineView
                {
                    ContractId = contract.Id,
                    AccountAmount = balances.Sum(balance => balance.AccountAmount),
                    ProfitAmount = balances.Sum(balance => balance.ProfitAmount),
                    OverdueAccountAmount = balances.Sum(balance => balance.OverdueAccountAmount),
                    OverdueProfitAmount = balances.Sum(balance => balance.OverdueProfitAmount),
                    PenyAmount = balances.Sum(balance => balance.PenyAmount),
                    TotalAccountAmount = balances.Sum(balance => balance.TotalAccountAmount),
                    TotalProfitAmount = balances.Sum(balance => balance.TotalProfitAmount),
                    ExpenseAmount = balances.Sum(balance => balance.ExpenseAmount) + creditLineBalance.ExpenseAmount,
                    PrepaymentBalance = balances.Sum(balance => balance.PrepaymentBalance) + creditLineBalance.PrepaymentBalance,
                    CurrentDebt = balances.Sum(balance => balance.CurrentDebt),
                    TotalRepaymentAmount = balances.Sum(balance => balance.TotalRepaymentAmount),
                    TotalRedemptionAmount = balances.Sum(balance => balance.TotalRedemptionAmount),
                    MonthPaySum = balances.Sum(balance => balance.MonthPaySum),
                    NextPaymentDateDateTime = balances.Any() ? balances.Min(balance => balance.NextPaymentDateDateTime) : DateTime.MaxValue,
                    NextPaymentDate = contract.BuyoutDate != null ? contract.BuyoutDate.Value.ToString("yyyy-MM-dd") : (balances.Any() ? 
                        balances.Min(balance => balance.NextPaymentDateDateTime).ToString("yyyy-MM-dd") : null),
                    CreditLineLimit = balances.Sum(balance => balance.CreditLineLimit) + creditLineBalance.CreditLineLimit,
                    AvailableCreditLineLimit = balances.Sum(balance => balance.CreditLineLimit) + creditLineBalance.CreditLineLimit,
                    UsedCreditLineLimit = balances.Sum(balance => balance.TotalAccountAmount),
                    PenyAccountAmount = balances.Sum(balance => balance.PenyAccountAmount),
                    PenyProfitAmount = balances.Sum(balance => balance.PenyProfitAmount),
                    TotalLoanAmount = balances.Sum(balance => balance.TotalLoanAmount),
                    InitialCreditLineLimit = contract.LoanCost,
                    DebtCost = balances.Sum(balance => balance.DebtCost),
                    BuyOutDate = contract.BuyoutDate?.ToString("yyyy-MM-dd"),
                    DaysOverdue = balances.Any() ? balances.Max(balance => balance.DaysOverdue) : 0
                };
            }

            if (contract.ContractClass == ContractClass.Credit || contract.ContractClass == ContractClass.Tranche)
            {
                return await GetTrancheOrCreditBalance(contract);
            }

            throw new NotImplementedException("Contract class is not Credit, Tranche or CreditLine");
        }

        private async Task<ContractBalanceOnlineView> GetTrancheOrCreditBalance(Contract contract)
        {
            DateTime nextPaymentDate = contract.NextPaymentDate ?? DateTime.Now;
            var schedule = await _contractPaymentScheduleService.GetNextPaymentSchedule(contract.Id, true);
            decimal monthPaySum = 0;

            var balance = await _contractService.GetBalance(contract.Id);
            if (schedule != null)
            {
                if (schedule.Date > DateTime.Today)
                {
                    monthPaySum += balance.CurrentDebt;
                }

                if (schedule.Date > DateTime.Today && schedule.Date <= new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1).AddMonths(1).AddDays(-1))
                {
                    monthPaySum += schedule.DebtCost + schedule.PercentCost;
                }
            }

            int daysOverdue = 0;
            if (contract.NextPaymentDate.HasValue)
            {
                if (contract.NextPaymentDate < DateTime.Today)
                {
                    daysOverdue = (DateTime.Today - contract.NextPaymentDate.Value).Days;
                }
            }

            return new ContractBalanceOnlineView
            {
                AccountAmount = balance?.AccountAmount ?? 0,
                ContractId = contract.Id,
                CurrentDebt = balance?.CurrentDebt ?? 0,
                ExpenseAmount = balance?.ExpenseAmount ?? 0,
                OverdueAccountAmount = balance?.OverdueAccountAmount ?? 0,
                OverdueProfitAmount = balance?.OverdueProfitAmount ?? 0,
                PenyAmount = balance?.PenyAmount ?? 0,
                PrepaymentBalance = balance?.PrepaymentBalance ?? 0,
                ProfitAmount = balance?.ProfitAmount ?? 0,
                TotalAccountAmount = balance?.TotalAcountAmount ?? 0,
                TotalProfitAmount = balance?.TotalProfitAmount ?? 0,
                TotalRepaymentAmount = balance?.TotalRepaymentAmount ?? 0,
                TotalRedemptionAmount = balance?.TotalRedemptionAmount ?? 0,
                MonthPaySum = monthPaySum,
                NextPaymentDateDateTime = nextPaymentDate,
                NextPaymentDate = contract.BuyoutDate != null ? null : nextPaymentDate.ToString("yyyy-MM-dd"),
                PenyAccountAmount = balance?.PenyAccount ?? 0,
                PenyProfitAmount = balance?.PenyProfit ?? 0,
                TotalLoanAmount = contract.LoanCost,
                InitialCreditLineLimit = contract.LoanCost,
                DebtCost = balance == null ? 0 : (balance.AccountAmount  +
                balance.ProfitAmount +
                balance.OverdueAccountAmount +
                balance.OverdueProfitAmount +
                balance.PenyAccount +
                balance.PenyProfit),
                BuyOutDate = contract.BuyoutDate?.ToString("yyyy-MM-dd"),
                DaysOverdue = daysOverdue

            };
        }

    }
}
