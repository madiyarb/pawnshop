using System;

namespace Pawnshop.Data.Models.Contracts
{
    public class PaymentAmount
    {
        public decimal Amount { get; set; }
        public string Desc { get; set; }
        public DateTime? NextPaymentDate { get; set; }
    }
}
