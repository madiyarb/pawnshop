using System;
using System.Collections.Generic;
using System.Text;

namespace Pawnshop.Services.Models.UKassa
{
    public class UKassaCashOperationRequest
    {
        public decimal amount { get; set; }
        public int kassa { get; set; }
        public int operation_type { get; set; }
    }
}
