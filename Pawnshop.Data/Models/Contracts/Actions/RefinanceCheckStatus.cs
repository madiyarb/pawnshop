using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Pawnshop.Data.Models.Contracts.Actions
{
    public enum RefinanceCheckStatus : short
    {
        Available = 0,
        Declined = 10,
        WrongParams = 20
    }
}
