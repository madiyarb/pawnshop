using System;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Pawnshop.Core;
using Pawnshop.Data.Access;
using Pawnshop.Data.Models.ClientSUSNStatuses;
using Pawnshop.Data.Models.OuterServiceSettings;
using Pawnshop.Data.Models.SUSNRequests;
using Pawnshop.Data.Models.SUSNStatuses;
using Pawnshop.Services.SUSNStatuses;
using Serilog;
using JsonSerializer = System.Text.Json.JsonSerializer;

namespace Pawnshop.Services.SUSN
{
    public sealed class TasLabSUSNService : ITasLabSUSNService
    {
        private HttpClient _httpClient;
        private readonly JsonSerializerOptions _serializerOptions;
        private readonly ClientSUSNStatusesRepository _clientSusnStatusesRepository;
        private readonly SUSNRequestsRepository _susnRequestsRepository;
        private readonly SUSNStatusesRepository _susnStatusesRepository;
        private readonly ILogger _logger;
        private readonly IRepository<OuterServiceSetting> _outerServiceSettingRepository;


        public TasLabSUSNService(HttpClient httpClient, IOptions<TasLabSUSNServiceOptions> options,
            SUSNRequestsRepository susnRequestsRepository,
            SUSNStatusesRepository susnStatusesRepository,
            ILogger logger,
            ClientSUSNStatusesRepository clientSusnStatusesRepository,
            OuterServiceSettingRepository outerServiceSettingRepository)
        {
            _httpClient = httpClient;
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic",
                Convert.ToBase64String(
                    System.Text.Encoding.ASCII.GetBytes(
                        $"{options.Value.UserName}:{options.Value.Secret}")));
            _httpClient.DefaultRequestHeaders.TryAddWithoutValidation("ENDPOINT-KEY", "fcb_susn");
            _susnRequestsRepository = susnRequestsRepository;
            _susnStatusesRepository = susnStatusesRepository;
            _logger = logger;
            _clientSusnStatusesRepository = clientSusnStatusesRepository;
            _httpClient.Timeout = TimeSpan.FromSeconds((double)options.Value.TimeoutSeconds);
            _outerServiceSettingRepository = outerServiceSettingRepository;
        }



        public async Task GetSUSNStatus(string iin, int clientId, CancellationToken cancellationToken)
        {
            try
            {
                string endpointUrl = _outerServiceSettingRepository.Find(new { Code = Constants.SUSN }).URL;
                var content = new StringContent(JsonSerializer.Serialize(new TASLabSUSNRequest
                {
                    iin = iin
                }, _serializerOptions), Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync(endpointUrl, content,
                    cancellationToken: cancellationToken);
                response.EnsureSuccessStatusCode();

                var PKBSUSNStatuses =
                    JsonConvert.DeserializeObject<SUSNGetStatusResponse>(await response.Content.ReadAsStringAsync());
                //JsonSerializer.Deserialize<SUSNGetStatusResponse>(await response.Content.ReadAsStringAsync(),
                //    _serializerOptions);//TODO если обновить до .net5 можно использовать вот это https://stackoverflow.com/questions/59097784/system-text-json-deserialize-json-with-automatic-casting
                //повесив на статус код [JsonNumberHandling(JsonNumberHandling.AllowReadingFromString)] там или число или строка 
                Guid requestGuid = Guid.NewGuid();
                if (PKBSUSNStatuses.status.Any(status => status.statusCode == "0"))
                {
                    await _susnRequestsRepository.Insert(new SUSNRequest(requestGuid, iin, clientId, true, false,
                        string.Empty));
                }
                else
                {
                    await _susnRequestsRepository.Insert(new SUSNRequest(requestGuid, iin, clientId, true, true,
                        string.Empty));
                }

                foreach (var PKBSUSNStatus in PKBSUSNStatuses.status.Where(status => status.statusCode != "0"))
                {
                    var status = await _susnStatusesRepository.GetByCode(PKBSUSNStatus.statusCode);
                    if (status == null)
                    {
                        var susnStatus = new SUSNStatus(PKBSUSNStatus.statusNameRu, PKBSUSNStatus.statusNameKz,
                            PKBSUSNStatus.statusCode, false, false);
                        await _susnStatusesRepository.Insert(susnStatus);
                        status = await _susnStatusesRepository.GetByCode(PKBSUSNStatus.statusCode);
                    }

                    await _clientSusnStatusesRepository.Insert(new ClientSUSNStatus(clientId, status.Id,
                        requestGuid));
                }
            }
            catch (Exception exception)
            {
                _logger.Error(exception, exception.Message);
                await _susnRequestsRepository.Insert(new SUSNRequest(Guid.NewGuid(), iin, clientId, false, false, exception?.Message ?? string.Empty));
            }
        }
    }
}
