using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace Pawnshop.Data.Models.Crm
{
    public class CrmMultifieldItem
    {
        /// <summary>
        /// Значение поля
        /// </summary>

        [JsonProperty("VALUE")]
        public string Value { get; set; }

        /// <summary>
        /// Вид контакта (MOBILE, HOME, FAX, WORK, PAGER, MAILING, OTHER)
        /// </summary>
        [JsonProperty("VALUE_TYPE")]
        public string ValueType { get; set; }
    }
}
