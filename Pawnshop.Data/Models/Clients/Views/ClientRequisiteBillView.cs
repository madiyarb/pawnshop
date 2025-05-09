namespace Pawnshop.Data.Models.Clients.Views
{
    public sealed class ClientRequisiteBillView : ClientRequisiteView
    {
        public int BankId { get; set; }
        public string BankName { get; set; }
        public string BankCode { get; set; }

        public string IBAN
        {
            get { return Value; }
            set { Value = value; }
        }
    }
}
