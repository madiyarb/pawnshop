using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Pawnshop.Core;
using Pawnshop.Core.Exceptions;
using Pawnshop.Data.Access;
using Pawnshop.Data.Models.DebtorRegistry;
using Pawnshop.Data.Models.LegalCollection;
using Pawnshop.Services.DebtorRegisrty.Dtos;
using RestSharp.Extensions;

namespace Pawnshop.Services.DebtorRegisrty.HttpService
{
    public class DebtorRegisterHttpService : IDebtorRegisterHttpService
    {
        private readonly HttpClient _httpClient;
        private readonly OuterServiceSettingRepository _settingsRepository;
        private const string Api = "api/debtor-register";
        
        public DebtorRegisterHttpService(HttpClient httpClient, OuterServiceSettingRepository settingsRepository)
        {
            _httpClient = httpClient;
            _settingsRepository = settingsRepository;
            
            _httpClient.BaseAddress = GetApiUrl(Constants.LEGAL_COLLECTION_INTEGRATION_SETTING_CODE);
        }

        public async Task<PagedResponse<DebtorDetailsDto>> GetByFilters(FilteredDebtorRegisterHttpRequest request)
        {
            var url = $"{_httpClient.BaseAddress}/list";
            var jsonContent =
                new StringContent(JsonConvert.SerializeObject(request), Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync(url, jsonContent);

            var httpStatusCode = response.EnsureSuccessStatusCode().StatusCode;
            if (httpStatusCode != HttpStatusCode.OK)
            {
                throw new PawnshopApplicationException($"http response status: {httpStatusCode}");
            }

            var jsonString = await response.Content.ReadAsStringAsync();
            var pagedResponse = JsonConvert.DeserializeObject<PagedResponse<DebtorDetailsDto>>(jsonString);

            return pagedResponse;
        }
        
        public async Task<List<DebtorDetailsResponseDto>> Details(string iin)
        {
            var url = $"{_httpClient.BaseAddress}/details?iin={iin}";

            var response = await _httpClient.GetAsync(url);

            var httpStatusCode = response.EnsureSuccessStatusCode().StatusCode;
            if (httpStatusCode != HttpStatusCode.OK)
            {
                throw new PawnshopApplicationException($"http response status: {httpStatusCode}");
            }

            var jsonString = await response.Content.ReadAsStringAsync();
            var pagedResponse = JsonConvert.DeserializeObject<List<DebtorDetailsResponseDto>>(jsonString);

            return pagedResponse;
        }

        public async Task<List<DebtorDetailsDto>> GetListByIdentityNumbers(List<string> identityNumbers)
        {
            var url = $"{_httpClient.BaseAddress}/get-by-iins";
            var jsonContent =
                new StringContent(JsonConvert.SerializeObject(identityNumbers), Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync(url, jsonContent);

            var httpStatusCode = response.EnsureSuccessStatusCode().StatusCode;
            if (httpStatusCode != HttpStatusCode.OK)
            {
                throw new PawnshopApplicationException($"http response status: {httpStatusCode}");
            }

            var jsonString = await response.Content.ReadAsStringAsync();
            var pagedResponse = JsonConvert.DeserializeObject<List<DebtorDetailsDto>>(jsonString);

            return pagedResponse;
        }


        private Uri GetApiUrl(string login)
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
            
            var builder = new UriBuilder(url);
            builder.Path += Api;

            _httpClient.BaseAddress = builder.Uri;
            
            return builder.Uri;
        }
    }
}