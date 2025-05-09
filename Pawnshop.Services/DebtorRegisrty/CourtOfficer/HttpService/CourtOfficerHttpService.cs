using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Pawnshop.Core;
using Pawnshop.Core.Exceptions;
using Pawnshop.Data.Access;
using Pawnshop.Data.Models.DebtorRegistry.CourtOfficer;
using Pawnshop.Data.Models.LegalCollection;
using RestSharp.Extensions;

namespace Pawnshop.Services.DebtorRegisrty.CourtOfficer.HttpService
{
    public class CourtOfficerHttpService : ICourtOfficerHttpService
    {
        private readonly HttpClient _httpClient;
        private readonly OuterServiceSettingRepository _settingsRepository;

        public CourtOfficerHttpService(HttpClient httpClient, OuterServiceSettingRepository settingsRepository)
        {
            _httpClient = httpClient;
            _settingsRepository = settingsRepository;
        }

        public async Task<PagedResponse<CourtOfficerDto>> Filtered(CourtOfficersHttRequest request)
        {
            var baseUrl = GetUrl(Constants.LEGAL_COLLECTION_INTEGRATION_SETTING_CODE);
            var url = $"{baseUrl}/api/courtofficer/filtered";

            try
            {
                var jsonContent =
                    new StringContent(JsonConvert.SerializeObject(request), Encoding.UTF8, "application/json");
                
                HttpResponseMessage response = await _httpClient.PostAsync(url, jsonContent);

                response.EnsureSuccessStatusCode();
                
                string responseBody = await response.Content.ReadAsStringAsync();

                var detailsResponse = JsonConvert.DeserializeObject<PagedResponse<CourtOfficerDto>>(responseBody);
                return detailsResponse;
            }
            catch (Exception e)
            {
                throw new PawnshopApplicationException($"Не удалось получить данные по ЧСИ. {e.Message}");
            }
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