using Microsoft.Extensions.Options;
using Pawnshop.Core.Exceptions;
using Pawnshop.Data.Models.MobileApp;
using Pawnshop.Services.Estimation.v2;
using System.Net.Http;
using System.Text.Json;
using System.Text;
using System.Threading.Tasks;
using System;

namespace Pawnshop.Services.Contracts
{
    public class RequestMobileAppService : IRequestMobileAppService
    {
        private readonly string _baseUrl;
        private readonly string _apiKey;

        private readonly HttpClient _httpClient;

        public RequestMobileAppService(
            IOptions<EstimationServiceOptions> options,
            HttpClient httpClient)
        {
            _baseUrl = options?.Value?.BaseUrl ?? throw new ArgumentNullException(nameof(_baseUrl));
            _apiKey = options?.Value?.ApiKey ?? throw new ArgumentNullException(nameof(_apiKey));
            _httpClient = httpClient;
        }

        public async Task UpdateStatusInMobileApp(UpdateOnStatusPositionRegistration appId)
        {
            string endPoint = "/api/v1/reissue";

            await SendPostRequest(_baseUrl + endPoint, JsonSerializer.Serialize(appId));
        }

        private async Task<bool> SendPostRequest(string url, string jsonData)
        {
            _httpClient.DefaultRequestHeaders.Add("X-API-Key", _apiKey);
            var content = new StringContent(jsonData, Encoding.UTF8, "application/json");
            HttpResponseMessage response = await _httpClient.PostAsync(url, content);

            if (response.IsSuccessStatusCode)
                return response.IsSuccessStatusCode;
            else
                throw new PawnshopApplicationException($"Ошибка при отправке запроса: {response.StatusCode} - {response.ReasonPhrase}");
        }
    }
}
