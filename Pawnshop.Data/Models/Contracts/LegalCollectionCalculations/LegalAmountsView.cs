using System;
using System.Collections.Generic;
using System.Text;

namespace Pawnshop.Data.Models.Contracts.LegalCollectionCalculations
{
    public class LegalAmountsViewModel
    {
        /// <summary>
        /// Сумма на авансе.
        /// </summary>
        public decimal DepositAmount { get; set; }

        /// <summary>
        /// Сумма для оплаты всех задолженностей по Legal.
        /// </summary>
        public decimal TotalDebtAmount { get; set; }

        /// <summary>
        /// Сумма для выкупа по Legal.
        /// </summary>
        public decimal RedemptionAmount { get; set; }

        /// <summary>
        /// Сумма для оплаты/продления к внесению клиентом (за минусом аванса)
        /// </summary>
        public decimal AmountPayMinusDepo { get; set; }

        /// <summary>
        /// Сумма для выкупр к внесению клиентом (за минусом аванса)
        /// </summary>
        public decimal RedemptionAmountMinusDepo { get; set; }

        public bool DisplayConditionForZeroSum { get; set; } = false;
    }
}