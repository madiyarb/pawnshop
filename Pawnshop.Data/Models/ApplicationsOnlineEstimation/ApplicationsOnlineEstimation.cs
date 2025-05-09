using System;

namespace Pawnshop.Data.Models.ApplicationsOnlineEstimation
{
    public sealed class ApplicationsOnlineEstimation
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

        public ApplicationsOnlineEstimation() { }


        public ApplicationsOnlineEstimation(Guid applicationOnlineId, int? estimationServiceСlientId, int? estimationServicePledgeId, int? estimationServiceApplyId)
        {
            Id = Guid.NewGuid();
            Status = ApplicationOnlineEstimationStatus.OnEstimation.ToString();
            ApplicationOnlineId = applicationOnlineId;
            if (estimationServiceСlientId == null)
            {
                EstimationServiceСlientId = 0;
            }
            else
            {
                EstimationServiceСlientId = estimationServiceСlientId.Value;
            }

            if (estimationServicePledgeId == null)
            {
                EstimationServicePledgeId = 0;
            }
            else
            {
                EstimationServicePledgeId = estimationServicePledgeId.Value;

            }

            if (estimationServiceApplyId == null)
            {
                EstimationServiceApplyId = 0;
            }
            else
            {
                EstimationServiceApplyId = estimationServiceApplyId.Value;
            }
            CreateDate = DateTime.Now;
            UpdateDate = DateTime.Now;
        }
    }
}
