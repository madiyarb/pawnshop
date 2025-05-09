using System;

namespace Pawnshop.Web.Models.CreditLine
{
    public sealed class CreditLinePaymentBinding
    {
        public int PayTypeId { get; set; }
        public decimal Amount { get; set; }
        public DateTime? Date { get; set; }
    }
}
