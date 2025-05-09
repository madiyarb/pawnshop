using Newtonsoft.Json;

namespace Pawnshop.Data.Models.Contracts.Views
{
    public sealed class ContractExpenseOnlineInfo
    {
        [JsonIgnore]
        public int ContractId { get; set; }
        public bool HasAdditionalExpenses { get; set; }
    }
}
