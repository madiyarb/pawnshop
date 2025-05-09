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
using Pawnshop.Data.Models.LegalCollection;
using Pawnshop.Data.Models.LegalCollection.Action.HttpService;
using Pawnshop.Data.Models.LegalCollection.Create;
using Pawnshop.Data.Models.LegalCollection.Details.HttpService;
using Pawnshop.Data.Models.LegalCollection.Documents;
using Pawnshop.Data.Models.LegalCollection.Dtos;
using Pawnshop.Data.Models.LegalCollection.GetFiltered.HttpService;
using Pawnshop.Data.Models.LegalCollection.HttpService;
using Pawnshop.Services.LegalCollection.HttpServices.Interfaces;
using RestSharp.Extensions;

namespace Pawnshop.Services.LegalCollection
{
    public class LegalCaseHttpService : ILegalCaseHttpService
    {
        private readonly OuterServiceSettingRepository _settingsRepository;
        private readonly HttpClient _httpClient;

        public LegalCaseHttpService(HttpClient httpClient, OuterServiceSettingRepository settingsRepository)
        {
            _httpClient = httpClient;
            _settingsRepository = settingsRepository;
        }

        public async Task<int> CreateLegalCaseHttpRequest(CreateLegalCaseCommand request)
        {
            var baseUrl = GetUrl(Constants.LEGAL_COLLECTION_INTEGRATION_SETTING_CODE);
            var url = $"{baseUrl}/api/legalcase/create";
            try
            {
                var jsonContent =
                    new StringContent(JsonConvert.SerializeObject(request), Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync(url, jsonContent);

                response.EnsureSuccessStatusCode();

                var jsonString = await response.Content.ReadAsStringAsync();
                var responseId = JsonConvert.DeserializeObject<int>(jsonString);

                return responseId;
            }
            catch (Exception e)
            {
                throw new PawnshopApplicationException($"При Http запросе произошла ошибка: {e.Message}");
            }
        }

        public async Task<LegalCaseActionOptionsResponse> GetLegalCaseActionOptions(int legalCaseId)
        {
            var baseUrl = GetUrl(Constants.LEGAL_COLLECTION_INTEGRATION_SETTING_CODE);
            var url = $"{baseUrl}/api/legalcase/action-options?legalCaseId={legalCaseId}";

            try
            {
                HttpResponseMessage response = await _httpClient.GetAsync(url);

                response.EnsureSuccessStatusCode();

                string responseBody = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<LegalCaseActionOptionsResponse>(responseBody);
            }
            catch (Exception e)
            {
                throw new PawnshopApplicationException(
                    $"Не удалось получить данные по делу с Id: {legalCaseId}. {e.Message}");
            }
        }

        public async Task<List<LegalCaseDetailsResponse>> GetLegalCaseDetails(int legalCaseId)
        {
            var baseUrl = GetUrl(Constants.LEGAL_COLLECTION_INTEGRATION_SETTING_CODE);
            var url = $"{baseUrl}/api/legalcase/details?legalCaseId={legalCaseId}";

            try
            {
                HttpResponseMessage response = await _httpClient.GetAsync(url);

                response.EnsureSuccessStatusCode();

                string responseBody = await response.Content.ReadAsStringAsync();

                var detailsResponse = JsonConvert.DeserializeObject<List<LegalCaseDetailsResponse>>(responseBody);
                return detailsResponse;
            }
            catch (Exception e)
            {
                throw new PawnshopApplicationException(
                    $"Не удалось получить данные по делу с Id: {legalCaseId}. {e.Message}");
            }
        }

        public async Task<PagedResponse<FilteredLegalCasesResponse>> GetFilteredLegalCase(FilteredHttpRequest request)
        {
            var baseUrl = GetUrl(Constants.LEGAL_COLLECTION_INTEGRATION_SETTING_CODE);
            var url = $"{baseUrl}/api/legalcase/filtered";
            var jsonContent =
                new StringContent(JsonConvert.SerializeObject(request), Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync(url, jsonContent);

            var httpStatusCode = response.EnsureSuccessStatusCode().StatusCode;
            if (httpStatusCode != HttpStatusCode.OK)
            {
                throw new PawnshopApplicationException(
                    $"Legal Collection service http response status: {httpStatusCode}");
            }

            var jsonString = await response.Content.ReadAsStringAsync();
            var pagedResponse = JsonConvert.DeserializeObject<PagedResponse<FilteredLegalCasesResponse>>(jsonString);

            return pagedResponse;
        }

        public async Task<List<LegalCaseDetailsResponse>> UpdateLegalCase(UpdateLegalCaseCommand request)
        {
            var baseUrl = GetUrl(Constants.LEGAL_COLLECTION_INTEGRATION_SETTING_CODE);
            var url = $"{baseUrl}/api/legalcase/update";
            var jsonContent = new StringContent(JsonConvert.SerializeObject(request), Encoding.UTF8, "application/json");

            var response = await _httpClient.PutAsync(url, jsonContent);

            var httpStatusCode = response.EnsureSuccessStatusCode().StatusCode;
            if (httpStatusCode != HttpStatusCode.OK)
            {
                throw new PawnshopApplicationException(
                    $"Legal Collection service http response status: {httpStatusCode}");
            }

            var jsonString = await response.Content.ReadAsStringAsync();
            var pagedResponse = JsonConvert.DeserializeObject<List<LegalCaseDetailsResponse>>(jsonString);

            return pagedResponse;
        }

        public async Task<int> CreateLegalCaseDocument(CreateLegalCaseDocumentCommand command)
        {
            var baseUrl = GetUrl(Constants.LEGAL_COLLECTION_INTEGRATION_SETTING_CODE);
            var url = $"{baseUrl}/api/legalcasedocument/createlegalcasedocument";
            try
            {
                var jsonContent =
                    new StringContent(JsonConvert.SerializeObject(command), System.Text.Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync(url, jsonContent);

                response.EnsureSuccessStatusCode();

                var jsonString = await response.Content.ReadAsStringAsync();
                var responseId = JsonConvert.DeserializeObject<int>(jsonString);

                return responseId;
            }
            catch (Exception e)
            {
                throw new PawnshopApplicationException($"При Http запросе произошла ошибка: {e.Message}");
            }
        }

        public async Task<int> DeleteLegalCaseDocument(int documentId)
        {
            var baseUrl = GetUrl(Constants.LEGAL_COLLECTION_INTEGRATION_SETTING_CODE);
            var url = $"{baseUrl}/api/legalcasedocument/deletelegalcasedocument?Id={documentId}";

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
                    $"Не удалось удалить документ с Id: {documentId}. {e.Message}");
            }
        }

        public async Task<int> CloseLegalCaseHttpRequest(int legalCaseId, int authorId)
        {
            var baseUrl = GetUrl(Constants.LEGAL_COLLECTION_INTEGRATION_SETTING_CODE);
            var url = $"{baseUrl}/api/legalcase/close";
            string fullUrl = $"{url}?legalCaseId={legalCaseId}&authorId={authorId}";
            
            HttpResponseMessage response = await _httpClient.PutAsync(fullUrl, null);

            response.EnsureSuccessStatusCode();

            var jsonString = await response.Content.ReadAsStringAsync();
            var responseId = JsonConvert.DeserializeObject<int>(jsonString);

            return responseId;
        }
        
        public async Task<int> CloseLegalCaseHttpRequest(int legalCaseId)
        {
            var baseUrl = GetUrl(Constants.LEGAL_COLLECTION_INTEGRATION_SETTING_CODE);
            var url = $"{baseUrl}/api/legalcase/close";
            string fullUrl = $"{url}?legalCaseId={legalCaseId}";
            
            HttpResponseMessage response = await _httpClient.PutAsync(fullUrl, null);

            response.EnsureSuccessStatusCode();

            var jsonString = await response.Content.ReadAsStringAsync();
            var responseId = JsonConvert.DeserializeObject<int>(jsonString);

            return responseId;
        }

        public async Task<int> CancelLegalCaseHttpRequest(int legalCaseId)
        {
            var baseUrl = GetUrl(Constants.LEGAL_COLLECTION_INTEGRATION_SETTING_CODE);
            var url = $"{baseUrl}/api/legalcase/cancel";
            string fullUrl = $"{url}?legalCaseId={legalCaseId}";
            
            HttpResponseMessage response = await _httpClient.PutAsync(fullUrl, null);

            response.EnsureSuccessStatusCode();

            var jsonString = await response.Content.ReadAsStringAsync();
            var responseId = JsonConvert.DeserializeObject<int>(jsonString);

            return responseId;
        }
        
        public async Task<List<LegalCaseDetailsResponse>> RollbackLegalCaseHttpRequest(int legalCaseId)
        {
            var baseUrl = GetUrl(Constants.LEGAL_COLLECTION_INTEGRATION_SETTING_CODE);
            var url = $"{baseUrl}/api/legalcase/rollback";
            string fullUrl = $"{url}?legalCaseId={legalCaseId}";
            
            HttpResponseMessage response = await _httpClient.PutAsync(fullUrl, null);

            response.EnsureSuccessStatusCode();

            var jsonString = await response.Content.ReadAsStringAsync();
            var responseId = JsonConvert.DeserializeObject<List<LegalCaseDetailsResponse>>(jsonString);

            return responseId;
        }

        public async Task<List<ChangeCourseActionDto>> GetChangeCourseVarious(int legalCaseId)
        {
            var baseUrl = GetUrl(Constants.LEGAL_COLLECTION_INTEGRATION_SETTING_CODE);
            var url = $"{baseUrl}/api/legalcase/change-course-various?legalCaseId={legalCaseId}";

            try
            {
                HttpResponseMessage response = await _httpClient.GetAsync(url);

                response.EnsureSuccessStatusCode();

                string responseBody = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<List<ChangeCourseActionDto>>(responseBody);
            }
            catch (Exception e)
            {
                throw new PawnshopApplicationException(
                    $"Не удалось получить данные для смены статуса по делу с Id: {legalCaseId}. {e.Message}");
            }
        }

        public async Task<List<LegalCaseDetailsResponse>> ChangeCourse(ChangeLegalCaseCourseRequest request)
        {
            var baseUrl = GetUrl(Constants.LEGAL_COLLECTION_INTEGRATION_SETTING_CODE);
            var url = $"{baseUrl}/api/legalcase/change-course";
            try
            {
                var jsonContent =
                    new StringContent(JsonConvert.SerializeObject(request), Encoding.UTF8,
                        "application/json");

                var response = await _httpClient.PostAsync(url, jsonContent);

                response.EnsureSuccessStatusCode();

                var jsonString = await response.Content.ReadAsStringAsync();
                var responseId = JsonConvert.DeserializeObject<List<LegalCaseDetailsResponse>>(jsonString);

                return responseId;
            }
            catch (Exception e)
            {
                throw new PawnshopApplicationException(
                    $"Не удалось сменить направление у дела с Id: {request.LegalCaseId}. {e.Message}");
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
                throw new PawnshopApplicationException("Не удалось получить Url Legal-collection");
            }
            
            return url;
        }
    }
}