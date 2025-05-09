using System;
using Pawnshop.Data.Models.Contracts;
using System.Collections.Generic;
using Pawnshop.AccountingCore.Models;

namespace Pawnshop.Web.Models.Reports.DelayReport
{
    public class DelayReportQueryModel
    {
        public int BeginDelayCount { get; set; }

        public int EndDelayCount { get; set; }

        public List<int> BranchIds { get; set; }

        public CollateralType CollateralType { get; set; }
    }
}