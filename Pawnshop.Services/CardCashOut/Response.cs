namespace Pawnshop.Services.CardCashOut
{
    public sealed class Response
    {
        public int? StatusCode { get; private set; }
        public string Body { get; private set; }
        public Response(int? statusCode, string body)
        {
            StatusCode = statusCode;
            Body = body;
        }
    }
}
