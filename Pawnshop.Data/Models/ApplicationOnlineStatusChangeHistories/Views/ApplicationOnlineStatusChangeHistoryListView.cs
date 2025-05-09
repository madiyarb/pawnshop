using System.Collections.Generic;

namespace Pawnshop.Data.Models.ApplicationOnlineStatusChangeHistories.Views
{
    public sealed class ApplicationOnlineStatusChangeHistoryListView
    {
        public List<ApplicationOnlineStatusChangeHistoryView> ApplicationOnlineStatusChangeHistories { get; set; }

        public int Count { get; set; }

        public void FillStatuses()
        {
            foreach (var history in ApplicationOnlineStatusChangeHistories)
            {
                history.FillStatuses();
            }
        }
    }
}
