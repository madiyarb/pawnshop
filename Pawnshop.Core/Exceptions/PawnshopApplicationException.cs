using System;

namespace Pawnshop.Core.Exceptions
{
    public class PawnshopApplicationException : Exception
    {
        public string[] Messages { get; set; }

        public PawnshopApplicationException()
        {
            Messages = new string[0];
        }

        public PawnshopApplicationException(string message) : base(message)
        {
            Messages = new[] { message };
        }

        public PawnshopApplicationException(string message, Exception innerException) : base(message, innerException)
        {
            Messages = new[] { message };
        }

        public PawnshopApplicationException(params string[] messages)
        {
            if (messages == null) throw new ArgumentNullException(nameof(messages));

            Messages = messages;
        }
    }
}