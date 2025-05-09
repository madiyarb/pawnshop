namespace Pawnshop.Web.Models.AbsOnlineCardCashOut
{
    public sealed class CreateCardCashOutBinding
    {
        public int ClientId { get; set; }
        public int ContractId { get; set; }
        public string CustomerReference { get; set; }
    }
}
