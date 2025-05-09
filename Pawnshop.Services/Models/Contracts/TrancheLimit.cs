using System;
using System.Collections.Generic;
using System.Text;

namespace Pawnshop.Services.Models.Contracts
{
    public class TrancheLimit
    {
        /// <summary>
        /// максимальная сумма транша по категории
        /// </summary>
        public decimal MaxSumWithLimit { get; set; } = 0;
        /// <summary>
        /// максимально возможная сумма транша с учетом ОД кредитной линии
        /// </summary>
        public decimal MaxTrancheSum { get; set; } = 0;
        /// <summary>
        /// добавочный лимит
        /// </summary>
        public decimal AdditionalLimit { get; set; } = 0;
        /// <summary>
        /// максимально доступная сумма по motorCost
        /// </summary>
        public decimal MaxAvailableAmount { get; set; } = 0;
    }
}
