using Pawnshop.Data.Models.TasOnline;
using Pawnshop.Data.Models.TMF;
using System;
using System.Collections.Generic;
using System.Text;

namespace Pawnshop.Services.Models.Filters
{
    public class TmfPaymentFilter
    {
        public DateTime? BeginDate { get; set; }

        public DateTime? EndDate { get; set; }

        public int? ClientId { get; set; }
        public TMFPaymentStatus? Status { get; set; }
    }
}
