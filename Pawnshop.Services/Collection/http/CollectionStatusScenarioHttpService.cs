using Newtonsoft.Json;
using Pawnshop.Core;
using Pawnshop.Data.Access;
using Pawnshop.Data.Models.Collection;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Pawnshop.Services.Collection.http
{
    public class CollectionStatusScenarioHttpService : ICollectionHttpService<CollectionStatusScenario>
    {
        private readonly HttpClient _httpClient;
        private readonly OuterServiceSettingRepository _settingRepository;
        public CollectionStatusScenarioHttpService(HttpClient httpClient, OuterServiceSettingRepository settingRepository)
        {
            _httpClient = httpClient;
            _settingRepository = settingRepository;
        }
        public async Task<List<CollectionStatusScenario>> List()
        {
            var settings = _settingRepository.Find(new { Code = Constants.COLLECTION_INTEGRATION_SETTING_CODE });

            var response = await _httpClient.GetAsync(settings.URL + "/api/StatusScenario/getstatusscenariolist");
            var responseContent = response.Content.ReadAsStringAsync().Result;
            var model = JsonConvert.DeserializeObject<List<CollectionStatusScenario>>(responseContent);

            return model;
        }
        public async Task<CollectionStatusScenario> Get(string id)
        {
            var settings = _settingRepository.Find(new { Code = Constants.COLLECTION_INTEGRATION_SETTING_CODE });

            var response = await _httpClient.GetAsync(settings.URL + $"/api/StatusScenario/getStatusScenario?id={id}");
            var responseContent = response.Content.ReadAsStringAsync().Result;
            var model = JsonConvert.DeserializeObject<CollectionStatusScenario>(responseContent);

            return model;
        }
        public async Task<List<CollectionStatusScenario>> GetByContractId(string contractId)
        {
            throw new NotImplementedException();
        }
        public async Task<int> Create(CollectionStatusScenario item)
        {
            var settings = _settingRepository.Find(new { Code = Constants.COLLECTION_INTEGRATION_SETTING_CODE });

            var dataJson = JsonConvert.SerializeObject(item);
            var data = new StringContent(dataJson, Encoding.UTF8, "application/json");
            var response = _httpClient.PostAsync(settings.URL + "/api/StatusScenario/createStatusScenario", data).Result;////api/StatusScenario/createStatusScenario
            var responseContent = response.Content.ReadAsStringAsync().Result;

            if (response.StatusCode == System.Net.HttpStatusCode.OK)
                return JsonConvert.DeserializeObject<int>(responseContent);

            return 0;
        }
        public async Task<int> Update(CollectionStatusScenario item)
        {
            var settings = _settingRepository.Find(new { Code = Constants.COLLECTION_INTEGRATION_SETTING_CODE });

            var dataJson = JsonConvert.SerializeObject(item);
            var data = new StringContent(dataJson, Encoding.UTF8, "application/json");
            var response = _httpClient.PostAsync(settings.URL + "/api/StatusScenario/updateStatusScenario", data).Result;

            if (response.StatusCode == System.Net.HttpStatusCode.OK)
                return 1;

            return 0;
        }
        public async Task<int> Delete(string id)
        {
            var settings = _settingRepository.Find(new { Code = Constants.COLLECTION_INTEGRATION_SETTING_CODE });

            var response = _httpClient.GetAsync(settings.URL + $"/api/StatusScenario/deleteStatusScenario?id={id}").Result;

            if (response.StatusCode == System.Net.HttpStatusCode.OK)
                return 1;

            return 0;
        }
    }
}
