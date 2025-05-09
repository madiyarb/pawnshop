namespace Pawnshop.Services.CreditLines.Buyout
{
    public sealed class CreditLineAccountBalancesDistributionForBuyOut : CreditLineAccountBalancesDistribution
    {
        public decimal ExpenseValue { get; set; }
        public bool ContainExpense { get; set; }
        public decimal ExtraExpensesCost { get; set; }
    }
}
