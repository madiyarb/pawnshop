using Newtonsoft.Json;

namespace Pawnshop.Web.Models.Estimates
{
    public class EvaluationResultResponse
    {
        [JsonProperty("success")]
        public bool Success { get; set; }

        [JsonProperty("message")]
        public string Message { get; set; }
    }
}
