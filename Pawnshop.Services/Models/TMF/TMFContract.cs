using System;
using System.Collections.Generic;
using System.Text;

namespace Pawnshop.Services.Models.TMF
{
    public class TMFContract
    {
        public string Id { get; set; }
        public string Contract { get; set; }
        public decimal Amount { get; set; }
        public decimal TotalAmount { get; set; }
    }
}
