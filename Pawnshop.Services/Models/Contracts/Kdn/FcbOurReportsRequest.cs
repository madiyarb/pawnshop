using System;
using System.Collections.Generic;
using System.Text;

namespace Pawnshop.Services.Models.Contracts.Kdn
{
    public class FcbOurReportsRequest
    {
        public int Page { get; set; }
        public int ItemsPerPage { get; set; }
        public DateTime DateFrom { get; set; }
        public DateTime DateTo { get; set; }
    }
}
