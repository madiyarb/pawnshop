using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Pawnshop.Web.Models.Reports.BuyoutContractsWithInscriptionReport
{
    public class BuyoutContractsWithInscriptionReportQueryModel : ReportQueryBase
    {
        public List<int> BranchIds { get; set; }
    }
}
