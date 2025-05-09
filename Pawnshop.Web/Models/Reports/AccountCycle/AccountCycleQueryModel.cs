using System;

namespace Pawnshop.Web.Models.Reports.AccountCycle
{
    public class AccountCycleQueryModel
    {
        public DateTime BeginDate { get; set; }

        public DateTime EndDate { get; set; }

        public int BranchId { get; set; }
    }
}