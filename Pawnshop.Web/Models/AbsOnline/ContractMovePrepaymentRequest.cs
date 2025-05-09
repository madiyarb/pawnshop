namespace Pawnshop.Web.Models.AbsOnline
{
    public sealed class ContractMovePrepaymentRequest
    {
        public string SourceContractNumber { get; set; }
        public string RecipientContractNumber { get; set; }
        public string Amount { get; set; }
        public string Note { get; set; } = string.Empty;
    }
}
