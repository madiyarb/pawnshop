using System;

namespace Pawnshop.Web.Models.Reports.AccountAnalysis
{
    public class AccountAnalysisQueryModel
    {
        public int Month { get; set; }

        public int Year { get; set; }

        public int BranchId { get; set; }

        public int AccountPlanId { get; set; }
    }
}