using System;
using System.Collections.Generic;
using System.Text;

namespace Pawnshop.Data.Models.Mintos
{
    /// <summary>
    /// График платежей оплат инвесторам
    /// </summary>
    public class MintosInvestorScheduleItem
    {
        /// <summary>
        /// Номер платежа по порядку(1-12 например)
        /// </summary>
        public int number { get; set; }

        /// <summary>
        /// Дата платежа по графику
        /// </summary>
        public string date { get; set; }

        /// <summary>
        /// Сумма погашения ОД
        /// </summary>
        public decimal principal_amount { get; set; } = 0;

        /// <summary>
        /// Сумма погашения процентов
        /// </summary>
        public decimal interest_amount { get; set; } = 0;

        /// <summary>
        /// Сумма погашения штрафов
        /// </summary>
        public decimal delayed_amount { get; set; } = 0;

        /// <summary>
        /// Общая сумма платежа
        /// </summary>
        public decimal sum { get; set; } = 0;

        /// <summary>
        /// Остаток ОД
        /// </summary>
        public decimal total_remaining_principal { get; set; } = 0;
    }
}
