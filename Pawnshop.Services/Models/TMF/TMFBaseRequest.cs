using System;
using System.Collections.Generic;
using System.Text;

namespace Pawnshop.Services.Models.TMF
{
    public class TMFBaseRequest
    {
        public string MethodName { get; set; }
        public string IIN { get; set; }
        public string Contract { get; set; }
        public decimal Amount { get; set; }
    }
}
