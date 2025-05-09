using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Pawnshop.Web.Models.Reports.AccountMonitoringReport
{
    public class AccountMonitoringReportQueryModel
    {
        public DateTime BeginDate { get; set; }
        public DateTime EndDate { get; set; }

        public List<int> BranchIds { get; set; }

        public int AccountType { get; set; }
        public int AccountPlanId { get; set; }
    }
}
