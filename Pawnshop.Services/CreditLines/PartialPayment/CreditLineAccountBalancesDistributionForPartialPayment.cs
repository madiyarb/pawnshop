using System.Collections.Generic;

namespace Pawnshop.Services.CreditLines.PartialPayment
{
    public sealed class CreditLineAccountBalancesDistributionForPartialPayment : CreditLineAccountBalancesDistribution
    {
        /// <summary>
        /// Сумма начисленных процентов на текущую дату
        /// </summary>
        public decimal CurrentProfit { get; set; }

        /// <summary>
        /// Сумма ЧДП
        /// </summary>
        public decimal PartialPaymentAmount { get; set; }

        /// <summary>
        /// Основной долг после ЧДП
        /// </summary>
        public decimal DebtAfterPayment { get; set; }

        /// <summary>
        /// Денег хватит на оплату текущей задолжности
        /// </summary>
        public bool EnoughFundsForPayment { get; set; }

        /// <summary>
        /// Денег хватит на выкуп
        /// </summary>
        public bool EnoughFundsForBuyOut { get; set; }


        public List<CreditLineTransferPartialPayment> CreditLineTransfers { get; set; }
    }
}
