using System.Collections.Generic;

namespace Pawnshop.Data.Models.ApplicationsOnline.Views
{
    public sealed class ApplicationOnlineListView
    {
        public IEnumerable<ApplicationOnlineListItemView> List { get; set; }
        public int Count { get; set; }

        public void FillAdditionalInfo()
        {
            foreach (var item in List)
            {
                item.FillKey();
                item.FillStatuses();
                item.FillTimes();
            }
        }
    }
}
