namespace Pawnshop.Data.Models.Contracts.Views
{
    public sealed class ContractOnlineBuyoutAvailableView
    {

        /// <summary>
        /// Может быть выполнен
        /// </summary>
        public bool CanBeProcessed { get; set; }

        /// <summary>
        /// Описание ответа
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Технические причины
        /// </summary>
        public bool TechnicalIssues { get; set; }

        /// <summary>
        /// Авансовый счет 
        /// </summary>
        public decimal AdvanceAccountBalance { get; set; }
        
        /// <summary>
        /// Сумма необходимая для выкупа
        /// </summary>
        public decimal BuyoutAmount { get; set; }

        /// <summary>
        /// Процент
        /// </summary>
        public decimal Percent { get; set; }

        /// <summary>
        /// Основной долг
        /// </summary>
        public decimal MainDebt { get; set; }

        /// <summary>
        /// Автоматически закроется 
        /// </summary>
        public bool AutoClose { get; set; }

        /// <summary>
        /// Текущая задолжность
        /// </summary>
        public decimal CurrentDebt { get; set; }

        /// <summary>
        /// Необходимо еще денег для осуществления выкупа
        /// </summary>
        public decimal HowAdditionalManyMoneyNeedForBuyOut { get; set; }

        /// <summary>
        /// Договор будет выкуплен автоматически в близжайшее время 
        /// </summary>
        public bool AutoBuyOutCommingSoon { get; set; }

        public string ReasonCode { get; set; }

    }
}
