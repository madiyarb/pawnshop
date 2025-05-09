using Newtonsoft.Json;
using Microsoft.AspNetCore.Mvc;

namespace Pawnshop.Data.Models.MobileApp
{
    public class MobileAppModel
    {
        [BindProperty(Name = "brand")]
        [JsonProperty("brand")]
        public string Mark { get; set; }
        [BindProperty(Name = "model")]
        [JsonProperty("model")]
        public string Model { get; set; }
        [JsonProperty("individual_id_number")]
        [BindProperty(Name = "individual_id_number")]
        public string IdentityNumber { get; set; }
        [BindProperty(Name = "body_number")]
        [JsonProperty("body_number")]
        public string BodyNumber { get; set; }
        [BindProperty(Name = "prod_year")]
        [JsonProperty("prod_year")]
        public int ReleaseYear { get; set; }
        [BindProperty(Name = "amount")]
        [JsonProperty("amount")]
        public int? LoanCost { get; set; }
        [BindProperty(Name = "apply_number")]
        [JsonProperty("apply_number")]
        public string ContractNumber { get; set; }
    }
}