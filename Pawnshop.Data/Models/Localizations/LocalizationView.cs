using Newtonsoft.Json;

namespace Pawnshop.Data.Models.Localizations
{
    public class LocalizationView
    {
        [JsonProperty("lang")]
        public string Language { get; set; }

        [JsonProperty("value")]
        public string Value { get; set; }
    }
}
