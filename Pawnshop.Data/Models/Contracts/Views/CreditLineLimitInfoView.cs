using Newtonsoft.Json;

namespace Pawnshop.Data.Models.Contracts.Views
{
    public sealed class CreditLineLimitInfoView
    {
        [JsonIgnore]
        public int ContractId { get; set; }
        public decimal InitialCreditLineLimit { get; set; }
        public decimal UsedCreditLineLimit { get; set; }
        public decimal AvailableCreditLineLimit { get; set; }
    }
}
