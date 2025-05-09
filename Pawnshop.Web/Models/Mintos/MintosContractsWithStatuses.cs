using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Pawnshop.Web.Models.Mintos
{

    public class MintosContractsWithStatuses
    {
        [JsonProperty("data")]
        public Dictionary<int, ContractInfo> All;
    }
}
