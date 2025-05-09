using Newtonsoft.Json;

namespace Pawnshop.Data.Models.Insurances
{
    public class InsuranceCancelPolicyRequestVariablesBPM
    {
        [JsonProperty("V_REQUEST_ID")]
        public string Request { get; set; }
        [JsonProperty("V_CANCEL_AUTHOR_ID")]
        public string CancelAuthorId { get; set; }
    }
}
