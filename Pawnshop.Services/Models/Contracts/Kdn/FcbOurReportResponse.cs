using System;
using System.Collections.Generic;
using System.Text;

namespace Pawnshop.Services.Models.Contracts.Kdn
{
    public class FcbOurReportResponse
    {
        public int Total { get; set; }
        public List<OurReportItem> ReportItems { get; set; }
    }

    public class OurReportItem
    {
        public int OrganizationId { get; set; }
        public string Author { get; set; }
        public DateTime RequestDate { get; set; }
        public string IIN { get; set; }
        public int ResponseType { get; set; } // 1 - KDN, 2 - Report
        public bool? IsSuccess { get; set; }
    }
}
