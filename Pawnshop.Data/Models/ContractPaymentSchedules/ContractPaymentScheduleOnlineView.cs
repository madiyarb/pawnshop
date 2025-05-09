using System.Collections.Generic;

namespace Pawnshop.Data.Models.ContractPaymentSchedules
{
    public class ContractPaymentScheduleOnlineView
    {
        /// <summary>
        /// Порядковый номер
        /// </summary>
        public int Number { get; set; }


        /// <summary>
        /// Номер договора 
        /// </summary>
        public string ContractNumber { get; set; }

        /// <summary>
        /// Дата оплаты
        /// </summary>
        public string Date { get; set; }
        /// <summary>
        /// Фактическая дата оплаты
        /// </summary>
        public string? ActualDate { get; set; }

        /// <summary>
        /// Остаток основного долга
        /// </summary>
        public decimal DebtLeft { get; set; }
        /// <summary>
        /// Основной долг
        /// </summary>
        public decimal DebtCost { get; set; }
        /// <summary>
        /// Процент
        /// </summary>
        public decimal PercentCost { get; set; }
        /// <summary>
        /// Штраф
        /// </summary>
        public decimal PenaltyCost { get; set; }

        /// <summary>
        /// Ежемесячный платеж 
        /// </summary>
        public decimal MonthlyPayment { get; set; }

        /// <summary>
        /// Итоговый ежемесячный платеж с учетом реструктуризации 
        /// </summary>
        public decimal TotalMonthlyPayment { get; set; }

        /// <summary>
        /// Статус платежа
        /// </summary>
        public int Status { get; set; }

        /// <summary>
        /// Дней просрочки 
        /// </summary>
        public int DaysOverdue { get; set; }
    }
}
