using System;
using System.Collections.Generic;
using System.Linq;
using Pawnshop.AccountingCore.Models;

namespace Pawnshop.Web.Models.Reports.DelayReportForSoftCollection
{
    public class DelayReportForSoftCollectionModel
    {
        public int BeginDelayCount { get; set; }

        public int EndDelayCount { get; set; }

        public string BranchName { get; set; }

        public CollateralType CollateralType { get; set; }

        public List<dynamic> List { get; set; }

        public dynamic Group => new
        {
            LoanCost = this.List.Sum(item => (decimal)item.LoanCost),
            PenaltyPecentCost = this.List.Sum(item => (decimal)item.PenaltyPecentCost),
            TotalPercentCost = this.List.Sum(item => (decimal)item.TotalPercentCost)
        };
    }
}