using System;
using System.Collections.Generic;
using System.Text;

namespace Pawnshop.Services.Models.Filters
{
    public class BlackoutFilter
    {
        public DateTime? Date { get; set; }
        public bool? IsPersonal { get; set; }
    }
}