using System;

namespace Pawnshop.Web.Models.AbsOnline
{
    public class UnsetFcbContractViewModel
    {
        /// <summary>
        /// Параметр шины <b><u>application_id</u></b> (номер займа)
        /// </summary>
        public string ContractNumber { get; set; }

        /// <summary>
        /// Параметр шины <b><u>date</u></b> (дата открытия)
        /// </summary>
        public DateTime? Date { get; set; }

        /// <summary>
        /// Параметр шины <b><u>value</u></b> (сумма займа)
        /// </summary>
        public decimal LoanCost { get; set; }
    }
}
