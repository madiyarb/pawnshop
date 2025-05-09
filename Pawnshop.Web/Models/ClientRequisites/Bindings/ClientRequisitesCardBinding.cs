namespace Pawnshop.Web.Models.ClientRequisites.Bindings
{
    public sealed class ClientRequisitesCardBinding
    {
        public string Note { get; set; }
        public bool IsDefault { get; set; }
        public string CardNumber { get; set; }
        public string CardExpiryDate { get; set; }
        public string CardHolderName { get; set; }
    }
}
