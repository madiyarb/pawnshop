using System.Collections.Generic;

namespace Pawnshop.Data.Models.ClientDocumentLogItems.Views
{
    public sealed class ClientDocumentLogListItemView
    {
        public int Count { get; set; }
        public List<ClientDocumentLogItemView> Items { get; set; }
    }
}
