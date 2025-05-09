using System;
using System.Net.Http.Headers;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using RestSharp;
using Pawnshop.Data.Models.MobileApp;
using Microsoft.Extensions.Options;
using Pawnshop.Core.Options;
using Pawnshop.Services.HardCollection.HttpClientService.Interfaces;

namespace Pawnshop.Services.HardCollection.HttpClientService.Impl
{
    public class HttpSender : IHttpSender
    {
        private readonly string _mobileAppUser;
        private readonly string _mobileAppPassword;
        private readonly string _mobileAppUrl;
        private static string _mobAppToken;
        private static DateTime _mobAppTokenTime;

        public HttpSender(IOptions<EnviromentAccessOptions> options)
        {
            _mobileAppUrl = options.Value.MobileAppUrl;
            _mobileAppPassword = options.Value.MobileAppPassword;
            _mobileAppUser = options.Value.MobileAppUser;
        }

        public async Task<HttpResponseMessage> SendRequestAsync(string jsonModel, string url)
        {
            var token = await GetTokenAsync();
            var absoluteUrl = GetUrl(url);
            using (var client = new HttpClient())
            {
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                var response = await client.PostAsync(
                    absoluteUrl,
                     new StringContent(jsonModel, Encoding.UTF8, "application/json"));

                return response;
            }
        }

        private async Task<string> GetTokenAsync()
        {
            if (_mobAppToken != string.Empty && _mobAppTokenTime.Date == DateTime.Now.Date)
                return _mobAppToken;

            var url = GetUrl("auth/login");
            var client = new RestClient(url);

            var request = new RestRequest(Method.POST);
            request.Parameters.Clear();
            request.AlwaysMultipartFormData = true;
            request.AddParameter("username", _mobileAppUser);
            request.AddParameter("password", _mobileAppPassword);
            IRestResponse response = await client.ExecuteAsync(request);

            if (response.IsSuccessful)
            {
                _mobAppToken = JsonConvert.DeserializeObject<Token>(response.Content).AccessToken;
                _mobAppTokenTime = DateTime.Now;
            }

            return _mobAppToken;
        }

        private string GetUrl(string requestUrl)
        {
            if (string.IsNullOrWhiteSpace(requestUrl))
                throw new ArgumentNullException(nameof(requestUrl));

            return $@"{_mobileAppUrl}{requestUrl}";
        }
    }
}
