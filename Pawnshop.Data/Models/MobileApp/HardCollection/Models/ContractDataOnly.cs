using Newtonsoft.Json;
using Pawnshop.Data.Models.MobileApp.HardCollection.ViewModels;
using System;
using System.Collections.Generic;
using System.Text;

namespace Pawnshop.Data.Models.MobileApp.HardCollection.Models
{
    public class ContractDataOnly
    {
        [JsonProperty("Contract")]
        public HardCollectionContract Contract { get; set; }
    }
}
