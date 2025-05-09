using Pawnshop.Data.Access;
using Pawnshop.Data.Models.ApplicationsOnline;
using Pawnshop.Data.Models.ApplicationsOnlineEstimation;
using Pawnshop.Data.Models.Comments;
using Pawnshop.Services.Estimation.Request;
using Pawnshop.Services.Estimation.Response;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Pawnshop.Data.Models.ApplicationsOnlineCar;
using Pawnshop.Data.Models.Clients;
using Pawnshop.Services.Estimation.Exceptions;
using Pawnshop.Services.Exceptions;
using Serilog;
using JsonException = System.Text.Json.JsonException;
using JsonSerializer = System.Text.Json.JsonSerializer;
using UnexpectedResponseException = Pawnshop.Services.Estimation.Exceptions.UnexpectedResponseException;

namespace Pawnshop.Services.Estimation
{
    public sealed class OldEstimationService
    {
        private readonly HttpClient _httpClient;
        private readonly ApplicationOnlineRepository _applicationOnlineRepository;
        private readonly ClientRepository _clientRepository;
        private readonly ApplicationOnlineCarRepository _applicationOnlineCarRepository;
        private readonly string _baseUrl;
        private readonly string _username;
        private readonly string _secret;
        private readonly ClientDocumentProviderRepository _clientDocumentProviderRepository;
        private readonly ApplicationsOnlineEstimationRepository _applicationsOnlineEstimationRepository;
        private readonly CommentsRepository _commentsRepository;
        private static JsonSerializerOptions _serializerOptions;
        private readonly ClientDocumentTypeRepository _clientDocumentTypeRepository;
        private readonly ApplicationOnlineFileRepository _applicationOnlineFileRepository;
        private readonly ApplicationOnlineRefinancesRepository _applicationsOnlineRefinancesRepository;
        private readonly ILogger _logger;

        public OldEstimationService(
            HttpClient httpClient,
            ApplicationOnlineRepository applicationOnlineRepository,
            ClientRepository clientRepository,
            ApplicationOnlineCarRepository applicationOnlineCarRepository,
            ClientDocumentProviderRepository clientDocumentProviderRepository,
            ApplicationsOnlineEstimationRepository applicationsOnlineEstimationRepository,
            CommentsRepository commentsRepository,
            ClientDocumentTypeRepository clientDocumentTypeRepository,
            ApplicationOnlineFileRepository applicationOnlineFileRepository,
            ApplicationOnlineRefinancesRepository applicationOnlineRefinancesRepository,
            IOptions<OldEstimationServiceOptions> options,
            ILogger logger)
        {
            _httpClient = httpClient;
            _baseUrl = options.Value.BaseUrl;
            _username = options.Value.UserName;
            _secret = options.Value.Secret;
            _applicationOnlineRepository = applicationOnlineRepository;
            _clientRepository = clientRepository;
            _applicationOnlineCarRepository = applicationOnlineCarRepository;
            _httpClient.BaseAddress = new Uri(_baseUrl);
            _serializerOptions = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };
            _clientDocumentProviderRepository = clientDocumentProviderRepository;
            _applicationsOnlineEstimationRepository = applicationsOnlineEstimationRepository;
            _commentsRepository = commentsRepository;
            _clientDocumentTypeRepository = clientDocumentTypeRepository;
            _applicationOnlineFileRepository = applicationOnlineFileRepository;
            _applicationsOnlineRefinancesRepository = applicationOnlineRefinancesRepository;
            _logger = logger;

        }

        public async Task<ApplicationsOnlineEstimation> SendToEstimation(Guid applicationId, int userId, CancellationToken cancellationToken)
        {
            var files = (await _applicationOnlineFileRepository.GetListView(applicationId, 0, 9999))?.Files;

            List<CarGallery> carGalleries = new List<CarGallery>();
            List<LicenseGallery> license = new List<LicenseGallery>();
            List<Gallery> gallery = new List<Gallery>();
            if (files != null)
            {
                var carFiles = files.Where(file => file.Category == "CarPhoto").ToList();

                int number = 0;
                for (int i = 0; i < carFiles.Count(); i++)
                {
                    number++;
                    carGalleries.Add(new CarGallery(carFiles[i].EstimationStorageFileName, carFiles[i].Title, number));
                }

                var licenseFiles = files.Where(file => file.Category == "CarDocument").ToList();
                for (int i = 0; i < licenseFiles.Count; i++)
                {
                    number++;
                    license.Add(new LicenseGallery(licenseFiles[i].EstimationStorageFileName, licenseFiles[i].Title,
                        number));
                }

                var clientPhoto = files.Where(file => file.Category == "ClientDocument").ToList();

                for (int i = 0; i < clientPhoto.Count; i++)
                {
                    number++;
                    gallery.Add(new Gallery(clientPhoto[i].EstimationStorageFileName, clientPhoto[i].Title, number));
                }
            }

            var bearer = await GetAuthorizationBearer(_username, _secret, cancellationToken);

            _httpClient.DefaultRequestHeaders.Clear();
            _httpClient.DefaultRequestHeaders.Add(
                "Authorization", $"Bearer {bearer}");

            #region отправка заявки
            var application = _applicationOnlineRepository.Get(applicationId);


            var client = _clientRepository.Get(application.ClientId);

            var document = client.Documents
                .Where(document => document.DocumentType.Code == "PASSPORTKZ" ||
                                   document.DocumentType.Code == "IDENTITYCARD" || document.DocumentType.Code == "RESIDENCE")
                .OrderByDescending(document => document.DateExpire).FirstOrDefault();

            var car = _applicationOnlineCarRepository.Get(application.ApplicationOnlinePositionId);

            ValidateClientForSendingEstimation(client, car, document);

            if (document == null)
            {
                throw new NotEnoughDataForEstimationException(nameof(document), null,
                    "У клиента нету документа удостовреяющего личность Пасспорт/Удостоверение/ВНЖ");
            }

            string estimationDocumentType = "";
            switch (document.DocumentType.Code)
            {
                case "PASSPORTKZ":
                    estimationDocumentType = "passport";
                    break;
                case "IDENTITYCARD":
                    estimationDocumentType = "id_card";
                    break;
                case "RESIDENCE":
                    estimationDocumentType = "residence";
                    break;
            }

            string estimationProviderType = "";

            switch (document.Provider.Code)
            {
                case "MIA_RK":
                    estimationProviderType = "MVD_RK";
                    break;
                case "MURK":
                    estimationProviderType = "MU_RK";
                    break;
                default:
                    estimationProviderType = "MVD_RK";
                    break;
            }


            var comments = _commentsRepository.GetLastComment(applicationId);
            string commentText = "";
            if (comments != null)
            {
                commentText = comments.CommentText;
            }

            var estimationRequest = new EstimationRequest
            {
                App_id = application.Id.ToString(),
                Name = client.Name,
                Surname = client.Surname,
                Middle_name = client.Patronymic,
                Amount = ((int)application.ApplicationAmount).ToString(),
                Interest_rate = "0",//Процентная ставка ?
                Type = "differentiated",
                Garantee = "0",//Поручительство менеджера всегда 0 в онлайне
                Notes = commentText,
                Is_refinance = IsRefinance(application), 
                Birthday = client.BirthDay.Value.ToString("yyyy-M-dd"),
                Body_number = car.BodyNumber,
                License_plate = car.TransportNumber,
                Registration_issue_date = car.TechPassportDate.Value.ToString("yyyy-M-dd"),
                Brand = car.Mark,
                Model = car.Model,
                Model_id = car.VehicleModelId.Value,
                Car_gallery = carGalleries, // Фото машины
                Color = car.Color,
                Holder_name = client.Surname,
                Prod_year = car.ReleaseYear.ToString(),
                License_gallery = license,//Тех пасспорт
                Indebtedness = "0", 
                Credit_type = GetEstimationCreditType(application),// 1 - первый транш , 2 - реф, 3 - добор
                Parent_contract_id = "0",
                Doc_type = estimationDocumentType, // id_card/passport/residence
                Gallery = gallery, //Фото клиента
                Individual_id_number = client.IdentityNumber,
                License_number = document.Number,
                License_date_of_issue = document.Date.Value.ToString("yyyy-M-dd"),
                License_date_of_end = document.DateExpire.Value.ToString("yyyy-M-dd"), //TODO DateExpire > текущей даты отваливается
                Place_of_birth = document.BirthPlace,
                License_issuer = estimationProviderType, //  MVD_RK/MU_RK
                Registration_number = car.TechPassportNumber,
                Date = await GetRefinanceData(application)
            };

            try
            {
                var content = new StringContent(JsonSerializer.Serialize(estimationRequest, _serializerOptions), Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync($"/api/crm/new-apply/", content, cancellationToken);

                if (!response.IsSuccessStatusCode)
                {
                    throw new UnexpectedResponseException($"Service return unexpected status code : {response.StatusCode}." +
                                                          $" With body {await response.Content.ReadAsStringAsync()}");
                }

                EstimationResponse estimationResponse =
                    JsonSerializer.Deserialize<EstimationResponse>(await response.Content.ReadAsStringAsync(), _serializerOptions);

                ApplicationsOnlineEstimation estimation = new ApplicationsOnlineEstimation(applicationId,
                    estimationResponse.Data.Client_id, estimationResponse.Data.Pledge_id, estimationResponse.Data.Apply_id);
                _applicationsOnlineEstimationRepository.Insert(estimation);

                application.ChangeStatus(ApplicationOnlineStatus.OnEstimation, userId);
                await _applicationOnlineRepository.Update(application);

                return estimation;
            }
            catch (HttpRequestException exception)
            {
                _logger.Error(exception, exception.Message);
                throw new ServiceUnavailableException($"{_baseUrl}/api/crm/new-apply/", exception.Message);
            }
            catch (TaskCanceledException exception)
            {
                _logger.Error(exception, exception.Message);
                throw new ServiceUnavailableException($"{_baseUrl}/api/crm/new-apply/", exception.Message);
            }
            catch (JsonException exception)
            {
                _logger.Error(exception, exception.Message);
                throw new UnexpectedResponseException(exception.Message);
            }


            #endregion
        }


        private void ValidateClientForSendingEstimation(Client client, ApplicationOnlineCar car, ClientDocument document)
        {
            if (client.BirthDay == null)
            {
                throw new NotEnoughDataForEstimationException(nameof(client.BirthDay), client.BirthDay.ToString(),
                    "У клиента не заполнено поле Дата рождения");
            }

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

        }

        private async Task<string> GetAuthorizationBearer(string username, string password, CancellationToken cancellationToken)
        {

            MultipartFormDataContent content = new MultipartFormDataContent
            {
                { new StringContent(username), nameof(username) },
                { new StringContent(password), nameof(password) }
            };

            var bearer = await _httpClient.PostAsync("/api/auth/login", content, cancellationToken);

            var response = await bearer.Content.ReadAsStringAsync();

            return JsonConvert.DeserializeObject<BearerResponse>(response).Access_token;
        }

        private string IsRefinance(ApplicationOnline application)
        {
            if (application.Type == ApplicationOnlineType.Refinance.ToString())
                return "1";
            return "0";
        }

        private string GetEstimationCreditType(ApplicationOnline application)
        {
            if (application.Type == ApplicationOnlineType.AdditionalTranche.ToString())
                return "3";

            if (application.Type == ApplicationOnlineType.Refinance.ToString())
                return "2";

            if (application.Type == ApplicationOnlineType.BasicTranche.ToString())
                return "1";

            return "1";
        }

        private async Task<RefinanceData> GetRefinanceData(ApplicationOnline application)
        {
            if (application.Type == ApplicationOnlineType.Refinance.ToString())
            {
                var refinances = await 
                    _applicationsOnlineRefinancesRepository.GetApplicationOnlineRefinancesByApplicationId(
                        application.Id);
                return new RefinanceData
                {
                    Refinance_list = string.Join(',', refinances.Select(refinance => refinance.ContractNumber).ToList())
                };
            }
                
            return null;
        }

    }
}
