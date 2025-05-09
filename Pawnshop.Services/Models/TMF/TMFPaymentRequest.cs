using System;
using System.Collections.Generic;
using System.Text;

namespace Pawnshop.Services.Models.TMF
{
    public class TMFPaymentRequest
    {
        public string ContractId { get; set; }
        public decimal Amount { get; set; }
    }
}
