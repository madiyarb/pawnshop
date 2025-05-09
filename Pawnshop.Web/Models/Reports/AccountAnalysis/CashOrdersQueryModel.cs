using System;

namespace Pawnshop.Web.Models.Reports.AccountAnalysis
{
    public class CashOrdersQueryModel
    {
        public int Month { get; set; }

        public int Year { get; set; }

        public int BranchId { get; set; }

        public int? DebitAccountId { get; set; }

        public int? CreditAccountId { get; set; }
    }
}