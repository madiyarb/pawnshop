using System;
using Pawnshop.Data.Models.Contracts;
using System.Collections.Generic;
using Pawnshop.AccountingCore.Models;

namespace Pawnshop.Web.Models.Reports.DelayReportForSoftCollection
{
    public class DelayReportForSoftCollectionQueryModel
    {
        public int BeginDelayCount { get; set; }

        public int EndDelayCount { get; set; }

        public List<int> BranchIds { get; set; }

        public CollateralType CollateralType { get; set; }
    }
}