using System.Collections.Generic;
using System.Threading.Tasks;
using Pawnshop.Data.Models.Auction.HttpRequestModels;
using Pawnshop.Data.Models.LegalCollection.Details;

namespace Pawnshop.Services.Auction.Interfaces
{
    public interface ICreateAuctionService
    {
        Task<List<LegalCasesDetailsViewModel>> Create(CreateAuctionCommand command);
    }
}