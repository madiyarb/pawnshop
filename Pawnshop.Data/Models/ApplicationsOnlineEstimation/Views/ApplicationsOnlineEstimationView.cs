using System;
using Pawnshop.Core;

namespace Pawnshop.Data.Models.ApplicationsOnlineEstimation.Views
{
    public sealed class ApplicationsOnlineEstimationView
    {
        public Guid Id { get; set; }

        public Guid ApplicationOnlineId { get; set; }

        public string Status { get; set; }

        public DateTime CreateDate { get; set; }

        public DateTime UpdateDate { get; set; }

        public int EstimationServiceСlientId { get; set; }

        public int EstimationServicePledgeId { get; set; }

        public int EstimationServiceApplyId { get; set; }

        public decimal? EvaluatedAmount { get; set; }

        public decimal? IssuedAmount { get; set; }

        public string? ValuerName { get; set; }

        public void FillStatuses()
        {
            ApplicationOnlineEstimationStatus estimationStatus;
            if (Enum.TryParse(Status, out estimationStatus))
            {
                Status = estimationStatus.GetDisplayName();
            }
        }
    }
}
