namespace Pawnshop.Services.Auction
{
    public class CalculatedAuctionSum
    {
        /// <summary>
        /// сумма к оплате
        /// </summary>
        public decimal ToPayOff { get; set; }
        
        /// <summary>
        /// сумма к списанию
        /// </summary>
        public decimal ToWithdraw { get; set; }
    }
}