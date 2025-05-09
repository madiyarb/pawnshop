using System;
using System.Collections.Generic;
using System.Text;

namespace Pawnshop.Services.Models.UKassa
{
    public class UKassaGetShiftsResponse
    {
        public List<Shift> Shifts { get; set; }
    }

    public class Shift
    {
        public int id { get; set; }
        public int number { get; set; }
        public bool is_active { get; set; }
        public string created_at { get; set; }
        public string closed_at { get; set; }
    }
}
