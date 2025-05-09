using System.Threading.Tasks;

namespace Pawnshop.Services.Auction.Interfaces
{
    public interface ICreditLineBuyOutByAuctionService
    {
        Task BuyoutByAuctionAsync(int creditLineId);
    }
}