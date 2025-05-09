namespace Pawnshop.Web.Models.AbsOnline
{
    public class ContractArrears
    {
        /// <summary>
        /// Параметр шины <b><u>id</u></b>
        /// </summary>
        public string ContractNumber { get; set; }

        /// <summary>
        /// Параметр шины <b><u>number</u></b>
        /// </summary>
        public string Number { get; set; }

        /// <summary>
        /// Параметр шины <b><u>amount</u></b>
        /// </summary>
        public decimal Amount { get; set; }

        /// <summary>
        /// Параметр шины <b><u>totalamount</u></b>
        /// </summary>
        public decimal Totalamount { get; set; }

        /// <summary>
        /// Параметр шины <b><u>currentAmount</u></b>
        /// </summary>
        public decimal CurrentAmount { get; set; }

        /// <summary>
        /// Параметр шины <b><u>debt_main</u></b>
        /// </summary>
        public decimal DebtMain { get; set; }

        /// <summary>
        /// Параметр шины <b><u>debt_percent</u></b>
        /// </summary>
        public decimal DebtPercent { get; set; }

        /// <summary>
        /// Параметр шины <b><u>penalties</u></b>
        /// </summary>
        public decimal Penalties { get; set; }

        /// <summary>
        /// Параметр шины <b><u>pay_day_expired</u></b>
        /// </summary>
        public int PayDayExpired { get; set; }
    }
}
