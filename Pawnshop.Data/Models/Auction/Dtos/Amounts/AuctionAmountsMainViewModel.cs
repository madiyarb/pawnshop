namespace Pawnshop.Data.Models.Auction.Dtos.Amounts
{
    public class AuctionAmountsMainViewModel
    {
        /// <summary>
        /// Основной долг
        /// </summary>
        public decimal AccountAmount { get; set; }
        
        /// <summary>
        /// Предоплата
        /// </summary>
        public decimal PrePayment { get; set; }
        
        /// <summary>
        /// Неоплаченные расоды по договору КЛ
        /// </summary>
        public decimal UnpaidExpenses  { get; set; }
        
        /// <summary>
        /// Просроченный основной долг
        /// </summary>
        public decimal OverdueAccount { get; set; }
        
        /// <summary>
        /// Проценты начисленные
        /// </summary>
        public decimal Profit { get; set; }
        
        /// <summary>
        /// Проценты просроченные
        /// </summary>
        public decimal OverdueProfit { get; set; }
        
        /// <summary>
        /// Пеня на долг просроченный
        /// </summary>
        public decimal PenyAccount { get; set; }
        
        /// <summary>
        /// Пеня на проценты просроченные
        /// </summary>
        public decimal PenyProfit { get; set; }
        
        /// <summary>
        /// Общая сумма к списанию
        /// </summary>
        public decimal TotalAmount { get; set; }
        
        /// <summary>
        /// Сумма ДКП
        /// </summary>
        public decimal BuyOutSum { get; set; }
        
        /// <summary>
        /// Общая сумма списания с внебалансовых счетов
        /// </summary>
        public decimal AmountToWriteOffFromOffBalanceAccounts { get; set; }
        
        /// <summary>
        /// Общая сумма списания с балансовых счетов
        /// </summary>
        /// <returns></returns>
        public decimal AmountToWriteOffFromBalanceAccounts { get; set; }
        
        /// <summary>
        /// Сумма, подлежащая возврату заемщику
        /// </summary>
        /// <returns></returns>
        public decimal ReturnAmountToBorrower { get; set; }
    }
}