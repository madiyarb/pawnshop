using Newtonsoft.Json;
using Pawnshop.AccountingCore.Models;
using Pawnshop.Data.Models.LoanSettings;
using System.Collections.Generic;
using System;

namespace Pawnshop.Data.Models.Contracts
{
    /// <summary>
    /// Данные контракта для списка контрактов
    /// </summary>
    public class ContractListInfo
    {
        /// <summary>
        /// Идентификатор
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Номер
        /// </summary>
        public string ContractNumber { get; set; }

        /// <summary>
        /// Тип контракта
        /// </summary>
        public ContractClass ContractClass { get; set; }

        /// <summary>
        /// Дата
        /// </summary>
        public DateTime ContractDate { get; set; }

        /// <summary>
        /// Дата возврата
        /// </summary>
        public DateTime MaturityDate { get; set; }

        /// <summary>
        /// Сумма займа
        /// </summary>
        public decimal LoanCost { get; set; }

        /// <summary>
        /// Вид кредита
        /// </summary>
        public int LoanPeriod { get; set; }

        /// <summary>
        /// Процентная ставка
        /// </summary>
        public decimal LoanPercent { get; set; }

        /// <summary>
        /// Вид удержания процентов
        /// </summary>
        public PercentPaymentType? PercentPaymentType { get; set; }

        /// <summary>
        /// Дата следующей оплаты
        /// </summary>
        public DateTime? NextPaymentDate { get; set; }

        /// <summary>
        /// Идентификатор клиента
        /// </summary>
        public int ClientId { get; set; }

        /// <summary>
        /// Полное имя клиента
        /// </summary>
        public string ClientFullName { get; set; }

        /// <summary>
        /// Филиал
        /// </summary>
        public string BranchName { get; set; }

        /// <summary>
        /// Автор
        /// </summary>
        public string AuthorFullName { get; set; }

        /// <summary>
        /// Наименование продукта
        /// </summary>
        public string ProductName { get; set; }

        /// <summary>
        /// Плавающая ставка
        /// </summary>
        public bool IsFloatingDiscrete { get; set; }

        /// <summary>
        /// Период погашения процентов
        /// </summary>
        public PeriodType? PaymentPeriodType { get; set; }

        /// <summary>
        /// Настройка/продукт
        /// </summary>
        public int? SettingId { get; set; }

        /// <summary>
        /// Залог
        /// </summary>
        public string GrnzCollat { get; set; }

        /// <summary>
        /// Статус (для фронта)
        /// </summary>
        public ContractDisplayStatus DisplayStatus { get; set; }

        /// <summary>
        /// Идентификатор кредитной линии
        /// </summary>
        public int? CreditLineId { get; set; }

        /// <summary>
        /// Список траншей
        /// </summary>
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public IList<ContractListInfo> Tranches { get; set; }

        public string MaidenName { get; set; }

        public int CollateralType { get; set; }

        /// <summary>
        /// Наличие обременения
        /// </summary>
        public bool HasEncumbrance { get; set; }

        public DateTime? BuyoutDate { get; set; }

        public ContractStatus Status { get; set; } = ContractStatus.Draft;

        public bool Locked { get; set; } = false;

        public bool CreatedToday => ContractDate.Date == DateTime.Now.Date;

        public string CollectionStatusCode { get; set; }

        public int DelayDays { get; set; }

        public DateTime? DeleteDate { get; set; }

        /// <summary>
        /// Признак создан ли договор из Online заявки
        /// </summary>
        public bool CreatedInOnline { get; set; }
    }
}
