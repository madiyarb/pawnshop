using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Pawnshop.Web.Models.Reports.AttractionChannelReport
{
    public class AttractionChannelReportQueryModel
    {
        public List<int> BranchIds { get; set; }

        public DateTime BeginDate { get; set; }

        public DateTime EndDate { get; set; }

        public int CollateralType { get; set; }
    }
}
