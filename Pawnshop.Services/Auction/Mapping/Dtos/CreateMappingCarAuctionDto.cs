using System;

namespace Pawnshop.Services.Auction.Mapping.Dtos
{
    public class CreateMappingCarAuctionDto
    {
        public int? AuctionId { get; set; }
        public Guid? OrderRequestId { get; set; }
    }
}