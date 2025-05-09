using System.Collections.Generic;

namespace Pawnshop.Data.Models.ApplicationOnlineFileLogItems.Views
{
    public sealed class ApplicationOnlineFileLogItemListView
    {
        public int Count { get; set; }

        public List<ApplicationOnlineFileLogItemView> Items { get; set; }
    }
}
