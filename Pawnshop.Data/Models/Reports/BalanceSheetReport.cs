using Pawnshop.AccountingCore.Models;
using System.Collections;
using System.Collections.Generic;

namespace Pawnshop.Data.Models.Reports
{
    public class BalanceSheetReport
    {
        public int AccountId { get; set; }
        public string PlanCode { get; set; }
        public string ClientName { get; set; }
        public string IdentityNumber { get; set; }
        public string ContractNumber { get; set; }
        public int ClientId { get; set; }
        public int ContractId { get; set; }
        public int BranchId { get; set; }
        public string BranchName { get; set; }
        public decimal DebitTurns { get; set; }
        public decimal CreditTurns { get; set; }
        public decimal IncomingBalance { get; set; }
        public decimal OutgoingBalance { get; set; }
        public bool PlanIsActive { get; set; }
        public CollateralType CollateralType { get; set; }
    }
}
