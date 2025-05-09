using System;
using System.ComponentModel;
using Newtonsoft.Json;
using Pawnshop.Data.CustomTypes;

namespace Pawnshop.Data.Models.Insurances
{
    public class InsuranceRequestData : IJsonObject
    {
        [DisplayName("Полное имя(наименование)")]
        [JsonProperty("fullName")]
        public string FullName { get; set; }

        [DisplayName("ИИН/БИН клиента")]
        [JsonProperty("iin")]
        public string IdentityNumber { get; set; }

        [DisplayName("Дата рождения/регистрации")]
        [JsonProperty("birthDate")]
        public DateTime BirthDay { get; set; }

        [DisplayName("Номер документа")]
        [JsonProperty("docNumber")]
        public string DocumentNumber { get; set; }

        [DisplayName("Когда выдан документ")]
        [JsonProperty("docDate")]
        public DateTime DocumentDate { get; set; }

        [DisplayName("Орган выдачи документа")]
        [JsonProperty("docIssue")]
        public string Provider { get; set; }

        [DisplayName("Тип документа")]
        [JsonProperty("docType")]
        public int DocumentType { get; set; }

        public string DocumentTypeName { get; set; }

        [DisplayName("Номер договора")]
        [JsonProperty("loanAgreementNumber")]
        public string ContractNumber { get; set; }

        [DisplayName("Адрес клиента")]
        [JsonProperty("address")]
        public string Address { get; set; }

        [DisplayName("Мобильный телефон")]
        [JsonProperty("phoneNumber")]
        public string PhoneNumber { get; set; }

        [DisplayName("Email")]
        [JsonProperty("email")]
        public string Email { get; set; }

        [DisplayName("Резидент РК")]
        [JsonProperty("resident")]
        public int IsResident { get; set; }

        [DisplayName("Дата начала страхования")]
        [JsonProperty("startDate")]
        public DateTime InsuranceStartDate { get; set; }

        [DisplayName("Дата окончания страхования")]
        [JsonProperty("endDate")]
        public DateTime InsuranceEndDate { get; set; }

        [DisplayName("Период страхования")]
        [JsonProperty("term")]
        public int InsurancePeriod { get; set; }

        [DisplayName("Страховая сумма")]
        [JsonProperty("creditSum")]
        public decimal InsuranceAmount { get; set; }

        [DisplayName("Страховая премия")]
        [JsonProperty("insurancePremium")]
        public decimal InsurancePremium { get; set; }

        [DisplayName("Тариф страхования")]
        [JsonProperty("percent")]
        public decimal InsuranceRate { get; set; }

        [JsonProperty("login")]
        public string Login { get; set; }

        [DisplayName("Тип страхования")]
        [JsonProperty("insuranceType")]
        public string InsuranceType { get; set; }

        [DisplayName("Сумма к выдаче")]
        [JsonProperty("eds")]
        public decimal Eds { get; set; }

        [DisplayName("Номер первой заявки")]
        [JsonProperty("partnerID")]
        public string PrevPolicyNumber { get; set; }

        [DisplayName("Номер последней заявки")]
        [JsonProperty("secondPartnerID")]
        public string LastPolicyNumber { get; set; }

        [JsonProperty("metaData")]
        public LogMetaData MetaDataItem { get; set; }

        [DisplayName("Сумма доплаты к страховой премии")]
        [JsonProperty("premium2")]
        public int Premium2 { get; set; }

        [DisplayName("Страховая премия, рассчитанная при текущем доборе, минус страховая премия предыдущего полиса")]
        [JsonProperty("sum2")]
        public int EsbdAmount { get; set; }

        public decimal AdditionCost { get; set; }
        public decimal LoanCost { get; set; }
        public decimal SurchargeAmount { get; set; }
        public decimal YearPremium { get; set; }
        public string DescrOfInsuranceCalc { get; set; }
        public int AlgorithmVersion { get; set; }
        public decimal AmountToAddIfBorder { get; set; }
        public decimal? AmountToDecrease4Pensioner { get; set; }
        public DateTime? AditionContractEndDate { get; set; }
        public bool CategoryChanged { get; set; }
    }
}