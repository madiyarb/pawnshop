using System.Collections.Generic;

namespace Pawnshop.Data.Models.ApplicationOnlineCarLogItems.Views
{
    public sealed class ApplicationOnlineCarLogListItemView
    {
        public int Count { get; set; }

        public List<ApplicationOnlineCarLogItemView> Items { get; set; }
    }
}
