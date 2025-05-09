using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Pawnshop.Core;
using Pawnshop.Core.Exceptions;
using Pawnshop.Data.Access;
using Pawnshop.Data.Models.LegalCollection;
using Pawnshop.Data.Models.LegalCollection.Dtos;
using Pawnshop.Services.LegalCollection.Inerfaces;
using RestSharp.Extensions;

namespace Pawnshop.Services.LegalCollection
{
    public class LegalCollectionTaskStatusHttpService : ILegalCollectionTaskStatusHttpService
    {
        private readonly HttpClient _httpClient;
        private readonly OuterServiceSettingRepository _settingsRepository;

        public LegalCollectionTaskStatusHttpService(HttpClient httpClient, OuterServiceSettingRepository settingsRepository)
        {
            _httpClient = httpClient;
            _settingsRepository = settingsRepository;
        }

        public async Task<PagedResponse<LegalCaseTaskStatusDto>> List()
        {
            var baseUrl = GetUrl(Constants.LEGAL_COLLECTION_INTEGRATION_SETTING_CODE);
            
            var url = $"{baseUrl}/api/legal-collection/task-status/list";

            var response = await _httpClient.PostAsync(url, null);

            var httpStatusCode = response.EnsureSuccessStatusCode().StatusCode;
            if (httpStatusCode != HttpStatusCode.OK)
            {
                throw new PawnshopApplicationException(
                    $"Legal Collection service http response status: {httpStatusCode}");
            }

            var jsonString = await response.Content.ReadAsStringAsync();
            var pagedResponse = JsonConvert.DeserializeObject<PagedResponse<LegalCaseTaskStatusDto>>(jsonString);

            return pagedResponse;
        }


        private string GetUrl(string login)
        {
            var settings = _settingsRepository.Find(new { Code = login });
            if (settings is null)
            {
                throw new PawnshopApplicationException("Не удалось получить настройки Legal-collection");
            }
            
            var url = settings.URL;
            if (!url.HasValue())
            {
                throw new PawnshopApplicationException("Не удалось получить Url микросервиса Legal-collection");
            }
            
            return url;
        }
    }
}