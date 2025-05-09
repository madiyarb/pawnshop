using Newtonsoft.Json;
using Pawnshop.Data.Models.LegalCollection.Dtos.LegalCase;
using Pawnshop.Services.Collection.http;
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
    public class LegalCollectionStageScenariosHttpService : ICollectionHttpService<LegalCaseStageScenarioDto>
    {
        private readonly HttpClient _httpClient;
        private readonly OuterServiceSettingRepository _settingsRepository;

        public LegalCollectionStageScenariosHttpService(HttpClient httpClient, OuterServiceSettingRepository settingsRepository)
        {
            _httpClient = httpClient;
            _settingsRepository = settingsRepository;
        }

        public async Task<List<LegalCaseStageScenarioDto>> List()
        {
            var baseUrl = GetUrl(Constants.LEGAL_COLLECTION_INTEGRATION_SETTING_CODE);
            var response = await _httpClient.GetAsync($"{baseUrl}/api/legalcasestagescenario/getlegalcasestagescenariolist");
            response.EnsureSuccessStatusCode();
            var responseContent = await response.Content.ReadAsStringAsync();
            var model = JsonConvert.DeserializeObject<List<LegalCaseStageScenarioDto>>(responseContent);

            return model;
        }

        public async Task<LegalCaseStageScenarioDto> Get(string id)
        {
            var baseUrl = GetUrl(Constants.LEGAL_COLLECTION_INTEGRATION_SETTING_CODE);
            var response = await _httpClient.GetAsync($"{baseUrl}/api/legalcasestagescenario/getlegalcasestagescenario?id={id}");
            response.EnsureSuccessStatusCode();
            var responseContent = await response.Content.ReadAsStringAsync();
            var model = JsonConvert.DeserializeObject<LegalCaseStageScenarioDto>(responseContent);

            return model;
        }

        public async Task<List<LegalCaseStageScenarioDto>> GetByContractId(string contractId)
        {
            throw new NotImplementedException();
        }

        public async Task<int> Create(LegalCaseStageScenarioDto item)
        {
            var baseUrl = GetUrl(Constants.LEGAL_COLLECTION_INTEGRATION_SETTING_CODE);
            var dataJson = JsonConvert.SerializeObject(item);
            var data = new StringContent(dataJson, Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync($"{baseUrl}/api/legalcasestagescenario/createlegalcasestagescenario", data);
            response.EnsureSuccessStatusCode();
            var responseContent = await response.Content.ReadAsStringAsync();

            if (response.StatusCode == System.Net.HttpStatusCode.OK)
                return JsonConvert.DeserializeObject<int>(responseContent);

            return 0;
        }

        public async Task<int> Update(LegalCaseStageScenarioDto item)
        {
            var baseUrl = GetUrl(Constants.LEGAL_COLLECTION_INTEGRATION_SETTING_CODE);
            var dataJson = JsonConvert.SerializeObject(item);
            var data = new StringContent(dataJson, Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync($"{baseUrl}/api/legalcasestagescenario/updatelegalcasestagescenario", data);
            response.EnsureSuccessStatusCode();

            return 1;
        }

        public async Task<int> Delete(string id)
        {
            var baseUrl = GetUrl(Constants.LEGAL_COLLECTION_INTEGRATION_SETTING_CODE);
            var response = await _httpClient.GetAsync($"{baseUrl}/api/StatusScenario/deleteStatusScenario?id={id}");
            response.EnsureSuccessStatusCode();

            return 1;
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
