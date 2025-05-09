using System;
using System.Collections.Generic;
using Pawnshop.Data.Models.Insurances;

namespace Pawnshop.Web.Models.Reports.InsuranceReport
{
    public class InsuranceReportModel
    {
        public DateTime BeginDate { get; set; }

        public DateTime EndDate { get; set; }

        public string BranchName { get; set; }

        public InsuranceStatus? Status { get; set; }

        public List<dynamic> List { get; set; }
    }
}
