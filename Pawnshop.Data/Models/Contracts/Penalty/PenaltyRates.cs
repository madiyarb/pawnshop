using System;
using System.Collections.Generic;
using System.Text;

namespace Pawnshop.Data.Models.Contracts.Penalty
{
    public class PenaltyRates
    {
        /// <summary>
        /// Дата начала действия ставки
        /// </summary>
        public DateTime Date { get; set; }
        /// <summary>
        /// Ставка
        /// </summary>
        public decimal Rate { get; set; }
        /// <summary>
        /// Сумма просрочки
        /// </summary>
        public decimal? OverdueSum { get; set; }

        public bool IsFromOverdue { get; set; } = false;
        
        public PenaltyRates(DateTime date, decimal? overdueSum, decimal rate, bool isFromOverdue)
        {
            Date = date;
            OverdueSum = overdueSum;
            Rate = rate;
            IsFromOverdue = isFromOverdue;
        }
    }
}
