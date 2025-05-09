using System;

namespace Pawnshop.Services.CardCashOut
{
    public sealed class UnexpectedResponseException : Exception
    {
        public Response Response;
        public string ExceptionMessage;
        public string RequestBody;
        public UnexpectedResponseException(Response response, string exceptionMessage, string requestBody = "")
        {
            Response = response;
            ExceptionMessage = exceptionMessage;
            RequestBody = requestBody;
        }
    }
}
