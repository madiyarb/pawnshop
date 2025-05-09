using Newtonsoft.Json;
using Pawnshop.Data.Access;
using Pawnshop.Data.Models.LegalCollection.Dtos.LegalCase;
using Pawnshop.Services.LegalCollection.HttpServices.Dtos.LegalCaseNotificationHistory;
using Pawnshop.Services.LegalCollection.Inerfaces;
using Pawnshop.Services.Models.List;
using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Pawnshop.Core;
using Pawnshop.Core.Exceptions;
using RestSharp.Extensions;

namespace Pawnshop.Services.LegalCollection.HttpServices
{
    public class LegalCollectionNotificationHttpService : ILegalCollectionNotificationHttpService
    {
        private readonly HttpClient _httpClient;
        private readonly UserRepository _userRepository;
        private readonly OuterServiceSettingRepository _settingsRepository;

        public LegalCollectionNotificationHttpService(
            HttpClient httpClient, 
            UserRepository userRepository,
            OuterServiceSettingRepository settingsRepository)
        {
            _httpClient = httpClient;
            _userRepository = userRepository;
            _settingsRepository = settingsRepository;
        }

        public async Task<int> Create(CreateLegalCaseNotificationHistoryCommand request)
        {
            var baseUrl = GetUrl(Constants.LEGAL_COLLECTION_INTEGRATION_SETTING_CODE);
            var jsonContent =
                    new StringContent(JsonConvert.SerializeObject(request), Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync($"{baseUrl}/api/legalcasenotificationhistory/createlegalcasenotificationhistory", jsonContent);
            var responseContent = await response.Content.ReadAsStringAsync();
            var model = JsonConvert.DeserializeObject<int>(responseContent);

            return model;
        }

        public async Task<ListModel<LegalCaseNotificationHistoryDto>> List()
        {
            var baseUrl = GetUrl(Constants.LEGAL_COLLECTION_INTEGRATION_SETTING_CODE);
            var response = await _httpClient.GetAsync($"{baseUrl}/api/legalcasenotificationhistory/getlegalcasenotificationhistorylist");
            var responseContent = await response.Content.ReadAsStringAsync();
            var model = JsonConvert.DeserializeObject<ListModel<LegalCaseNotificationHistoryDto>>(responseContent);
            model.List.ForEach(x => x.Author = _userRepository.Get(x.CreateBy));
            return model;
        }
        
        public async Task<ListModel<LegalCaseNotificationHistoryDto>> PagedList(int itemFrom, int pageSize)
        {
            var baseUrl = GetUrl(Constants.LEGAL_COLLECTION_INTEGRATION_SETTING_CODE);
            var response = await _httpClient.GetAsync($"{baseUrl}/api/legalcasenotificationhistory/getlegalcasenotificationhistorypagedlist?pageSize={pageSize}&itemFrom={itemFrom}");
            var responseContent = await response.Content.ReadAsStringAsync();
            var model = JsonConvert.DeserializeObject<ListModel<LegalCaseNotificationHistoryDto>>(responseContent);
            model.List.ForEach(x => x.Author = _userRepository.Get(x.CreateBy));
            return model;
        }

        public async Task<int> Update(UpdateLegalCaseNotificationHistoryCommand request)
        {
            var baseUrl = GetUrl(Constants.LEGAL_COLLECTION_INTEGRATION_SETTING_CODE);
            var jsonContent =
                    new StringContent(JsonConvert.SerializeObject(request), Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync($"{baseUrl}/api/legalcasenotificationhistory/updatelegalcasenotificationhistory", jsonContent);
            response.EnsureSuccessStatusCode();
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
