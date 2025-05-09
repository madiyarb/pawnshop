namespace Pawnshop.Web.Models.ClientRequisites.Bindings
{
    public sealed class ClientRequisitesBillBinding
    {
        public string Note { get; set; }
        public bool IsDefault { get; set; }
        public int BankId { get; set; }
        public string IBAN { get; set; }
    }
}
