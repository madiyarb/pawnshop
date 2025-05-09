namespace Pawnshop.Web.Models.ApplicationOnline
{
    public class ApplicationOnlineFinanceInfo
    {
        /// <summary>
        /// Среднемесячный доход за 6 мес со слов клиента
        /// </summary>
        public decimal IncomeAmount { get; set; }

        /// <summary>
        /// Среднемесячный доход за 6 мес со слов клиента скорректированный менеджером
        /// </summary>
        public decimal CorrectedIncomeAmount { get; set; }

        /// <summary>
        /// Расходы по прочим платежам со слов клиента
        /// </summary>
        public decimal ExpenseAmount { get; set; }

        /// <summary>
        /// Расходы по прочим платежам со слов клиента скорректированный менеджером
        /// </summary>
        public decimal CorrectedExpenseAmount { get; set; }
    }
}
