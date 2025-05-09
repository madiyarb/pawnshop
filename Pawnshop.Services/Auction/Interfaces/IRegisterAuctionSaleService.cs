using System.Threading.Tasks;
using Pawnshop.Data.Models.Auction;

namespace Pawnshop.Services.Auction.Interfaces
{
    public interface IRegisterAuctionSaleService
    {
        Task RegisterAsync(RegisterAuctionSaleRequest request);
    }
}