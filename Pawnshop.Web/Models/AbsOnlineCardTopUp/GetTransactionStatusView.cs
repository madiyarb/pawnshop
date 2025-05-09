namespace Pawnshop.Web.Models.AbsOnlineCardTopUp
{
    public sealed class GetTransactionStatusView
    {
        public string AmountAuthorised { get; set; }
        public string AmountRefunded { get; set; }
        public string AmountRequested { get; set; }
        public string AmountSettled { get; set; }
        public string IssuerBank { get; set; }
        public string MerchantLocalDateTime { get; set; }
        public string MerchantOnlineAddress { get; set; }
        public string OrderId { get; set; }
        public string PurchaserEmail { get; set; }
        public string PurchaserName { get; set; }
        public string PurchaserPhone { get; set; }
        public string RspCode { get; set; }
        public string TransactionStatus { get; set; }
    }
}
