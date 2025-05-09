using System;

namespace Pawnshop.Web.Models.Reports.SfkTransferedReport
{
    public class SfkTransferedReportQueryModel
    {
        public DateTime EndDate { get; set; }

        public int PoolNumber { get; set; }

        public int ContractStatus { get; set; }
    }
}
