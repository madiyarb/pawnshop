using Pawnshop.Core;

namespace Pawnshop.Data.Models.Online1C
{
    /// <summary>Поступление денег</summary>
    public class Online1CReportPrepayment : IEntity
    {
        public int Id { get; set; }
        public string IdentityNumber { get; set; }
        public string ClientName { get; set; }
        public string ContractNumber { get; set; }
        public decimal Amount { get; set; }
        public int CashOperation { get; set; }
        public int OnlineOperation { get; set; }
        public int OnlineProvider { get; set; }
        public int CollateralType { get; set; }
        public string BranchName { get; set; }
        public string CashOrderBranchName { get; set; }
    }
}