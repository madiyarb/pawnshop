using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Pawnshop.Data.Models.Contracts;
using Pawnshop.Services.Auction.Interfaces;
using Pawnshop.Services.Contracts;

namespace Pawnshop.Services.Auction
{
    public class GetAuctionDebtAmounts : IGetAuctionDebtAmounts
    {
        private readonly IGetAuctionAccountsService _auctionAccountsService;
        private readonly IContractService _contractService;
        
        private const string OverdueAccount = "OVERDUE_ACCOUNT";
        private const string Account = "ACCOUNT";
        private const string Depo = "DEPO";

        public GetAuctionDebtAmounts(IGetAuctionAccountsService auctionAccountsService, IContractService contractService)
        {
            _auctionAccountsService = auctionAccountsService;
            _contractService = contractService;
        }

        public async Task<decimal> GetDebtAmount(int contractId)
        {
            Contract foundContract = await _contractService.GetOnlyContractAsync(contractId);
            if (foundContract is null)
            {
                throw new InvalidOperationException("Договор не найден");
            }

            var accounts = await _auctionAccountsService
                    .GetAccounts(foundContract, new List<string> { Account, OverdueAccount });

            var depoAccounts = await _auctionAccountsService.GetPrePaymentAccounts(foundContract.Id, foundContract);
            
            var accountsAmount = Math.Abs(accounts?.Select(a => a.Balance).Sum() ?? 0);
            var depoAmount = Math.Abs(depoAccounts?.Select(a => a.Balance).Sum() ?? 0);

            return accountsAmount - depoAmount;
        }
    }
}