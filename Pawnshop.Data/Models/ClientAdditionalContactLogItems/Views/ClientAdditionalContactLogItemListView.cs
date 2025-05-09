using System.Collections.Generic;

namespace Pawnshop.Data.Models.ClientAdditionalContactLogItems.Views
{
    public sealed class ClientAdditionalContactLogItemListView
    {
        public int Count { get; set; }
        public List<ClientAdditionalContactLogItemView> Items { get; set; }
    }
}
