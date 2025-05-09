using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;

// удалить после успешного маппинга
namespace Pawnshop.Services.Auction.Mapping.Interfaces
{
    public interface IAuctionMappingService
    {
        Task<bool> HandleCarFile(IFormFile file);
        Task<bool> HandleExpenseFile(IFormFile file);
    }
}
