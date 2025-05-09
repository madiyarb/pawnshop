using System.Xml.Linq;

namespace Pawnshop.Data.Models.Clients.Views
{
    public sealed class ClientRequisiteCardView : ClientRequisiteView
    {
        public string CardExpiryDate { get; set; }
        public string CardHolderName { get; set; }

        public string CardNumber
        {
            get { return Value; }
            set { Value = value; }
        }
    }
}
