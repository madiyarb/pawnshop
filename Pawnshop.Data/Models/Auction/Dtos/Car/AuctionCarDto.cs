using Pawnshop.Data.Models.Auction.Dtos.Client;
using Pawnshop.Data.Models.Auction.Dtos.Contract;

namespace Pawnshop.Data.Models.Auction.Dtos.Car
{
    public class AuctionCarDto
    {
        public int Id { get; set; }
        public string TransportNumber { get; set; }
        public string BodyNumber { get; set; }
        public string Mark { get; set; }
        public string Model { get; set; }
        public string Color { get; set; }
        public int ReleaseYear { get; set; }
        public AuctionClientDto? Client { get; set; }
        public AuctionContractDto? Contract { get; set; }
    }
}