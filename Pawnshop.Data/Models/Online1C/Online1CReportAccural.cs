using Pawnshop.Core;

namespace Pawnshop.Data.Models.Online1C
{
    /// <summary>Начисления</summary>
    public class Online1CReportAccural : IEntity
    {
        public int Id { get; set; }
        public int CollateralType { get; set; }
        public string BranchName { get; set; }
        public decimal InterestAmount { get; set; }
        public decimal PenaltyAmount { get; set; }
        public decimal ProvisionDebt { get; set; }
        public decimal ProvisionInterest { get; set; }
        public decimal ProvisionOverdueDebt { get; set; }
        public decimal ProvisionOverdueInterest { get; set; }
        public decimal OverdueDebt { get; set; }
        public decimal OverdueInterest { get; set; }

    }
}
