using System.Collections.Generic;

namespace Pawnshop.Web.Models.AbsOnline
{
    /// <summary>
    /// Результат расчета графика погашения
    /// </summary>
    public class CalculateScheduleViewModel
    {
        /// <summary>
        /// Параметр шины <b><u>pay_insured_value</u></b> (сумма страховки)
        /// </summary>
        public decimal InsuredAmount { get; set; }

        /// <summary>
        /// Параметр шины <b><u>pay_total_value</u></b> (итоговая сумма)
        /// </summary>
        public decimal TotalAmount { get; set; }

        /// <summary>
        /// Параметр шины <b><u>pay_client_value</u></b> (сумма на руки)
        /// </summary>
        public decimal ClientAmount { get; set; }

        /// <summary>
        /// Параметр шины <b><u>percent</u></b> (процентная ставка (месячная))
        /// </summary>
        public decimal Percent { get; set; }

        /// <summary>
        /// Параметр шины <b><u>insurance_is</u></b> (признак наличия страховки)
        /// </summary>
        public bool Insurance { get; set; }

        /// <summary>
        /// Параметр шины <b><u>insurance_economy</u></b> (экономия денежных денег при выборе страховки)
        /// </summary>
        public decimal? InsuranceEconomy { get; set; }

        /// <summary>
        /// Параметр шины <b><u>shedule</u></b> (график погашения)
        /// </summary>
        public List<PaymentScheduleViewModel> ScheduleList { get; set; }
    }
}
