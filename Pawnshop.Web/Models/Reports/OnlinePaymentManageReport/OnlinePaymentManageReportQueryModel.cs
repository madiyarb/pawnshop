using System;

namespace Pawnshop.Web.Models.Reports.OnlinePaymentManageReport
{
    public class OnlinePaymentManageReportQueryModel
    {
        public DateTime BeginDate { get; set; }
        public DateTime EndDate { get; set; }

        public int Status { get; set; }
        public int ProcessingType { get; set; }
    }
}