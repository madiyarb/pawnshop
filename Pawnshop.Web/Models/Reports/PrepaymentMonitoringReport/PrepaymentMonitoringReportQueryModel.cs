using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Pawnshop.Web.Models.Reports.PrepaymentMonitoringReport
{
    public class PrepaymentMonitoringReportQueryModel
    {
        public DateTime BeginDate { get; set; }
        public DateTime EndDate { get; set; }

        public List<int> BranchIds { get; set; }
    }
}
