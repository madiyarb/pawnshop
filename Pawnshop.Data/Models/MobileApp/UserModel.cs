using Newtonsoft.Json;
using Pawnshop.Data.Models.Membership;
using System;
using System.Collections.Generic;
using System.Text;

namespace Pawnshop.Data.Models.MobileApp
{
    public class UserModel
    {
        [JsonProperty("name")]
        public string FullName { get; set; }
        [JsonProperty("email")]
        public string Email { get; set; }
        [JsonProperty("role")]
        public string Role { get; set; }
        [JsonProperty("p_nid")]
        public int UserId { get; set; }
        [JsonProperty("blocked")]
        public bool Locked { get; set; }
        [JsonProperty("username")]
        public string UserName { get; set; }
        [JsonProperty("branchIdList")]
        public string GroupList { get; set; }
    }
}