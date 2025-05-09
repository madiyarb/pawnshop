using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;

namespace Pawnshop.Data.Models.Dictionaries.Address
{
    public class AddressGeonimType : ITranslatedDictionary, IEGOVType
    {
        [JsonProperty("id")]
        public int Id { get; set; }
        [JsonProperty("value_ru")]
        public string NameRus { get; set; }
        [JsonProperty("value_kz")]
        public string NameKaz { get; set; }
        [JsonProperty("short_value_ru")]
        public string ShortNameRus { get; set; }
        [JsonProperty("short_value_kz")]
        public string ShortNameKaz { get; set; }
        [JsonProperty("code")]
        public string Code { get; set; }
        [JsonProperty("actual")]
        public bool IsActual { get; set; }
    }
}
