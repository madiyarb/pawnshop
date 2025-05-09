using System;

namespace Pawnshop.Data.Models.Reports
{
    public class ReportLogModel
    {
        public DateTime CreateDate { get; set; }
        public DateTime ReportDate { get; set; }
        public string ReportName { get; set; }
        public string Request { get; set; }
        public string AuthorName { get; set; }
        public bool IsSuccessful { get; set; }
    }
}
