using System;
using System.Collections.Generic;
using System.Linq;

namespace Pawnshop.Web.Models.Reports.AccountAnalysis
{
    public class AccountAnalysisModel
    {
        public DateTime BeginDate { get; set; }

        public DateTime EndDate { get; set; }

        public int BranchId { get; set; }

        public string BranchName { get; set; }

        public int AccountId { get; set; }

        public string AccountCode { get; set; }

        public List<dynamic> List { get; set; }

        public dynamic Group { get; set; }

        public dynamic Total
        {
            get
            {
                return new
                {
                    FromCredit = this.List.Sum(l => (long)(l.FromCredit == null ? 0 : l.FromCredit)),
                    ToDebit = this.List.Sum(l => (long)(l.ToDebit == null ? 0 : l.ToDebit)),                    
                };
            }
        }
    }
}