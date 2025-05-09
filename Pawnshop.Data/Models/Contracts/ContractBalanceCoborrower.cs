namespace Pawnshop.Data.Models.Contracts
{
    public class ContractBalanceCoborrower
    {
        public int ClientId { get; set; }
        public decimal AccountAmount { get; set; }
        public decimal OverdueAccountAmount { get; set; }
    }
}
