namespace Pawnshop.Data.Models.Reports
{
    public class BalanceSheetReportTotal
    {
        public decimal DebitTurns { get; set; }
        public decimal CreditTurns { get; set; }
        public decimal IncomingBalance { get; set; }
        public decimal OutgoingBalance { get; set; }
        public bool PlanIsActive { get; set; }
    }
}
