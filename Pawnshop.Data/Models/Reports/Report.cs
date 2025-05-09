using System;
using System.Collections.Generic;
using System.Text;

namespace Pawnshop.Data.Models.Reports
{
    public class Report
    {
        public int Id { get; set; }
        public string ReportName { get; set; }
        public string ReportCode { get; set; }
        public int ReportTypeId { get; set; }
        public DateTime CreateDate { get; set; }
        public DateTime DeleteDate { get; set; }
    }
}
