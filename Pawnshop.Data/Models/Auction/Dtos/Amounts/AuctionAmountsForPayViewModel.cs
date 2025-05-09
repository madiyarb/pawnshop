namespace Pawnshop.Data.Models.Auction.Dtos.Amounts
{
    /// <summary>
    /// Расчёт сумм которые подлежат оплате(при наличии) при выкупе по Аукциону
    /// </summary>
    public class AuctionAmountsForPayViewModel
    {
        /// <summary>
        /// Неоплаченные расходы по договору подлежащие оплате
        /// </summary>
        public decimal UnpaidExpensesForPay { get; set; } = 0.00m;
        
        /// <summary>
        /// Просроченный основной долг подлежащий оплате
        /// </summary>
        public decimal OverdueAccountForPay { get; set; } = 0.00m;
        
        /// <summary>
        /// Остаток основного долга подлежащий оплате
        /// </summary>
        public decimal AccountAmountForPay { get; set; } = 0.00m;
        
        /// <summary>
        /// Проценты просроченные подлежащие оплате
        /// </summary>
        public decimal OverdueProfitForPay { get; set; } = 0.00m;
        
        /// <summary>
        /// Проценты начисленные подлежащие оплате
        /// </summary>
        public decimal ProfitForPay { get; set; } = 0.00m;
        
        /// <summary>
        /// Пеня на долг просроченный подлежащая оплате
        /// </summary>
        public decimal PenyAccountForPay { get; set; }
        
        /// <summary>
        /// Пеня на проценты просроченные подлежащие оплате
        /// </summary>
        public decimal PenyProfitForPay { get; set; }
    }
}