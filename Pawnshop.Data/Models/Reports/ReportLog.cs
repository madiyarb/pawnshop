using System;
using System.Collections.Generic;
using System.Text;

namespace Pawnshop.Data.Models.Reports
{
    public class ReportLog
    {
        public int Id { get; set; }
        public int ReportId { get; set; }
        public string Request { get; set; }
        public int AuthorId { get; set; }
        public string AuthorName { get; set; }
        public bool IsSuccessful { get; set; }
        public DateTime CreateDate { get; set; }
        public DateTime DeleteDate { get; set; }
    }
}
