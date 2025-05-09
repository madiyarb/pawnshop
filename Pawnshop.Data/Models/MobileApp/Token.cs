using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace Pawnshop.Data.Models.MobileApp
{
    public class Token
    {
        [JsonProperty("access_token")]
        public string AccessToken { get; set; }
    }
}
