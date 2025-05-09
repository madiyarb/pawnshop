using Newtonsoft.Json;

namespace Pawnshop.Web.Models.Auth
{
    public class TokenModel
    {
        [JsonProperty("token")]
        public string Token { get; set; }
    }
}
