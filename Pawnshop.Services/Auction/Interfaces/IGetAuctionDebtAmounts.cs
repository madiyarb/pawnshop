using System.Threading.Tasks;

namespace Pawnshop.Services.Auction.Interfaces
{
    public interface IGetAuctionDebtAmounts
    {
        Task<decimal> GetDebtAmount(int contractId);
    }
}