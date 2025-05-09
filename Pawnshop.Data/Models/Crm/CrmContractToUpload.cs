using Newtonsoft.Json;

namespace Pawnshop.Data.Models.Crm
{
    public class CrmContractToUpload
    {
        /// <summary>
        /// Наименование сделки/договора
        /// </summary>
        [JsonProperty("TITLE")]
        public string Title { get; set; }

        /// <summary>
        /// Идентификатор контракта
        /// </summary>
        [JsonProperty("UF_CRM_1554465200529")]
        public string ContractId { get; set; }

        /// <summary>
        /// статус сделки
        /// </summary>
        [JsonProperty("STAGE_ID")]
        public string Stage { get; set; }

        /// <summary>
        /// Идентификатор категории
        /// </summary>
        [JsonProperty("CATEGORY_ID")]
        public string CategoryId { get; set; }

        /// <summary>
        /// Стоимость кредита
        /// </summary>
        [JsonProperty("OPPORTUNITY")]
        public decimal LoanCost { get; set; }

        /// <summary>
        /// идентификатор контакта
        /// </summary>
        [JsonProperty("CONTACT_ID")]
        public string ClientCrmId { get; set; }

        /// <summary>
        /// Путь привлечения
        /// </summary>
        [JsonProperty("UF_CRM_1602525272410")]
        public string AttractionChannel { get; set; }

        /// <summary>
        /// Дата подписания договора
        /// </summary>
        [JsonProperty("UF_CRM_1631676899")]
        public string SignDate { get; set; }

        /// <summary>
        /// Идентификатор сделки битрикс из мобильного приложения
        /// </summary>
        [JsonProperty("BITRIX_ID")]
        public string BitrixId { get; set; }
    }
}
