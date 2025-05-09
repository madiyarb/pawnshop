using Pawnshop.Core;

namespace Pawnshop.Data.Models.Online1C
{
    /// <summary>Освоение аванса</summary>
    public class Online1CReportDebitDepos : IEntity
    {
        public int Id { get; set; }
        public string IdentityNumber { get; set; }
        public string ClientName { get; set; }
        public string ContractNumber { get; set; }
        public decimal Amount { get; set; }
        public int CollateralType { get; set; }
        public string BranchName { get; set; }
    }
}