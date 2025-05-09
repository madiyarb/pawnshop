using System;
using System.Collections.Generic;
using System.Text;

namespace Pawnshop.Services.Exceptions
{
    public class UnexpectedResponseException : Exception
    {
        public (int, string) Response;
        public string Request;

        public UnexpectedResponseException((int, string) response, string request)
        {
            Response = response;
            Request = request;
        }
    }
}
