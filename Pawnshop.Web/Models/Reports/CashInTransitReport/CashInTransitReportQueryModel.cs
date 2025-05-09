using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Pawnshop.Web.Models.Reports.CashInTransitReport
{
    public class CashInTransitReportQueryModel : ReportQueryBase
    {
        public int OrderType { get; set; }
    }
}
