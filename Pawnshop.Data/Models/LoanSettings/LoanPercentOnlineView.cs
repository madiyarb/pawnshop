namespace Pawnshop.Data.Models.LoanSettings
{
    public class LoanPercentOnlineView
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; }
        public int Insurance { get; set; }
        public decimal Percent { get; set; }
        public decimal MinPercent { get; set; }
        public decimal MaxPercent { get; set; }
        public decimal MinSum { get; set; }
        public decimal MaxSum { get; set; }
        public int? MinTrancheSum { get; set; }
        public int MinMonth { get; set; }
        public int MaxMonth { get; set; }
        public int ScheduleType { get; set; }
        public bool? isActual { get; set; }
    }
}
