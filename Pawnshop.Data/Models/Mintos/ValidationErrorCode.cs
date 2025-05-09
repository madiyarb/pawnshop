using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Pawnshop.Data.Models.Mintos
{
    public enum ValidationErrorCode : short
    {
        NotSaved = 10,
        WrongStatus = 20,
        BadDate = 30,
        NotLoadedPayment = 40
    }
}
