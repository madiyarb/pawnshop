using System;
using System.Collections.Generic;
using System.Text;

namespace Pawnshop.Data.Models.MobileApp
{
    public class ContractDataForTranche
    {
        /// <summary>
        /// Идентификатор родительского договора
        /// </summary>
        public int? ParentContractId { get; set; }
        /// <summary>
        /// Информация по машине
        /// </summary>
        public string Car { get; set; }
        /// <summary>
        /// Номер Договора
        /// </summary>
        public string ContractNumber { get; set; }
        /// <summary>
        /// Сумма кредита на машину
        /// </summary>
        public decimal LoanCost { get; set; }
        /// <summary>
        /// Остаток кредита
        /// </summary>
        public decimal DebtLeftCost { get; set; }
        /// <summary>
        /// Основной долг (ОД)
        /// </summary>
        public decimal MainDebt { get; set; }
        /// <summary>
        /// Сумма выкупа
        /// </summary>
        public decimal BuyoutAmount { get; set; }
        /// <summary>
        /// Количество просроченных дни 
        /// </summary>
        public int DelayDayCount { get; set; }
        /// <summary>
        /// Сумма оценки кредита
        /// </summary>
        public decimal EstimatedCost { get; set; }  
    }
}
