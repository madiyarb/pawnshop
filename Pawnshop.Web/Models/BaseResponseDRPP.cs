using System.Net;

namespace Pawnshop.Web.Models
{
    public class BaseResponseDRPP : BaseResponse
    {
        public string Code { get; set; }

        public BaseResponseDRPP() { }

        public BaseResponseDRPP(HttpStatusCode statusCode, string message, DRPPResponseStatusCode code)
        {
            Status = (int)statusCode;
            Message = message;
            Code = code.ToString();
        }
    }
}
