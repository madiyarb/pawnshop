using System;
using System.Collections.Generic;
using System.Text;

namespace Pawnshop.Services.Models.Filters
{
    public class UKassaReportFilter
    {
        public int ShiftId { get; set; }
        public DateTime? ReportDate { get; set; }
        public int? BranchId { get; set; }
        public int? Status { get; set; }
    }
}
