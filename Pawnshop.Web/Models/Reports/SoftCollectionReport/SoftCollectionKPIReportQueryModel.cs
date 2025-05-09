using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Pawnshop.Web.Models.Reports.SoftCollectionReport
{
    public class SoftCollectionKPIReportQueryModel
    {
        public List<int> BranchIds { get; set; }

        public int Month { get; set; }

        public int Year { get; set; }

        public bool FromBegining { get; set; }
    }
}
