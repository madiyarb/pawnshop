using Newtonsoft.Json;

namespace Pawnshop.Data.Models.Contracts.Views
{
    public sealed class RealtyInfoOnlineVIew
    {
        [JsonIgnore]
        public int ContractId { get; set; }
        public int RealtyType { get; set; }
        public string RCA { get; set; }
        public string CadastralNumber { get; set; }
        public double EstimatedCost { get; set; }
        public string YearOfConstruction { get; set; }
    }
}
