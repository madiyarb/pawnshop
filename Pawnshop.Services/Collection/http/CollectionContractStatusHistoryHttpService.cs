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
    public class CollectionContractStatusHistoryHttpService : ICollectionHttpService<CollectionHistory>
    {
        private readonly HttpClient _httpClient;
        private readonly OuterServiceSettingRepository _settingRepository;
        public CollectionContractStatusHistoryHttpService(HttpClient httpClient, OuterServiceSettingRepository settingRepository)
        {
            _httpClient = httpClient;
            _settingRepository = settingRepository;
        }
        public async Task<List<CollectionHistory>> List()
        {
            var settings = _settingRepository.Find(new { Code = Constants.COLLECTION_INTEGRATION_SETTING_CODE });

            var response = _httpClient.GetAsync(settings.URL + $"/api/history/getcontracthistorylist").Result;
            var responseContent = response.Content.ReadAsStringAsync().Result;
            var model = JsonConvert.DeserializeObject<List<CollectionHistory>>(responseContent);



            return model;
        }
        public async Task<CollectionHistory> Get(string id)
        {
            var settings = _settingRepository.Find(new { Code = Constants.COLLECTION_INTEGRATION_SETTING_CODE });

            var response = _httpClient.GetAsync(settings.URL + $"/api/history/getcontracthistory?id={id}").Result;
            var responseContent = response.Content.ReadAsStringAsync().Result;
            var model = JsonConvert.DeserializeObject<CollectionHistory>(responseContent);

            return model;
        }
        public async Task<List<CollectionHistory>> GetByContractId(string contractId)
        {
            var settings = _settingRepository.Find(new { Code = Constants.COLLECTION_INTEGRATION_SETTING_CODE });

            var response = _httpClient.GetAsync(settings.URL + $"/api/history/getcontracthistorybycontractid?contractId={contractId}").Result;
            var responseContent = response.Content.ReadAsStringAsync().Result;
            var model = JsonConvert.DeserializeObject<List<CollectionHistory>>(responseContent);

            return model;
        }
        public async Task<int> Create(CollectionHistory item)
        {
            var settings = _settingRepository.Find(new { Code = Constants.COLLECTION_INTEGRATION_SETTING_CODE });

            var dataJson = JsonConvert.SerializeObject(item);
            var data = new StringContent(dataJson, Encoding.UTF8, "application/json");
            var response = _httpClient.PostAsync(settings.URL + "/api/history/createcontracthistory", data).Result;
            var responseContent = response.Content.ReadAsStringAsync().Result;

            if (response.StatusCode == System.Net.HttpStatusCode.OK)
                return JsonConvert.DeserializeObject<int>(responseContent);

            return 0;
        }
        public async Task<int> Update(CollectionHistory item)
        {
            var settings = _settingRepository.Find(new { Code = Constants.COLLECTION_INTEGRATION_SETTING_CODE });

            var dataJson = JsonConvert.SerializeObject(item);
            var data = new StringContent(dataJson, Encoding.UTF8, "application/json");
            var response = _httpClient.PostAsync(settings.URL + "/api/history/updatecontracthistory", data).Result;

            if (response.StatusCode == System.Net.HttpStatusCode.OK)
                return 1;

            return 0;
        }
        public async Task<int> Delete(string id)
        {
            var settings = _settingRepository.Find(new { Code = Constants.COLLECTION_INTEGRATION_SETTING_CODE });

            var response = _httpClient.GetAsync(settings.URL + $"/api/history/deletecontracthistory?id={id}").Result;

            if (response.StatusCode == System.Net.HttpStatusCode.OK)
                return 1;

            return 0;
        }
    }
}
