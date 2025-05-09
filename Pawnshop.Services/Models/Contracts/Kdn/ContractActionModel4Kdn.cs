using Pawnshop.Data.Models.Contracts.Actions;

namespace Pawnshop.Services.Models.Contracts.Kdn
{
    public class ContractActionModel4Kdn
    {
        public ContractAction Action { get; set; }
        public int? SettingId { get; set; }
        public int? additionLoanPeriod { get; set; }
        public int? subjectId { get; set; }
        public decimal? SurchargeAmount { get; set; }
        public decimal AdditionCost { get; set; }
        public int? PositionEstimatedCost { get; set; }
    }
}
