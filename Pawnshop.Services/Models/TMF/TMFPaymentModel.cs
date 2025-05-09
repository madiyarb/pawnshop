using Pawnshop.Data.Models.Clients;
using System;
using System.Collections.Generic;
using System.Text;

namespace Pawnshop.Services.Models.TMF
{
    public class TMFPaymentModel
    {
        public TMFContract Contract { get; set; }
        public Client Client { get; set; }
        public decimal Cost { get; set; }
        public string Note { get; set; }
    }
}
