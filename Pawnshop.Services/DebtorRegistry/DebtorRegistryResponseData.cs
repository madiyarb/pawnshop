using System.Collections.Generic;
using System;
using Newtonsoft.Json;

namespace Pawnshop.Services.DebtorRegistry
{
    public sealed class DebtorRegistryResponseData
    {

        [JsonProperty("il_date")]
        public DateTime IlDate { get; set; }

        [JsonProperty("category_ru")]
        public string CategoryRu { get; set; }

        [JsonProperty("recovery_amount")]
        public string RecoveryAmount { get; set; }

        [JsonProperty("disa_department_name_ru")]
        public string DisaDepartmentNameRu { get; set; }

        [JsonProperty("recoverer_type_ru")]
        public string RecovererTypeRu { get; set; }

        [JsonProperty("kbk_name_ru")]
        public string KbkNameRu { get; set; }
    }
}
