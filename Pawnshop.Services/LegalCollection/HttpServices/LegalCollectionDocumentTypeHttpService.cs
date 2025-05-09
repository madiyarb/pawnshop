using System;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Pawnshop.Core;
using Pawnshop.Core.Exceptions;
using Pawnshop.Data.Access;
using Pawnshop.Data.Models.LegalCollection.DocumentType.HttpServie;
using Pawnshop.Data.Models.LegalCollection.Dtos;
using Pawnshop.Services.LegalCollection.HttpServices.Interfaces;
using RestSharp.Extensions;
using Pawnshop.Services.Models.List;

namespace Pawnshop.Services.LegalCollection.HttpServices
{
    public class LegalCollectionDocumentTypeHttpService : ILegalCollectionDocumentTypeHttpService
    {
        private readonly HttpClient _httpClient;
        private readonly OuterServiceSettingRepository _settingsRepository;

        public LegalCollectionDocumentTypeHttpService(HttpClient httpClient, OuterServiceSettingRepository settingsRepository)
        {
            _httpClient = httpClient;
            _settingsRepository = settingsRepository;
        }

        public async Task<LegalCollectionDocumentTypeDto> Create(CreateDocumentTypeHttpRequest request)
        {
            var baseUrl = GetUrl(Constants.LEGAL_COLLECTION_INTEGRATION_SETTING_CODE);
            var url = $"{baseUrl}/api/legal-collection/document-type/create";
            
            var jsonContent =
                new StringContent(JsonConvert.SerializeObject(request), System.Text.Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync(url, jsonContent);

            response.EnsureSuccessStatusCode();

            var jsonString = await response.Content.ReadAsStringAsync();
            var responseModel = JsonConvert.DeserializeObject<LegalCollectionDocumentTypeDto>(jsonString);

            return responseModel;
        }

        public async Task<LegalCollectionDocumentTypeDto> Details(DetailsDocumentTypeHttRequest request)
        {
            var baseUrl = GetUrl(Constants.LEGAL_COLLECTION_INTEGRATION_SETTING_CODE);
            var url = $"{baseUrl}/api/legal-collection/document-type/details?documentTypeId={request.DocumentTypeId}";
            
            try
            {
                HttpResponseMessage response = await _httpClient.GetAsync(url);

                response.EnsureSuccessStatusCode();

                string responseBody = await response.Content.ReadAsStringAsync();

                var detailsResponse = JsonConvert.DeserializeObject<LegalCollectionDocumentTypeDto>(responseBody);
                return detailsResponse;
            }
            catch (Exception e)
            {
                throw new PawnshopApplicationException(
                    $"Не удалось получить данные по типу документа с Id: {request.DocumentTypeId}. {e.Message}");
            }
        }

        public async Task<ListModel<LegalCollectionDocumentTypeDto>> List(DocumentTypesHttpRequest request)
        {
            var baseUrl = GetUrl(Constants.LEGAL_COLLECTION_INTEGRATION_SETTING_CODE);
            var url = $"{baseUrl}/api/legal-collection/document-type/list";
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
            var pagedResponse = JsonConvert.DeserializeObject<ListModel<LegalCollectionDocumentTypeDto>>(jsonString);

            return pagedResponse;
        }

        public async Task<LegalCollectionDocumentTypeDto> Update(UpdateDocumentTypeHttpRequest request)
        {
            var baseUrl = GetUrl(Constants.LEGAL_COLLECTION_INTEGRATION_SETTING_CODE);
            var url = $"{baseUrl}/api/legal-collection/document-type/update";
            var jsonContent =
                new StringContent(JsonConvert.SerializeObject(request), Encoding.UTF8, "application/json");

            var response = await _httpClient.PutAsync(url, jsonContent);

            var httpStatusCode = response.EnsureSuccessStatusCode().StatusCode;
            if (httpStatusCode != HttpStatusCode.OK)
            {
                throw new PawnshopApplicationException(
                    $"Legal Collection service http response status: {httpStatusCode}");
            }

            var jsonString = await response.Content.ReadAsStringAsync();
            var responseModel = JsonConvert.DeserializeObject<LegalCollectionDocumentTypeDto>(jsonString);

            return responseModel;
        }

        public async Task<int> Delete(DeleteDocumentTypeHttpRequest request)
        {
            var baseUrl = GetUrl(Constants.LEGAL_COLLECTION_INTEGRATION_SETTING_CODE);
            var url = $"{baseUrl}/api/legal-collection/document-type/delete?id={request.DocumentTypeId}";
            
            var response = await _httpClient.DeleteAsync(url);
            
            var httpStatusCode = response.EnsureSuccessStatusCode().StatusCode;
            if (httpStatusCode != HttpStatusCode.OK)
            {
                throw new PawnshopApplicationException(
                    $"Legal Collection service http response status: {httpStatusCode}");
            }

            var jsonString = await response.Content.ReadAsStringAsync();
            var responseId = JsonConvert.DeserializeObject<int>(jsonString);

            return responseId;
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