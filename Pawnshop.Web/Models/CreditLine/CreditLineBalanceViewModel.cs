using System.Collections.Generic;
using Pawnshop.Data.Models.CreditLines;

namespace Pawnshop.Web.Models.CreditLine
{
    public sealed class CreditLineBalanceViewModel
    {
        /// <summary>
        /// Идентификатор договора
        /// </summary>
        public int ContractId { get; set; }

        /// <summary>
        /// Текущая задолженность
        /// </summary>
        public decimal SummaryCurrentDebt { get; set; }

        /// <summary>
        /// Расходы по договору
        /// </summary>
        public decimal SummaryExpenseAmount { get; set; }

        /// <summary>
        /// Аванс
        /// </summary>
        public decimal SummaryPrepaymentBalance { get; set; }

        /// <summary>
        /// Балансы договора
        /// </summary>
        public List<ContractBalance> ContractBalances { get; set; }

        /// <summary>
        /// Список сумм перевода ДС для договоров 
        /// </summary>
        public List<CreditLineTransferViewModel> Transfers { get; set; }
    }
}
