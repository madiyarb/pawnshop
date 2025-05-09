using Microsoft.Extensions.Options;
using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using Newtonsoft.Json;
using Pawnshop.Core;
using Pawnshop.Data.Access;
using Pawnshop.Data.Models.ClientDebtorRegistryData;
using Pawnshop.Data.Models.ClientDebtorRegistryRequests;
using Pawnshop.Data.Models.OuterServiceSettings;
using Pawnshop.Services.Exceptions;
using Serilog;
using JsonException = System.Text.Json.JsonException;
using JsonSerializer = System.Text.Json.JsonSerializer;

namespace Pawnshop.Services.DebtorRegistry
{
    public sealed class DebtorRegistryService : IDebtorRegistryService
    {

        private readonly HttpClient _httpClient;
        private readonly string username;
        private readonly string password;
        private readonly ClientDebtorRegistryRequestsRepository _clientDebtorRegistryRequestsRepository;
        private readonly ClientDebtorRegistryDataRepository _clientDebtorRegistryDataRepository;
        private readonly ILogger _logger;
        private readonly IRepository<OuterServiceSetting> _outerServiceSettingRepository;
        public DebtorRegistryService(HttpClient httpClient,
            IOptions<DebtorRegistryServiceOptions> options,
            ClientDebtorRegistryRequestsRepository clientDebtorRegistryRequestsRepository,
            ClientDebtorRegistryDataRepository clientDebtorRegistryDataRepository,
            OuterServiceSettingRepository outerServiceSettingRepository,
            ILogger logger)
        {
            username = options.Value.UserName ?? throw new ArgumentNullException(nameof(username));
            password = options.Value.Secret ?? throw new ArgumentNullException(nameof(password));
            _httpClient = httpClient;
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic",
                Convert.ToBase64String(
                    System.Text.Encoding.ASCII.GetBytes(
                        $"{username}:{password}")));
            _httpClient.DefaultRequestHeaders.TryAddWithoutValidation("ENDPOINT-KEY", "egov_bankrupt_by_iin");
            _clientDebtorRegistryRequestsRepository = clientDebtorRegistryRequestsRepository;
            _clientDebtorRegistryDataRepository = clientDebtorRegistryDataRepository;
            _logger = logger;
            _httpClient.Timeout = TimeSpan.FromSeconds((double)options.Value.TimeoutSeconds);
            _outerServiceSettingRepository = outerServiceSettingRepository;
        }

        public async Task<DebtorRegistryResponse> GetInfoFromDebtorRegistry(string iin, int clientId, CancellationToken cancellationToken)
        {
            try 
            {
                string endpointUrl = _outerServiceSettingRepository.Find(new { Code = Constants.DEBTOR_REGISTRY }).URL;
                var query = HttpUtility.ParseQueryString(string.Empty);
                query.Add("iin", iin);
                var response = await _httpClient.GetAsync($"{endpointUrl}{query}", cancellationToken: cancellationToken);
                response.EnsureSuccessStatusCode();
                var data = JsonConvert.DeserializeObject<DebtorRegistryResponse>(
                    await response.Content.ReadAsStringAsync());
                Guid requestGuid = Guid.NewGuid();
                await _clientDebtorRegistryRequestsRepository.Insert(new ClientDebtorRegistryRequest(requestGuid, clientId));
                foreach (var info in data.data)
                {
                    await _clientDebtorRegistryDataRepository.Insert(new ClientDebtorRegistryData(info.CategoryRu,
                        info.RecoveryAmount, info.KbkNameRu, info.DisaDepartmentNameRu, info.RecovererTypeRu,
                        requestGuid));
                }

                return data;

            }
            catch (HttpRequestException exception)
            {
                _logger.Error(exception, exception.Message);
                throw new ServiceUnavailableException( exception.Message);
            }
            catch (TaskCanceledException exception)
            {
                _logger.Error(exception, exception.Message);
                throw new ServiceUnavailableException(exception.Message);
            }
            catch (JsonException exception)
            {
                _logger.Error(exception, exception.Message);
                throw new ServiceUnavailableException( exception.Message);
            }
        }
    }
}
