using System.Net;

namespace Pawnshop.Web.Models
{
    public class BaseResponse
    {
        public string Message { get; set; }

        public int Status { get; set; }

        public BaseResponse()
        {
            
        }


        public BaseResponse(HttpStatusCode statusCode, string message)
        {
            Status = (int)statusCode;
            Message = message;
        }
    }
}
