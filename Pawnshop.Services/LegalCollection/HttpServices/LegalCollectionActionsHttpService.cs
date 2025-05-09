using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Pawnshop.Core;
using Pawnshop.Core.Exceptions;
using Pawnshop.Data.Access;
using Pawnshop.Data.Models.LegalCollection.Dtos;
using Pawnshop.Services.LegalCollection.HttpServices.Dtos.LegalCaseAction;
using Pawnshop.Services.LegalCollection.HttpServices.Interfaces;
using RestSharp.Extensions;

namespace Pawnshop.Services.LegalCollection
{
    public class LegalCollectionActionsHttpService : ILegalCollectionActionsHttpService
    {
        private readonly HttpClient _httpClient;
        private readonly OuterServiceSettingRepository _settingsRepository;

        public LegalCollectionActionsHttpService(HttpClient httpClient, OuterServiceSettingRepository settingsRepository)
        {
            _httpClient = httpClient;
            _settingsRepository = settingsRepository;
        }

        public async Task<LegalCaseActionDto> Create(CreateLegalCaseActionsCommand request)
        {
            var baseUrl = GetUrl(Constants.LEGAL_COLLECTION_INTEGRATION_SETTING_CODE);
            var url = $"{baseUrl}/api/legacaseactions/create";
            try
            {
                var jsonContent =
                    new StringContent(JsonConvert.SerializeObject(request), Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync(url, jsonContent);
                response.EnsureSuccessStatusCode();
                var jsonString = await response.Content.ReadAsStringAsync();
                var responseModel = JsonConvert.DeserializeObject<LegalCaseActionDto>(jsonString);

                return responseModel;
            }
            catch (Exception e)
            {
                throw new PawnshopApplicationException($"Не удалось создать действие Legal collection. {e.Message}");
            }
        }

        public async Task<LegalCaseActionDto> Details(LegalCaseActionDetailsQuery request)
        {
            var baseUrl = GetUrl(Constants.LEGAL_COLLECTION_INTEGRATION_SETTING_CODE);
            var url = $"{baseUrl}/api/legacaseactions/details?Id={request.Id}";
            
            try
            {
                HttpResponseMessage response = await _httpClient.GetAsync(url);

                response.EnsureSuccessStatusCode();

                string responseBody = await response.Content.ReadAsStringAsync();

                var detailsResponse = JsonConvert.DeserializeObject<LegalCaseActionDto>(responseBody);
                return detailsResponse;
            }
            catch (Exception e)
            {
                throw new PawnshopApplicationException(
                    $"Не удалось получить данные по действию с Id: {request.Id}. {e.Message}");
            }
        }

        public async Task<LegalCaseActionsList> List()
        {
            var baseUrl = GetUrl(Constants.LEGAL_COLLECTION_INTEGRATION_SETTING_CODE);
            var url = $"{baseUrl}/api/legacaseactions/list";

            try
            {
                HttpResponseMessage response = await _httpClient.GetAsync(url);

                response.EnsureSuccessStatusCode();

                string responseBody = await response.Content.ReadAsStringAsync();
                var responseResult = JsonConvert.DeserializeObject<List<LegalCaseActionDto>>(responseBody);
                return new LegalCaseActionsList
                {
                    Count = responseResult.Count,
                    List = responseResult
                };
            }
            catch (Exception e)
            {
                throw new PawnshopApplicationException($"Не удалось получить данные по действиям. {e.Message}");
            }
        }

        public async Task<LegalCaseActionDto> Update(UpdateLegalCaseActionsCommand request)
        {
            var baseUrl = GetUrl(Constants.LEGAL_COLLECTION_INTEGRATION_SETTING_CODE);
            var url = $"{baseUrl}/api/legacaseactions/update";
            
            try
            {
                var jsonContent =
                    new StringContent(JsonConvert.SerializeObject(request), Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync(url, jsonContent);
                response.EnsureSuccessStatusCode();
                var jsonString = await response.Content.ReadAsStringAsync();
                var responseModel = JsonConvert.DeserializeObject<LegalCaseActionDto>(jsonString);

                return responseModel;
            }
            catch (Exception e)
            {
                throw new PawnshopApplicationException(
                    $"Не удалось обновить данные по действию с Id: {request.Id}. {e.Message}");
            }
        }
        
        public async Task<int> Delete(DeleteLegalCaseActionCommand request)
        {
            var baseUrl = GetUrl(Constants.LEGAL_COLLECTION_INTEGRATION_SETTING_CODE);
            var url = $"{baseUrl}/api/legacaseactions/delete?id={request.Id}";

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
                    $"Не удалось удалить действие с Id: {request.Id}. {e.Message}");
            }
        }
        
        private string GetUrl(string code)
        {
            var settings = _settingsRepository.Find(new { Code = code });
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