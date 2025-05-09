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
using Pawnshop.Services.LegalCollection.HttpServices.Dtos.LegalCaseStage;
using Pawnshop.Services.LegalCollection.HttpServices.Interfaces;
using RestSharp.Extensions;

namespace Pawnshop.Services.LegalCollection.HttpServices
{
    public class LegalCollectionStagesHttpService : ILegalCollectionStagesHttpService
    {
        private readonly HttpClient _httpClient;
        private readonly OuterServiceSettingRepository _settingsRepository;

        public LegalCollectionStagesHttpService(HttpClient httpClient, OuterServiceSettingRepository settingsRepository)
        {
            _httpClient = httpClient;
            _settingsRepository = settingsRepository;
        }

        public async Task<LegalCaseStageDto> Create(CreateLegalCaseStageCommand request)
        {
            var baseUrl = GetUrl(Constants.LEGAL_COLLECTION_INTEGRATION_SETTING_CODE);
            var url = $"{baseUrl}/api/legalcasestage/create";
            try
            {
                var jsonContent =
                    new StringContent(JsonConvert.SerializeObject(request), Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync(url, jsonContent);
                response.EnsureSuccessStatusCode();
                var jsonString = await response.Content.ReadAsStringAsync();
                var responseModel = JsonConvert.DeserializeObject<LegalCaseStageDto>(jsonString);

                return responseModel;
            }
            catch (Exception e)
            {
                throw new PawnshopApplicationException(
                    $"Не удалось создать стадию Legal collection. {e.Message}");
            }
        }

        public async Task<LegalCaseStageDto> Details(DetailsLegalCaseStageQuery request)
        {
            var baseUrl = GetUrl(Constants.LEGAL_COLLECTION_INTEGRATION_SETTING_CODE);
            var url = $"{baseUrl}/api/legalcasestage/details?Id={request.Id}";
            try
            {
                HttpResponseMessage response = await _httpClient.GetAsync(url);

                response.EnsureSuccessStatusCode();

                string responseBody = await response.Content.ReadAsStringAsync();

                var detailsResponse = JsonConvert.DeserializeObject<LegalCaseStageDto>(responseBody);
                return detailsResponse;
            }
            catch (Exception e)
            {
                throw new PawnshopApplicationException(
                    $"Не удалось получить данные по стадии с Id: {request.Id}. {e.Message}");
            }
        }

        public async Task<LegalCaseStagesList> List()
        {
            var baseUrl = GetUrl(Constants.LEGAL_COLLECTION_INTEGRATION_SETTING_CODE);
            var url = $"{baseUrl}/api/legalcasestage/list";

            try
            {
                HttpResponseMessage response = await _httpClient.GetAsync(url);

                response.EnsureSuccessStatusCode();

                string responseBody = await response.Content.ReadAsStringAsync();
                var resultResponse = JsonConvert.DeserializeObject<List<LegalCaseStageDto>>(responseBody);

                return new LegalCaseStagesList
                {
                    Count = resultResponse.Count,
                    List = resultResponse
                };
            }
            catch (Exception e)
            {
                throw new PawnshopApplicationException(
                    $"Не удалось данные по стадиям Legal-Case. {e.Message}");
            }
        }

        public async Task<LegalCaseStageDto> Update(UpdateLegalCaseStageCommand request)
        {
            var baseUrl = GetUrl(Constants.LEGAL_COLLECTION_INTEGRATION_SETTING_CODE);
            var url = $"{baseUrl}/api/legalcasestage/update";
            
            try
            {
                var jsonContent = 
                    new StringContent(JsonConvert.SerializeObject(request), Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync(url, jsonContent);
                response.EnsureSuccessStatusCode();
                var jsonString = await response.Content.ReadAsStringAsync();
                var responseModel = JsonConvert.DeserializeObject<LegalCaseStageDto>(jsonString);

                return responseModel;
            }
            catch (Exception e)
            {
                throw new PawnshopApplicationException(
                    $"Не удалось обновить данные по стадии Legal-Case с Id: {request.Id}. {e.Message}");
            }
        }
        
        public async Task<int> Delete(DeleteLegalCaseStageCommand request)
        {
            var baseUrl = GetUrl(Constants.LEGAL_COLLECTION_INTEGRATION_SETTING_CODE);
            var url = $"{baseUrl}/api/legalcasestage/delete?id={request.Id}";

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
                    $"Не удалось удалить стадию Legal-Case с Id: {request.Id}. {e.Message}");
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