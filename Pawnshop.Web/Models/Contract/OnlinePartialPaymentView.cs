using System.Net;

namespace Pawnshop.Web.Models.Contract
{
    public class OnlinePartialPaymentView : BaseResponse
    {
        public OnlinePartialPaymentView() { }

        public OnlinePartialPaymentView(HttpStatusCode statusCode, string message) : base(statusCode, message) { }
    }
}
