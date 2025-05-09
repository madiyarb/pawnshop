using System.Collections.Generic;

namespace Pawnshop.Data.Models.Clients.Views
{
    public sealed class ClientRequisiteListView
    {
        public List<ClientRequisiteCardView> ClientRequisiteCards { get; set;}

        public List<ClientRequisiteBillView> ClientRequisiteBills { get; set;}
    }
}
