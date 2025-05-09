using System;
using System.Collections.Generic;
using System.Text;

namespace Pawnshop.Data.Models.Reports
{
    public class ReportType
    {
        public int Id { get; set; }
        public string ReportTypeName { get; set; }
        public string ReportTypeCode { get; set; }
        public DateTime CreateDate { get; set; }
        public DateTime DeleteDate { get; set; }
    }
}
