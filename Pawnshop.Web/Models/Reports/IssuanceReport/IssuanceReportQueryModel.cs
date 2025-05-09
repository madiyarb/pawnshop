using System;

namespace Pawnshop.Web.Models.Reports.IssuanceReport
{
    public class IssuanceReportQueryModel
    {
        public DateTime BeginDate { get; set; }

        public DateTime EndDate { get; set; }
        public int BranchId { get; set; }
    }
}
