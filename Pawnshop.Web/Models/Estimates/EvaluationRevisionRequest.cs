using Newtonsoft.Json;

namespace Pawnshop.Web.Models.Estimates
{
    public class EvaluationRevisionRequest
    {
        [JsonProperty("app_id")]
        public string ApplicatinId { get; set; }

        [JsonProperty("note")]
        public string Note { get; set; }

        [JsonProperty("author")]
        public string AuthorName { get; set; }
    }
}
