namespace Pawnshop.Web.Models.AbsOnlineCardCashOut
{
    public sealed class GetCashOutTransactionStatusView
    {
        public int? Amount { get; set; }
        public string CardIssuerCountry { get; set; }
        public string MaskedCardNumber { get; set; }
        public string MerchantLocalDateTime { get; set; }
        public string MerchantOnlineAddress { get; set; }
        public string ReceiverName { get; set; }
        public int? RspCode { get; set; }
        public string SenderName { get; set; }
        public bool? Success { get; set; }
        public int? TransactionCurrencyCode { get; set; }
        public string TransactionStatus { get; set; }
        public string Verified3D { get; set; }
    }
}
