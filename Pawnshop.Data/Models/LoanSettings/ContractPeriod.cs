using System;
using System.Collections.Generic;
using System.Text;

namespace Pawnshop.Data.Models.LoanSettings
{
    public class ContractPeriod
    {
        public int Period { get; set; }
        public PeriodType PeriodType { get; set; }
        public int LoanPeriod => Period * (int) PeriodType;
    }
}
