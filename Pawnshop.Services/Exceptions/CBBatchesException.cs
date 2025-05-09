using System;
using System.Collections.Generic;
using System.Text;

namespace Pawnshop.Services.Exceptions
{
    public class CBBatchesException : Exception
    {
        public CBBatchesException() : base()
        {
        }
        public CBBatchesException(string message) : base(message)
        {
        }
    }
}
