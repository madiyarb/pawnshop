using System;

namespace Pawnshop.Web.Models.Reports.AccountMfoReport
{
    public class AccountMfoReportQueryModel
    {
        public DateTime BeginDate { get; set; }

        public DateTime EndDate { get; set; }

        public int PoolNumber { get; set; }
    }
}