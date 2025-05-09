using Newtonsoft.Json;
using Pawnshop.Core;
using Pawnshop.Services.Models.UKassa;
using Pawnshop.Data.Models.OuterServiceSettings;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Pawnshop.Core.Exceptions;
using System.Threading;
using System.Linq;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using System.Net;

namespace Pawnshop.Services.Integrations.UKassa
{
    public class UKassaHttpService : IUKassaHttpService
    {
        private readonly HttpClient _http;
        private readonly IRepository<OuterServiceSetting> _outerServiceSettingRepository;
        private static string token;
        private readonly IMemoryCache _memoryCache;
        private readonly ILogger<UKassaHttpService> _logger;

        public UKassaHttpService(HttpClient http, IRepository<OuterServiceSetting> outerServiceSettingRepository, IMemoryCache memoryCache,
            ILogger<UKassaHttpService> logger)
        {
            _http = http;
            _outerServiceSettingRepository = outerServiceSettingRepository;
            _memoryCache = memoryCache;
            _logger = logger;
        }

        public (bool, string) SendRequest(string idempKey, string url, string requestData)
        {
            _logger.LogInformation($"Start SendRequest. idempKey: {idempKey}");
            //проверяю по кэшу отправляли ли мы такой запрос
            var alreadySent = _memoryCache.Get(idempKey);
            if (alreadySent != null)
            {
               _logger.LogInformation($"SendRequest double idempKey: {idempKey}. Return");
               return (false, "Already_sent");
            }

            //добавляю в кэш ключ чтоб не отправлять такой же запрос еще раз
            _memoryCache.Set(idempKey, 1, new MemoryCacheEntryOptions { AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5) });

            Thread.Sleep(1000);
            _http.DefaultRequestHeaders.Clear();
            var settings = _outerServiceSettingRepository.Find(new { Code = "UKASSA" });
            CheckLogin(settings.URL);
            var data = new StringContent(requestData, Encoding.UTF8, "application/json");
            _http.DefaultRequestHeaders.Add("Authorization", "Token " + token);
            _http.DefaultRequestHeaders.Add("IDEMPOTENCE-KEY", idempKey);
            var response = _http.PostAsync($"{settings.URL}/operation/{url}/", data).Result;
            if (response.StatusCode == HttpStatusCode.Conflict)
            {
                var temp = JsonConvert.DeserializeObject<UKassaGenerateCheckRequest>(requestData);
                temp.skip_idempotence_error = true;
                var tempRequestData = JsonConvert.SerializeObject(temp);
                var tempData = new StringContent(tempRequestData, Encoding.UTF8, "application/json");
                response = _http.PostAsync($"{settings.URL}/operation/{url}/", tempData).Result;
            }

            var responseContent = response.Content.ReadAsStringAsync();
            string responseContentString = responseContent.Result;
            if (response.IsSuccessStatusCode)
                return (true, responseContentString);
            return (false, responseContentString);
        }

        public List<Shift> GetShiftReports(int kassaId, DateTime dateFrom, DateTime dateTo)
        {
            var settings = _outerServiceSettingRepository.Find(new { Code = "UKASSA" });
            CheckLogin(settings.URL);
            _http.DefaultRequestHeaders.Clear();
            _http.DefaultRequestHeaders.Add("Authorization", "Token " + token);
            var formContent = new GetShiftReportsRequest()
            {
                kassa = kassaId,
                start = dateFrom,
                end = dateTo
            };
            var data = new StringContent(JsonConvert.SerializeObject(formContent), Encoding.UTF8, "application/json");
            var response = _http.PostAsync($"{settings.URL}/kassa/get_shift_reports/", data).Result;
            var responseContent = response.Content.ReadAsStringAsync();
            var model = JsonConvert.DeserializeObject<List<Shift>>(responseContent.Result);
            return model;
        }

        public UKassaReportResponse GetZReport(int shiftId)
        {
            var settings = _outerServiceSettingRepository.Find(new { Code = "UKASSA" });
            CheckLogin(settings.URL);
            _http.DefaultRequestHeaders.Clear();
            _http.DefaultRequestHeaders.Add("Authorization", "Token " + token);
            var formContent = new GetZReportRequest()
            {
                shift = shiftId
            };
            var data = new StringContent(JsonConvert.SerializeObject(formContent), Encoding.UTF8, "application/json");
            var response = _http.PostAsync($"{settings.URL}/kassa/get_z_report_section/", data).Result;
            var responseContent = response.Content.ReadAsStringAsync();
            var model = JsonConvert.DeserializeObject<UKassaReportResponse>(responseContent.Result);
            return model;
        }

        public UKassaReportResponse GetXReport(int kassaId)
        {
            var settings = _outerServiceSettingRepository.Find(new { Code = "UKASSA" });
            CheckLogin(settings.URL);
            _http.DefaultRequestHeaders.Clear();
            _http.DefaultRequestHeaders.Add("Authorization", "Token " + token);
            var formContent = new GetXReportRequest()
            {
                kassa = kassaId
            };
            var data = new StringContent(JsonConvert.SerializeObject(formContent), Encoding.UTF8, "application/json");
            var response = _http.PostAsync($"{settings.URL}/kassa/get_shift_report/", data).Result;
            var responseContent = response.Content.ReadAsStringAsync();
            var model = JsonConvert.DeserializeObject<UKassaReportResponse>(responseContent.Result);
            return model;
        }

        public List<UKassaOperation> GetShiftOperations(int shiftId)
        {
            var settings = _outerServiceSettingRepository.Find(new { Code = "UKASSA" });
            CheckLogin(settings.URL);
            _http.DefaultRequestHeaders.Clear();
            _http.DefaultRequestHeaders.Add("Authorization", "Token " + token);
            var response = _http.GetAsync($"{settings.URL}/kassa/get_shift_operations/?shift={shiftId}").Result;
            var responseContent = response.Content.ReadAsStringAsync();
            var model = JsonConvert.DeserializeObject<List<UKassaOperation>>(responseContent.Result);
            return model;
        }

        public Shift GetActiveShift(int kassaId)
        {
            var settings = _outerServiceSettingRepository.Find(new { Code = "UKASSA" });
            CheckLogin(settings.URL);
            _http.DefaultRequestHeaders.Clear();
            _http.DefaultRequestHeaders.Add("Authorization", "Token " + token);
            var response = _http.GetAsync($"{settings.URL}/kassa/get_shift_list/?kassa={kassaId}").Result;
            var responseContent = response.Content.ReadAsStringAsync();
            var model = JsonConvert.DeserializeObject<List<Shift>>(responseContent.Result);
            return model.FirstOrDefault(x => x.is_active);
        }

        public void Login()
        {
            var settings = _outerServiceSettingRepository.Find(new { Code = "UKASSA" });
            var formContent = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("email", settings.Login),
                new KeyValuePair<string, string>("hashline", Guid.NewGuid().ToString()),
                new KeyValuePair<string, string>("password", settings.Password),
                new KeyValuePair<string, string>("code", "")
            });
            var response = _http.PostAsync($"{settings.URL}/auth/login/", formContent).Result;
            var responseContent = response.Content.ReadAsStringAsync();
            var model = JsonConvert.DeserializeObject<UKassaLoginResponse>(responseContent.Result);
            token = model.auth_token;
        }

        public void CheckLogin(string url)
        {
            var response = _http.GetAsync($"{url}/auth/get_user/").Result;
            if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            { Login(); }
        }

        public void Logout(string token)
        {
            throw new NotImplementedException();
        }
    }
}
