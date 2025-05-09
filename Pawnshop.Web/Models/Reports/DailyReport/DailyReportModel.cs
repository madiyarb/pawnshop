using System;
using System.Collections.Generic;

namespace Pawnshop.Web.Models.Reports.DailyReport
{
    public class DailyReportModel
    {
        public DateTime CurrentDate { get; set; }

        public string BranchName { get; set; }

        public List<dynamic> List { get; set; }

        public dynamic Group { get; set; }
    }
}