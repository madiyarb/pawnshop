using Pawnshop.Data.Models.Auction.Dtos.Car;
using Pawnshop.Data.Models.Auction.Dtos.CarAuction;

namespace Pawnshop.Data.Models.Auction.HttpRequestModels
{
    public class CreateCarAuctionRequest
    {
        public string? AuthorId { get; set; }
        public string? AuthorName { get; set; }
        public CreateAuctionCarDto Car { get; set; }
        public AuctionRequest Auction { get; set; }
    }
}