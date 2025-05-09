using Pawnshop.Data.Models.ApplicationsOnline;
using System;
using Pawnshop.Core;
using Newtonsoft.Json;

namespace Pawnshop.Data.Models.ApplicationOnlineStatusChangeHistories.Views
{
    public sealed class ApplicationOnlineStatusChangeHistoryView
    {
        public Guid Id { get; set; }

        [JsonProperty("applicationId")]
        public Guid ApplicationOnlineId { get; set; }

        public int? UserId { get; set; }

        public string UserName { get; set; }

        public string UserRole { get; set; }

        public DateTime CreateDate { get; set; }

        public decimal ApplicationAmount { get; set; }

        public decimal? EstimatedCost { get; set; }

        public int Stage { get; set; }

        public string Status { get; set; }

        public string Decision { get; set; }

        public string DeclineReason { get; set; }


        public void FillStatuses()
        {
            ApplicationOnlineStatus applicationOnlineStatus;
            if (Enum.TryParse(Status, out applicationOnlineStatus))
            {
                Status = applicationOnlineStatus.GetDisplayName();
            }
        }
    }
}
