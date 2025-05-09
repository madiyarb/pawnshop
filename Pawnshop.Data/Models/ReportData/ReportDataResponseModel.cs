using System;
using System.Collections.Generic;
using System.Text;

namespace Pawnshop.Data.Models.ReportData
{
    public class ReportDataResponseModel
    {
        public string Code { get; set; }
        public string Message { get; set; }
        public List<int> ErrorRows { get; set; }

    }
}
