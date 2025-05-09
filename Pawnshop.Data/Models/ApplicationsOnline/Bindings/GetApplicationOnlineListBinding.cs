using System;
using System.Collections.Generic;

namespace Pawnshop.Data.Models.ApplicationsOnline.Bindings
{
    public sealed class GetApplicationOnlineListBinding
    {
        public bool? Insurance { get; set; }
        public string? EstimationStatus { get; set; }
        public decimal? InsurancePremium { get; set; }
        public decimal? TotalApplicationAmount { get; set; }
        public decimal? Percent { get; set; }
        public Guid? ApplicationId { get; set; }
        public string? IIN { get; set; }
        public string? Name { get; set; }
        public string? Surname { get; set; }
        public string? Patronymic { get; set; }
        public string? PhoneNumber { get; set; }
        public IEnumerable<string> ApplicationStatus { get; set; }
        public string? Manager { get; set; }
        public decimal? LoanCost { get; set; }
        public int? ProductId { get; set; }
        public string? PartnerCode { get; set; }
        public string? ApplicationPartnerCode { get; set; }
        public int? Stage { get; set; }
        public DateTime? CreateDateBefore { get; set; }
        public DateTime? CreateDateAfter { get; set; }
        public DateTime? UpdateDateBefore { get; set; }
        public DateTime? UpdateDateAfter { get; set; }
        public int? RejectReason { get; set; }
        public string? CarMark{ get; set; }
        public string? CarModel { get; set; }
        public int? CarYear { get; set; }
        public string? CarNumber { get; set; }
        public decimal? MarketCost { get; set; }
        public int? LTV { get; set; }
        public string ApplicationNumber { get; set; }
        public int? ClientId { get; set; }
        public string ContractNumber { get; set; }
        public int? SalesManagerId { get; set; }
        public int? MinutesLeftAfterUpdate { get; set; }

    }
}
