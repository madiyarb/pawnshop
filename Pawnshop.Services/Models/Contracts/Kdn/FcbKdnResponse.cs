using System;
using System.Collections.Generic;
using System.Text;

namespace Pawnshop.Services.Models.Contracts.Kdn
{
    public class FcbKdnResponse
    {
        public int Id { get; set; }
        public string RequestId { get; set; }
        public int CreditInfoId { get; set; }
        public string IIN { get; set; }
        public decimal KdnScore { get; set; }
        public decimal Debt { get; set; }
        public decimal Income { get; set; }
        public DateTime DateApplication { get; set; }
        public string ReportDate { get; set; }
        public int ErrorCode { get; set; }
        public string ErrorMessage { get; set; }
        public Guid CorrelationId { get; set; }
    }
}
