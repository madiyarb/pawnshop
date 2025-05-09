namespace Pawnshop.Data.Models.Auction.Dtos.Mapping
{
    public class AuctionMappingCarApiDto
    {
        public int? Iterator { get; set; }
        public bool IsFinal { get; set; }
        public string? AuthorId { get; set; }
        public string AuthorName { get; set; }
        public CreateCarModel Car { get; set; }
        public CreateAuctionModel? Auction { get; set; }
    }
}
