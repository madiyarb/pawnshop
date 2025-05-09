namespace Pawnshop.Data.Models.CreditLines
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
        /// Проценты начисленные
        /// </summary>
        public decimal ProfitAmount { get; set; }

        /// <summary>
        /// Просроченный основной долг
        /// </summary>
        public decimal OverdueAccountAmount { get; set; }

        /// <summary>
        /// Проценты просроченные
        /// </summary>
        public decimal OverdueProfitAmount { get; set; }

        /// <summary>
        /// Пеня на основной долг 
        /// </summary>
        public decimal PenyAccount { get; set; }

        /// <summary>
        /// Пеня на % 
        /// </summary>
        public decimal PenyProfit { get; set; }

        /// <summary>
        /// Пеня на долг просроченный
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
        /// Текущая задолжность по ПО ОД
        /// </summary>
        public decimal RepaymentAccountAmount { get; set; }

        /// <summary>
        /// Текущая задолжность по ПО %
        /// </summary>
        public decimal RepaymentProfitAmount { get; set; }

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
        /// Начисленные проценты на внебалансе
        /// </summary>
        public decimal ProfitOffBalance { get; set; }

        /// <summary>
        /// Просроченные проценты на внебалансе
        /// </summary>
        public decimal OverdueProfitOffBalance { get; set; }

        /// <summary>
        /// Пеня на просроченный основной долг на внебалансе
        /// </summary>
        public decimal PenyAccountOffBalance { get; set; }

        /// <summary>
        /// Пеня на просроченные проценты на внебалансе
        /// </summary>
        public decimal PenyProfitOffBalance { get; set; }

        /// <summary>
        /// Номер договора
        /// </summary>
        public string ContractNumber { get; set; }

        /// <summary>
        /// Ведуться начисления на внебаланс
        /// </summary>
        public bool IsOffBalance { get; set; }

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
        public decimal AmortizedPenyAccount { get; set; }

        /// <summary>
        /// Амортизированая пеня на проценты просроченные
        /// </summary>
        public decimal AmortizedPenyProfit { get; set; }
    }
}
