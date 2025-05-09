using System;

namespace Pawnshop.Core.Queries
{
    public class ReportGenerateQuery
    {
        public DateTime BeginDate { get; set; }
        public DateTime EndDate { get; set; }
        public int AccountSettingId { get; set; }
        public int TermId { get; set; }
        public string? ContractIds { get; set; }
        public string? ClientIds { get; set; }
        public string? BranchIds { get; set; }
        public string? CollateralTypes { get; set; }
        public int RowCount { get; set; }
        public int StartRow { get; set; }
        public bool? IsDebit { get; set; }
    }
}