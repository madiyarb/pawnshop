using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Pawnshop.Data.Access;
using Pawnshop.Services.ApplicationOnlineFileStorage;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using System.Threading;
using System;
using System.Linq;
using Pawnshop.Core.Extensions;
using Pawnshop.Services.Exceptions;
using Serilog;

namespace Pawnshop.Services.Estimation.Images
{
    public sealed class ApplicationOnlineEstimationImageService : IApplicationOnlineEstimationImageService
    {
        private readonly ApplicationOnlineFileRepository _applicationOnlineFileRepository;
        private readonly string _baseUrl;
        private readonly IFileStorageService _fileStorageService;
        private readonly HttpClient _httpClient;
        private readonly ILogger _logger;

        public ApplicationOnlineEstimationImageService(HttpClient httpClient,
            ApplicationOnlineFileRepository applicationOnlineFileRepository,
            FileStorageService fileStorageService,
            IOptions<OldEstimationServiceOptions> options, 
            ILogger logger)
        {
            _fileStorageService = fileStorageService;
            _applicationOnlineFileRepository = applicationOnlineFileRepository;
            _baseUrl = options.Value.BaseUrl ?? throw new ArgumentNullException(nameof(_baseUrl));
            _httpClient = httpClient;
            _httpClient.BaseAddress = new Uri(_baseUrl);
            _logger = logger;
        }


        public async Task<string> UploadImage(Stream stream, string fileName, CancellationToken cancellationToken)
        {
            try 
            { 
                #region Загрузка файла для приложения оценщиков

                using var request = new HttpRequestMessage(HttpMethod.Post, "/api/media/upload");
                using var content = new MultipartFormDataContent
                {
                    { new StreamContent(stream), "file", fileName }
                };

                request.Content = content;

                var response = await _httpClient.SendAsync(request, cancellationToken);

                var responseBody = await response.Content.ReadAsStringAsync();

                return JsonConvert.DeserializeObject<EstimationUploadImageResponse>(responseBody).Path;

                #endregion

            }
            catch (HttpRequestException exception)
            {
                _logger.Error(exception, exception.Message);
                throw new ServiceUnavailableException(_baseUrl, exception.Message);
            }
            catch (TaskCanceledException exception)
            {
                _logger.Error(exception, exception.Message);
                throw new ServiceUnavailableException(_baseUrl, exception.Message);
            }
            catch (JsonException exception)
            {
                _logger.Error(exception, exception.Message);
                throw new ServiceUnavailableException(_baseUrl, exception.Message);
            }
        }

        public async Task UploadImagesToEstimationService(Guid applicationOnlineId, CancellationToken cancellationToken)
        {
            var files = (await _applicationOnlineFileRepository.GetApplicationOnlineFilesForEstimation(applicationOnlineId)).ToList();

            for (int i = 0; i < files.Count; i++)
            {
                if (string.IsNullOrEmpty(files[i].EstimationStorageFileName))
                {
                    var file = await _fileStorageService.Download(files[i].StorageFileId, cancellationToken);
                    files[i].EstimationStorageFileName = await UploadImage(file.Stream, FixFileName(files[i].Id.ToString(), files[i].ContentType), cancellationToken);
                    await _applicationOnlineFileRepository.Update(files[i]);
                }
            }
        }


        private string FixFileName(string fileName, string contentType)
        {
            if (string.IsNullOrEmpty(contentType))
                return fileName;

            if (string.Equals("application/pdf", contentType.ToLowerInvariant()))
            {
                return $"{fileName}{".pdf"}";
            }
            return fileName;
        }
    }
}
