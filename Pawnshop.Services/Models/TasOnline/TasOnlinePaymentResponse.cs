using Newtonsoft.Json;

namespace Pawnshop.Services.Models.TasOnline
{
    public class TasOnlinePaymentResponse : TasOnlineBaseResponse
    {
        [JsonProperty("doc_number")]
        public string TasOnlineDocumentId { get; set; }
    }
}