using System.Threading.Tasks;
using Pawnshop.Data.Models.CashOrders;

namespace Pawnshop.Services.Auction.Interfaces
{
    public interface ICancelAuctionOperationService
    {
        Task CancelAsync(CashOrder cashOrder);
    }
}