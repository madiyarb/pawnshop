using Pawnshop.Data.Models.Contracts;
using System;
using Pawnshop.AccountingCore.Models;

namespace Pawnshop.Web.Models.Reports.ConsolidateProfitReport
{
    public class ConsolidateProfitReportQueryModel
    {
        public DateTime BeginDate { get; set; }

        public DateTime EndDate { get; set; }

        public CollateralType? CollateralType { get; set; }
    }
}
