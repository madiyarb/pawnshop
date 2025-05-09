using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace Pawnshop.Services.Models.TMF
{
    public class TMFPaymentResultResponse
    {
        // change if TMF will change the name
        //public string Result { get; set; } = String.Empty;
        public string TransactionId { get; set; }
        
    }
}
