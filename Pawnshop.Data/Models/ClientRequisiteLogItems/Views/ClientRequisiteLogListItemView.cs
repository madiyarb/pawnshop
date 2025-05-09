using System.Collections.Generic;

namespace Pawnshop.Data.Models.ClientRequisiteLogItems.Views
{
    public sealed class ClientRequisiteLogListItemView
    {
        public int Count { get; set; }

        public List<ClientRequisiteLogItemView> Items { get; set; }
    }
}
