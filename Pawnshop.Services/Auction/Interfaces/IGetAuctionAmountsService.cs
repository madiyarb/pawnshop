using System.Threading.Tasks;
using Pawnshop.Data.Models.Auction.Dtos.Amounts;

namespace Pawnshop.Services.Auction.Interfaces
{
    public interface IGetAuctionAmountsService
    {
        Task<AuctionAmountsCompositeViewModel> GetAmounts(int contractId);
    }
}