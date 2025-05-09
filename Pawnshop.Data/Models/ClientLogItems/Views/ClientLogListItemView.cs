using System.Collections.Generic;

namespace Pawnshop.Data.Models.ClientLogItems.Views
{
    public sealed class ClientLogListItemView
    {
        public int Count { get; set; }
        public List<ClientLogItemView> Items { get; set; }
    }
}
