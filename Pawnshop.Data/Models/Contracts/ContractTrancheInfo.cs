using Pawnshop.Data.Models.Base;
using System;

namespace Pawnshop.Data.Models.Contracts
{
    /// <summary>
    /// Информация о транше договора
    /// </summary>
    public class ContractTrancheInfo
    {
        /// <summary>
        /// Идентификатор
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Номер
        /// </summary>
        public string ContractNumber { get; set; }

        /// <summary>
        /// Дата
        /// </summary>
        public DateTime ContractDate { get; set; }

        /// <summary>
        /// Сумма займа
        /// </summary>
        public decimal LoanCost { get; set; }

        /// <summary>
        /// Статус (для фронта)
        /// </summary>
        public ContractDisplayStatus Status { get; set; }

        /// <summary>
        /// Амортизированое задолженность
        /// </summary>
        public AmortizedDebtInfo AmortizedDebtInfo { get; set;}
        /// <summary>
        /// Срочная задолженность
        /// </summary>
        public DebtInfo UrgentDebt { get; set; }

        /// <summary>
        /// Просроченная задолженность
        /// </summary>
        public DebtInfo OverdueDebt { get; set; }

        /// <summary>
        /// Итоговая задолженность
        /// </summary>
        public DebtInfo TotalDebt { get; set; }

        /// <summary>
        /// Доп. расходы
        /// </summary>
        public decimal ExpenseAmount { get; set; }
        
        /// <summary>
        /// Аванс
        /// </summary>
        public decimal PrepaymentBalance { get; set; }

        /// <summary>
        /// Итоговая сумма для погашения текущей задолженности
        /// </summary>
        public decimal TotalRepaymentAmount { get; set; }

        /// <summary>
        /// Итоговая сумма для выкупа
        /// </summary>
        public decimal TotalRedemptionAmount { get; set; }
    }
}
