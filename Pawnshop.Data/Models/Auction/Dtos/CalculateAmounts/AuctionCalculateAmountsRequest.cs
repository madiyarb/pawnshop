namespace Pawnshop.Data.Models.Auction.Dtos.CalculateAmounts
{
    public class AuctionCalculateAmountsRequest
    {
        public int ContractId { get; set; }
        public decimal? CalculateSum { get; set; }
    }
}