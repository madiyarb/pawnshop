using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Pawnshop.Web.Models.Mintos
{
    public class DateWithTimeZone
    {
        [JsonProperty("date")]
        public DateTime Date;

        [JsonProperty("timezone_type")]
        public int TimezoneType;

        [JsonProperty("timezone")]
        public string Timezone;
    }
}
