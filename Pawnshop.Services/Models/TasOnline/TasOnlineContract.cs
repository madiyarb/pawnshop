namespace Pawnshop.Services.Models.TasOnline
{
    public class TasOnlineContract
    {
        public string Id { get; set; }
        public string Number { get; set; }
        public decimal Amount { get; set; }
        public decimal TotalAmount { get; set; } = default;
        public string Note { get; set; } = string.Empty;
    }
}