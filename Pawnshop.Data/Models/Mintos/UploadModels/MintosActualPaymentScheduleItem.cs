using System;
using System.Collections.Generic;
using System.Text;

namespace Pawnshop.Data.Models.Mintos
{
    public class MintosActualPaymentScheduleItem
    {
        /// <summary>
        /// Дата платежа по договору
        /// </summary>
        public string date { get; set; }

        /// <summary>
        /// Сумма платежа по договору
        /// </summary>
        public decimal amount { get; set; }
    }
}
