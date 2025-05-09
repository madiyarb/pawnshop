namespace Pawnshop.Data.Models.Auction.Dtos.Amounts
{
    public class AuctionAmountsCompositeViewModel
    {
        public AuctionAmountsMainViewModel MainInfo { get; set; } = new AuctionAmountsMainViewModel();
        public AuctionAmountsForPayViewModel AmountsForPay { get; set; } = new AuctionAmountsForPayViewModel();
        public AuctionAmountsToWithdrawViewModel AmountsToWithdraw { get; set; } = new AuctionAmountsToWithdrawViewModel();
    }
}