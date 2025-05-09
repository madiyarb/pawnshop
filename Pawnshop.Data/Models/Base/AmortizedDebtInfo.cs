using Newtonsoft.Json;
using Pawnshop.Data.Models.ClientDeferments;
using System;
using System.Collections.Generic;
using System.Text;

namespace Pawnshop.Data.Models.Base
{
    /// <summary>
    /// Амортизированое задолженность
    /// </summary>
    public class AmortizedDebtInfo
    {
        /// <summary>
        /// Отсроченное вознаграждение
        /// </summary>
        public decimal DefermentProfit { get; set; }

        /// <summary>
        /// Амортизированное вознаграждение
        /// </summary>
        public decimal AmortizedProfit { get; set; }

        /// <summary>
        /// Амортизированая пеня на долг просроченный
        /// </summary>
        public decimal AmortizedDebtPenalty { get; set; }

        /// <summary>
        /// Амортизированая пеня на проценты просроченные
        /// </summary>
        public decimal AmortizedLoanPenalty { get; set; }

        /// <summary>
        /// Отсрочка платежа для клиента
        /// </summary>
        public ClientDeferment ClientDeferment { get; set; }
    }
}
