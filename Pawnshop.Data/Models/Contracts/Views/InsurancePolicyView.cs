using Newtonsoft.Json;

namespace Pawnshop.Data.Models.Contracts.Views
{
    public sealed class InsurancePolicyView
    {
        [JsonIgnore]
        public int ContractId { get; set; }
        public string PoliceNumber { get; set; }
        public decimal InsurancePremium { get; set; }
    }
}
