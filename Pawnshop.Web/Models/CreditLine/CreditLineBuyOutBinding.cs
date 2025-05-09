using System;
using System.Collections.Generic;

namespace Pawnshop.Web.Models.CreditLine
{
    public sealed class CreditLineBuyOutBinding
    {
        public int PayTypeId { get; set; }
        public List<int> BuyOutContracts { get; set; }
        public bool BuyOutCreditLine { get; set; }
        public int BuyoutReasonId { get; set; }
        public int? ExpenseId { get; set; }
        public DateTime? Date { get; set; }
    }
}
