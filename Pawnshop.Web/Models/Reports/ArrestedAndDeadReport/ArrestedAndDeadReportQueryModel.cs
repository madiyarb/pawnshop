using System;
using Pawnshop.Data.Models.Contracts;
using System.Collections.Generic;
using Pawnshop.AccountingCore.Models;

namespace Pawnshop.Web.Models.Reports.DelayReport
{
    public class ArrestedAndDeadReportQueryModel
    {
        public DateTime EndDelayCount { get; set; }

        public List<int> BranchIds { get; set; }

        public CollateralType CollateralType { get; set; }
    }
}