using Newtonsoft.Json;
using System;

namespace Pawnshop.Data.Models.MobileApp.HardCollection.ViewModels
{
    public class HardCollectionContract
    {
        /// <summary>
        /// Идентификатор контракта
        /// </summary>
        [JsonProperty("ContractId")]
        public int ContractId { get; set; }

        /// <summary>
        /// Идентификатор клиента
        /// </summary>
        [JsonProperty("ClientId")]
        public int ClientId { get; set; }
        /// <summary>
        /// Номер контракта
        /// </summary>
        [JsonProperty("ContractNumber")]
        public string ContractNumber { get; set; }

        /// <summary>
        /// Статус контракта
        /// </summary>
        [JsonProperty("ContractStatus")]
        public string ContractStatus { get; set; }

        /// <summary>
        /// Дата выдачи
        /// </summary>
        [JsonProperty("ContractDate")]
        public DateTime ContractDate { get; set; }

        /// <summary>
        /// Дата возврата
        /// </summary>
        [JsonProperty("MaturityDate")]
        public DateTime MaturityDate { get; set; }

        /// <summary>
        /// Срок кредита в месяцах
        /// </summary>
        [JsonProperty("LoanPeriod")]
        public int LoanPeriod { get; set; }

        /// <summary>
        /// Основной долг
        /// </summary>
        [JsonProperty("LoanAmount")]
        public decimal LoanAmount { get; set; }

        /// <summary>
        /// Просроченный основной долг
        /// </summary>
        [JsonProperty("OverdueAccountAmount")]
        public decimal OverdueAccountAmount { get; set; }

        /// <summary>
        /// Проценты начисленные
        /// </summary>
        [JsonProperty("ProfitAmount")]
        public decimal ProfitAmount { get; set; }

        /// <summary>
        /// Просроченные проценты начисленные
        /// </summary>
        [JsonProperty("OverdueProfitAmount")]
        public decimal OverdueProfitAmount { get; set; }

        /// <summary>
        /// Штраф
        /// </summary>
        [JsonProperty("PenyAmount")]
        public decimal PenyAmount { get; set; }

        /// <summary>
        /// Аванс
        /// </summary>
        [JsonProperty("PrepaymentBalance")]
        public decimal PrepaymentBalance { get; set; }

        /// <summary>
        /// Расходы итого
        /// </summary>
        [JsonProperty("ExpenseAmount")]
        public decimal ExpenseAmount { get; set; }

        /// <summary>
        /// Общая сумма задолженности
        /// </summary>
        [JsonProperty("CurrentDebt")]
        public decimal CurrentDebt { get; set; }

        /// <summary>
        /// Сумма выкупа
        /// </summary>
        [JsonProperty("TotalRedemptionAmount")]
        public decimal TotalRedemptionAmount { get; set; }

        /// <summary>
        /// Сумма продления
        /// </summary>
        [JsonProperty("ProlongAmount")]
        public decimal ProlongAmount { get; set; }

        /// <summary>
        /// Филиал договора
        /// </summary>
        [JsonProperty("BranchId")]
        public int BranchId { get; set; }

        /// <summary>
        /// Филиал договора для СБ 
        /// </summary>
        [JsonProperty("SelectedBranchId")]
        public int? SelectedBranchId { get; set; }

        /// <summary>
        /// Дни просрочки
        /// </summary>
        [JsonProperty("DelayDays")]
        public int DelayDays { get; set; }
    }
}
