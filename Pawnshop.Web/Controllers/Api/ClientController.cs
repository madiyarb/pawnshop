using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Pawnshop.Core.Exceptions;
using Pawnshop.Core.Extensions;
using Pawnshop.Core.Utilities;
using Pawnshop.Core;
using Pawnshop.Data.Access;
using Pawnshop.Data.Models.AbsOnline;
using Pawnshop.Data.Models.ApplicationsOnline.Bindings;
using Pawnshop.Data.Models.Audit;
using Pawnshop.Data.Models.Clients;
using Pawnshop.Data.Models.Dictionaries;
using Pawnshop.Services.Clients;
using Pawnshop.Services.KFM;
using Pawnshop.Web.Engine.Middleware;
using Pawnshop.Web.Engine.Services.Interfaces;
using Pawnshop.Web.Engine;
using Pawnshop.Web.Models.Clients;
using Pawnshop.Web.Models.List;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System;
using System.Data.Common;
using System.Threading.Tasks;
using Pawnshop.Data.Models.Clients.Views;
using Pawnshop.Web.Models;
using Pawnshop.Web.Models.ClientRequisites.Bindings;
using Pawnshop.Data.Models.OnlineTasks;
using System.Threading;
using MediatR;
using Pawnshop.Data.Models.Contracts.Views;
using Microsoft.AspNetCore.Http;
using Pawnshop.Data.Models.Auction.Dtos.Client;
using Pawnshop.Data.Models.MobileApp.HardCollection.Commands;
using Serilog;
using Pawnshop.Services.ClientExternalValidation;
using Pawnshop.Services.Bankruptcy;
using Pawnshop.Services.ClientDeferments.Interfaces;
using Pawnshop.Data.Access.LegalCollection;
using Pawnshop.Data.Models;
using Pawnshop.Web.Models.Contract;

namespace Pawnshop.Web.Controllers.Api
{
    [Authorize(Permissions.ClientView)]
    public class ClientController : Controller
    {
        private readonly ClientRepository _repository;
        private readonly UserRepository _userRepository;
        private readonly ISessionContext _sessionContext;
        private readonly RequisiteTypeRepository _requisiteTypeRepository;
        private readonly IClientBlackListService _clientBlackListService;
        private readonly IClientModelValidateService _clientModelValidateService;
        private readonly IDomainService _domainService;
        private readonly IClientContactService _clientContactService;
        private readonly IKFMService _kfmService;
        private readonly ClientDocumentTypeRepository _clientDocumentTypeRepository;
        private readonly ClientDocumentProviderRepository _clientDocumentProviderRepository;
        private readonly ILogger _logger;
        private readonly BankruptcyService _bankruptcyService;
        private readonly ClientLegalFormRepository _clientLegalFormRepository;
        private readonly IClientDefermentService _clientDefermentService;
        private readonly IClientService _clientService;

        public ClientController(
            ClientRepository repository,
            UserRepository userRepository,
            ISessionContext sessionContext,
            RequisiteTypeRepository requisiteTypeRepository,
            IClientBlackListService clientBlackListService,
            IClientModelValidateService clientModelValidateService,
            IDomainService domainService,
            IClientContactService clientContactService,
            IKFMService kfmService,
            ClientDocumentTypeRepository clientDocumentTypeRepository,
            ClientDocumentProviderRepository clientDocumentProviderRepository,
            ILogger logger,
            BankruptcyService bankruptcyService, 
            IClientDefermentService clientDefermentService,
            ClientLegalFormRepository clientLegalFormRepository,
            IClientService clientService)
        {
            _repository = repository;
            _userRepository = userRepository;
            _sessionContext = sessionContext;
            _requisiteTypeRepository = requisiteTypeRepository;
            _clientBlackListService = clientBlackListService;
            _clientModelValidateService = clientModelValidateService;
            _domainService = domainService;
            _clientContactService = clientContactService;
            _kfmService = kfmService;
            _clientDocumentTypeRepository = clientDocumentTypeRepository;
            _clientDocumentProviderRepository = clientDocumentProviderRepository;
            _logger = logger;
            _bankruptcyService = bankruptcyService;
            _clientLegalFormRepository = clientLegalFormRepository;
            _clientDefermentService = clientDefermentService;
            _clientService = clientService;
        }

        [HttpPost("/api/client/list")]
        public ListModel<Client> List([FromBody] ListQueryModel<ClientListQueryModel> listQuery)
        {
            List<Client> list = _repository.List(listQuery, listQuery.Model);

            list.ForEach(p =>
            {
                p.IsInBlackList = _clientBlackListService.IsClientInBlackList(p.Id);
                p.MobilePhoneList = _clientContactService.GetMobilePhoneContacts(p.Id).Select(c => new ClientContact
                {
                    Address = c.Address,
                    ContactTypeId = c.ContactTypeId,
                    Id = c.Id,
                    IsDefault = c.IsDefault,
                    VerificationExpireDate = c.VerificationExpireDate,
                    SendUkassaCheck = c.SendUkassaCheck,
                    ContactCategoryId = c.ContactCategoryId,
                    IsActual = c.IsActual,
                    SourceId = c.SourceId
                }).ToList();
            });

            return new ListModel<Client>
            {
                List = list,
                Count = _repository.Count(listQuery, listQuery.Model)
            };
        }

        [HttpPost("/api/client/card"), ProducesResponseType(typeof(Client), 200)]
        public IActionResult Card([FromBody] int id)
        {
            if (id <= 0) throw new ArgumentOutOfRangeException(nameof(id));

            var result = _repository.Get(id);
            result.IsInBlackList = _clientBlackListService.IsClientInBlackList(id);

            if (result == null) throw new InvalidOperationException();
            return Ok(result);
        }

        //код для поиска по иину для сайта
        [HttpPost("/api/client/searchByIdentityNumber")]
        public string FindByIdentityNumber([FromBody] string IdentityNumber)
        {
            var result = _repository.FindByIdentityNumber(IdentityNumber);
            if (result == null) throw new PawnshopApplicationException("Клиент не найден");

            return result.IdentityNumber;
        }
        
        // поиск клиента по ИИН для аукциона
        [HttpGet("/api/client/{IIN}")]
        [ProducesResponseType(typeof(AuctionClientDto), 200)]
        [ProducesResponseType(404)]
        public IActionResult GetByIin(string IIN)
        {
            var result = _clientService.GetByIin(IIN);
            if (result == null)
            {
                return NotFound();
            }

            return Ok(result) ;
        }

        [HttpPost("/api/client/save"), Authorize(Permissions.ClientManage)]
        [Event(EventCode.ClientSaved, EventMode = EventMode.Response, EntityType = EntityType.Client)]
        public async Task<IActionResult> Save([FromBody] Client model, [FromServices] IMediator mediator)
        {
            ModelState.Validate();

            if (!model.IsInBlackList)
                _clientModelValidateService.ValidateClientModel(model);

            if (!(model.UserId > 0))
            {
                var user = _userRepository.SearchUserByIdentityNumber(model.IdentityNumber);
                if (user != null) model.UserId = user.Id;
            }

            try
            {
                //проставляем реквизитам автора и дату создания
                foreach (var requisite in model.Requisites.Where(x => !(x.Id > 0)))
                {
                    requisite.AuthorId = _sessionContext.UserId;
                    requisite.CreateDate = DateTime.Now;
                    requisite.IsCorrectValue();
                    var requisiteType = _requisiteTypeRepository.Get(requisite.RequisiteTypeId);
                    if (!requisite.IsMatchesTheMask(requisiteType.Mask)) throw new PawnshopApplicationException($"Значение реквизита {requisite.Value} не прошло проверку"); ;
                }

                foreach (var document in model.Documents.Where(x => !(x.Id > 0)))
                {
                    document.AuthorId = _sessionContext.UserId;
                    document.CreateDate = DateTime.Now;
                }

                foreach (var address in model.Addresses.Where(x => !(x.Id > 0)))
                {
                    address.AuthorId = _sessionContext.UserId;
                    address.CreateDate = DateTime.Now;
                }

                if (model.Id > 0)
                {
                    _repository.Update(model);
                }
                else
                {
                    model.AuthorId = _sessionContext.UserId;
                    model.CreateDate = DateTime.Now;
                    _repository.Insert(model);
                }

                if (model.Addresses.Any())
                {
                    await mediator.Send(new SendAddressCommand() { clientAddresses = model.Addresses.ToArray(), ClientId = model.Id });
                }

                if (model.LegalForm.Id == _clientLegalFormRepository.Find(new { Code = Constants.INDIVIDUAL }).Id)
                {
                    await _bankruptcyService.CheckIndividualClient(model.IdentityNumber);
                }
                else
                {
                    await _bankruptcyService.CheckCompany(model.IdentityNumber);
                }
            }
            catch (SqlException e)
            {
                if (e.Number == 2627)
                {
                    throw new PawnshopApplicationException("Поле ИИН/БИН должно быть уникальным");
                }
                throw new PawnshopApplicationException(e.Message);
            }
            catch (PawnshopApplicationException e)
            {
                var message = string.Empty;
                message = e.Message.Replace("REQUISITE_MUST_BE_UNIQUE", "Данный реквизит  указан по другому клиенту");
                message = e.Message.Replace("DOCUMENT_MUST_BE_UNIQUE", "Документ должен быть уникальным");
                message = e.Message.Replace("ADDRESS_MUST_BE_UNIQUE", "Адрес должен быть уникальным");

                throw new PawnshopApplicationException(string.IsNullOrEmpty(message) ? e.Message : message);
            }

            return Ok(model);
        }

        [HttpPost("/api/client/saveEstimationCompany"), Authorize(Permissions.ClientManage)]
        [Event(EventCode.ClientSaved, EventMode = EventMode.Response, EntityType = EntityType.Client)]
        public IActionResult SaveEstimationCompany([FromBody] Client model)
        {
            ModelState.Validate();

            if (!(model.UserId > 0))
            {
                var user = _userRepository.SearchUserByIdentityNumber(model.IdentityNumber);
                if (user != null) model.UserId = user.Id;
            }

            try
            {

                if (model.Id > 0)
                {
                    _repository.Update(model);
                }
                else
                {
                    model.AuthorId = _sessionContext.UserId;
                    model.CreateDate = DateTime.Now;
                    model.CardType = CardType.Standard;
                    model.SubTypeId = _domainService.GetDomainValue(Constants.CLIENT_SUB_TYPE, Constants.ESTIMATION_COMPANY).Id;
                    _repository.Insert(model);
                }
            }
            catch (SqlException e)
            {
                throw new PawnshopApplicationException(e.Message);
            }
            catch (PawnshopApplicationException e)
            {
                var message = string.Empty;
                message = e.Message.Replace("REQUISITE_MUST_BE_UNIQUE", "Реквизит должен быть уникальным");
                message = e.Message.Replace("DOCUMENT_MUST_BE_UNIQUE", "Документ должен быть уникальным");
                message = e.Message.Replace("ADDRESS_MUST_BE_UNIQUE", "Адрес должен быть уникальным");

                throw new PawnshopApplicationException(string.IsNullOrEmpty(message) ? e.Message : message);
            }

            return Ok(model);
        }

        [Obsolete("Use /api/dictionary/estimationCompanies or /api/estiomationCompany/list")]
        [HttpPost("/api/client/listEstimationCompanies")]
        public ListModel<Client> ListEstimationCompanies([FromBody] ListQueryModel<ClientListQueryModel> listQuery)
        {
            var subTypeId = _domainService.GetDomainValue(Constants.CLIENT_SUB_TYPE, Constants.ESTIMATION_COMPANY).Id;
            var list = _repository.ListEstimationCompanies(new Core.Queries.ListQuery(), subTypeId);
            return new ListModel<Client>
            {
                List = list,
                Count = _repository.CountEstimationCompanies(new Core.Queries.ListQuery(), subTypeId)
            };
        }

        [HttpPost("/api/client/saveMerchant"), Authorize(Permissions.ClientManage)]
        [Event(EventCode.ClientSaved, EventMode = EventMode.Response, EntityType = EntityType.Client)]
        public IActionResult SaveMerchant([FromBody] ClientMerchantContactDto clientDto)
        {
            ModelState.Validate();
            var client = clientDto.Client;
            if (!client.IsInBlackList)
                _clientModelValidateService.ValidateMerchantClientModel(client);

            if (!(client.UserId > 0))
            {
                var user = _userRepository.SearchUserByIdentityNumber(client.IdentityNumber);
                if (user != null) client.UserId = user.Id;
            }

            try
            {
                foreach (var requisite in client.Requisites.Where(x => x.Id <= 0))
                {
                    requisite.AuthorId = _sessionContext.UserId;
                    requisite.CreateDate = DateTime.Now;

                    var requisiteType = _requisiteTypeRepository.Get(requisite.RequisiteTypeId);
                    if (!requisite.IsMatchesTheMask(requisiteType.Mask)) throw new PawnshopApplicationException($"Значение реквизита {requisite.Value} не прошло проверку"); ;
                }

                foreach (var document in client.Documents.Where(x => x.Id <= 0))
                {
                    document.AuthorId = _sessionContext.UserId;
                    document.CreateDate = DateTime.Now;
                }

                if (client.Id > 0)
                {
                    _repository.Update(client);
                }
                else
                {
                    client.AuthorId = _sessionContext.UserId;
                    client.CreateDate = DateTime.Now;
                    _repository.Insert(client);//id
                }

                try
                {
                    _clientContactService.SaveMerchant(client.Id, clientDto.Contacts);
                }
                catch (PawnshopApplicationException e)
                {
                    throw new PawnshopApplicationException(string.Join(",", e.Messages));
                }

            }
            catch (SqlException e)
            {
                if (e.Number == 2627)
                {
                    throw new PawnshopApplicationException("Поле ИИН/БИН должно быть уникальным");
                }
                throw new PawnshopApplicationException(e.Message);
            }
            catch (PawnshopApplicationException e)
            {
                var message = string.Empty;
                message = e.Message.Replace("DOCUMENT_MUST_BE_UNIQUE", "Документ должен быть уникальным");

                throw new PawnshopApplicationException(string.IsNullOrEmpty(message) ? e.Message : message);
            }

            return Ok(clientDto);
        }

        [HttpPost("/api/client/create-simple"), Authorize(Permissions.ClientManage)]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(int))]
        public async Task<IActionResult> CreateSimple([FromBody] CreateSimpleClientCommand command)
        {
            try
            {
                ModelState.Validate();
                var clientId = await _clientService.CreateSimpleClientAsync(command);
                return Ok(clientId);
            }
            catch (SqlException e)
            {
                if (e.Number == 2627)
                {
                    return BadRequest("Поле ИИН/БИН должно быть уникальным");
                }
                throw new PawnshopApplicationException(e.Message);
            }
            catch (PawnshopApplicationException e)
            {
                throw new PawnshopApplicationException(e.Messages);
            }
        }

        [HttpPost("/api/client/delete"), Authorize(Permissions.ClientManage)]
        [Event(EventCode.ClientDeleted, EventMode = EventMode.Request, EntityType = EntityType.Client)]
        public IActionResult Delete([FromBody] int id)
        {
            if (id <= 0) throw new ArgumentOutOfRangeException(nameof(id));
            var count = _repository.RelationCount(id);
            if (count > 0)
            {
                throw new Exception("Невозможно удалить клиента, так как он привязан к договору, кассовому ордеру или другому документу");
            }

            _repository.Delete(id);
            return Ok();
        }

        [HttpPost]
        public IActionResult CheckForContract([FromBody] Client model, bool printCheck)
        {
            _clientModelValidateService.ValidateClientModel(model, printCheck: printCheck);
            return Ok();
        }

        [HttpPost("/api/client/from-mobile")]
        public IActionResult CreateClientFromMobile(
            [FromBody] ClientCreateFromMobileRequest model,
            [FromServices] OnlineTasksRepository onlineTasksRepository,
            [FromServices] ClientProfileRepository clientProfileRepository,
            [FromServices] IDomainService domainService)
        {
            var response = new ClientCreateFromMobileResponse();

            try
            {
                #region check binding data
                if (string.IsNullOrEmpty(model.IIN))
                {
                    return StatusCode((int)HttpStatusCode.BadRequest,
                        new BaseResponseDRPP(HttpStatusCode.BadRequest, $"IIN from model is empty ",
                            DRPPResponseStatusCode.WrongIIN));
                }

                if (_clientModelValidateService.ValidateIdentityNumber(model.IIN))
                {
                    return StatusCode((int)HttpStatusCode.BadRequest,
                        new BaseResponseDRPP(HttpStatusCode.BadRequest, $"IIN from model is wrong : {model.IIN} ",
                            DRPPResponseStatusCode.WrongIIN));
                }

                if (string.IsNullOrEmpty(model.Phone))
                {
                    return StatusCode((int)HttpStatusCode.BadRequest,
                        new BaseResponseDRPP(HttpStatusCode.BadRequest, $"Phone from model is empty ",
                            DRPPResponseStatusCode.WrongIIN));
                }

                if (!RegexUtilities.IsValidKazakhstanPhone(model.Phone))
                {
                    return StatusCode((int)HttpStatusCode.BadRequest,
                        new BaseResponseDRPP(HttpStatusCode.BadRequest, $"Phone from model is wrong : {model.Phone} ",
                            DRPPResponseStatusCode.WrongPhone));
                }
                #endregion

                var client = _repository.FindByIdentityNumber(model.IIN);
                var contactPhone = _clientContactService.Find(new { Address = model.Phone, IsDefault = true });
                bool created = false;

                var contactCategory = domainService
                    .GetDomainValue(Constants.DOMAIN_CONTACT_CATEGORY, Constants.DOMAIN_VALUE_CONTACT_CONTRACT);

                var source = domainService
                    .GetDomainValue(Constants.DOMAIN_CONTACT_SOURCE, Constants.DOMAIN_VALUE_CONTRACT_CLIENT_PROFILE);

                if (client == null)
                {
                    using (var transaction = _repository.BeginTransaction())
                    {
                        client = new Client
                        {
                            AuthorId = _sessionContext.UserId,
                            BeneficiaryCode = 19,
                            CardType = CardType.Standard,
                            CitizenshipId = 118,
                            CreateDate = DateTime.Now,
                            IdentityNumber = model.IIN,
                            IsResident = true,
                            LegalFormId = 16,
                            MobilePhone = model.Phone,
                            PartnerCode = model.PartnerCode,
                        };

                        _repository.Insert(client);

                        var clientContact = new ClientContact
                        {
                            Address = model.Phone,
                            AuthorId = _sessionContext.UserId,
                            ClientId = client.Id,
                            ContactTypeId = 1,
                            CreateDate = DateTime.Now,
                            IsDefault = true,
                            ContactCategoryId = contactCategory.Id,
                            SourceId = source.Id,
                            VerificationExpireDate = DateTime.Today.AddMonths(12),
                        };

                        _clientContactService.SaveWithoutChecks(clientContact);

                        var clientProfile = new ClientProfile
                        {
                            ClientId = client.Id,
                            AuthorId = Constants.ADMINISTRATOR_IDENTITY,
                            CreateDate = DateTime.Now
                        };
                        clientProfileRepository.Insert(clientProfile);

                        transaction.Commit();
                    }

                    response.ClientId = client.Id;
                    created = true;
                }
                else
                {
                    response.ClientId = client.Id;
                    bool clientUpdated = false;
                    if (!client.BeneficiaryCode.HasValue)
                    {
                        clientUpdated = true;
                        client.BeneficiaryCode = 19;
                    }
                    if (string.IsNullOrEmpty(client.PartnerCode))
                    {
                        clientUpdated = true;
                        client.PartnerCode = model.PartnerCode;
                    }
                    if (clientUpdated)
                    {
                        _repository.Update(client);
                    }

                    var clientContacts = _clientContactService.GetMobilePhoneContacts(client.Id);
                    var phone = clientContacts.FirstOrDefault(x => x.Address == model.Phone);


                    using (var transaction = _repository.BeginTransaction())
                    {
                        clientContacts.Where(x => x.IsDefault && x.Id != (phone?.Id ?? 0))
                            .ToList()
                            .ForEach(x =>
                            {
                                x.IsDefault = false;
                                _clientContactService.SaveWithoutChecks(x);
                            });

                        if (phone == null)
                        {
                            var clientContact = new ClientContact
                            {
                                Address = model.Phone,
                                AuthorId = _sessionContext.UserId,
                                ClientId = client.Id,
                                ContactTypeId = 1,
                                CreateDate = DateTime.Now,
                                IsDefault = true,
                                ContactCategoryId = contactCategory.Id,
                                SourceId = source.Id,
                                VerificationExpireDate = DateTime.Today.AddMonths(12),
                            };

                            _clientContactService.SaveWithoutChecks(clientContact);
                        }
                        else if (phone != null && !phone.IsDefault)
                        {
                            phone.IsDefault = true;
                            phone.VerificationExpireDate = DateTime.Today.AddMonths(12);

                            _clientContactService.SaveWithoutChecks(phone);
                        }
                        else
                        {
                            phone.VerificationExpireDate = DateTime.Today.AddMonths(12);

                            _clientContactService.SaveWithoutChecks(phone);
                        }

                        transaction.Commit();
                    }
                }

                if (contactPhone != null && contactPhone.ClientId != client.Id)
                {
                    var existContactClient = _repository.GetOnlyClient(contactPhone.ClientId);

                    var desc = $@"Попытка зарегистрировать аккаунт с данными, которые уже существуют в системе.
Существующие аккаунты: ИИН - {existContactClient.IdentityNumber}, телефон - {contactPhone.Address}.
Данные, с которыми пытаются зарегистрироваться: ИИН - {client.IdentityNumber}, телефон - {model.Phone}.";

                    var task = new OnlineTask(Guid.NewGuid(), OnlineTaskType.ResolutionSituation.ToString(),
                        Constants.ADMINISTRATOR_IDENTITY, desc, "Ошибка регистрации: дублирование данных.", null, contactPhone.ClientId);
                    onlineTasksRepository.Insert(task);
                }

                return StatusCode((int)(created ? HttpStatusCode.Created : HttpStatusCode.OK), response);
            }
            catch (PawnshopApplicationException exception)
            {
                return StatusCode((int)HttpStatusCode.Locked,
                    new BaseResponseDRPP(HttpStatusCode.Locked, exception.Message,
                        DRPPResponseStatusCode.UnspecifiedPawnshopApplicationException));
            }
            catch (DbException exception)
            {
                return StatusCode((int)HttpStatusCode.InsufficientStorage,
                    new BaseResponseDRPP(HttpStatusCode.InsufficientStorage, exception.Message,
                        DRPPResponseStatusCode.UnspecifiedDatabaseProblems));
            }
            catch (Exception exception)
            {
                return StatusCode((int)HttpStatusCode.InternalServerError,
                    new BaseResponseDRPP(HttpStatusCode.InternalServerError, exception.Message,
                        DRPPResponseStatusCode.UnspecifiedProblem));
            }
        }

        [HttpPut("/api/client/from-mobile")]
        public IActionResult UpdateClientFromMobile([FromBody] ClientFromMobile model)
        {
            try
            {
                var client = _repository.Get(model.ClientId);

                if (client == null)
                    return NotFound("Клиент не найден.");

                client.Name = model.Name.IsNullOrEmpty(client.Name);
                client.Surname = model.Surname.IsNullOrEmpty(client.Surname);
                client.Patronymic = model.Patronymic.IsNullOrEmpty(client.Patronymic);
                client.FullName = $"{client.Surname} {client.Name} {client.Patronymic}";
                client.BirthDay = model.BirthDay.CompareResultForDb(client.BirthDay);
                client.IsMale = model.IsMale.HasValue ? model.IsMale.Value : client.IsMale;

                if (model.Document == null || string.IsNullOrEmpty(model.Document.Number))
                {
                    _repository.Update(client);
                    return Ok();
                }

                var clientDocuments = _repository.GetClientDocumentsByClientId(model.ClientId);
                var document = clientDocuments.FirstOrDefault(x => x.Number == model.Document.Number) ??
                    new ClientDocument
                    {
                        AuthorId = Constants.ADMINISTRATOR_IDENTITY,
                        ClientId = model.ClientId,
                        CreateDate = DateTime.Now,
                        Number = model.Document.Number,
                    };

                document.Date = model.Document.IssueDate.CompareResultForDb(document.Date);
                document.DateExpire = model.Document.ExpireDate.CompareResultForDb(document.DateExpire);
                document.BirthPlace = model.Document.BirthPlace.IsNullOrEmpty(document.BirthPlace);
                document.DocumentType = new ClientDocumentType { Code = Constants.ANOTHER };
                document.TypeId = 1;

                if (!string.IsNullOrEmpty(model.Document.TypeCode))
                {
                    var docTypes = _clientDocumentTypeRepository.List(null);

                    List<string> allowedDocTypes = new List<string> { Constants.IDENTITYCARD, Constants.PASSPORTKZ, Constants.RESIDENCE };

                    var docType = docTypes.Where(x => allowedDocTypes.Contains(x.Code) && x.Code == model.Document.TypeCode)
                        .FirstOrDefault(x => Regex.IsMatch(model.Document.Number, x.NumberMask.Replace("\\\\", "\\")))
                        ?? docTypes.FirstOrDefault(x => x.Code == Constants.ANOTHER);

                    document.DocumentType = new ClientDocumentType { Code = docType.Code };
                    document.TypeId = docType.Id;
                }

                if (!string.IsNullOrEmpty(model.Document.IssuerCode))
                {
                    var docProvider = _clientDocumentProviderRepository.Find(new { Code = model.Document.IssuerCode });
                    document.Provider = docProvider;
                    document.ProviderId = docProvider.Id;
                }

                if (document.Id == 0)
                {
                    client.Documents.Add(document);
                }
                else
                {
                    var docIndex = client.Documents.FindIndex(doc => doc.Id == document.Id);
                    client.Documents.RemoveAt(docIndex);
                    client.Documents.Add(document);
                }

                _repository.Update(client);
                return Ok();
            }
            catch (Exception ex)
            {
                return StatusCode((int)HttpStatusCode.InternalServerError, new BaseResponseDRPP(HttpStatusCode.InternalServerError, ex.Message,
                    DRPPResponseStatusCode.UnspecifiedProblem));
            }
        }

        [HttpGet("/api/clients/{id}/isTrustable")]
        public async Task<IActionResult> CheckClientForTrust([FromRoute] int id,
            [FromServices] ClientRepository clientRepository,
            [FromServices] ClientsBlackListRepository clientlbakBlackListRepository,
            [FromServices] LocalizationRepository localizationRepository,
            [FromServices] IClientExpiredSchedulesGetterService expiredSchedulesGetter,
            [FromServices] IClientExternalValidationService externalValidationService,
            CancellationToken cancellationToken)
        {
            try
            {
                var client = clientRepository.Get(id);
                var errorLocalization =
                    localizationRepository.GetByCode(Constants.LOCALIZATION_TRUSTABLE_ERROR_MESSAGE);

                if (client == null)
                {
                    return BadRequest(new BaseResponseDRPP(HttpStatusCode.BadRequest, $"Client {id} not found",
                        DRPPResponseStatusCode.ClientNotFound));
                }

                var externalIntegrationCheck = await externalValidationService.CheckClientForExternalIntegration(client, cancellationToken);
                if (!externalIntegrationCheck)
                {
                    return Ok(new ClientIsTrustableCheckView(false, errorLocalization.Localizations));
                }
                var clientsInBlackList = clientlbakBlackListRepository.GetClientsBlackListsByClientId(id);

                if (clientsInBlackList.Count > 0)
                {
                    return Ok(new ClientIsTrustableCheckView(false, errorLocalization.Localizations));
                }

                var activeDeferment = _clientDefermentService.GetActiveDeferments(id);
                if (activeDeferment.Any())
                {
                    return Ok(new ClientIsTrustableCheckView(false, errorLocalization.Localizations));
                }

                if (client.BirthDay != null)
                {
                    if (DateTime.Now.Year - client.BirthDay.Value.Year <=
                        Constants.CLIENT_MINIMAL_AGE) //Проверяем на возраст 
                    {
                        if (DateTime.Now.Year - client.BirthDay.Value.Year < Constants.CLIENT_MINIMAL_AGE)
                            return Ok(new ClientIsTrustableCheckView(false, errorLocalization.Localizations));

                        if (!(DateTime.Now.Year - client.BirthDay.Value.Year == Constants.CLIENT_MINIMAL_AGE
                              && DateTime.Now.AddYears(client.BirthDay.Value.Year - DateTime.Now.Year) >=
                              client.BirthDay.Value))
                            return Ok(new ClientIsTrustableCheckView(false, errorLocalization.Localizations));
                    }
                }

                var isFindKFMList = await _kfmService.FindByIdentityNumberAsync(client.IdentityNumber);


                if (isFindKFMList)
                    return Ok(new ClientIsTrustableCheckView(false, errorLocalization.Localizations));
                if (expiredSchedulesGetter.SomeCurrentContractsOnExpiredNow(client.Id))
                {
                    return Ok(new ClientIsTrustableCheckView(false, errorLocalization.Localizations));
                }

                return Ok(new ClientIsTrustableCheckView(true));
            }
            catch (PawnshopApplicationException exception)
            {
                _logger.Error(exception, exception.Message);
                return StatusCode((int)HttpStatusCode.Locked,
                    new BaseResponseDRPP(HttpStatusCode.Locked, exception.Message,
                        DRPPResponseStatusCode.UnspecifiedPawnshopApplicationException));
            }
            catch (DbException exception)
            {
                _logger.Error(exception, exception.Message);
                return StatusCode((int)HttpStatusCode.InsufficientStorage,
                    new BaseResponseDRPP(HttpStatusCode.InsufficientStorage, exception.Message,
                        DRPPResponseStatusCode.UnspecifiedDatabaseProblems));
            }
            catch (Exception exception)
            {
                _logger.Error(exception, exception.Message);
                return StatusCode((int)HttpStatusCode.InternalServerError,
                    new BaseResponseDRPP(HttpStatusCode.InternalServerError, exception.Message,
                        DRPPResponseStatusCode.UnspecifiedProblem));
            }
        }

        [HttpGet("/api/clients/{id}/applicationonlinelist")]
        public async Task<IActionResult> GetApplicationOnlineList(
            [FromRoute] int id,
            [FromServices] ApplicationOnlineRepository repository,
            [FromQuery] PageBinding pageBinding,
            [FromQuery] GetApplicationOnlineListBinding binding)
        {
            return Ok(await repository.GetList(binding, pageBinding.Offset, pageBinding.Limit, id));
        }

        [HttpGet("/api/clients/{id}/applicationonline")]
        [ProducesResponseType(typeof(ApplicationOnlineClientView), 200)]
        [ProducesResponseType(typeof(BaseResponse), 400)]
        public IActionResult GetClient(
            [FromRoute] int id,
            [FromServices] ClientRepository repository)
        {
            var client = repository.GetClientForApplicationOnlineView(id);
            if (client == null)
            {
                return NotFound(new BaseResponse(HttpStatusCode.NotFound, $"Клиент c идентификатором : {id} не найден"));
            }
            return Ok(client);
        }

        [HttpPost("/api/clients/{id}/applicationonline")]
        [ProducesResponseType(204)]
        [ProducesResponseType(typeof(BaseResponse), 400)]
        public IActionResult UpdateClient(
            [FromRoute] int id,
            [FromBody] ApplicationOnlineClientBinding binding,
            [FromServices] ClientRepository repository,
            [FromServices] IClientModelValidateService validateService,
            [FromServices] ApplicationOnlineRepository applicationOnlineRepository)
        {
            var client = repository.GetClientForApplicationOnline(id);

            if (client == null)
                return NotFound(new BaseResponse(HttpStatusCode.NotFound, $"Клиент c идентификатором : {id} не найден"));

            var applications = applicationOnlineRepository.GetListOnEstimateByClientId(id);

            if (applications.Any())
                return BadRequest("Нельзя изменять данные клиента пока имеется заявка в статусе \"На оценке\"");

            client.Update(binding.Name, binding.Surname, binding.Patronymic, binding.IdentityNumber, binding.BirthDay, binding.IsMale, binding.EMail, binding.PartnerCode, binding.ReceivesASP);

            var errors = validateService.ValidateFIO(Constants.FullName, Constants.FullName, client.FullName);

            if (errors.Any())
            {
                return BadRequest(new BaseResponse(HttpStatusCode.BadRequest, string.Join("\r\n", errors)));
            }

            var clientContacts = new List<ClientContactDto>();

            if (!string.IsNullOrEmpty(binding.EMail))
            {
                var clientEmails = _clientContactService.GetEmailContacts(id);

                if (!clientEmails.Any(x => x.Address == binding.EMail))
                {
                    var contactType = _domainService.GetDomainValue(Constants.DOMAIN_CONTACT_TYPE_CODE, Constants.DOMAIN_VALUE_EMAIL_CODE);

                    clientContacts.Add(new ClientContactDto
                    {
                        Address = binding.EMail,
                        ContactTypeId = contactType.Id,
                        IsActual = true,
                        IsDefault = false,
                    });
                }
            }

            if (!string.IsNullOrEmpty(binding.AdditionalPhone))
            {
                var clientAdditionalPhones = _clientContactService.GetMobilePhoneContacts(id);

                if (clientAdditionalPhones.Any(x => !x.IsDefault && x.Address == binding.AdditionalPhone))
                {
                    var contactType = _domainService.GetDomainValue(Constants.DOMAIN_CONTACT_TYPE_CODE, Constants.DOMAIN_VALUE_MOBILE_PHONE_CODE);

                    clientContacts.Add(new ClientContactDto
                    {
                        Address = binding.AdditionalPhone,
                        ContactTypeId = contactType.Id,
                        IsActual = true,
                        IsDefault = false,
                    });
                }
            }

            using (var transaction = repository.BeginTransaction())
            {
                if (clientContacts.Any())
                {
                    try
                    {
                        _clientContactService.ValidateDtoContacts(id, clientContacts);
                        _clientContactService.SaveMerchant(id, clientContacts);
                    }
                    catch (PawnshopApplicationException exception)
                    {
                        return BadRequest(new BaseResponse(HttpStatusCode.BadRequest, string.Join("\r\n", exception.Messages)));
                    }
                    catch (Exception ex)
                    {
                        return BadRequest(ex.Message);
                    }
                }

                repository.Update(client);

                transaction.Commit();
            }

            return NoContent();
        }


        [AllowAnonymous]
        [HttpGet("/api/online/information/clients/{id}/contracts")]
        [ProducesResponseType(typeof(BaseListView<ContractOnlineInfoView>),200)]
        [ProducesResponseType(typeof(BaseResponseDRPP),404)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> GetInfoAboutContracts(
            [FromRoute]int id,
            [FromQuery] PageBinding pageBinding,
            [FromQuery] ClientContractsOnlineQuery query,
            [FromServices] ContractRepository contractRepository,
            [FromServices] LegalCaseContractsStatusRepository legalCaseContractsStatusRepository,
            [FromServices] ContractExpenseRepository expenseRepository,
            [FromServices] ContractPositionRepository contractPositionRepository,
            [FromServices] InsurancePolicyRepository insurancePolicyRepository)
        {
            var client = await _repository.GetOnlyClientAsync(id);
            if (client == null)
                new BaseResponseDRPP(HttpStatusCode.NotFound, "Клиент не найден",
                    DRPPResponseStatusCode.ClientNotFound);

            var baseContractsList = await contractRepository
                .GetBaseContractInfo(limit: pageBinding.Limit, 
                offset: pageBinding.Offset,
                clientId:id,
                isActive: query.IsActive,
                contractClass: query.ContractClass,
                contractId: query.ContractId,
                creditLineId: query.CreditLineId);

            var baseContractInfo = baseContractsList.List;
            List<ContractOnlineInfoView> response = new List<ContractOnlineInfoView>();

            var baseContractInfoOnlineViews = baseContractInfo.ToList();
            var creditLineInfos =
                 contractRepository.GetCreditLineInfos(baseContractInfoOnlineViews.Select(contract => contract.Id).ToList());

            var expenses =
                 expenseRepository.GetContractExpenseInfoForOnline(baseContractInfoOnlineViews.Select(contract => contract.Id)
                    .ToList());

            var carInfos =  contractPositionRepository.GetCarOnlineInfo(baseContractInfoOnlineViews
                .Select(contract => contract.Id)
                .ToList());

            var realties = 
                contractPositionRepository.GetRealtiesOnlineInfo(baseContractInfo.Select(contract => contract.Id)
                    .ToList());

            var insurance = insurancePolicyRepository.GetInsurancePoliciesOnlineInfo(baseContractInfo.Select(contract => contract.Id)
                .ToList());

            await Task.WhenAll(creditLineInfos, expenses, carInfos, realties, insurance);

             foreach (var contract in baseContractInfoOnlineViews.Where(contract => query.ContractId == null || contract.Id == query.ContractId.Value))
             {
                 response.Add( new ContractOnlineInfoView(baseContractInfo: contract,
                     creditLineLimitInfo: creditLineInfos.Result.FirstOrDefault(c => c.ContractId == contract.Id),
                     contractExpenseOnlineInfo: expenses.Result.FirstOrDefault(c => c.ContractId == contract.Id),
                     carInfo: carInfos.Result.FirstOrDefault(c => c.ContractId == contract.Id),
                     realtyInfo: realties.Result.Where(c => c.ContractId == contract.Id),
                     insuranceInfo: insurance.Result.FirstOrDefault(c => c.ContractId == contract.Id) ));
             }

            return Ok(new BaseListView<ContractOnlineInfoView>
            {
                Limit = baseContractsList.Limit,
                Count = baseContractsList.Count,
                List = response,
                Offset = baseContractsList.Offset
            });
        }

    }
}
