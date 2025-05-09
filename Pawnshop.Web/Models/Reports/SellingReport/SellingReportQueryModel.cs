using System;
using Pawnshop.Data.Models.Sellings;
using System.Collections.Generic;

namespace Pawnshop.Web.Models.Reports.SellingReport
{
    public class SellingReportQueryModel
    {
        public DateTime BeginDate { get; set; }

        public DateTime EndDate { get; set; }

        public List<int> BranchIds { get; set; }

        public SellingStatus? Status { get; set; }
    }
}