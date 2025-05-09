using System;
using System.Collections.Generic;

namespace Pawnshop.Data.Models.SchedulePayments
{
    public class ScheduleInfoView
    {
        /// <summary>
        /// Сумма клиенту на руки
        /// </summary>
        public decimal ClientAmount { get; set; }

        /// <summary>
        /// Дата первого платежа
        /// </summary>
        public DateTime FirstPaymentDate { get; set; }

        /// <summary>
        /// Страховка
        /// </summary>
        public bool Insurance { get; set; }

        /// <summary>
        /// Экономия при страховке (может быть отрицательной)
        /// </summary>
        public decimal InsuranceEconomy { get; set; }

        /// <summary>
        /// Сумма страховки
        /// </summary>
        public decimal InsuredAmount { get; set; }

        /// <summary>
        /// Сумма первого платежа (ежемесячный платеж)
        /// </summary>
        public decimal PaymentAmount { get; set; }

        /// <summary>
        /// Процентная ставка (месячная)
        /// </summary>
        public decimal Percent { get; set; }

        /// <summary>
        /// Сумма кредита итого
        /// </summary>
        public decimal TotalAmount { get; set; }

        /// <summary>
        /// График платежей
        /// </summary>
        public List<SchedulePaymentView> ScheduleList { get; set; }
    }
}
