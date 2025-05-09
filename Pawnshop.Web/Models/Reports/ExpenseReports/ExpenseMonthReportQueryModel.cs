using System;

namespace Pawnshop.Web.Models.Reports.ExpenseReports
{
    public class ExpenseMonthReportQueryModel
    {
        public int Month { get; set; }

        public int Year { get; set; }

        public int BranchId { get; set; }
    }
}