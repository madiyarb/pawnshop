using System.Collections.Generic;

namespace Pawnshop.Data.Models.OnlineTasks.Views
{
    public sealed class OnlineTaskListView
    {
        public IEnumerable<OnlineTaskView> Tasks { get; set; }

        public int Count { get; set; }

        public void FillData()
        {
            foreach (var task in Tasks)
            {
                task.FillStatuses();
            }
        }
    }
}
