using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Pawnshop.Core;
using Pawnshop.Core.Exceptions;
using Pawnshop.Data.Access;
using Pawnshop.Data.Models.LegalCollection.Dtos;
using Pawnshop.Services.LegalCollection.HttpServices.Interfaces;
using RestSharp.Extensions;

namespace Pawnshop.Services.LegalCollection
{
    public class LegalCollectionStatusesHttpService : ILegalCollectionStatusesHttpService
    {
        private readonly HttpClient _httpClient;
        private readonly OuterServiceSettingRepository _settingsRepository;
        private readonly ILogger<LegalCollectionStatusesHttpService> _logger;

        public LegalCollectionStatusesHttpService(HttpClient httpClient, OuterServiceSettingRepository settingsRepository,
            ILogger<LegalCollectionStatusesHttpService> logger)
        {
            _httpClient = httpClient;
            _settingsRepository = settingsRepository;
            _logger = logger;
        }

        public async Task<List<LegalCaseStatusDto>> List()
        {
            var baseUrl = GetUrl(Constants.LEGAL_COLLECTION_INTEGRATION_SETTING_CODE);
            var url = $"{baseUrl}/getlegalcasestatuses";

            try
            {
                HttpResponseMessage response = await _httpClient.GetAsync(url);

                response.EnsureSuccessStatusCode();

                string responseBody = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<List<LegalCaseStatusDto>>(responseBody);
            }
            catch (Exception e)
            {
                _logger.LogError("При попытке получить LegalCaseStatus произошла ошибка. {Error}", e.Message);
                return null;
            }
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