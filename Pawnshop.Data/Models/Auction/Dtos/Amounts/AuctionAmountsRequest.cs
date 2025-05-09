namespace Pawnshop.Data.Models.Auction.Dtos.Amounts
{
    public class AuctionAmountsRequest
    {
        public int ContractId { get; set; }
        public decimal? InputAmount { get; set; }
    }
}