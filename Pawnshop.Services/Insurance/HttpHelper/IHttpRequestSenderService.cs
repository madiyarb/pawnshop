using System.Net.Http;
using System.Threading.Tasks;

namespace Pawnshop.Services.Insurance.HttpHelper
{
    public interface IHttpRequestSenderService
    {
        Task<HttpResponseMessage> SendCreateRequest(string requestModelString);
        Task<HttpResponseMessage> SendCancelRequest(string requestModelString);
    }
}
