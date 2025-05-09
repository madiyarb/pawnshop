using System;

namespace Pawnshop.Services.ApplicationOnlineFileStorage.Exceptions
{
    public sealed class UnexpectedResponseException : Exception
    {
        public string Message { get; set; }

        public UnexpectedResponseException(string message) : base(message)
        {
            Message = message;

        }
    }
}
