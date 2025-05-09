namespace Pawnshop.Data.Models.Auction.HttpRequestModels
{
    public class ContractBuyOutByAuctionCommand
    {
        public int ContractId { get; set; }
        public string PayTypeCode { get; set; }
        public string BuyOutReasonCode { get; set; }
        public string ExpenseCode { get; set; }
    }
}