using System.Threading.Tasks;
using Pawnshop.Data.Models.Auction.Dtos.Mapping;
using Pawnshop.Data.Models.Auction.HttpResponseModel;
using Pawnshop.Services.Auction.Mapping.Dtos;

// удалить после успешного маппинга
namespace Pawnshop.Services.Auction.Mapping.HttpServices.Interfaces
{
    public interface IAuctionMappingHttpService
    {
        Task<CreateMappingCarAuctionDto> SendCarDataAsync(AuctionMappingCarApiDto request);
        Task<bool> SendExpenseDataAsync(AuctionMappingExpenseApiDto request);
    }
}
