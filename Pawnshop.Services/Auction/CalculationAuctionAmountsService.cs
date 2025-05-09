using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Pawnshop.Data.Access;
using Pawnshop.Data.Models.AccountingCore;
using Pawnshop.Data.Models.Auction.Dtos.Amounts;
using Pawnshop.Data.Models.Contracts;
using Pawnshop.Data.Models.Contracts.Expenses;
using Pawnshop.Services.Auction.Interfaces;
using Pawnshop.Services.Contracts;

namespace Pawnshop.Services.Auction
{
    public class CalculationAuctionAmountsService : ICalculationAuctionAmountsService
    {
        private const string Account = "ACCOUNT"; // Основной долг:
        private const string OverdueAccount = "OVERDUE_ACCOUNT"; // Просроченный основной долг:
        private const string Depo = "DEPO"; // Авансовый счет
        private const string Profit = "PROFIT"; // Проценты начисленные: 
        private const string OverdueProfit = "OVERDUE_PROFIT"; // Проценты просроченные:
        private const string PenyAccount = "PENY_ACCOUNT"; // Пеня на долг просроченный: 
        private const string PenyProfit = "PENY_PROFIT"; // Пеня на проценты просроченные
        const string ProfitOffbalance = "PROFIT_OFFBALANCE"; // Начисленные проценты на внебалансе
        const string OverdueProfitOffbalance = "OVERDUE_PROFIT_OFFBALANCE"; // Просроченные проценты на внебалансе
        const string PenyAccountOffBalance = "PENY_ACCOUNT_OFFBALANCE"; // Пеня на просроченный основной долг на внебалансе
        const string PenyProfitOffBalance = "PENY_PROFIT_OFFBALANCE"; // Пеня на просроченные проценты на внебалансе
        
        private readonly IContractService _contractService;
        private readonly AccountRepository _accountRepository;
        private readonly IGetAuctionAccountsService _auctionAccountsService;

        public CalculationAuctionAmountsService(
            IContractService contractService,
            AccountRepository accountRepository,
            IGetAuctionAccountsService auctionAccountsService)
        {
            _contractService = contractService;
            _accountRepository = accountRepository;
            _auctionAccountsService = auctionAccountsService;
        }

        public async Task<AuctionAmountsCompositeViewModel> GetCalculatedAmounts(AuctionAmountsRequest request)
        {
            Contract foundContract = await _contractService.GetOnlyContractAsync(request.ContractId);
            if (foundContract is null)
            {
                return null;
            }

            var accounts = await _auctionAccountsService.GetAccounts(foundContract, new List<string>
            {
                Account,
                Profit,
                OverdueProfit,
                OverdueAccount,
                PenyAccount,
                PenyProfit,

                ProfitOffbalance,
                OverdueProfitOffbalance,
                PenyAccountOffBalance,
                PenyProfitOffBalance
            });

            return await GetAmounts(request, foundContract, accounts);
        }

        private async Task<AuctionAmountsCompositeViewModel> GetAmounts(AuctionAmountsRequest request, Contract contract,
            IEnumerable<Account> accounts)
        {
            var prePayment = await GetPrePayment(contract);
            var accountAmount = GetAccountsSum(accounts.Where(a => a.Code == Account));
            var profit = GetAccountsSum(accounts.Where(a => a.Code == Profit));
            var overdueProfit = GetAccountsSum(accounts.Where(a => a.Code == OverdueProfit));
            var overdueAccount = GetAccountsSum(accounts.Where(a => a.Code == OverdueAccount));
            var pennyAccount = GetAccountsSum(accounts.Where(a => a.Code == PenyAccount));
            var pennyProfit = GetAccountsSum(accounts.Where(a => a.Code == PenyProfit));
            
            // суммы на внебалансовых счетах
            var profitOffBalance = GetAccountsSum(accounts.Where(a => a.Code == ProfitOffbalance));
            var overdueProfitOffBalance = GetAccountsSum(accounts.Where(a => a.Code == OverdueProfitOffbalance));
            var pennyAccountOffBalance = GetAccountsSum(accounts.Where(a => a.Code == PenyAccountOffBalance));
            var pennyProfitOffBalance = GetAccountsSum(accounts.Where(a => a.Code == PenyProfitOffBalance));

            var availableAmount = prePayment + (decimal)request.InputAmount;
            
            var auctionAmountsForPay = new AuctionAmountsForPayViewModel();
            var auctionAmountsToWithdraw = new AuctionAmountsToWithdrawViewModel();

            // Расчёт именно в такой последовательности !!!
            var unpaidExpensesWithdrawsResult = CalculateWithdraws(ref availableAmount, 0);
            auctionAmountsForPay.UnpaidExpensesForPay = unpaidExpensesWithdrawsResult.ToPayOff;
            auctionAmountsToWithdraw.UnpaidExpensesToWithdraw = unpaidExpensesWithdrawsResult.ToWithdraw;

            var overdueAccountWithdrawResult = CalculateWithdraws(ref availableAmount, overdueAccount);
            auctionAmountsForPay.OverdueAccountForPay = overdueAccountWithdrawResult.ToPayOff;
            auctionAmountsToWithdraw.OverdueAccountToWithdraw = overdueAccountWithdrawResult.ToWithdraw;

            var overdueAccountAmountWithdrawResult = CalculateWithdraws(ref availableAmount, accountAmount);
            auctionAmountsForPay.AccountAmountForPay = overdueAccountAmountWithdrawResult.ToPayOff;
            auctionAmountsToWithdraw.AccountAmountToWithdraw = overdueAccountAmountWithdrawResult.ToWithdraw;

            var overdueProfitWithdrawResult = CalculateWithdraws(ref availableAmount, overdueProfit);
            auctionAmountsForPay.OverdueProfitForPay = overdueProfitWithdrawResult.ToPayOff;
            auctionAmountsToWithdraw.OverdueProfitToWithdraw = overdueProfitWithdrawResult.ToWithdraw;

            var profitWithdrawResult = CalculateWithdraws(ref availableAmount, profit);
            auctionAmountsForPay.ProfitForPay = profitWithdrawResult.ToPayOff;
            auctionAmountsToWithdraw.ProfitToWithdraw = profitWithdrawResult.ToWithdraw;

            var pennyWithdrawResult = CalculateWithdraws(ref availableAmount, pennyAccount);
            auctionAmountsForPay.PenyAccountForPay = pennyWithdrawResult.ToPayOff;
            auctionAmountsToWithdraw.PenyAccountToWithdraw = pennyWithdrawResult.ToWithdraw;

            var pennyProfitWithdrawResult = CalculateWithdraws(ref availableAmount, pennyProfit);
            auctionAmountsForPay.PenyProfitForPay = pennyProfitWithdrawResult.ToPayOff;
            auctionAmountsToWithdraw.PenyProfitToWithdraw = pennyProfitWithdrawResult.ToWithdraw;
            
            // Общая сумма на балансовых счетах + неоплаченные расходы
            var totalAmount = 
                accountAmount
                + overdueAccount
                + profit
                + overdueProfit
                + pennyAccount
                + pennyProfit
                + 0;
            
            // сумма выкупа
            var buyOutSum = request.InputAmount;
            if (buyOutSum > totalAmount)
            {
                buyOutSum = totalAmount;
            }

            // сумма списания с внебалансовых счетов
            var totalAmountToWriteOffFromOffBalanceAccounts =
                profitOffBalance
                + overdueProfitOffBalance
                + pennyAccountOffBalance
                + pennyProfitOffBalance;

            // сумма списания с балансовых счетов
            var totalAmountToWriteOffFromBalanceAccounts =
                overdueAccountWithdrawResult.ToWithdraw
                + overdueAccountAmountWithdrawResult.ToWithdraw
                + overdueProfitWithdrawResult.ToWithdraw
                + profitWithdrawResult.ToWithdraw
                + pennyWithdrawResult.ToWithdraw
                + pennyProfitWithdrawResult.ToWithdraw;

            var returnAmountToBorrower = request.InputAmount + prePayment - totalAmount;
            
            var auctionMainAmounts = new AuctionAmountsMainViewModel
            {
                PrePayment = prePayment,
                AccountAmount = accountAmount,
                Profit = profit,
                OverdueProfit = overdueProfit,
                OverdueAccount = overdueAccount,
                PenyAccount = pennyAccount,
                PenyProfit = pennyProfit,
                UnpaidExpenses = 0,
                TotalAmount = totalAmount,
                BuyOutSum = (decimal)buyOutSum,
                AmountToWriteOffFromOffBalanceAccounts = totalAmountToWriteOffFromOffBalanceAccounts,
                AmountToWriteOffFromBalanceAccounts = totalAmountToWriteOffFromBalanceAccounts,
                ReturnAmountToBorrower = (decimal)returnAmountToBorrower > 0 ? (decimal)returnAmountToBorrower : 0
            };
            
            return new AuctionAmountsCompositeViewModel
            {
                MainInfo = auctionMainAmounts,
                AmountsForPay = auctionAmountsForPay,
                AmountsToWithdraw = auctionAmountsToWithdraw
            };
        }
        
        private async Task<decimal> GetPrePayment(Contract contract)
        {
            var prePaymentAccounts = await _auctionAccountsService.GetPrePaymentAccounts(contract.Id, contract);
            
            return GetAccountsSum(prePaymentAccounts);
        }

        private static decimal GetAccountsSum(IEnumerable<Account> accounts)
        {
            var sum = 0.00m;
            if (accounts.Any())
            {
                sum = accounts.Select(a => a.Balance).Sum();
            }

            if (sum < 0)
            {
                return Math.Abs(sum);
            }

            return sum;
        }

        private CalculatedAuctionSum CalculateWithdraws(ref decimal availableSum, decimal calculatedSum)
        {
            var result = new CalculatedAuctionSum();
            
            if (availableSum <= 0)
            {
                result.ToWithdraw = calculatedSum;
                return result;
            }

            if (availableSum >= calculatedSum)
            {
                result.ToPayOff = calculatedSum;
                availableSum -= calculatedSum;
            }
            else
            {
                result.ToPayOff = availableSum;
                result.ToWithdraw = calculatedSum - availableSum;
                availableSum = 0;
            }

            return result;
        }
    }
}
