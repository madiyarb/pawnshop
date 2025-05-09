using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace Pawnshop.Data.Models.MobileApp
{
    public class ModelForBlackList : MobileAppModel, IApplicationModel
    {
        [JsonProperty("license_plate")]
        public string TransportNumber { get; set; }
        [JsonProperty("client_reason_list")]
        public List<string> ClientReasonList { get; set; }
        [JsonProperty("car_reason")]
        public string CarReason { get; set; }
        [JsonProperty("author_id")]
        public int AuthorId { get; set; }
        [JsonProperty("send_type")]
        public BlackListSendType BlackListSendType { get; set; }


        /// <summary>
        /// Имя
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// Фамилия
        /// </summary>
        public string SurName { get; set; }
        /// <summary>
        /// Отчество
        /// </summary>
        [JsonProperty("middle_name")]
        public string Patronymic { get; set; }
        /// <summary>
        /// Дата рождения
        /// </summary>
        [JsonProperty("birthday")]
        public DateTime? BirthDay { get; set; }
        /// <summary>
        /// Вид документа
        /// </summary>
        [JsonProperty("doc_type")]
        public string DocumentTypeCode { get; set; }
        /// <summary>
        /// Место рождения/регистрации
        /// </summary>
        [JsonProperty("place_of_birth")]
        public string BirthPlace { get; set; }
        /// <summary>
        /// Номер документа
        /// </summary>
        [JsonProperty("license_number")]
        public string DocumentNumber { get; set; }
        /// <summary>
        /// Дата выдачи документа
        /// </summary>
        [JsonProperty("license_date_of_issue")]
        public DateTime? DocumentDate { get; set; }
        /// <summary>
        /// Срок действия документа
        /// </summary>
        [JsonProperty("license_date_of_end")]
        public DateTime? DocumentDateExpire { get; set; }
        /// <summary>
        /// Кем выдан документ
        /// </summary>
        [JsonProperty("license_issuer")]
        public string DocumentProviderCode { get; set; }
        /// <summary>
        /// Страна гражданства
        /// </summary>
        [JsonProperty("country_code")]
        public string CountryCode { get; set; }
    }
}
