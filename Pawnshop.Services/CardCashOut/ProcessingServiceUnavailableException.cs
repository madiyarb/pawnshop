using System;

namespace Pawnshop.Services.CardCashOut
{
    public sealed class ProcessingServiceUnavailableException : Exception
    {
        public string Url { get; }
        public string Details { get; }

        public ProcessingServiceUnavailableException(string url, string details) :
        base($"Processing service '{url}' unavailable and throw unhandled error '{details}'")
        {
            Url = url;
            Details = details;
        }
}
}
