namespace Pawnshop.Data.Models.Auction.Dtos.Amounts
{
    /// <summary>
    /// Расчёт сумм которые будут списаны(при наличии) при выкупе по Аукциону
    /// </summary>
    public class AuctionAmountsToWithdrawViewModel
    {
        /// <summary>
        /// Неоплаченные расходы по договору которые будут списаны(при наличии)
        /// </summary>
        public decimal UnpaidExpensesToWithdraw { get; set; } = 0.00m;
        
        /// <summary>
        /// Просроченный основной долг который будет списан(при наличии)
        /// </summary>
        public decimal OverdueAccountToWithdraw { get; set; } = 0.00m;
        
        /// <summary>
        /// Остаток основного долга который будет списан(при наличии)
        /// </summary>
        public decimal AccountAmountToWithdraw { get; set; } = 0.00m;
        
        /// <summary>
        /// Проценты просроченные которые будут списаны(при наличии)
        /// </summary>
        public decimal OverdueProfitToWithdraw { get; set; } = 0.00m;
        
        /// <summary>
        /// Проценты начисленные которые будут списаны(при наличии)
        /// </summary>
        public decimal ProfitToWithdraw { get; set; } = 0.00m;
        
        /// <summary>
        /// Пеня на долг просроченный которая будет списана(при наличии)
        /// </summary>
        public decimal PenyAccountToWithdraw { get; set; } = 0.00m;
        
        /// <summary>
        /// Пеня на проценты просроченные которая будет списана(при наличии)
        /// </summary>
        public decimal PenyProfitToWithdraw { get; set; } = 0.00m;
    }
}