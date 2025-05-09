using System;
using System.Collections.Generic;
using System.Text;

namespace Pawnshop.Data.Models.Postponements
{
    public enum PostponementType : short
    {
        PaymentsAllInOneDate = 10,
        PaymentsOnlyPercent = 20,
        PaymentsWithoutPercent = 30
    }
}
