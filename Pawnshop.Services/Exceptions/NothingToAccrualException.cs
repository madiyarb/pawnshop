using System;
using System.Collections.Generic;
using System.Text;

namespace Pawnshop.Services.Exceptions
{
    public class NothingToAccrualException : Exception
    {
        public NothingToAccrualException(string message) : base(message)
        { 
        }
    }
}
