using System.Data;
using System.Threading.Tasks;
using Pawnshop.Data.Models.Auction;

namespace Pawnshop.Data.Access.Auction.Interfaces
{
    public interface IAuctionContractExpenseRepository
    {
        IDbTransaction BeginTransaction();
        Task InsertAsync(AuctionContractExpense auctionContractExpense);
    }
}