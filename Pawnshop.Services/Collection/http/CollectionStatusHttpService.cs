using Pawnshop.Data.Models.Collection;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System;
using Newtonsoft.Json;
using Pawnshop.Data.Access;
using Pawnshop.Core;

namespace Pawnshop.Services.Collection.http
{
    public class CollectionStatusHttpService : ICollectionHttpService<CollectionStatus>
    {
        private readonly HttpClient _httpClient;
        private readonly OuterServiceSettingRepository _settingRepository;
        public CollectionStatusHttpService(HttpClient httpClient, OuterServiceSettingRepository settingRepository)
        {
            _httpClient = httpClient;
            _settingRepository = settingRepository;
        }
        public async Task<List<CollectionStatus>> List()
        {
            var settings = _settingRepository.Find(new { Code = Constants.COLLECTION_INTEGRATION_SETTING_CODE });

            var response = _httpClient.GetAsync(settings.URL + "/api/status/getStatusList").Result;
            var responseContent = response.Content.ReadAsStringAsync().Result;
            var model = JsonConvert.DeserializeObject<List<CollectionStatus>>(responseContent);

            return model;
        }
        public async Task<CollectionStatus> Get(string id)
        {
            var settings = _settingRepository.Find(new { Code = Constants.COLLECTION_INTEGRATION_SETTING_CODE });

            var response = _httpClient.GetAsync(settings.URL + $"/api/status/getStatus?id={id}").Result;
            var responseContent = response.Content.ReadAsStringAsync().Result;
            var model = JsonConvert.DeserializeObject<CollectionStatus>(responseContent);

            return model;
        }
        public async Task<List<CollectionStatus>> GetByContractId(string contractId)
        {
            throw new NotImplementedException();
        }
        public async Task<int> Create(CollectionStatus item)
        {
            var settings = _settingRepository.Find(new { Code = Constants.COLLECTION_INTEGRATION_SETTING_CODE });

            var dataJson = JsonConvert.SerializeObject(item);
            var data = new StringContent(dataJson, Encoding.UTF8, "application/json");
            var response = _httpClient.PostAsync(settings.URL + $"/api/status/createStatus", data).Result;
            var responseContent = response.Content.ReadAsStringAsync().Result;

            if (response.StatusCode == System.Net.HttpStatusCode.OK)
                return JsonConvert.DeserializeObject<int>(responseContent);

            return 0;
        }
        public async Task<int> Update(CollectionStatus item)
        {
            var settings = _settingRepository.Find(new { Code = Constants.COLLECTION_INTEGRATION_SETTING_CODE });

            var dataJson = JsonConvert.SerializeObject(item);
            var data = new StringContent(dataJson, Encoding.UTF8, "application/json");
            var response = _httpClient.PostAsync(settings.URL + $"/api/status/updateStatus", data).Result;

            if (response.StatusCode == System.Net.HttpStatusCode.OK)
                return 1;

            return 0;
        }
        public async Task<int> Delete(string id)
        {
            var settings = _settingRepository.Find(new { Code = Constants.COLLECTION_INTEGRATION_SETTING_CODE });

            var response = _httpClient.GetAsync(settings.URL + $"/api/status/deleteStatus?id={id}").Result;

            if (response.StatusCode == System.Net.HttpStatusCode.OK)
                return 1;

            return 0;
        }
    }
}
