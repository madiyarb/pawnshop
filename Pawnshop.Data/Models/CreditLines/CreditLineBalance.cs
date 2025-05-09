using System.Collections.Generic;
using Pawnshop.Data.Models.Contracts;

namespace Pawnshop.Data.Models.CreditLines
{
    public sealed class CreditLineBalance
    {
        /// <summary>
        /// Идентификатор договора
        /// </summary>
        public int ContractId { get; set; }

        /// <summary>
        /// Балансы договоров
        /// </summary>
        public List<ContractBalance> ContractsBalances { get; set; }

        /// <summary>
        /// Основной долг
        /// </summary>
        public decimal SummaryAccountAmount { get; set; }

        /// <summary>
        /// Проценты начисленные
        /// </summary>
        public decimal SummaryProfitAmount { get; set; }

        /// <summary>
        /// Просроченный основной долг
        /// </summary>
        public decimal SummaryOverdueAccountAmount { get; set; }

        /// <summary>
        /// Проценты просроченные
        /// </summary>
        public decimal SummaryOverdueProfitAmount { get; set; }

        /// <summary>
        /// Пеня на долг просроченный
        /// </summary>
        public decimal SummaryPenyAmount { get; set; }

        /// <summary>
        /// Итоговая сумма по основному долгу
        /// </summary>
        public decimal SummaryTotalAcountAmount { get; set; }

        /// <summary>
        /// Итоговая сумма по процентам
        /// </summary>
        public decimal SummaryTotalProfitAmount { get; set; }

        /// <summary>
        /// Расходы по договору
        /// </summary>
        public decimal SummaryExpenseAmount { get; set; }

        /// <summary>
        /// Аванс
        /// </summary>
        public decimal SummaryPrepaymentBalance { get; set; }

        /// <summary>
        /// Текущая задолжность по ОД
        /// </summary>
        public decimal SummaryRepaymentAccountAmount { get; set; }

        /// <summary>
        /// Текущая задолженность
        /// </summary>
        public decimal SummaryCurrentDebt { get; set; }

        /// <summary>
        /// Итоговая сумма для погашения текущей задолженности
        /// </summary>
        public decimal SummaryTotalRepaymentAmount { get; set; }

        /// <summary>
        /// Итоговая сумма для выкупа
        /// </summary>
        public decimal SummaryTotalRedemptionAmount { get; set; }
        /// <summary>
        /// Начисленные проценты на внебалансе
        /// </summary>
        public decimal SummaryProfitOffBalance { get; set; }

        /// <summary>
        /// Просроченные проценты на внебалансе
        /// </summary>
        public decimal SummaryOverdueProfitOffBalance { get; set; }

        /// <summary>
        /// Пеня на просроченный основной долг на внебалансе
        /// </summary>
        public decimal SummaryPenyAccountOffBalance { get; set; }

        /// <summary>
        /// Пеня на просроченные проценты на внебалансе
        /// </summary>
        public decimal SummaryPenyProfitOffBalance { get; set; }

        /// <summary>
        /// Пеня на основной долг 
        /// </summary>
        public decimal SummaryPenyAccount { get; set; }

        /// <summary>
        /// Пеня на % 
        /// </summary>
        public decimal SummaryPenyProfit { get; set; }

        /// <summary>
        /// Отсроченное вознаграждение
        /// </summary>
        public decimal SummaryDefermentProfit { get; set; }

        /// <summary>
        /// Амортизированное вознаграждение
        /// </summary>
        public decimal SummaryAmortizedProfit { get; set; }

        /// <summary>
        /// Амортизированая пеня на долг просроченный
        /// </summary>
        public decimal SummaryAmortizedDebtPenalty { get; set; }

        /// <summary>
        /// Амортизированая пеня на проценты просроченные
        /// </summary>
        public decimal SummaryAmortizedLoanPenalty { get; set; }


    }
}
