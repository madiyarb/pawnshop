using System.Threading.Tasks;
using Pawnshop.Data.Models.Auction;

namespace Pawnshop.Services.Auction.Interfaces
{
    public interface IRegisterAuctionExpenseService
    {
        Task RegisterAsync(RegisterAuctionExpenseRequest command);
    }
}