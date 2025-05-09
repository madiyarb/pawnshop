using Microsoft.Extensions.Options;
using Pawnshop.AccountingCore.Models;
using Pawnshop.Core;
using Pawnshop.Data.Access;
using Pawnshop.Data.Models.ApplicationOnlineFiles;
using Pawnshop.Data.Models.ApplicationsOnlineCar;
using Pawnshop.Data.Models.ApplicationsOnlineEstimation;
using Pawnshop.Data.Models.Clients;
using Pawnshop.Services.ApplicationOnlineFileStorage;
using Pawnshop.Services.Estimation.Exceptions;
using Pawnshop.Services.Estimation.v2.Request;
using Pawnshop.Services.Estimation.v2.Response;
using Serilog;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using System.Threading;
using System;

namespace Pawnshop.Services.Estimation.v2
{
    public class EstimationService
    {
        private readonly string _apiKey;
        private readonly string _baseUrl;
        private static JsonSerializerOptions _serializerOptions;
        private readonly ApplicationOnlineCarRepository _applicationOnlineCarRepository;
        private readonly ApplicationOnlineFileRepository _applicationOnlineFileRepository;
        private readonly ApplicationOnlineRefinancesRepository _applicationOnlineRefinancesRepository;
        private readonly ApplicationOnlineRepository _applicationOnlineRepository;
        private readonly ApplicationsOnlineEstimationRepository _applicationsOnlineEstimationRepository;
        private readonly ClientRepository _clientRepository;
        private readonly ContractPositionRepository _contractPositionRepository;
        private readonly ContractRepository _contractRepository;
        private readonly IFileStorageService _fileStorageService;
        private readonly HttpClient _httpClient;
        private readonly ILogger _logger;

        public EstimationService(
            ApplicationOnlineCarRepository applicationOnlineCarRepository,
            ApplicationOnlineFileRepository applicationOnlineFileRepository,
            ApplicationOnlineRepository applicationOnlineRepository,
            ApplicationsOnlineEstimationRepository applicationsOnlineEstimationRepository,
            ApplicationOnlineRefinancesRepository applicationOnlineRefinancesRepository,
            ClientRepository clientRepository,
            ContractPositionRepository contractPositionRepository,
            ContractRepository contractRepository,
            IFileStorageService fileStorageService,
            HttpClient httpClient,
            ILogger logger,
            IOptions<EstimationServiceOptions> options)
        {
            _applicationOnlineCarRepository = applicationOnlineCarRepository;
            _applicationOnlineFileRepository = applicationOnlineFileRepository;
            _applicationOnlineRepository = applicationOnlineRepository;
            _applicationsOnlineEstimationRepository = applicationsOnlineEstimationRepository;
            _applicationOnlineRefinancesRepository = applicationOnlineRefinancesRepository;
            _clientRepository = clientRepository;
            _contractPositionRepository = contractPositionRepository;
            _contractRepository = contractRepository;
            _fileStorageService = fileStorageService;
            _logger = logger;

            _serializerOptions = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                IgnoreNullValues = true,
            };

            _baseUrl = options?.Value?.BaseUrl ?? throw new ArgumentNullException(nameof(_baseUrl));
            _apiKey = options?.Value?.ApiKey ?? throw new ArgumentNullException(nameof(_apiKey));
            _httpClient = httpClient;
            _httpClient.BaseAddress = new Uri(_baseUrl);
        }


        public async Task<int> CreateApplication(Guid applicationId, int estimationClientId, int estimationPledgeId, CancellationToken cancellationToken)
        {
            try
            {
                var application = await _applicationOnlineRepository.GetOnlyApplicationOnline(applicationId);
                var refinanceList = await _applicationOnlineRefinancesRepository.GetApplicationOnlineRefinancesByApplicationId(applicationId);
                var creditLineId = application.CreditLineId;
                var creditType = (int)CreditType.NewCreditLine;
                var autoPrice = (int?)default;

                if (refinanceList != null && refinanceList.Any())
                {
                    creditType = (int)CreditType.Refinance;
                }
                else if (creditLineId.HasValue)
                {
                    var creditLine = _contractRepository.GetOnlyContract(creditLineId.Value);

                    if (creditLine.Status == ContractStatus.Signed)
                    {
                        var contractPosition = await _contractPositionRepository.GetContractPositionByContractId(creditLineId.Value);
                        autoPrice = contractPosition.FirstOrDefault().EstimatedCost;
                        creditType = (int)CreditType.NewTranche;
                    }
                }

                var requestModel = new ApplyCreationRequest(
                    Convert.ToInt32(application.ApplicationAmount),
                    estimationPledgeId,
                    estimationClientId,
                    creditType,
                    applicationId.ToString(),
                    creditLineId,
                    autoPrice,
                    refinanceList?.Select(x => x.RefinancedContractId));

                var request = new HttpRequestMessage(HttpMethod.Post, $"{_httpClient.BaseAddress}/api/v1/apply");
                request.Headers.Add("X-API-Key", _apiKey);
                request.Content = new StringContent(JsonSerializer.Serialize(requestModel, _serializerOptions), null, "application/json");

                var response = await _httpClient.SendAsync(request, cancellationToken);
                var responseBody = await response.Content.ReadAsStringAsync();
                var applyCreationResponse = JsonSerializer.Deserialize<ApplyResponse>(responseBody, _serializerOptions);

                if (!response.IsSuccessStatusCode)
                    throw new Exception(applyCreationResponse.Message);

                return applyCreationResponse.Id;
            }
            catch (Exception ex)
            {
                _logger.Error($"Error create apply to estimation service for application {applicationId}.\r\n{ex.Message}");
                throw ex;
            }
        }

        public async Task<int> CreateClient(Guid applicationId, IEnumerable<int> filesIds, CancellationToken cancellationToken)
        {
            try
            {
                var client = await _clientRepository.GetByApplicationOnlineId(applicationId);
                ValidateClient(client);

                var documents = _clientRepository.GetClientDocumentsByClientId(client.Id);
                var document = documents
                    .Where(document => document.DocumentType.Code == "PASSPORTKZ" ||
                                       document.DocumentType.Code == "IDENTITYCARD" || document.DocumentType.Code == "RESIDENCE")
                    .OrderByDescending(document => document.DateExpire).FirstOrDefault();
                ValidateClientDocument(document);

                var requestModel = new ClientCreationRequest(
                    client.Name,
                    client.Surname,
                    client.Patronymic,
                    client.BirthDay.Value.ToString("yyyy-MM-dd"),
                    document.DocumentType.Code,
                    client.IdentityNumber,
                    document.Number,
                    document.Date.Value.ToString("yyyy-MM-dd"),
                    document.DateExpire.Value.ToString("yyyy-MM-dd"),
                    document.BirthPlace,
                    document.Provider.Code,
                    filesIds);

                var request = new HttpRequestMessage(HttpMethod.Post, $"{_httpClient.BaseAddress}/api/v1/client");
                request.Headers.Add("X-API-Key", _apiKey);
                request.Content = new StringContent(JsonSerializer.Serialize(requestModel, _serializerOptions), null, "application/json");

                var response = await _httpClient.SendAsync(request, cancellationToken);
                var responseBody = await response.Content.ReadAsStringAsync();
                var clientCreationResponse = JsonSerializer.Deserialize<ClientResponse>(responseBody, _serializerOptions);

                if (!response.IsSuccessStatusCode)
                    throw new Exception(clientCreationResponse.Message);

                return clientCreationResponse.Id;
            }
            catch (Exception ex)
            {
                _logger.Error($"Error create client to estimation service for application {applicationId}.\r\n{ex.Message}");
                throw ex;
            }
        }

        public async Task<int> CreatePledge(Guid applicationId, IEnumerable<int> filesIds, CancellationToken cancellationToken)
        {
            try
            {
                var car = await _applicationOnlineCarRepository.GetByApplicationId(applicationId);
                ValidateCarModel(car);

                var client = await _clientRepository.GetByApplicationOnlineId(applicationId);

                var requestModel = new PledgeCreationRequest(
                    car.Mark,
                    car.TechPassportNumber,
                    car.BodyNumber,
                    car.TransportNumber,
                    car.TechPassportDate.Value.ToString("yyyy-MM-dd"),
                    car.Model,
                    car.VehicleModelId.Value,
                    car.Color,
                    client.FullName,
                    car.ReleaseYear.Value,
                    filesIds);

                var request = new HttpRequestMessage(HttpMethod.Post, $"{_httpClient.BaseAddress}/api/v1/pledge");
                request.Headers.Add("X-API-Key", _apiKey);
                request.Content = new StringContent(JsonSerializer.Serialize(requestModel, _serializerOptions), null, "application/json");

                var response = await _httpClient.SendAsync(request, cancellationToken);
                var responseBody = await response.Content.ReadAsStringAsync();
                var pledgeCreationResponse = JsonSerializer.Deserialize<PledgeResponse>(responseBody, _serializerOptions);

                if (!response.IsSuccessStatusCode)
                    throw new Exception(pledgeCreationResponse.Message);

                return pledgeCreationResponse.Id;
            }
            catch (Exception ex)
            {
                _logger.Error($"Error create car to estimation service for application {applicationId}.\r\n{ex.Message}");
                throw ex;
            }
        }

        public async Task ResendApply(Guid applicationId, int estimationApplyId, CancellationToken cancellationToken)
        {
            try
            {
                var requestModel = new ApplyResendRequest(estimationApplyId);
                var request = new HttpRequestMessage(HttpMethod.Post, $"{_httpClient.BaseAddress}/api/v1/sendToProgress");
                request.Headers.Add("X-API-Key", _apiKey);
                request.Content = new StringContent(JsonSerializer.Serialize(requestModel, _serializerOptions), null, "application/json");

                var response = await _httpClient.SendAsync(request, cancellationToken);
                var responseBody = await response.Content.ReadAsStringAsync();
                var applyCreationResponse = JsonSerializer.Deserialize<ApplyResponse>(responseBody, _serializerOptions);

                if (!response.IsSuccessStatusCode)
                    throw new Exception(applyCreationResponse.Message);
            }
            catch (Exception ex)
            {
                _logger.Error($"Error resend apply to estimation service for application {applicationId}.\r\n{ex.Message}");
                throw ex;
            }
        }

        public async Task SendAdditionalFiles(Guid applicationId, int estimationApplyId, IEnumerable<ApplicationOnlineFile> filesDetails, CancellationToken cancellationToken)
        {
            try
            {
                var applicationFiles = filesDetails != null && filesDetails.Any() ? filesDetails : await _applicationOnlineFileRepository.GetFilesForEstimation(applicationId);
                var additionalFiles = applicationFiles.Where(x => !x.EstimationServiceFileId.HasValue);

                foreach (var fileInfo in additionalFiles)
                {
                    var file = await _fileStorageService.Download(fileInfo.StorageFileId, cancellationToken);
                    var type = fileInfo.ApplicationOnlineFileCode.Category == Constants.APPLICATION_FILE_CLIENT_DOC_CODE ?
                        Constants.ESTIMATE_FILE_CLIENT_CODE : Constants.ESTIMATE_FILE_PLEDGE_CODE;

                    fileInfo.EstimationStorageFileName = FixFileName(fileInfo.Id.ToString(), fileInfo.ContentType);
                    fileInfo.EstimationServiceFileId = await UploadAdditionalFile(applicationId, estimationApplyId, fileInfo.Id, file.Stream,
                        fileInfo.EstimationStorageFileName, type, cancellationToken);

                    await _applicationOnlineFileRepository.Update(fileInfo);
                }
            }
            catch (Exception ex)
            {
                _logger.Error($"Error upload additional files for application {applicationId}.\r\n{ex.Message}");
                throw ex;
            }
        }

        public async Task SendFiles(Guid applicationId, IEnumerable<ApplicationOnlineFile> filesDetails, CancellationToken cancellationToken)
        {
            try
            {
                var applicationFiles = filesDetails != null && filesDetails.Any() ? filesDetails : await _applicationOnlineFileRepository.GetFilesForEstimation(applicationId);
                var additionalFiles = applicationFiles.Where(x => !x.EstimationServiceFileId.HasValue);

                foreach (var fileInfo in additionalFiles)
                {
                    var file = await _fileStorageService.Download(fileInfo.StorageFileId, cancellationToken);

                    fileInfo.EstimationStorageFileName = FixFileName(fileInfo.Id.ToString(), fileInfo.ContentType);
                    fileInfo.EstimationServiceFileId = await UploadFile(applicationId, fileInfo.Id, file.Stream,
                        fileInfo.EstimationStorageFileName, fileInfo.ApplicationOnlineFileCode.EstimationServiceCodeId.Value, cancellationToken);

                    await _applicationOnlineFileRepository.Update(fileInfo);
                }
            }
            catch (Exception ex)
            {
                _logger.Error($"Error upload files for application {applicationId}.\r\n{ex.Message}");
                throw ex;
            }
        }

        public async Task<ApplicationsOnlineEstimation> SendToEstimation(Guid applicationId, CancellationToken cancellationToken)
        {
            var estimationInfo = _applicationsOnlineEstimationRepository.GetLastByApplicationId(applicationId);
            var estimationClientId = estimationInfo?.EstimationServiceСlientId;
            var estimationPledgeId = estimationInfo?.EstimationServicePledgeId;
            var estimationApplyId = estimationInfo?.EstimationServiceApplyId;

            var filesDetails = await _applicationOnlineFileRepository.GetFilesForEstimation(applicationId);

            if (estimationApplyId.HasValue)
                await SendAdditionalFiles(applicationId, estimationApplyId.Value, filesDetails, cancellationToken);
            else
                await SendFiles(applicationId, filesDetails, cancellationToken);


            var clientFileIds = filesDetails
                .Where(x => x.ApplicationOnlineFileCode.Category == Constants.APPLICATION_FILE_CLIENT_DOC_CODE)
                .Select(x => x.EstimationServiceFileId.Value);

            if (estimationClientId.HasValue)
                await UpdateClient(applicationId, estimationClientId.Value, clientFileIds, cancellationToken);
            else
                estimationClientId = await CreateClient(applicationId, clientFileIds, cancellationToken);


            var pledgeFileIds = filesDetails
                .Where(x => x.ApplicationOnlineFileCode.Category != Constants.APPLICATION_FILE_CLIENT_DOC_CODE)
                .Select(x => x.EstimationServiceFileId.Value);

            if (estimationPledgeId.HasValue)
                await UpdatePledge(applicationId, estimationPledgeId.Value, pledgeFileIds, cancellationToken);
            else
                estimationPledgeId = await CreatePledge(applicationId, pledgeFileIds, cancellationToken);


            if (estimationApplyId.HasValue)
                await ResendApply(applicationId, estimationApplyId.Value, cancellationToken);
            else
                estimationApplyId = await CreateApplication(applicationId, estimationClientId.Value, estimationPledgeId.Value, cancellationToken);

            var estimation = new ApplicationsOnlineEstimation(applicationId, estimationClientId, estimationPledgeId, estimationApplyId);
            _applicationsOnlineEstimationRepository.Insert(estimation);

            return estimation;
        }

        public async Task UpdateClient(Guid applicationId, int estimationClientId, IEnumerable<int> filesIds, CancellationToken cancellationToken)
        {
            try
            {
                var client = await _clientRepository.GetByApplicationOnlineId(applicationId);
                ValidateClient(client);

                var documents = _clientRepository.GetClientDocumentsByClientId(client.Id);
                var document = documents
                    .Where(document => document.DocumentType.Code == Constants.PASSPORTKZ ||
                                       document.DocumentType.Code == Constants.IDENTITYCARD || document.DocumentType.Code == Constants.RESIDENCE)
                    .OrderByDescending(document => document.DateExpire).FirstOrDefault();
                ValidateClientDocument(document);

                var requestModel = new ClientUpdateRequest(
                    estimationClientId,
                    client.Name,
                    client.Surname,
                    client.Patronymic,
                    client.BirthDay.Value.ToString("yyyy-MM-dd"),
                    document.DocumentType.Code,
                    client.IdentityNumber,
                    document.Number,
                    document.Date.Value.ToString("yyyy-MM-dd"),
                    document.DateExpire.Value.ToString("yyyy-MM-dd"),
                    document.BirthPlace,
                    document.Provider.Code,
                    filesIds);

                var request = new HttpRequestMessage(HttpMethod.Post, $"{_httpClient.BaseAddress}/api/v1/clientUpdate");
                request.Headers.Add("X-API-Key", _apiKey);
                request.Content = new StringContent(JsonSerializer.Serialize(requestModel, _serializerOptions), null, "application/json");

                var response = await _httpClient.SendAsync(request, cancellationToken);
                var responseBody = await response.Content.ReadAsStringAsync();
                var clientCreationResponse = JsonSerializer.Deserialize<ClientResponse>(responseBody, _serializerOptions);

                if (!response.IsSuccessStatusCode)
                    throw new Exception(clientCreationResponse.Message);
            }
            catch (Exception ex)
            {
                _logger.Error($"Error update client to estimation service for application {applicationId}.\r\n{ex.Message}");
                throw ex;
            }
        }

        public async Task UpdatePledge(Guid applicationId, int estimationPledgeId, IEnumerable<int> filesIds, CancellationToken cancellationToken)
        {
            try
            {
                var car = await _applicationOnlineCarRepository.GetByApplicationId(applicationId);
                ValidateCarModel(car);

                var client = await _clientRepository.GetByApplicationOnlineId(applicationId);

                var requestModel = new PledgeUpdateRequest(
                    estimationPledgeId,
                    car.Mark,
                    car.TechPassportNumber,
                    car.BodyNumber,
                    car.TransportNumber,
                    car.TechPassportDate.Value.ToString("yyyy-MM-dd"),
                    car.Model,
                    car.VehicleModelId.Value,
                    car.Color,
                    client.FullName,
                    car.ReleaseYear.Value,
                    filesIds);

                var request = new HttpRequestMessage(HttpMethod.Post, $"{_httpClient.BaseAddress}/api/v1/pledgeUpdate");
                request.Headers.Add("X-API-Key", _apiKey);
                request.Content = new StringContent(JsonSerializer.Serialize(requestModel, _serializerOptions), null, "application/json");

                var response = await _httpClient.SendAsync(request, cancellationToken);
                var responseBody = await response.Content.ReadAsStringAsync();
                var pledgeCreationResponse = JsonSerializer.Deserialize<PledgeResponse>(responseBody, _serializerOptions);

                if (!response.IsSuccessStatusCode)
                    throw new Exception(pledgeCreationResponse.Message);
            }
            catch (Exception ex)
            {
                _logger.Error($"Error create car to estimation service for application {applicationId}.\r\n{ex.Message}");
                throw ex;
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

        private async Task<int> UploadAdditionalFile(Guid applicationId, int applyId, Guid fileId, Stream stream, string fileName, string type, CancellationToken cancellationToken)
        {
            try
            {
                var request = new HttpRequestMessage(HttpMethod.Post, $"{_httpClient.BaseAddress}/api/v1/addAdditionalPhoto");
                request.Headers.Add("X-API-Key", _apiKey);
                request.Content = new MultipartFormDataContent
                {
                    { new StreamContent(stream), "file", fileName },
                    { new StringContent(type), "type" },
                    { new StringContent(applyId.ToString()), "apply_id" }
                };

                var response = await _httpClient.SendAsync(request, cancellationToken);
                var responseBody = await response.Content.ReadAsStringAsync();
                var fileUploadResponse = JsonSerializer.Deserialize<FileUploadResponse>(responseBody, _serializerOptions);

                if (!response.IsSuccessStatusCode)
                    throw new Exception($"Error upload file to estimation service: {fileUploadResponse.Message}");

                return fileUploadResponse.Id;
            }
            catch (Exception ex)
            {
                _logger.Error($"Error upload additional file {fileId} for application {applicationId}.\r\n{ex.Message}");
                throw ex;
            }
        }

        private async Task<int> UploadFile(Guid applicationId, Guid fileId, Stream stream, string fileName, int fileCode, CancellationToken cancellationToken)
        {
            try
            {
                var request = new HttpRequestMessage(HttpMethod.Post, $"{_httpClient.BaseAddress}/api/v1/upload");
                request.Headers.Add("X-API-Key", _apiKey);
                request.Content = new MultipartFormDataContent
                {
                    { new StreamContent(stream), "file", fileName },
                    { new StringContent(fileCode.ToString()), "custom_properties" }
                };

                var response = await _httpClient.SendAsync(request, cancellationToken);
                var responseBody = await response.Content.ReadAsStringAsync();
                var fileUploadResponse = JsonSerializer.Deserialize<FileUploadResponse>(responseBody, _serializerOptions);

                if (!response.IsSuccessStatusCode)
                    throw new Exception($"Error upload file to estimation service: {fileUploadResponse.Message}");

                return fileUploadResponse.Id;
            }
            catch (Exception ex)
            {
                _logger.Error($"Error upload file {fileId} for application {applicationId}.\r\n{ex.Message}");
                throw ex;
            }
        }

        private void ValidateCarModel(ApplicationOnlineCar car)
        {
            if (string.IsNullOrEmpty(car.BodyNumber))
            {
                throw new NotEnoughDataForEstimationException(nameof(car.BodyNumber), car.BodyNumber,
                    "Не указан номер кузова");
            }

            if (string.IsNullOrEmpty(car.TransportNumber))
            {
                throw new NotEnoughDataForEstimationException(nameof(car.TransportNumber), car.TransportNumber,
                    "Не указан номер авто");
            }

            if (car.TechPassportDate == null)
            {
                throw new NotEnoughDataForEstimationException(nameof(car.TechPassportDate), car.TechPassportDate.ToString(),
                    "Не заполнено поле дата выдачи тех паспорта");
            }

            if (string.IsNullOrEmpty(car.Mark))
            {
                throw new NotEnoughDataForEstimationException(nameof(car.Mark), car.Mark,
                    "Не заполнено поле Марка машины ");
            }

            if (string.IsNullOrEmpty(car.Model))
            {
                throw new NotEnoughDataForEstimationException(nameof(car.Model), car.Model,
                    "Не заполнено поле модель машины ");
            }

            if (car.VehicleModelId == null)
            {
                throw new NotEnoughDataForEstimationException(nameof(car.VehicleModelId), car.VehicleModelId.ToString(),
                    "Не заполнено поле модель машины ");
            }

            if (string.IsNullOrEmpty(car.Color))
            {
                throw new NotEnoughDataForEstimationException(nameof(car.Color), car.Color,
                    "Не заполнено поле цвет авто ");
            }

            if (car.ReleaseYear == null)
            {
                throw new NotEnoughDataForEstimationException(nameof(car.ReleaseYear), car.ReleaseYear.ToString(),
                    "Не заполнено поле год выпуска авто ");
            }

            if (car.TechPassportNumber == null)
            {
                throw new NotEnoughDataForEstimationException(nameof(car.TechPassportNumber), car.TechPassportNumber,
                    "Не заполнено поле тех паспорт авто ");
            }
        }

        private void ValidateClient(Client client)
        {
            if (client.BirthDay == null)
            {
                throw new NotEnoughDataForEstimationException(nameof(client.BirthDay), client.BirthDay.ToString(),
                    "У клиента не заполнено поле Дата рождения");
            }
        }

        private void ValidateClientDocument(ClientDocument document)
        {
            if (document is null)
            {
                throw new NotEnoughDataForEstimationException(nameof(document.Number), document.Number,
                    $"Нету ни одного документа");
            }

            if (string.IsNullOrEmpty(document.Number))
            {
                throw new NotEnoughDataForEstimationException(nameof(document.Number), document.Number,
                    $"Номер документа {document.DocumentType.Name}, идентификатор : {document.Id}  не валидный ");
            }

            if (document.Date == null)
            {
                throw new NotEnoughDataForEstimationException(nameof(document.Date), document.Date.ToString(),
                    $"Дата выдачи документа {document.DocumentType.Name}, идентификатор : {document.Id}  не заполнена или неверная ");
            }

            if (document.DateExpire == null)
            {
                throw new NotEnoughDataForEstimationException(nameof(document.DateExpire), document.DateExpire.ToString(),
                    $"Дата когда документ действительный {document.DocumentType.Name}, идентификатор : {document.Id}  не заполнена  ");
            }

            if (document.DateExpire <= DateTime.Now)
            {
                throw new NotEnoughDataForEstimationException(nameof(document.Date), document.Date.ToString(),
                    $"Дата когда документ действительный  {document.DocumentType.Name}, идентификатор : {document.Id}  меньше текущей  ");
            }

            if (string.IsNullOrEmpty(document.BirthPlace))
            {
                throw new NotEnoughDataForEstimationException(nameof(document.BirthPlace), document.BirthPlace,
                    $"Место рождения  {document.DocumentType.Name}, идентификатор : {document.Id} не заполнено");
            }

            if (document.Provider == null)
            {
                throw new NotEnoughDataForEstimationException(nameof(document.BirthPlace), document.BirthPlace,
                    $"Орган выдачи не указан, идентификатор : {document.Id} не заполнено");
            }

            if (document.DocumentType == null)
            {
                throw new NotEnoughDataForEstimationException(nameof(document.BirthPlace), document.BirthPlace,
                    $"Тип документа не указан, идентификатор : {document.Id} не заполнено");
            }
        }
    }
}
