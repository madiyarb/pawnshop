using System;
using Newtonsoft.Json;

namespace Pawnshop.Data.Models.Contracts.Views
{
    public sealed class ContractBalanceOnlineView
    {
        /// <summary>
        /// Идентификатор договора
        /// </summary>
        public int ContractId { get; set; }

        /// <summary>
        /// Основной долг
        /// </summary>
        public decimal AccountAmount { get; set; }

        /// <summary>
        /// Проценты начисленные(Начисленное вознаграждение)
        /// </summary>
        public decimal ProfitAmount { get; set; }

        /// <summary>
        /// Просроченный основной долг
        /// </summary>
        public decimal OverdueAccountAmount { get; set; }

        /// <summary>
        /// Проценты просроченные(просроченное вознаграждение)
        /// </summary>
        public decimal OverdueProfitAmount { get; set; }

        /// <summary>
        /// Пеня
        /// </summary>
        public decimal PenyAmount { get; set; }

        /// <summary>
        /// Пеня на ОД
        /// </summary>
        public decimal PenyAccountAmount { get; set; }

        /// <summary>
        /// Пеня на проценты
        /// </summary>
        public decimal PenyProfitAmount { get; set; }

        /// <summary>
        /// Итоговая сумма по основному долгу
        /// </summary>
        public decimal TotalAccountAmount { get; set; }

        /// <summary>
        /// Итоговая сумма по процентам
        /// </summary>
        public decimal TotalProfitAmount { get; set; }

        /// <summary>
        /// Расходы по договору
        /// </summary>
        public decimal ExpenseAmount { get; set; }

        /// <summary>
        /// Аванс
        /// </summary>
        public decimal PrepaymentBalance { get; set; }

        /// <summary>
        /// Текущая задолженность
        /// </summary>
        public decimal CurrentDebt { get; set; }

        /// <summary>
        /// Итоговая сумма для погашения текущей задолженности
        /// </summary>
        public decimal TotalRepaymentAmount { get; set; }

        /// <summary>
        /// Итоговая сумма для выкупа
        /// </summary>
        public decimal TotalRedemptionAmount { get; set; }

        /// <summary>
        /// Суммая для платежа в этом месяце 
        /// </summary>
        public decimal MonthPaySum { get; set; }

        /// <summary>
        /// Дата следующего платежа 
        /// </summary>
        [JsonIgnore]
        public DateTime NextPaymentDateDateTime { get; set; }

        public string NextPaymentDate { get; set; }

        /// <summary>
        /// Лимит кредитной линии 
        /// </summary>
        public decimal CreditLineLimit { get; set; }

        /// <summary>
        /// Остаток лимита кл
        /// </summary>
        public decimal AvailableCreditLineLimit { get; set; }

        /// <summary>
        /// Использованый лимит кредитной линии
        /// </summary>
        public decimal UsedCreditLineLimit { get; set; }

        /// <summary>
        /// Сумма первоначального ОД всех выданых Траншей/договоров
        /// </summary>
        public decimal TotalLoanAmount { get; set; }

        /// <summary>
        /// Первоначальный лимит КЛ
        /// </summary>
        public decimal InitialCreditLineLimit { get; set; }

        /// <summary>
        /// Сумма долга 
        /// </summary>
        public decimal DebtCost { get; set; }

        /// <summary>
        /// Дата выкупа
        /// </summary>
        public string BuyOutDate { get; set; }

        /// <summary>
        /// Дни просрочки
        /// </summary>
        public int DaysOverdue { get; set; }
    }
}
