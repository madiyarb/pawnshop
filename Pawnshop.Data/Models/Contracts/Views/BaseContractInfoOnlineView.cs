using Pawnshop.Data.Helpers;
using System;
using Newtonsoft.Json;

namespace Pawnshop.Data.Models.Contracts.Views
{
    public sealed class BaseContractInfoOnlineView
    {
        public int Id { get; set; }
        public int? CreditLineId { get; set; }
        public string ContractNumber { get; set; }

        [JsonConverter(typeof(TasAppDateTimeConverter))]
        public DateTime? NextPaymentDate { get; set; }

        [JsonConverter(typeof(TasAppDateTimeConverter))]
        public DateTime? ContractDate { get; set; }

        [JsonConverter(typeof(TasAppDateTimeConverter))]
        public DateTime? MaturityDate { get; set; }

        [JsonConverter(typeof(TasAppDateTimeConverter))]
        public DateTime? BuyoutDate { get; set; }
        public int ContractClass { get; set; }
        public int CollateralType { get; set; }

        [JsonIgnore]
        public int LoanPeriod { get; set; }
        public int LoanPeriodDays => LoanPeriod;
        public int LoanPeriodMonth => (int)Math.Round((decimal)LoanPeriod / (decimal)30.0);
        public bool IsActive { get; set; }
        public bool IsOverdue { get; set; }
        public bool IsOffBalance { get; set; }
        public bool IsShortDiscrete { get; set; }
        public bool InLegalCollection { get; set; }
        public int ScheduleType { get; set; }
        public int BranchId { get; set; }
    }
}
