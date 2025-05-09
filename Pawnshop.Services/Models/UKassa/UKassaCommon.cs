using System;
using System.Collections.Generic;
using System.Text;

namespace Pawnshop.Services.Models.UKassa
{
    public class Company
    {
        public string server_site { get; set; }
        public string name { get; set; }
        public string seria_nds { get; set; }
        public string bin_iin { get; set; }
        public int type { get; set; }
        public string server { get; set; }
        public bool pay_nds { get; set; }
        public string number_nds { get; set; }
    }

    public class Kassa
    {
        public int shift { get; set; }
        public string name { get; set; }
        public int req_number { get; set; }
        public string factory_number { get; set; }
        public int id { get; set; }
        public string reg_number { get; set; }
        public string address { get; set; }
    }
}
