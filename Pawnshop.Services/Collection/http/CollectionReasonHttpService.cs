using Pawnshop.Data.Models.Collection;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System;
using Newtonsoft.Json;
using Pawnshop.Core;
using Pawnshop.Data.Access;

namespace Pawnshop.Services.Collection.http
{
    public class CollectionReasonHttpService : ICollectionHttpService<CollectionReason>
    {
        private readonly HttpClient _httpClient;
        private readonly OuterServiceSettingRepository _settingRepository;

        public CollectionReasonHttpService(HttpClient httpClient, OuterServiceSettingRepository settingRepository)
        {
            _httpClient = httpClient;
            _settingRepository = settingRepository;
        }
        public async Task<List<CollectionReason>> List()
        {
            var settings = _settingRepository.Find(new { Code = Constants.COLLECTION_INTEGRATION_SETTING_CODE });

            var response = _httpClient.GetAsync(settings.URL + $"/api/reason/getReasonList").Result;
            var responseContent = response.Content.ReadAsStringAsync().Result;
            var model = JsonConvert.DeserializeObject<List<CollectionReason>>(responseContent);

            return model;
        }
        public async Task<CollectionReason> Get(string id)
        {
            var settings = _settingRepository.Find(new { Code = Constants.COLLECTION_INTEGRATION_SETTING_CODE });

            var response = _httpClient.GetAsync(settings.URL + $"/api/reason/getreason?id={id}").Result;
            var responseContent = response.Content.ReadAsStringAsync().Result;
            var model = JsonConvert.DeserializeObject<CollectionReason>(responseContent);

            return model;
        }
        public async Task<List<CollectionReason>> GetByContractId(string contractId)
        {
            throw new NotImplementedException();
        }
        public async Task<int> Create(CollectionReason item)
        {
            var settings = _settingRepository.Find(new { Code = Constants.COLLECTION_INTEGRATION_SETTING_CODE });

            var dataJson = JsonConvert.SerializeObject(item);
            var data = new StringContent(dataJson, Encoding.UTF8, "application/json");
            var response = _httpClient.PostAsync(settings.URL + $"/api/reason/createReason", data).Result;
            var responseContent = response.Content.ReadAsStringAsync().Result;

            if (response.StatusCode == System.Net.HttpStatusCode.OK)
                return JsonConvert.DeserializeObject<int>(responseContent);

            return 0;
        }
        public async Task<int> Update(CollectionReason item)
        {
            var settings = _settingRepository.Find(new { Code = Constants.COLLECTION_INTEGRATION_SETTING_CODE });

            var dataJson = JsonConvert.SerializeObject(item);
            var data = new StringContent(dataJson, Encoding.UTF8, "application/json");
            var response = _httpClient.PostAsync(settings.URL + $"/api/reason/updateReason", data).Result;

            if (response.StatusCode == System.Net.HttpStatusCode.OK)
                return 1;

            return 0;
        }
        public async Task<int> Delete(string id)
        {
            var settings = _settingRepository.Find(new { Code = Constants.COLLECTION_INTEGRATION_SETTING_CODE });

            var response = _httpClient.GetAsync(settings.URL + $"/api/reason/deleteReason?id={id}").Result;

            if (response.StatusCode == System.Net.HttpStatusCode.OK)
                return 1;

            return 0;
        }
    }
}
