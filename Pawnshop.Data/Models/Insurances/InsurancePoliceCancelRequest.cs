using Newtonsoft.Json;
using Pawnshop.Data.CustomTypes;
using System;
using System.Collections.Generic;
using System.Text;

namespace Pawnshop.Data.Models.Insurances
{
    public class InsurancePoliceCancelRequest : IJsonObject
    {
        [JsonProperty("policyNumber")]
        public string PolicyNumber { get; set; }

        [JsonProperty("metaData")]
        public LogMetaData MetaDataItem { get; set; }
    }
}
