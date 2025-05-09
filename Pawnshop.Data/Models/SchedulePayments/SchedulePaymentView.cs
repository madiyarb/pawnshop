using System;

namespace Pawnshop.Data.Models.SchedulePayments
{
    public class SchedulePaymentView
    {
        /// <summary>
        /// Сумма платежа
        /// </summary>
        public decimal Amount { get; set; }

        /// <summary>
        /// Дата платежа
        /// </summary>
        public DateTime Date { get; set; }

        /// <summary>
        /// Номер платежа
        /// </summary>
        public int Number { get; set; }

        /// <summary>
        /// Погашение по основному долгу
        /// </summary>
        public decimal PrincipalDebt { get; set; }

        /// <summary>
        /// Остаток от основного долга
        /// </summary>
        public decimal PrincipalDebtLeft { get; set; }

        /// <summary>
        /// Погашение по проценту
        /// </summary>
        public decimal ProfitAmount { get; set; }
    }
}
