using System;
using Pawnshop.AccountingCore.Models;

namespace Pawnshop.Data.Models.Online1C
{
    public class Online1CReportData
    {
        public DateTime? Date { get; set; }
        public DateTime? BeginDate { get; set; }
        public DateTime? EndDate { get; set; }
        public CollateralType? CollateralType { get; set; }
        public int? BranchId { get; set; }
        public Online1CReportType? ReportType { get; set; }
    }
}
