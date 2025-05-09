namespace Pawnshop.Web.Models.AbsOnline
{
    public class ServiceViewModel
    {
        /// <summary>
        /// Параметр шины <b><u>name</u></b>
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Параметр шины <b><u>pay_service_value</u></b>
        /// </summary>
        public decimal PayServiceValue { get; set; }

        /// <summary>
        /// Параметр шины <b><u>pay_total_value</u></b>
        /// </summary>
        public decimal PayTotalValue { get; set; }

        /// <summary>
        /// Параметр шины <b><u>application_value</u></b>
        /// </summary>
        public decimal LoanCost { get; set; }

        /// <summary>
        /// Параметр шины <b><u>insurance</u></b>
        /// </summary>
        public bool Insurance { get; set; }

        /// <summary>
        /// Параметр шины <b><u>percent</u></b>
        /// </summary>
        public decimal Percent { get; set; }
    }
}
