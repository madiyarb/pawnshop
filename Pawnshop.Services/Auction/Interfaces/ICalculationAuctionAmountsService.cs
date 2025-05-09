using System.Threading.Tasks;
using Pawnshop.Data.Models.Auction.Dtos.Amounts;

namespace Pawnshop.Services.Auction.Interfaces
{
    public interface ICalculationAuctionAmountsService
    {
        Task<AuctionAmountsCompositeViewModel> GetCalculatedAmounts(AuctionAmountsRequest request);
    }
}