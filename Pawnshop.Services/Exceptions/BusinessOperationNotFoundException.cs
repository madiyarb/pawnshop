using Pawnshop.Core.Exceptions;
using System;
using System.Collections.Generic;
using System.Text;

namespace Pawnshop.Services.Exceptions
{
    public class BusinessOperationNotFoundException : PawnshopApplicationException
    {
        public BusinessOperationNotFoundException() : base()
        {
        }

        public BusinessOperationNotFoundException(string message) : base(message)
        {
            Messages = new[] { message };
        }

        public BusinessOperationNotFoundException(string message, Exception innerException) : base(message, innerException)
        {
            Messages = new[] { message };
        }

        public BusinessOperationNotFoundException(string[] messages)
        {
            if (messages == null) throw new ArgumentNullException(nameof(messages));

            Messages = messages;
        }
    }
}
