using System.Threading.Tasks;
using Pawnshop.Data.Access.Interfaces;
using Pawnshop.Data.Models;
using Pawnshop.Services.Auction.Interfaces;

namespace Pawnshop.Services.Auction
{
    public class CarAuctionService : ICarAuctionService
    {
        private readonly IAuctionRepository _auctionRepository;

        public CarAuctionService(IAuctionRepository auctionRepository)
        {
            _auctionRepository = auctionRepository;
        }

        public async Task<int> CreateAsync(CarAuction auction)
        {
            var createdAuction = await _auctionRepository.CreateAsync(auction);
            return createdAuction.Id;
        }

        public async Task<CarAuction> GetByContractIdAsync(int contractId)
        {
            return await _auctionRepository.GetByContractIdAsync(contractId);
        }
    }
}