using System.Collections.Generic;

namespace Pawnshop.Data.Models.SUSNStatuses.Views
{
    public sealed class SUSNStatusListView
    {
        public List<SUSNStatusView> List { get; set; }

        public int Count { get; set; }
    }
}
