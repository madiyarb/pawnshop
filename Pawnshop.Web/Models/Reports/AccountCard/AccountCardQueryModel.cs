using System;

namespace Pawnshop.Web.Models.Reports.AccountCard
{
    public class AccountCardQueryModel
    {
        public DateTime BeginDate { get; set; }

        public DateTime EndDate { get; set; }

        public int BranchId { get; set; }

        public int AccountId { get; set; }
    }
}