using System;
using System.Collections.Generic;
using System.Text;

namespace Pawnshop.Data.Models.ReportData
{
    public class ReportDataModel
    {
        public DateTime Date { get; set; }
        public List<ReportDataRow> Rows { get; set; }

    }
}
