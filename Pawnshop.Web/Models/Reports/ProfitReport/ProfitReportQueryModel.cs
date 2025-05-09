using System;
using Pawnshop.Data.Models.Contracts;
using System.Collections.Generic;
using Pawnshop.AccountingCore.Models;

namespace Pawnshop.Web.Models.Reports.ProfitReport
{
    public class ProfitReportQueryModel
    {
        public DateTime BeginDate { get; set; }

        public DateTime EndDate { get; set; }

        public List<int> BranchIds { get; set; }

        public CollateralType? CollateralType { get; set; }
    }
}
