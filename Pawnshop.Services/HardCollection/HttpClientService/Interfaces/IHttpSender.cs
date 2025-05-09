using System.Net.Http;
using System.Threading.Tasks;

namespace Pawnshop.Services.HardCollection.HttpClientService.Interfaces
{
    public interface IHttpSender
    {
        Task<HttpResponseMessage> SendRequestAsync(string jsonModel, string url);
    }
}
