using Pawnshop.Core;

namespace Pawnshop.Data.Models.Online1C
{
    /// <summary>Выдача</summary>
    public class Online1CReportIssues : IEntity
    {
        public int Id { get; set; }
        public int CollateralType { get; set; }
        public string BranchName { get; set; }
        public int BranchId { get; set; }
        public bool IsShortTerm { get; set; }
        public decimal DebtAmount { get; set; }
        public decimal InsuranceAmount { get; set; }
        public decimal TotalAmount { get; set; }
        public int PayTypeId { get; set; }
        public bool IsBuyCar { get; set; }
        public decimal InitialFee { get; set; }
    }
}
