namespace Pawnshop.Services.CardCashOut
{
    public sealed class CardMerchantOptions
    {
        public string MerchantId { get; set; } = null!;
        public string MerchantKeyword { get; set; } = null!;
        public string UserId { get; set; } = null!;
        public string CardId { get; set; } = null!;
        public string UserLogin { get; set; } = null!;
        public string TerminalId { get; set; } = null!;
    }
}
