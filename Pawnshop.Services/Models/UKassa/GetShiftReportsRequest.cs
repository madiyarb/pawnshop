using System;
using System.Collections.Generic;
using System.Text;

namespace Pawnshop.Services.Models.UKassa
{
    public class GetShiftReportsRequest
    {
        public int kassa { get; set; }
        public DateTime start { get; set; }
        public DateTime end { get; set; }
    }
}
