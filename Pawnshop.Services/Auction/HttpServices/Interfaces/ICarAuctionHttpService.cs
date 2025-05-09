using System.Threading.Tasks;
using Pawnshop.Data.Models.Auction.HttpRequestModels;
using Pawnshop.Data.Models.Auction.HttpResponseModel;

namespace Pawnshop.Services.Auction.HttpServices.Interfaces
{
    public interface ICarAuctionHttpService
    {
        Task<CreatedAuctionDto> CreateCarAuction(CreateCarAuctionRequest request);
        Task RollbackCreated(int auctionId);
    }
}