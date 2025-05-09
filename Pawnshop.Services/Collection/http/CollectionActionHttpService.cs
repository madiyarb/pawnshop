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
    public class CollectionActionHttpService : ICollectionHttpService<CollectionActions>
    {
        private readonly HttpClient _httpClient;
        private readonly OuterServiceSettingRepository _settingRepository;
        public CollectionActionHttpService(HttpClient httpClient, OuterServiceSettingRepository settingRepository)
        {
            _httpClient = httpClient;
            _settingRepository = settingRepository;
        }
        public async Task<List<CollectionActions>> List()
        {
            var settings = _settingRepository.Find(new { Code = Constants.COLLECTION_INTEGRATION_SETTING_CODE });

            var response = _httpClient.GetAsync(settings.URL + "/api/action/getactionlist").Result;
            var responseContent = response.Content.ReadAsStringAsync().Result;
            var model = JsonConvert.DeserializeObject<List<CollectionActions>>(responseContent);

            return model;
        }
        public async Task<CollectionActions> Get(string id)
        {
            var settings = _settingRepository.Find(new { Code = Constants.COLLECTION_INTEGRATION_SETTING_CODE });

            var response = _httpClient.GetAsync(settings.URL + $"/api/action/getaction?id={id}").Result;
            var responseContent = response.Content.ReadAsStringAsync().Result;
            var model = JsonConvert.DeserializeObject<CollectionActions>(responseContent);

            return model;
        }
        public async Task<List<CollectionActions>> GetByContractId(string contractId)
        {
            throw new NotImplementedException();
        }
        public async Task<int> Create(CollectionActions action)
        {
            var settings = _settingRepository.Find(new { Code = Constants.COLLECTION_INTEGRATION_SETTING_CODE });

            var dataJson = JsonConvert.SerializeObject(action);
            var data = new StringContent(dataJson, Encoding.UTF8, "application/json");
            var response = _httpClient.PostAsync(settings.URL + $"/api/action/createAction", data).Result;
            var responseContent = response.Content.ReadAsStringAsync().Result;

            if (response.StatusCode == System.Net.HttpStatusCode.OK)
                return JsonConvert.DeserializeObject<int>(responseContent);

            return 0;
        }
        public async Task<int> Update(CollectionActions action)
        {
            var settings = _settingRepository.Find(new { Code = Constants.COLLECTION_INTEGRATION_SETTING_CODE });

            var dataJson = JsonConvert.SerializeObject(action);
            var data = new StringContent(dataJson, Encoding.UTF8, "application/json");
            var response = _httpClient.PostAsync(settings.URL + $"/api/action/updateAction", data).Result;

            if (response.StatusCode == System.Net.HttpStatusCode.OK)
                return 1;

            return 0;
        }
        public async Task<int> Delete(string id)
        {
            var settings = _settingRepository.Find(new { Code = Constants.COLLECTION_INTEGRATION_SETTING_CODE });

            var response = _httpClient.GetAsync(settings.URL + $"/api/action/deleteAction?id={id}").Result;

            if (response.StatusCode == System.Net.HttpStatusCode.OK)
                return 1;

            return 0;
        }
    }
}
