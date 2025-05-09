using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Pawnshop.Web.Models.Mintos
{
    public class ContractInfo
    {
        [JsonProperty("id")]
        public int Id;

        [JsonProperty("lender_id")]
        public string LenderId;

        [JsonProperty("created_at")]
        public DateWithTimeZone CreatedAt;

        [JsonProperty("status")]
        public string Status;
    }
}