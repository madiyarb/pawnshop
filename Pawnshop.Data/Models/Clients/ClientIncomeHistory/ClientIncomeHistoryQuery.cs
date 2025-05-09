namespace Pawnshop.Data.Models.Clients.ClientIncomeHistory
{
    public class ClientIncomeHistoryQuery
    {
        public string DomainCode { get; set; }
        public int Offset { get; set; } = 0;
        public int Limit { get; set; } = 3;
    }
}
