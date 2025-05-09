using System.Threading.Tasks;
using Pawnshop.Data.Models.Auction.HttpRequestModels;
using Pawnshop.Data.Models.Contracts.Actions;

namespace Pawnshop.Services.Auction.Interfaces
{
    public interface IContractBuyOutByAuctionService
    {
        Task<ContractAction> BuyOut(ContractBuyOutByAuctionCommand command);
    }
}