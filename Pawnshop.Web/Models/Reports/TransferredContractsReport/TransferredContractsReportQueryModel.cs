using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Pawnshop.Web.Models.Reports.TransferredContractsReport  
{
    public class TransferredContractsReportQueryModel
    {
        public DateTime ReportDate { get; set; }
        public List<int> BranchIds { get; set; }

    }
}
