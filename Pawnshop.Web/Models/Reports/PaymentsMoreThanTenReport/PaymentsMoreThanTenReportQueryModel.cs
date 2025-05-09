using System;
using System.Collections.Generic;

namespace Pawnshop.Web.Models.Reports.PaymentsMoreThanTenReport
{
    public class PaymentsMoreThanTenReportQueryModel
    {
        public DateTime BeginDate { get; set; }
        public DateTime EndDate { get; set; }

        public List<int> BranchIds { get; set; }
    }
}