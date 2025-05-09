using System;
using Pawnshop.Core;
using Pawnshop.Data.Models.ApplicationsOnline;

namespace Pawnshop.Data.Models.ApplicationOnlineStatusChangeHistories
{
    public sealed class ApplicationOnlineStatusChangeHistory
    {
        public Guid Id { get; set; }

        public Guid ApplicationOnlineId { get; set; }

        public int? UserId { get; set; }

        public string UserRole { get; set; }

        public DateTime CreateDate { get; set; }

        public decimal ApplicationAmount { get; set; }

        public decimal? EstimatedCost { get; set; }

        public int Stage { get; set; }

        public string Status { get; set; }

        public string Decision { get; set; }

        public string DeclineReason { get; set; }

        public ApplicationOnlineStatusChangeHistory(Guid applicationId, int? userId,
            DateTime createDate, decimal applicationAmount, decimal? estimatedCost,
            string status, string declineReason)
        {
            Id = Guid.NewGuid();
            ApplicationOnlineId = applicationId;
            UserId = userId;
            if (userId == Constants.ADMINISTRATOR_IDENTITY)
            {
                UserRole = "Admin";
            }
            else
            {
                UserRole = "Manager";
            }

            Status = status;
            ApplicationOnlineStatus applicationOnlineStatus;
            if (Enum.TryParse(status, out applicationOnlineStatus))
            {
                if (applicationOnlineStatus == ApplicationOnlineStatus.OnEstimation ||
                    applicationOnlineStatus == ApplicationOnlineStatus.Verification ||
                    applicationOnlineStatus == ApplicationOnlineStatus.Consideration ||
                    applicationOnlineStatus == ApplicationOnlineStatus.EstimationCompleted ||
                    applicationOnlineStatus == ApplicationOnlineStatus.Created ||
                    applicationOnlineStatus == ApplicationOnlineStatus.Approved)
                {
                    Stage = 1;
                }

                if (applicationOnlineStatus == ApplicationOnlineStatus.BiometricCheck ||
                    applicationOnlineStatus == ApplicationOnlineStatus.BiometricPassed ||
                    applicationOnlineStatus == ApplicationOnlineStatus.RequisiteCheck ||
                    applicationOnlineStatus == ApplicationOnlineStatus.ContractConcluded)
                {
                    Stage = 2;
                }

                if (applicationOnlineStatus == ApplicationOnlineStatus.Declined ||
                    applicationOnlineStatus == ApplicationOnlineStatus.Canceled)
                {
                    Decision = "Отказано";
                }
                else
                {
                    if (applicationOnlineStatus == ApplicationOnlineStatus.Created ||
                        applicationOnlineStatus == ApplicationOnlineStatus.BiometricCheck)
                    {
                        Decision = "Пришло от клиента";
                    }
                    else
                    {
                        Decision = "Одобрено";
                    }
                }
            }

            CreateDate = createDate;
            ApplicationAmount = applicationAmount;
            EstimatedCost = estimatedCost;
            DeclineReason = declineReason;
        }
    }
}
