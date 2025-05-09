using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Pawnshop.Web.Models.Reports.ConsolidateIssuanceReport
{
    public class ConsolidateIssuanceQueryModel
    {
        public List<int> BranchIds { get; set; }

        public DateTime BeginDate { get; set; }

        public DateTime EndDate { get; set; }
    }
}
