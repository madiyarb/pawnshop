namespace Pawnshop.Data.Models.Contracts
{
    public class ContractBalance
    {
        /// <summary>
        /// Идентификатор договора
        /// </summary>
        public int ContractId { get; set; }

        /// <summary>
        /// Основной долг
        /// </summary>
        public decimal AccountAmount { get; set; }

        /// <summary>
        /// Проценты начисленные(Начисленное вознаграждение)
        /// </summary>
        public decimal ProfitAmount { get; set; }

        /// <summary>
        /// Просроченный основной долг
        /// </summary>
        public decimal OverdueAccountAmount { get; set; }

        /// <summary>
        /// Проценты просроченные(просроченное вознаграждение)
        /// </summary>
        public decimal OverdueProfitAmount { get; set; }

        /// <summary>
        /// Пеня
        /// </summary>
        public decimal PenyAmount { get; set; }

        /// <summary>
        /// Итоговая сумма по основному долгу
        /// </summary>
        public decimal TotalAcountAmount { get; set; }

        /// <summary>
        /// Итоговая сумма по процентам
        /// </summary>
        public decimal TotalProfitAmount { get; set; }

        /// <summary>
        /// Расходы по договору
        /// </summary>
        public decimal ExpenseAmount { get; set; }

        /// <summary>
        /// Аванс
        /// </summary>
        public decimal PrepaymentBalance { get; set; }

        /// <summary>
        /// Текущая задолженность
        /// </summary>
        public decimal CurrentDebt { get; set; }

        /// <summary>
        /// Итоговая сумма для погашения текущей задолженности
        /// </summary>
        public decimal TotalRepaymentAmount { get; set; }

        /// <summary>
        /// Итоговая сумма для выкупа
        /// </summary>
        public decimal TotalRedemptionAmount { get; set; }

        /// <summary>
        /// Лимит кредитной линии 
        /// </summary>
        public decimal CreditLineLimit { get; set; }

        /// <summary>
        /// Пеня на ОД
        /// </summary>
        public decimal PenyAccount { get; set; }
        /// <summary>
        /// Пеня на проценты
        /// </summary>
        public decimal PenyProfit { get; set; }
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
    }
}
