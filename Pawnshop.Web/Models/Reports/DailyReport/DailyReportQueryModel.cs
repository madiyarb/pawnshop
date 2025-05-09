using System;

namespace Pawnshop.Web.Models.Reports.DailyReport
{
    public class DailyReportQueryModel
    {
        public DateTime CurrentDate { get; set; }

        public int BranchId { get; set; }
    }
}