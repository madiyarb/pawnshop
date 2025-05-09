using System;
using Pawnshop.Data.Models.TasOnline;

namespace Pawnshop.Services.Models.Filters
{
    public class TasOnlinePaymentFilter
    {
        public DateTime? BeginDate { get; set; }

        public DateTime? EndDate { get; set; }

        public int? ClientId { get; set; }
        public TasOnlinePaymentStatus? Status { get; set; }
    }
}