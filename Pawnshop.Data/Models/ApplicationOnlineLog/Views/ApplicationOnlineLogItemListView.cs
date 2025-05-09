using System.Collections.Generic;

namespace Pawnshop.Data.Models.ApplicationOnlineLog.Views
{
    public sealed class ApplicationOnlineLogItemListView
    {
        public int Count { get; set; }
        public List<ApplicationOnlineLogItemView> Items { get; set; }
    }
}
