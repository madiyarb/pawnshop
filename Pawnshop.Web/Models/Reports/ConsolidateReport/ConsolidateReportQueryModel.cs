using System;

namespace Pawnshop.Web.Models.Reports.ConsolidateReport
{
    public class ConsolidateReportQueryModel
    {
        public DateTime BeginDate { get; set; }

        public DateTime EndDate { get; set; }

        public int BranchId { get; set; }
        
        public bool? IsPeriod { get; set; }
    }
}