using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Pawnshop.Data.Models.Contracts;
using System;
using System.Collections.Generic;
using System.Text;

namespace Pawnshop.Data.Models.MobileApp
{
    public class ApprovedContract : IApplicationModel
    {
        /// <summary>
        /// Модель
        /// </summary>
        [JsonProperty("model_id")]
        public int ModelId { get; set; }
        /// <summary>
        /// Гос номер 
        /// </summary>
        [JsonProperty("license_plate")]
        public string TransportNumber { get; set; }
        /// <summary>
        /// Год выпуска
        /// </summary>
        [JsonProperty("prod_year")]
        public int ReleaseYear { get; set; }
        /// <summary>
        /// Номер кузова
        /// </summary>
        [JsonProperty("body_number")]
        public string BodyNumber { get; set; }
        /// <summary>
        /// Номер техпаспорта
        /// </summary>
        [JsonProperty("registration_number")]
        public string TechPassportNumber { get; set; }
        /// <summary>
        /// Дата техпаспорта
        /// </summary>
        [JsonProperty("registration_date")]
        public DateTime TechPassportDate { get; set; }
        /// <summary>
        /// Цвет
        /// </summary>
        public string Color { get; set; }
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
        /// ИИН
        /// </summary>
        [JsonProperty("individual_id_number")]
        public string IdentityNumber { get; set; }
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
        /// Идентификатор заявки в базе мобильного приложения
        /// </summary>
        [JsonProperty("application_id")]
        public int AppId { get; set; }
        /// <summary>
        /// Дата создания заявки
        /// </summary>
        [JsonProperty("application_date")]
        public DateTime ApplicationDate { get; set; }
        /// <summary>
        /// Сумма оценки
        /// </summary>
        [JsonProperty("estimated_cost")]
        public int EstimatedCost { get; set; }
        /// <summary>
        /// Первоначальный взнос
        /// </summary>
        [JsonProperty("pre_payment")]
        public int? PrePayment { get; set; }
        /// <summary>
        /// Желаемая сумма
        /// </summary>
        [JsonProperty("requested_sum")]
        public int RequestedSum { get; set; }
        /// <summary>
        /// Идентификатор пользователя создавшего заявку
        /// </summary>
        [JsonProperty("author_id")]
        public int AuthorId { get; set; }
        /// <summary>
        /// Сумма из реестра должников 
        /// </summary>
        [JsonProperty("debtors_register_sum")]
        public int DebtorsRegisterSum { get; set; }
        /// <summary>
        /// Сумма light лимита
        /// </summary>
        [JsonProperty("light_estimated_cost")]
        public int? LightCost { get; set; }
        /// <summary>
        /// Сумма turbo лимита
        /// </summary>
        [JsonProperty("turbo_estimated_cost")]
        public int? TurboCost { get; set; }
        /// <summary>
        /// Сумма motor лимита
        /// </summary>
        [JsonProperty("motor_estimated_cost")]
        public int? MotorCost { get; set; }
        /// <summary>
        /// Сумма лимита
        /// </summary>
        [JsonProperty("limit_sum")]
        public int? LimitSum { get; set; }
        /// <summary>
        /// Страна гражданства
        /// </summary>
        [JsonProperty("country_code")]
        public string CountryCode { get; set; }
        /// <summary>
        /// Гарантия менеджера
        /// </summary>
        [JsonProperty("manager_guarentee")]
        public bool ManagerGuarentee { get; set; }
        /// <summary>
        ///  Без права вождения
        /// </summary>
        [JsonProperty("without_driving")]
        public bool WithoutDriving { get; set; }
        /// <summary>
        ///  Признак рефинансирования (добора)
        /// </summary>
        public bool IsAddition { get; set; }
        /// <summary>
        /// Ссылка на родительский Договор
        /// </summary>
        [JsonProperty("parent_contract_id")]
        public int? ParentContractId { get; set; }
        /// <summary>
        /// Идентификатор заявки в Битриксе
        /// </summary>
        [JsonProperty("bitrix_id")]
        public int? BitrixId { get; set; }
        /// <summary>
        /// Имя/Наименование торговца
        /// </summary>
        [JsonProperty("merchant_name")]
        public string MerchantName { get; set; }
        /// <summary>
        /// Фамилия торговца
        /// </summary>
        [JsonProperty("merchant_surname")]
        public string? MerchantSurname { get; set; }
        /// <summary>
        /// Отчество торговца
        /// </summary>
        [JsonProperty("merchant_middle_name")]
        public string? MerchantMiddleName { get; set; }
        /// <summary>
        /// Дата рождения торговца
        /// </summary>
        [JsonProperty("merchant_birthday")]
        public DateTime? MerchantBirthDay { get; set; }
        /// <summary>
        /// Вид документа
        /// </summary>
        [JsonProperty("merchant_doc_type")]
        public string MerchantDocumentTypeCode { get; set; }
        /// <summary>
        /// Место рождения/регистрации торговца
        /// </summary>
        [JsonProperty("merchant_place_of_birth")]
        public string? MerchantBirthOfPlace { get; set; }
        /// <summary>
        /// ИИН/БИН торговца
        /// </summary>
        [JsonProperty("merchant_individual_id_number")]
        public string MerchantIdentityNumber { get; set; }
        /// <summary>
        /// Номер документа торговца
        /// </summary>
        [JsonProperty("merchant_license_number")]
        public string? MerchantLicenseNumber { get; set; }
        /// <summary>
        /// Дата выдачи документа торговца
        /// </summary>
        [JsonProperty("merchant_license_date_of_issue")]
        public DateTime? MerchantLicenseDate { get; set; }
        /// <summary>
        /// Срок действия документа торговца
        /// </summary>
        [JsonProperty("merchant_license_date_of_end")]
        public DateTime? MerchantLicenseDateExpire { get; set; }
        /// <summary>
        /// Кем выдан документ торговца
        /// </summary>
        [JsonProperty("merchant_license_issuer")]
        public string MerchantLicenseProvider { get; set; }
        /// <summary>
        /// Признак правовой формы
        /// </summary>
        [JsonProperty("definition_legal_person")]
        public string? DefinitionLegalPerson { get; set; }
        /// <summary>
        /// Признак автокредита
        /// </summary>
        [JsonProperty("is_autocredit")]
        public int? IsAutocredit { get; set; }
        // <summary>
        /// Перечисления признаков договора для заявки
        /// </summary>
        [JsonProperty("contract_class")]
        [JsonConverter(typeof(StringEnumConverter))]
        public ContractClass? ContractClass { get; set; }
    }
}
