using Pawnshop.Core;

namespace Pawnshop.Data.Models.Online1C
{
    /// <summary>Погашения</summary>
    public class Online1CReportPayment : IEntity
    {
        public int Id { get; set; }
        public int CollateralType { get; set; }
        public string BranchName { get; set; }
        public int BranchId { get; set; }
        public decimal DebtAmountShort { get; set; }
        public decimal DebtAmountLong { get; set; }
        public decimal OverdueDebtAmountShort { get; set; }
        public decimal OverdueDebtAmountLong { get; set; }
        public decimal InterestAmount { get; set; }
        public decimal OverdueInterestAmount { get; set; }
        public decimal PenaltyDebtAmount { get; set; }
        public decimal PenaltyInterestAmount { get; set; }
    }
}
