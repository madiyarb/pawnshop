using System;

namespace Pawnshop.Web.Models.CreditLine
{
    public sealed class CreditLinePartialPaymentBinding
    {
        public int PartialPaymentContractId { get; set; }
        public int PayTypeId { get; set; }
        public decimal Amount { get; set; }
        public DateTime? Date { get; set; } = DateTime.Now;
        public bool? CategoryChanged { get; set; } = false;
    }
}
