namespace Pawnshop.Services.Auction.HttpServices
{
    public class CarAuctionSettings
    {
        public string BaseUrl { get; set; }
        public int HttpTimeoutSeconds { get; set; } = 5;
    }
}