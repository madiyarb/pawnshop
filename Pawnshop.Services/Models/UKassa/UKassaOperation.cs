using System;
using System.Collections.Generic;
using System.Text;

namespace Pawnshop.Services.Models.UKassa
{
    public class UKassaOperation
    {
        public int type { get; set; }
        public string created_at { get; set; }
        public int id { get; set; }
        public decimal discount { get; set; }
        public decimal total_amount { get; set; }
        public string user { get; set; }
        public string fixed_check { get; set; }
        public bool is_offline { get; set; }
        public bool fixed_offline { get; set; }
        public decimal total_discount { get; set; }
    }
}
