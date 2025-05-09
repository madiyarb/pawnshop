using Pawnshop.Data.Models.Contracts.Actions;
using System;
using System.Collections.Generic;
using System.Text;

namespace Pawnshop.Data.Models.Processing
{
    public class ProcessingInfo
    {
        public decimal Amount { get; set; }
        public ProcessingType Type { get; set; }
        public string BankName { get; set; }
        public string BankNetwork { get; set; }
        public long Reference { get; set; }
    }
}
