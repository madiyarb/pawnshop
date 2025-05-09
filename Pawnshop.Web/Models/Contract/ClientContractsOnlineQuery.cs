namespace Pawnshop.Web.Models.Contract
{
    public class ClientContractsOnlineQuery
    {
        public bool? IsActive { get; set; }

        public int? ContractClass { get; set; }

        public int? ContractId { get; set; }

        public int? CreditLineId { get; set; }
    }
}
