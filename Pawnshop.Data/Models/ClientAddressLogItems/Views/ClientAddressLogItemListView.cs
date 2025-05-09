using System.Collections.Generic;

namespace Pawnshop.Data.Models.ClientAddressLogItems.Views
{
    public sealed class ClientAddressLogItemListView
    {
        public int Count { get; set; }
        public List<ClientAddressLogItemView> Items { get; set; }
    }
}
