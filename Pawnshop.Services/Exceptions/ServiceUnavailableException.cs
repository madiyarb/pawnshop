using System;

namespace Pawnshop.Services.Exceptions
{
    public sealed class ServiceUnavailableException : Exception
    {
        public string Url { get; }
        public string Details { get; }

        public ServiceUnavailableException(string url, string details) :
            base($"Service url : '{url}' unavailable and throw unhandled error '{details}'")
        {
            Url = url;
            Details = details;
        }

        public ServiceUnavailableException( string details) :
            base($"Service  unavailable and throw unhandled error '{details}'")
        {
            Details = details;
        }
    }
}
