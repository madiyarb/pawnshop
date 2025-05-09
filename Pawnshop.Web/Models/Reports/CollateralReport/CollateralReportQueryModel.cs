using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Pawnshop.Web.Models.Reports.CollateralReport  
{
    public class CollateralReportQueryModel
    {
        public DateTime ReportDate { get; set; }
        public List<int> BranchIds { get; set; }

    }
}
