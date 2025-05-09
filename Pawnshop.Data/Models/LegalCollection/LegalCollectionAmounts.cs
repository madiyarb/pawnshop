namespace Pawnshop.Data.Models.LegalCollection
{
    public class LegalCollectionAmounts
    {
        /// <summary>
        /// Вознаграждение
        /// </summary>
        public decimal? Reward { get; set; }
        
        /// <summary>
        /// Просроченное вознаграждение
        /// </summary>
        public decimal? OverdueReward { get; set; }
        
        /// <summary>
        /// Штраф/пеня на основной долг
        /// </summary>
        public decimal? PennyPrincipalDebt { get; set; }
        
        /// <summary>
        /// Штраф/пеня на вознаграждение/проценты
        /// </summary>
        public decimal? PennyProfit { get; set; }
        
        /// <summary>
        /// Основной долг
        /// </summary>
        public decimal? AccountAmount { get; set; }
        
        /// <summary>
        /// Просроченный основной долг
        /// </summary>
        public decimal? OverdueAccountAmount { get; set; }
        
        /// <summary>
        /// Начисленные проценты на внебалансе
        /// </summary>
        public decimal? PercentOnOffBalance  { get; set; }
        
        /// <summary>
        /// Просроченные проценты на внебалансе
        /// </summary>
        public decimal? OverduePercentOnOffBalance  { get; set; }
        
        /// <summary>
        /// Пеня на просроченный основной долг на внебалансе
        /// </summary>
        public decimal? PenaltyOnOverduePrincipalDebtOnOffBalance  { get; set; }
        
        /// <summary>
        /// Пеня на просроченные проценты на внебалансе
        /// </summary>
        public decimal? PenaltyOnOverduePercentOnOffBalance  { get; set; }
    }
}