using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Pawnshop.Web.Models.Reports.CollectionReport
{
    public class CollectionReportQueryModel
    {
        public List<int> BranchIds { get; set; }

        public string OverdueStatus { get; set; }

        public int CollateralType { get; set; }

    }
}
