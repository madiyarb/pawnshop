namespace Pawnshop.Services.ApplicationOnlineRefinances
{
    public sealed class ApplicationOnlineRefinancedContractInfo
    {
        /// <summary>
        /// Идентификатор контракта
        /// </summary>
        public int ContractId { get; set; }

        /// <summary>
        /// Идентификатор договора
        /// </summary>
        public string ContractNumber { get; set; }

        /// <summary>
        /// Текущая задолжность 
        /// </summary>
        public decimal CurrentDebt { get; set; } 

        /// <summary>
        /// Аванс
        /// </summary>
        public decimal PrepaymentBalance { get; set; }

        /// <summary>
        /// Основной долг
        /// </summary>
        public decimal AccountAmount { get; set; }

        /// <summary>
        /// Пеня
        /// </summary>
        public decimal PenyAmount { get; set; }

        /// <summary>
        /// Проценты начисленные
        /// </summary>
        public decimal PercentAmount { get; set; }

        /// <summary>
        /// Количество дней просрочки
        /// </summary>
        public int ExpiredDays { get; set; }

        /// <summary>
        /// Это кредитная линия 
        /// </summary>
        public bool IsCreditLine { get; set; }

        /// <summary>
        /// Задолжность итого 
        /// </summary>
        public decimal TotalRedemptionAmount { get; set; }

        /// <summary>
        /// Рефинанс обязателен
        /// </summary>
        public bool RefinanceRequired { get; set; }

        /// <summary>
        /// Выбрано для рефинасирования 
        /// </summary>
        public bool CheckedForRefinance { get; set; }

        /// <summary>
        /// Идентификатор кредитной линии
        /// </summary>
        public int? CreditLineId { get; set; }

    }
}
