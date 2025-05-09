using System.Collections.Generic;
using System.Threading.Tasks;
using Pawnshop.Data.Models.AccountingCore;
using Pawnshop.Data.Models.Contracts;

namespace Pawnshop.Services.Auction.Interfaces
{
    public interface IGetAuctionAccountsService
    {
        Task<IEnumerable<Account>> GetAccounts(Contract contract, List<string> accountsCodes);
        Task<IEnumerable<Account>> GetPrePaymentAccounts(int contractId, Contract? contract = null);
    }
}