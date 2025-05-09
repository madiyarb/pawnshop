using System;
using Newtonsoft.Json;
using Pawnshop.Core;
using Pawnshop.Data.Models.ApplicationsOnlineEstimation;

namespace Pawnshop.Data.Models.ApplicationsOnline
{
    public sealed class ApplicationOnlineListItemView
    {
        public bool? Insurance { get; set; }
        public string? EstimationStatus { get; set; } 
        public decimal? InsurancePremium { get; set; }
        public decimal? TotalApplicationAmount { get; set; }
        public decimal? LoanPercent { get; set; }
        public Guid? ApplicationId { get; set; }
        public string? IIN { get; set; }
        public string? Name { get; set; }
        public string? Surname { get; set; }
        public string? Patronymic { get; set; }
        public string? PhoneNumber { get; set; }
        public string? ApplicationStatus { get; set; }
        public string? ResposableManagerName { get; set; }
        public string? Manager { get; set; } //
        public decimal? LoanCost { get; set; }
        public int? ProductId { get; set; }
        public string? PartnerCode { get; set; }
        public int? Stage { get; set; }
        public DateTime? CreateDate { get; set; }
        public DateTime? UpdateDate { get; set; }

        [JsonIgnore]
        public int RejectReasonId { get; set; }
        public string? RejectReason { get; set; }
        public string? CarMark { get; set; }
        public string? CarModel { get; set; }
        public int? CarYear { get; set; }
        public string? CarNumber { get; set; }
        public decimal? MarketCost { get; set; }
        public int? LTV { get; set; }
        public Guid? Key { get; set; }
        public string ApplicationNumber { get; set; }
        public int ClientId { get; set; }
        public int LoanTerm { get; set; }
        public string ContractNumber { get; set; }

        public int? SalesManagerId { get; set; }

        public string SalesManager { get; set; }
        public int? MinutesLeftAfterUpdate { get; set; }
        public string ApplicationPartnerCode { get; set; }

        public void FillKey()
        {
            Key = ApplicationId;
        }

        public void FillStatuses()
        {
            ApplicationOnlineEstimationStatus estimationStatusEnum;
            if (Enum.TryParse(EstimationStatus, out estimationStatusEnum))
            {
                EstimationStatus = estimationStatusEnum.GetDisplayName();
            }

            ApplicationOnlineStatus applicationOnlineStatus;
            if (Enum.TryParse(ApplicationStatus, out applicationOnlineStatus))
            {
                ApplicationStatus = applicationOnlineStatus.GetDisplayName();
            }
        }

        public void FillTimes()
        {
            if (UpdateDate != null)
            {
                MinutesLeftAfterUpdate = (int)(DateTime.Now - UpdateDate).Value.TotalMinutes;
            }
        }
    }
}
