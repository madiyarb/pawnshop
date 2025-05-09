using Newtonsoft.Json;
using System;

namespace Pawnshop.Web.Models.CallBlackList
{
    public class CallBlackListView
    {
        public int Id { get; set; }

        public string PhoneNumber { get; set; }

        public string Reason { get; set; }

        public DateTime CreateDate { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public DateTime? UpdateDate { get; set; }

        public DateTime? ExpireDate { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public int? AuthorId { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string AuthorName { get; set; }
    }
}
