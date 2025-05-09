using Newtonsoft.Json;

namespace Pawnshop.Web.Models.CreditLine
{
    public class ValidationForNewTrancheViewModel
    {
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public int? CountPayment { get; set; }

        public bool IsCanOpen { get; set; }

        public string Message { get; set; }
    }
}
