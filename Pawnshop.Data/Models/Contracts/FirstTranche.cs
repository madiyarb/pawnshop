namespace Pawnshop.Data.Models.Contracts
{
    public class FirstTranche
    {
        public int? Id { get; set; }
        public decimal LoanCost { get; set; }
        public int LoanPeriod { get; set; }
        public int? SettingId { get; set; }
    }
}
