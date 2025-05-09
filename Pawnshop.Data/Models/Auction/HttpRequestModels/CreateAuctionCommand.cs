using Pawnshop.Data.Models.Auction.Dtos.Car;
using Pawnshop.Data.Models.Auction.Dtos.CarAuction;
using Pawnshop.Data.Models.LegalCollection;

namespace Pawnshop.Data.Models.Auction.HttpRequestModels
{
    public class CreateAuctionCommand
    {
        public UpdateLegalCaseCommand? UpdateLegalCaseCommand { get; set; }
        public CreateAuctionCarDto Car { get; set; }
        public CreateAuctionDto Auction { get; set; }
    }
}