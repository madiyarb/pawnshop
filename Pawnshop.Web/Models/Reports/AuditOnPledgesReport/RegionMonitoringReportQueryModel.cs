using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Pawnshop.Web.Models.AuditOnPledgesReport
{
    public class AuditOnPledgesReportQueryModel
    {
        public DateTime BeginDate { get; set; }

        public List<int> BranchIds { get; set; }
    }
}
