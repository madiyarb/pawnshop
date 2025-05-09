namespace Pawnshop.Services.CreditLines.PartialPayment
{
    public sealed class CreditLineTransferPartialPayment : CreditLineTransfer
    {
        public decimal DebtAfterPayment { get; set; }
        public decimal PaymentAmount { get; set; }
    }
}
