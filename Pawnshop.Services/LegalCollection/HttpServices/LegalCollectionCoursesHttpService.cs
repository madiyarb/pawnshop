using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Pawnshop.Core;
using Pawnshop.Core.Exceptions;
using Pawnshop.Data.Access;
using Pawnshop.Data.Models.LegalCollection.Dtos;
using Pawnshop.Services.LegalCollection.HttpServices.Dtos;
using Pawnshop.Services.LegalCollection.HttpServices.Interfaces;
using RestSharp.Extensions;

namespace Pawnshop.Services.LegalCollection
{
    public class LegalCollectionCoursesHttpService : ILegalCollectionCoursesHttpService
    {
        private readonly HttpClient _httpClient;
        private readonly OuterServiceSettingRepository _settingsRepository;

        public LegalCollectionCoursesHttpService(HttpClient httpClient, OuterServiceSettingRepository settingsRepository)
        {
            _httpClient = httpClient;
            _settingsRepository = settingsRepository;
        }

        public async Task<LegalCaseCourseDto> Create(CreateLegalCaseCourseCommand request)
        {
            var baseUrl = GetUrl(Constants.LEGAL_COLLECTION_INTEGRATION_SETTING_CODE);
            var url = $"{baseUrl}/api/legalcasecourse/create";
            try
            {
                var jsonContent =
                    new StringContent(JsonConvert.SerializeObject(request), Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync(url, jsonContent);
                response.EnsureSuccessStatusCode();
                var jsonString = await response.Content.ReadAsStringAsync();
                var responseModel = JsonConvert.DeserializeObject<LegalCaseCourseDto>(jsonString);

                return responseModel;
            }
            catch (Exception e)
            {
                throw new PawnshopApplicationException(
                    $"Не удалось создать Направление Legal collection. {e.Message}");
            }
        }

        public async Task<LegalCaseCourseDto> Details(int id)
        {
            var baseUrl = GetUrl(Constants.LEGAL_COLLECTION_INTEGRATION_SETTING_CODE);
            var url = $"{baseUrl}/api/legalcasecourse/details?id={id}";

            try
            {
                HttpResponseMessage response = await _httpClient.GetAsync(url);

                response.EnsureSuccessStatusCode();

                string responseBody = await response.Content.ReadAsStringAsync();

                var detailsResponse = JsonConvert.DeserializeObject<LegalCaseCourseDto>(responseBody);
                return detailsResponse;
            }
            catch (Exception e)
            {
                throw new PawnshopApplicationException(
                    $"Не удалось получить данные по направлению с Id: {id}. {e.Message}");
            }
        }

        public async Task<LegalCaseCourseList> List()
        {
            var baseUrl = GetUrl(Constants.LEGAL_COLLECTION_INTEGRATION_SETTING_CODE);
            var url = $"{baseUrl}/api/legalcasecourse/list";

            try
            {
                HttpResponseMessage response = await _httpClient.GetAsync(url);

                response.EnsureSuccessStatusCode();

                string responseBody = await response.Content.ReadAsStringAsync();
                var resultResponse = JsonConvert.DeserializeObject<List<LegalCaseCourseDto>>(responseBody);
                return new LegalCaseCourseList
                {
                    Count = resultResponse.Count,
                    List = resultResponse
                };
            }
            catch (Exception e)
            {
                throw new PawnshopApplicationException(
                    $"Не удалось получить данные по Направлениям Legal collection. {e.Message}");
            }
        }

        public async Task<LegalCaseCourseDto> Update(UpdateLegalCaseCourseCommand request)
        {
            var baseUrl = GetUrl(Constants.LEGAL_COLLECTION_INTEGRATION_SETTING_CODE);
            var url = $"{baseUrl}/api/legalcasecourse/update";
            
            try
            {
                var jsonContent =
                    new StringContent(JsonConvert.SerializeObject(request), Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync(url, jsonContent);
                response.EnsureSuccessStatusCode();
                var jsonString = await response.Content.ReadAsStringAsync();
                var responseModel = JsonConvert.DeserializeObject<LegalCaseCourseDto>(jsonString);

                return responseModel;
            }
            catch (Exception e)
            {
                throw new PawnshopApplicationException(
                    $"Не удалось обновить данные по направлению с Id: {request.Id}. {e.Message}");
            }
        }

        public async Task<int> Delete(DeleteLegalCaseCourseCommand request)
        {
            var baseUrl = GetUrl(Constants.LEGAL_COLLECTION_INTEGRATION_SETTING_CODE);
            var url = $"{baseUrl}/api/legalcasecourse/delete?id={request.Id}";

            try
            {
                HttpResponseMessage response = await _httpClient.GetAsync(url);

                response.EnsureSuccessStatusCode();

                string responseBody = await response.Content.ReadAsStringAsync();

                var detailsResponse = JsonConvert.DeserializeObject<int>(responseBody);
                return detailsResponse;
            }
            catch (Exception e)
            {
                throw new PawnshopApplicationException(
                    $"Не удалось удалить направление с Id: {request.Id}. {e.Message}");
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