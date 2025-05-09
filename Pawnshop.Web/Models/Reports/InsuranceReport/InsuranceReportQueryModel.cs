using System;
using Pawnshop.Data.Models.Insurances;

namespace Pawnshop.Web.Models.Reports.InsuranceReport
{
    public class InsuranceReportQueryModel
    {
        public DateTime BeginDate { get; set; }

        public DateTime EndDate { get; set; }

        public int BranchId { get; set; }

        public InsuranceStatus? Status { get; set; }
    }
}
