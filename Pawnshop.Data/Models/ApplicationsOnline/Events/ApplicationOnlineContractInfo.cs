using System;

namespace Pawnshop.Data.Models.ApplicationsOnline.Events
{
    public sealed class ApplicationOnlineContractInfo
    {
        public decimal? MonthlyPaymentAmount { get; set; }
        public DateTime? MaturityDate { get; set; }
    }
}
