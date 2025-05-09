using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using Microsoft.Extensions.Options;
using Pawnshop.Core;
using Pawnshop.Data.Access;
using Pawnshop.Data.Models.OuterServiceSettings;
using Pawnshop.Services.Exceptions;
using Serilog;

namespace Pawnshop.Services.TasLabBankrupt
{
    public sealed class TasLabBankruptInfoService : ITasLabBankruptInfoService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger _logger;
        private readonly IRepository<OuterServiceSetting> _outerServiceSettingRepository;
        public TasLabBankruptInfoService(HttpClient httpClient, IOptions<TasLabBankruptInfoServiceOptions> options,
            ILogger logger,
            OuterServiceSettingRepository outerServiceSettingRepository)
        {
            _httpClient = httpClient;
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic",
                Convert.ToBase64String(
                    System.Text.Encoding.ASCII.GetBytes(
                        $"{options.Value.UserName}:{options.Value.Secret}")));
            _logger = logger;
            _httpClient.Timeout = TimeSpan.FromSeconds((double)options.Value.TimeoutSeconds);
            _outerServiceSettingRepository = outerServiceSettingRepository;
            _httpClient.DefaultRequestHeaders.TryAddWithoutValidation("ENDPOINT-KEY", "mkb_bankrupt_by_iin");
        }

        public async Task<bool> IsClientBankruptFromDatabase(string iin, CancellationToken cancellationToken)
        {
            try 
            {
                string endpointUrl = _outerServiceSettingRepository.Find(new { Code = Constants.BANKRUPT_SHORT }).URL;
                var query = HttpUtility.ParseQueryString(string.Empty);
                query.Add("iins", iin);
                var response = await _httpClient
                    .GetAsync($"{endpointUrl}{query}", 
                        cancellationToken: cancellationToken);
                response.EnsureSuccessStatusCode();
                var responseString = await response.Content.ReadAsStringAsync();
                if (responseString == "[]")//TODO other responses? 
                    return false;
                return true;
            }
            catch (HttpRequestException exception)
            {
                _logger.Error(exception, exception.Message);
                throw new ServiceUnavailableException(exception.Message);
            }
            catch (TaskCanceledException exception)
            {
                _logger.Error(exception, exception.Message);
                throw new ServiceUnavailableException(exception.Message);
            }
            catch (JsonException exception)
            {
                _logger.Error(exception, exception.Message);
                throw new ServiceUnavailableException(exception.Message);
            }
        }

        public async Task<bool> IsClientBankruptOnline(string iin, CancellationToken cancellationToken)
        {
            try
            {
                string endpointUrl = _outerServiceSettingRepository.Find(new { Code = Constants.BANKRUPT_ONLINE }).URL;
                var query = HttpUtility.ParseQueryString(string.Empty);
                query.Add("iins", iin);
                var response = await _httpClient
                    .GetAsync($"{endpointUrl}{query}",
                        cancellationToken: cancellationToken);
                response.EnsureSuccessStatusCode();
                var responseString = await response.Content.ReadAsStringAsync();
                if (responseString == "[]")//TODO other responses? 
                    return false;
                return true;
            }
            catch (HttpRequestException exception)
            {
                _logger.Error(exception, exception.Message);
                throw new ServiceUnavailableException(exception.Message);
            }
            catch (TaskCanceledException exception)
            {
                _logger.Error(exception, exception.Message);
                throw new ServiceUnavailableException(exception.Message);
            }
            catch (JsonException exception)
            {
                _logger.Error(exception, exception.Message);
                throw new ServiceUnavailableException(exception.Message);
            }
        }
    }
}
