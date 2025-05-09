using Pawnshop.Data.Models.Auction.Mapping;
using System.Threading.Tasks;

// удалить после успешного маппинга
namespace Pawnshop.Data.Access.Auction.Mapping.Interfaces
{
    public interface IAuctionMappingRepository
    {
        public Task<AuctionCarDetails> GetCarDetailsByContractIdAsync(int contractId);
        public Task<int> GetBranchIdForExpenseByContractIdAsync(int contractId);
    }
}
