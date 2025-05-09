using Newtonsoft.Json;
using Pawnshop.Data.Models.LegalCollection.Dtos.LegalCase;
using Pawnshop.Services.LegalCollection.HttpServices.Dtos.LegalCaseNotificationTemplate;
using Pawnshop.Services.LegalCollection.Inerfaces;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Pawnshop.Core;
using Pawnshop.Core.Exceptions;
using Pawnshop.Data.Access;
using RestSharp.Extensions;

namespace Pawnshop.Services.LegalCollection.HttpServices
{
    public class LegalCollectionNotificationTemplateHttpService : ILegalCollectionNotificationTemplateHttpService
    {
        private readonly HttpClient _httpClient;
        private readonly OuterServiceSettingRepository _settingsRepository;

        public LegalCollectionNotificationTemplateHttpService(HttpClient httpClient, OuterServiceSettingRepository settingsRepository)
        {
            _httpClient = httpClient;
            _settingsRepository = settingsRepository;
        }

        public async Task<LegalCaseNotificationTemplateDto> Create(CreateLegalCaseNotificationTemplateCommand request)
        {
            var baseUrl = GetUrl(Constants.LEGAL_COLLECTION_INTEGRATION_SETTING_CODE);
            var jsonContent =
                    new StringContent(JsonConvert.SerializeObject(request), Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync($"{baseUrl}/api/LegalCaseNotificationTemplate/CreateLegalCaseNotificationTemplate", jsonContent);
            response.EnsureSuccessStatusCode();
            var responseContent = await response.Content.ReadAsStringAsync();
            var model = JsonConvert.DeserializeObject<LegalCaseNotificationTemplateDto>(responseContent);

            return model;
        }

        public async Task<List<LegalCaseNotificationTemplateDto>> List()
        {
            var baseUrl = GetUrl(Constants.LEGAL_COLLECTION_INTEGRATION_SETTING_CODE);
            var response = await _httpClient.GetAsync($"{baseUrl}/api/LegalCaseNotificationTemplate/GetLegalCaseNotificationTemplateList");
            var responseContent = await response.Content.ReadAsStringAsync();
            var model = JsonConvert.DeserializeObject<List<LegalCaseNotificationTemplateDto>>(responseContent);

            return model;
        }

        public async Task<LegalCaseNotificationTemplateDto> Card(LegalCaseNotificationTemplateCardQuery query)
        {
            var baseUrl = GetUrl(Constants.LEGAL_COLLECTION_INTEGRATION_SETTING_CODE);
            var response = await _httpClient.GetAsync($"{baseUrl}/api/legalcasenotificationtemplate/getlegalcasenotificationtemplatebyid?Id={query.Id}");
            var responseContent = await response.Content.ReadAsStringAsync();
            var model = JsonConvert.DeserializeObject<LegalCaseNotificationTemplateDto>(responseContent);

            return model;
        }

        public async Task<LegalCaseNotificationTemplateDto> Update(UpdateLegalCaseNotificationTemplateCommand request)
        {
            var baseUrl = GetUrl(Constants.LEGAL_COLLECTION_INTEGRATION_SETTING_CODE);
            var jsonContent =
                    new StringContent(JsonConvert.SerializeObject(request), Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync($"{baseUrl}/api/LegalCaseNotificationTemplate/UpdateLegalCaseNotificationTemplate", jsonContent);
            response.EnsureSuccessStatusCode();
            var responseContent = await response.Content.ReadAsStringAsync();
            var model = JsonConvert.DeserializeObject<LegalCaseNotificationTemplateDto>(responseContent);

            return model;
        }

        public async Task<int> Delete(DeleteLegalCaseNotificationTemplateCommand request)
        {
            var baseUrl = GetUrl(Constants.LEGAL_COLLECTION_INTEGRATION_SETTING_CODE);
            var response = await _httpClient.DeleteAsync($"{baseUrl}/api/LegalCaseNotificationTemplate/DeleteLegalCaseNotificationTemplate?Id={request.Id}");
            var responseContent = await response.Content.ReadAsStringAsync();
            var model = JsonConvert.DeserializeObject<int>(responseContent);

            return model;
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
