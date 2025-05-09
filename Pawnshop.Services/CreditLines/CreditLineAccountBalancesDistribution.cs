using System.Collections.Generic;
using Pawnshop.Data.Models.CreditLines;

namespace Pawnshop.Services.CreditLines
{
    public class CreditLineAccountBalancesDistribution
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
        /// Доступно на авансовом счету
        /// </summary>
        public decimal SummaryPrepaymentBalance { get; set; }

        /// <summary>
        /// Остаток на авансовом счету
        /// </summary>
        public decimal AfterPaymentPrepaymentBalance { get; set; }

        /// <summary>
        /// Итого с клиента
        /// </summary>
        public decimal PaymentAmountFromClient { get; set; }

        /// <summary>
        /// Итого к погашению, тг
        /// </summary> 
        public decimal TotalDue { get; set; }

        /// <summary>
        /// Балансы договора
        /// </summary>
        public List<ContractBalance> ContractBalances { get; set; }

        /// <summary>
        /// Итого к погашению, тг
        /// </summary> 
        public decimal TotalCost { get; set; }

        public Discount Discount { get; set; }

        public List<CreditLineTransfer> CreditLineTransfers { get; set;}
    }
}
