using System.Collections.Generic;

namespace Pawnshop.Services.CreditLines
{
    public class CreditLineTransfer
    {
        public int ContractId { get; set; }
        public string ContractNumber { get; set; }
        public decimal Amount { get; set; }
        public bool IsCreditLine { get; set; }
        public List<RefillableAccountsInfo> RefillableAccounts { get; set; } = new List<RefillableAccountsInfo>();
    }
}
