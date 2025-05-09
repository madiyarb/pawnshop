using System;
using System.Collections.Generic;
using System.Text;

namespace Pawnshop.Data.Models.ApplicationsOnlineEstimation.Views
{
    public sealed class ApplicationsOnlineEstimationListView
    {
        public int Count { get; set; }

        public List<ApplicationsOnlineEstimationView> Estimations { get; set; }

        public void FillStatuses()
        {
            foreach (var estimate in Estimations)
            {
                estimate.FillStatuses();
            }
        }
    }
}
