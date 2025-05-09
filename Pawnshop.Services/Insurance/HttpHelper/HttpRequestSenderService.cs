using Pawnshop.Core;
using Pawnshop.Data.Access;
using Pawnshop.Data.Models.OuterServiceSettings;
using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace Pawnshop.Services.Insurance.HttpHelper
{
    public class HttpRequestSenderService : IHttpRequestSenderService
    {
        private readonly HttpClient _httpClient;
        private readonly OuterServiceSettingRepository _settingRepository;

        public HttpRequestSenderService(HttpClient httpClient, OuterServiceSettingRepository settingRepository) 
        {  
            _httpClient = httpClient;
            _settingRepository = settingRepository;
        }

        public async Task<HttpResponseMessage> SendCreateRequest(string requestModelString)
        {
            var config = _settingRepository.Find(new { Code = Constants.CREATE_INSURANCE_BPM_MICROSERVICE });
            return await PostAsync(config, requestModelString);
        }

        public async Task<HttpResponseMessage> SendCancelRequest(string requestModelString)
        {
            var config = _settingRepository.Find(new { Code = Constants.ANNULMENT_INSURANCE_BPM_MICROSERVICE });
            return await PostAsync(config, requestModelString);
        }

        private async Task<HttpResponseMessage> PostAsync(OuterServiceSetting config, string requestModelString)
        {
            _httpClient.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue(AuthenticationSchemes.Basic.ToString(),
                    Convert.ToBase64String(Encoding.UTF8.GetBytes($"{config.Login}:{config.Password}")));

            var content = new StringContent(requestModelString, Encoding.UTF8, "application/json");

            return await _httpClient.PostAsync(config.URL, content);
        }
    }
}
