using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Pawnshop.Core;
using Pawnshop.Core.Exceptions;
using Pawnshop.Data.Access;
using Pawnshop.Data.Models.LegalCollection.Dtos;
using Pawnshop.Services.LegalCollection.HttpServices.Interfaces;
using RestSharp.Extensions;

namespace Pawnshop.Services.LegalCollection.HttpServices
{
    public class LegalCollectionChangeCourseHttpService : ILegalCollectionChangeCourseHttpService
    {
        private readonly HttpClient _httpClient;
        private readonly OuterServiceSettingRepository _settingsRepository;

        public LegalCollectionChangeCourseHttpService(HttpClient httpClient, OuterServiceSettingRepository settingsRepository)
        {
            _httpClient = httpClient;
            _settingsRepository = settingsRepository;
        }

        public async Task<ChangeCourseActionsResponse> List()
        {
            var baseUrl = GetUrl(Constants.LEGAL_COLLECTION_INTEGRATION_SETTING_CODE);
            var url = $"{baseUrl}/api/changecourseactions/list";

            try
            {
                HttpResponseMessage response = await _httpClient.GetAsync(url);

                if (response.IsSuccessStatusCode)
                {
                    string responseBody = await response.Content.ReadAsStringAsync();
                    var result = JsonConvert.DeserializeObject<List<ChangeCourseActionDto>>(responseBody);
                    
                    return new ChangeCourseActionsResponse
                    {
                        ChangeCourseActions = result,
                        Count = result.Count
                    };
                }

                string errorMessage = await response.Content.ReadAsStringAsync();
                throw new PawnshopApplicationException(
                    $"Не удалось получить данные по смене Направлений Legal collection. StatusCode: {response.StatusCode}. Сообщение: {errorMessage}");
            }
            catch (Exception e)
            {
                throw new PawnshopApplicationException(
                    $"Не удалось получить данные по смене Направлений Legal collection. {e.Message}");
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