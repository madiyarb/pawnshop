using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Pawnshop.AccountingCore.Models;

namespace Pawnshop.Web.Models.Reports.VintageAnalysis  
{
    public class VintageAnalysisQueryModel
    {
        public DateTime BeginDate { get; set; }

        public DateTime EndDate { get; set; }

        public DateTime ContractStartDate { get; set; }

        public DateTime ContractEndDate { get; set; }

        public int BeginDelayCount { get; set; }

        public int EndDelayCount { get; set; }

        public List<int> BranchIds { get; set; }

        public CollateralType CollateralType { get; set; }
    }
}
