using Newtonsoft.Json;
using System;

namespace Pawnshop.Data.Models.MobileApp
{
    public class ReissueCarModel
    {
        /// <summary>
        /// Государственный номер транспорта (Регистрационный номер)
        /// </summary>
        [JsonProperty("transport_number")]
        public string TransportNumber { get; set; }
        /// <summary>
        /// Регистрационный номер Тех. Паспорта
        /// </summary>
        [JsonProperty("tech_passport_number")]
        public string TechPassportNumber { get; set; }
        /// <summary>
        /// Дата выдачи Тех. Паспорта
        /// </summary>
        [JsonProperty("tech_passport_date")]
        public DateTime TechPassportDate { get; set; }
        /// <summary>
        /// Идентификатор заявки в мобильном приложении
        /// </summary>
        [JsonProperty("app_id")]
        public int AppId { get; set; }
    }
}