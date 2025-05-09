using System.Threading.Tasks;
using Pawnshop.Data.Models;

namespace Pawnshop.Services.Auction.Interfaces
{
    public interface ICarAuctionService
    {
        public Task<int> CreateAsync(CarAuction carAuction);
        public Task<CarAuction> GetByContractIdAsync(int contractId);
    }
}