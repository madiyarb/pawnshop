using Microsoft.AspNetCore.Mvc;
using System;

namespace Pawnshop.Web.Models.SchedulePayments
{
    public class CalculateFromMobileQuery
    {
        [FromQuery(Name = "productId")]
        public int ProductId { get; set; }

        [FromQuery(Name = "loanCost")]
        public decimal LoanCost { get; set; }

        [FromQuery(Name = "period")]
        public int Period { get; set; }

        [FromQuery(Name = "insurance")]
        public bool Insurance { get; set; }

        [FromQuery(Name = "firstPaymentDate")]
        public DateTimeOffset? FirstPaymentDate { get; set; } = null;

        [FromQuery(Name = "creditLineId")]
        public int? CreditLineId { get; set; }
    }
}
