using System;

namespace Pawnshop.Data.Models.Auction.HttpRequestModels
{
    public class ApproveAuctionCommand
    {
        public Guid RequestId { get; set; }
        public string AuthorName { get; set; }
    }
}