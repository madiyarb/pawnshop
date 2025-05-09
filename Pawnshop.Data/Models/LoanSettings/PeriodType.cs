using System;
using System.Collections.Generic;
using System.Text;

namespace Pawnshop.Data.Models.LoanSettings
{
    public enum PeriodType : short
    {
        Day = 1,
        Week = 7,
        Month = 30,
        HalfYear = 180,
        Year = 365
    }
}
