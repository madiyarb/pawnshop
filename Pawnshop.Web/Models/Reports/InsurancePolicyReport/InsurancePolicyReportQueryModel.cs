using System;
using System.Collections.Generic;

namespace Pawnshop.Web.Models.Reports.InsurancePolicyReport
{
    public class InsurancePolicyReportQueryModel
    {
        public DateTime BeginDate { get; set; }
        public DateTime EndDate { get; set; }

        public List<int> BranchIds { get; set; }
    }
}