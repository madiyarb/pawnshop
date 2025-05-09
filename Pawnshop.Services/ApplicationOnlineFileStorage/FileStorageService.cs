using Microsoft.Extensions.Options;
using Pawnshop.Services.Exceptions;
using System.IO;
using System.Net.Http.Headers;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using System.Threading;
using System;
using MetadataExtractor.Util;
using Pawnshop.Core.Exceptions;
using Serilog;

namespace Pawnshop.Services.ApplicationOnlineFileStorage
{
    public sealed class FileStorageService : IFileStorageService
    {
        private readonly string _baseUrl;
        private readonly HttpClient _httpClient;
        private readonly JsonSerializerOptions _serializerOptions;
        private readonly string password;
        private readonly string username;
        private readonly ILogger _logger;

        public FileStorageService(HttpClient httpClient, IOptions<FileStorageOptions> options, ILogger logger)
        {
            _baseUrl = options.Value.BaseUrl ?? throw new ArgumentNullException(nameof(_baseUrl));
            username = options.Value.UserName ?? throw new ArgumentNullException(nameof(username));
            password = options.Value.Secret ?? throw new ArgumentNullException(nameof(password));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _httpClient = httpClient;
            _httpClient.BaseAddress = new Uri(_baseUrl);
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic",
                Convert.ToBase64String(
                    System.Text.Encoding.ASCII.GetBytes(
                        $"{username}:{password}")));
            _serializerOptions = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };
        }

        public async Task<int> CreateListId(CancellationToken cancellationToken)
        {
            try
            {
                var response = await _httpClient.PostAsync($"/tst-api-gateway/tst-open-api/api/v1/files/list",
                    null, cancellationToken);

                if (response.IsSuccessStatusCode)
                {
                    var data = JsonSerializer.Deserialize<CreateListIdResponse>(
                        await response.Content.ReadAsStringAsync(), _serializerOptions);
                    return data.id;
                }

                throw new Exceptions.UnexpectedResponseException($"Service from url : {response.RequestMessage.RequestUri} " +
                                                                 $"returned unexpected status code {response.StatusCode} ");
            }
            catch (HttpRequestException exception)
            {
                _logger.Error(exception, exception.Message);
                throw new ServiceUnavailableException($"{_baseUrl}", exception.Message);
            }
            catch (TaskCanceledException exception)
            {
                _logger.Error(exception, exception.Message);
                throw new ServiceUnavailableException($"{_baseUrl}", exception.Message);
            }
            catch (JsonException exception)
            {
                _logger.Error(exception, exception.Message);
                throw new Exceptions.UnexpectedResponseException(exception.Message);
            }
        }

        public async Task<FileWithContentType> Download(Guid fileId, CancellationToken cancellationToken)
        {
            try
            {
                var response = await _httpClient.GetAsync($"/tst-api-gateway/tst-open-api/api/v1/files/{fileId}", cancellationToken);
                if (response.IsSuccessStatusCode)
                {
                    return new FileWithContentType
                    {
                        ContentType = response.Content.Headers.ContentType?.ToString(),
                        Stream = await response.Content.ReadAsStreamAsync()
                    };
                }
                throw new Exceptions.UnexpectedResponseException($"Service from url : {response.RequestMessage.RequestUri} " +
                                                                  $"returned unexpected status code {response.StatusCode} ");
            }
            catch (HttpRequestException exception) 
            {
                _logger.Error(exception, exception.Message);
                throw new ServiceUnavailableException($"{_baseUrl}", exception.Message);
            }
            catch (TaskCanceledException exception)
            {
                _logger.Error(exception, exception.Message);
                throw new ServiceUnavailableException($"{_baseUrl}", exception.Message);
            }
            catch (JsonException exception)
            {
                _logger.Error(exception, exception.Message);
                throw new Exceptions.UnexpectedResponseException(exception.Message);
            }

        }

        public async Task<UploadFileResponse> Upload(string listId, Stream stream, string comment,
            string businessType, string filename, CancellationToken cancellationToken)
        {
            var streamContent = new StreamContent(stream);
            var header = new byte[64];

            var bytesReadCount = await stream.ReadAsync(header, 0, header.Length, cancellationToken);

            if (bytesReadCount < header.Length)
                throw new PawnshopApplicationException("File not valid length ");
            using Stream memoryStream = new MemoryStream(header, false);
            stream.Position = 0;
            var fileType = FileTypeDetector.DetectFileType(memoryStream);

            if (fileType == FileType.Unknown)
            {
                streamContent.Headers.ContentType = new MediaTypeHeaderValue("application/pdf");
            }
            else
            {
                streamContent.Headers.ContentType = new MediaTypeHeaderValue(MimeMapping.MimeUtility.GetMimeMapping(fileType.ToString()));
            }
            try
            {
                using var content = new MultipartFormDataContent
                {
                    {
                        streamContent, "UploadFile", filename
                    },
                    {
                        new StringContent(comment), "Comment"
                    },
                    {
                        new StringContent(businessType), "BusinessType"
                    }
                };

                var response = await _httpClient.PostAsync($"/tst-api-gateway/tst-open-api/api/v1/files/list/{listId}",
                    content, cancellationToken);
                if (response.IsSuccessStatusCode)
                {
                    var data = JsonSerializer.Deserialize<UploadFileResponse>(
                        await response.Content.ReadAsStringAsync(), _serializerOptions);
                    return data;
                }
                throw new Exceptions.UnexpectedResponseException($"Service from url : {response.RequestMessage.RequestUri} " +
                                                                 $"returned unexpected status code {response.StatusCode} ");
            }
            catch (HttpRequestException exception)
            {
                _logger.Error(exception, exception.Message);
                throw new ServiceUnavailableException($"{_baseUrl}", exception.Message);
            }
            catch (TaskCanceledException exception)
            {
                _logger.Error(exception, exception.Message);
                throw new ServiceUnavailableException($"{_baseUrl}", exception.Message);
            }
            catch (JsonException exception)
            {
                _logger.Error(exception, exception.Message);
                throw new Exceptions.UnexpectedResponseException(exception.Message);
            }
        }
    }
}
