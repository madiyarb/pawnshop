using System;
using System.Collections.Generic;
using System.Text;

namespace Pawnshop.Services.Models.Filters
{
    public class HolidayFilter
    {
        public DateTime? BeginDate { get; set; }
        public DateTime? EndDate { get; set; }
        public DateTime? PayBeginDate { get; set; }
        public DateTime? PayEndDate { get; set; }
        public DateTime? PayDate { get; set; }
    }
}
