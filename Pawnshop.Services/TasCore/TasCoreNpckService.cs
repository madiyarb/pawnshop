using Microsoft.Extensions.Options;
using Pawnshop.Services.TasCore.DTO.NpckEsign;
using Pawnshop.Services.TasCore.Models.NpckEsign;
using Pawnshop.Services.TasCore.Options;
using Serilog;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System;

namespace Pawnshop.Services.TasCore
{
    public class TasCoreNpckService : ITasCoreNpckService
    {
        private readonly ILogger _logger;
        private readonly JsonSerializerOptions _serializerOptions;
        private HttpClient _httpClient;

        public TasCoreNpckService(
            ILogger logger,
            IOptions<TasCoreOptions> options,
            HttpClient httpClient)
        {
            _logger = logger;
            _httpClient = httpClient;
            _httpClient.BaseAddress = new Uri(options.Value.BaseUrl);
            _httpClient.Timeout = TimeSpan.FromSeconds((double)options.Value.TimeoutSeconds);

            _serializerOptions = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };
        }


        public async Task<TasCoreNpckEsignDocumentUploadResponse> DocumentUpload(Guid fileStorageId, string link, string fileName, Guid? signedFileStorageId, CancellationToken cancellationToken = default)
        {
            try
            {
                var request = new DocumentUploadRequest(link, signedFileStorageId, fileName);

                var content = new StringContent(JsonSerializer.Serialize(request, _serializerOptions), Encoding.UTF8, "application/json");
                var result = await _httpClient.PostAsync($"/api/npck/esign/documents/{fileStorageId}/upload",
                    content, cancellationToken: cancellationToken);

                var responseStr = await result.Content.ReadAsStringAsync();
                var response = JsonSerializer.Deserialize<DocumentUploadResponse>(responseStr, _serializerOptions);

                if (!result.IsSuccessStatusCode)
                    return new TasCoreNpckEsignDocumentUploadResponse { Success = false, Message = response.Message };

                return new TasCoreNpckEsignDocumentUploadResponse { Success = true, NpckFileId = response.FileId };
            }
            catch (Exception ex)
            {
                _logger.Error($"Ошибка загрузки файла {fileStorageId}: {ex.Message}");
                return new TasCoreNpckEsignDocumentUploadResponse { Success = false, Message = "Ошибка вызова сервиса TasCoreNpckService." };
            }
        }

        public async Task<TasCoreNpckEsignGenerateUrlResponse> GenerateUrl(string iin, string phone, string redirectUri, string language, List<Guid> documentIds, CancellationToken cancellationToken = default)
        {
            try
            {
                var request = new GenerateUrlRequest
                {
                    DocumentIds = documentIds.Select(x => x.ToString()).ToList(),
                    Iin = iin,
                    Language = language,
                    Phone = $"+{phone}",
                    RedirectUri = redirectUri,
                };

                var content = new StringContent(JsonSerializer.Serialize(request, _serializerOptions), Encoding.UTF8, "application/json");
                var result = await _httpClient.PostAsync($"/api/npck/esign/generate/url",
                    content, cancellationToken: cancellationToken);

                var responseStr = await result.Content.ReadAsStringAsync();
                var response = JsonSerializer.Deserialize<GenerateUrlResponse>(responseStr, _serializerOptions);

                if (!result.IsSuccessStatusCode)
                    return new TasCoreNpckEsignGenerateUrlResponse { Success = false, Message = response.Message };

                return new TasCoreNpckEsignGenerateUrlResponse { Success = true, SignUrl = response.Url };
            }
            catch (Exception ex)
            {
                _logger.Error($"Ошибка генерации ссылки для подписания клиенту {iin}: {ex.Message}");
                return new TasCoreNpckEsignGenerateUrlResponse { Success = false, Message = "Ошибка вызова сервиса TasCoreNpckService." };
            }
        }

        public async Task<TasCoreNpckEsignTokenResponse> GetToken(Guid applicationId, string code, string redirectUri, CancellationToken cancellationToken = default)
        {
            try
            {
                var request = new GetTokenRequest
                {
                    Code = code,
                    RedirectUri = redirectUri,
                };

                var content = new StringContent(JsonSerializer.Serialize(request, _serializerOptions), Encoding.UTF8, "application/json");
                var result = await _httpClient.PostAsync($"/api/npck/esign/token",
                    content, cancellationToken: cancellationToken);

                var responseStr = await result.Content.ReadAsStringAsync();
                var response = JsonSerializer.Deserialize<GetTokenResponse>(responseStr, _serializerOptions);

                if (!result.IsSuccessStatusCode)
                    return new TasCoreNpckEsignTokenResponse { Success = false, Message = response.Message };

                return new TasCoreNpckEsignTokenResponse { Success = true, Token = response.Token, ExpiresIn = response.ExpiresIn };
            }
            catch (Exception ex)
            {
                _logger.Error($"Ошибка получения токена по заявке {applicationId}: {ex.Message}");
                return new TasCoreNpckEsignTokenResponse { Success = false, Message = "Ошибка вызова сервиса TasCoreNpckService." };
            }
        }

        [Obsolete]
        public async Task<TasCoreNpckEsignSaveSignedFileResponse> SaveSingedFile(int listId, string code, string redirectUri, CancellationToken cancellationToken = default)
        {
            try
            {
                var request = new SaveSignedFileRequest
                {
                    Code = code,
                    RedirectUri = redirectUri,
                    ListId = listId
                };

                var content = new StringContent(JsonSerializer.Serialize(request, _serializerOptions), Encoding.UTF8, "application/json");
                var result = await _httpClient.PostAsync($"/api/npck/esign/documents/save",
                    content, cancellationToken: cancellationToken);

                var responseStr = await result.Content.ReadAsStringAsync();
                var response = JsonSerializer.Deserialize<SaveSignedFileResponse>(responseStr, _serializerOptions);

                if (!result.IsSuccessStatusCode)
                    return new TasCoreNpckEsignSaveSignedFileResponse { Success = false, Message = response.Message };

                return new TasCoreNpckEsignSaveSignedFileResponse { Success = true, SignedFiles = response.SignedFiles?.Select(x => new SignedFile(x.FileStorageId, x.EsignFileId, x.Name)).ToList() };
            }
            catch (Exception ex)
            {
                _logger.Error($"Ошибка получения файла: {ex.Message}");
                return new TasCoreNpckEsignSaveSignedFileResponse { Success = false, Message = "Ошибка вызова сервиса TasCoreNpckService." };
            }
        }
    }
}
