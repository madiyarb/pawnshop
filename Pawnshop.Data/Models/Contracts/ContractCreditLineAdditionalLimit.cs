using System;
using System.Collections.Generic;
using System.Text;

namespace Pawnshop.Data.Models.Contracts
{
    public class ContractCreditLineAdditionalLimit
    {
        public int Id { get; set; }
        /// <summary>
        /// минимальная суммая авто
        /// </summary>
        public int LowCost { get; set; }
        /// <summary>
        /// максимальная сумма авто
        /// </summary>
        public int HighCost { get; set; }
        /// <summary>
        /// добавочный лимит для LoanCost контракта от оценки авто
        /// </summary>
        public decimal LimitPercent { get; set; }
    }
}
