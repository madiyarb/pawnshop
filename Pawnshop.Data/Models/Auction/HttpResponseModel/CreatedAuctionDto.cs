using System;

namespace Pawnshop.Data.Models.Auction.HttpResponseModel
{
    public class CreatedAuctionDto
    {
        public int AuctionId { get; set; }
        public Guid OrderRequestExternalId { get; set; }
    }
}