using System;

namespace Pawnshop.Data.Access.Auction
{
    public struct AuctionCashOrderDto
    {
        public Guid AuctionRequestId { get; set; }
        public int CashOrderId { get; set; }
    }
}