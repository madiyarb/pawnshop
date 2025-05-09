using Pawnshop.Core.Exceptions;
using Pawnshop.Core.Queries;
using Pawnshop.Data.Access;
using Pawnshop.Data.Models.Clients;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Threading;
using ExpressMapper;
using Pawnshop.Services.SUSN;
using Pawnshop.Core;
using Pawnshop.Data.Models.Auction.Dtos.Client;
using Pawnshop.Data.Models.Contracts;
using Pawnshop.Data.Models.Dictionaries;
using Serilog;

namespace Pawnshop.Services.Clients
{
    public class ClientService : IClientService
    {
        private readonly ClientRepository _clientRepository;
        private readonly ClientProfileRepository _clientProfileRepository;
        private readonly ITasLabSUSNService _susnService;
        private readonly SUSNRequestsRepository _susnRequestsRepository;
        private readonly ClientSUSNStatusesRepository _clientSusnStatusesRepository;
        private readonly ContractLoanSubjectRepository _contractLoanSubjectRepository;
        private readonly ILogger _logger;
        private readonly ClientLegalFormRepository _legalFormRepository;
        private readonly LoanSubjectRepository _loanSubjectRepository;
        private readonly IClientModelValidateService _clientModelValidateService;
        private readonly ClientDocumentTypeRepository _clientDocumentTypeRepository;
        private readonly ClientDocumentProviderRepository _documentProviderRepository;
        private readonly UserRepository _userRepository;
        private readonly CountryRepository _countryRepository;

        public ClientService(
            ClientRepository clientRepository,
            ClientProfileRepository clientProfileRepository,
            ITasLabSUSNService susnService,
            SUSNRequestsRepository susnRequestsRepository,
            ClientSUSNStatusesRepository clientSusnStatusesRepository,
            ClientLegalFormRepository legalFormRepository,
            LoanSubjectRepository loanSubjectRepository,
            IClientModelValidateService clientModelValidateService,
            ClientDocumentTypeRepository clientDocumentTypeRepository,
            UserRepository userRepository, ClientDocumentProviderRepository documentProviderRepository,
            ContractLoanSubjectRepository contractLoanSubjectRepository,
            CountryRepository countryRepository,
            ILogger logger)
        {
            _clientRepository = clientRepository;
            _clientProfileRepository = clientProfileRepository;
            _susnService = susnService;
            _susnRequestsRepository = susnRequestsRepository;
            _clientSusnStatusesRepository = clientSusnStatusesRepository;
            _logger = logger;
            _legalFormRepository = legalFormRepository;
            _loanSubjectRepository = loanSubjectRepository;
            _contractLoanSubjectRepository = contractLoanSubjectRepository;
            _clientModelValidateService = clientModelValidateService;
            _clientDocumentTypeRepository = clientDocumentTypeRepository;
            _userRepository = userRepository;
            _documentProviderRepository = documentProviderRepository;
            _countryRepository = countryRepository;
        }

        public void CheckClientExists(int clientId)
        {
            var client = _clientRepository.GetOnlyClient(clientId);
            if (client == null)
                throw new PawnshopApplicationException("Клиент не найден");
        }

        public Client Get(int clientId)
        {
            var client = _clientRepository.Get(clientId);
            if (client == null)
                throw new PawnshopApplicationException($"Клиент с Id {clientId} не найден");

            return client;
        }

        public Client GetOnlyClient(int clientId)
        {
            return _clientRepository.GetOnlyClient(clientId);
        }

        public async Task<Client> GetOnlyClientAsync(int clientId)
        {
            return await _clientRepository.GetOnlyClientAsync(clientId);
        }

        // Для сервиса Аукцион
        public AuctionClientDto GetByIin(string clientIin)
        {
            var foundClient = _clientRepository.FindByIdentityNumber(clientIin);
            if (foundClient is null)
            {
                return null;
            }
            
            return Mapper.Map<Client, AuctionClientDto>(foundClient);
        }

        public ClientProfile GetClientProfile(int clientId)
        {
            var client = Get(clientId);
            var clientProfile = _clientProfileRepository.Get(client.Id);

            return clientProfile;
        }

        public void Save(Client entity)
        {
            if (entity.Id != 0)
                _clientRepository.Update(entity);
            else
                _clientRepository.Insert(entity);
        }
        
        public async Task SaveAsync(Client entity)
        {
            if (entity.Id != 0)
                _clientRepository.Update(entity);
            else
                _clientRepository.Insert(entity);
        }

        public Client GetBankByName(string bankName) =>
            _clientRepository.List(new ListQuery() { Page = null }, new { IsBank = true })?.FirstOrDefault(x => x.FullName.Equals(bankName));

        public int GetClientAge(Client client)
        {
            if (client.BirthDay.HasValue)
            {
                var birthDate = client.BirthDay.Value;
                var dateTimeToday = DateTime.Now;
                var difference = dateTimeToday.Subtract(birthDate);
                var firstDay = new DateTime(1, 1, 1);
                return (firstDay + difference).Year - 1;
            }
            return 0;
        }

        private async Task<bool> IsClientHasASPStatus(Client client)
        {
            try
            {
                var susnRequest = await _susnRequestsRepository.GetLastRequestByClientId(client.Id);
                if (susnRequest != null && susnRequest.CreateDate.Date == DateTime.Now.Date)
                {
                    var susnStatuses = await _clientSusnStatusesRepository.GetStatusesView(client.Id, susnRequest.Id);
                    if (susnStatuses != null && susnStatuses.AnySUSNStatus && susnStatuses.List.Any(status => status.Code == Constants.SUSN_ASP_STATUS_CODE))
                    {
                        return true;
                    }
                }
                else
                {
                    await _susnService.GetSUSNStatus(client.IdentityNumber, client.Id, CancellationToken.None);
                    susnRequest = await _susnRequestsRepository.GetLastRequestByClientId(client.Id);
                    if (susnRequest != null)
                    {
                        var susnStatuses = await _clientSusnStatusesRepository.GetStatusesView(client.Id, susnRequest.Id);
                        if (susnStatuses != null && susnStatuses.AnySUSNStatus && susnStatuses.List.Any(status => status.Code == Constants.SUSN_ASP_STATUS_CODE))
                        {
                            return true;
                        }
                    }
                }
            }
            catch (Exception exception)
            {
                _logger.Error(exception, exception.Message);
                var susnRequest = await _susnRequestsRepository.GetLastRequestByClientId(client.Id);
                if (susnRequest != null)
                {
                    var susnStatuses = await _clientSusnStatusesRepository.GetStatusesView(client.Id, susnRequest.Id);
                    if (susnStatuses != null && susnStatuses.AnySUSNStatus && susnStatuses.List.Any(status => status.Code == Constants.SUSN_ASP_STATUS_CODE))
                    {
                        return true;
                    }
                }
                return false;
            }

            return false;
        }

        public Client SetASPStatus(int clientId)
        {
            var client = Get(clientId);

            var susnResult = IsClientHasASPStatus(client).Result;
            client.ReceivesASP = susnResult;
            Save(client);
            return client;
        }

        public async Task<int> GetClientIdAsync(string iin)
        {
            var client = await _clientRepository.GetByIdentityNumberAsync(iin);
            if (client == null)
                return 0;

            return client.Id;
        }

        /// <summary>
        /// Создание клиента с неполными данными
        /// </summary>
        /// <returns>Идентфикатор созданного клиента</returns>
        public async Task<int> CreateSimpleClientAsync(CreateSimpleClientCommand command)
        {
            var existingClient = await _clientRepository.GetByIdentityNumberAsync(command.IIN);
            if (existingClient != null)
            {
                throw new PawnshopApplicationException("Клиент уже существет");
            }
            
            var legalForm = await _legalFormRepository.GetByCode(command.LegalFormCode);
            if (legalForm is null)
            {
                throw new ArgumentException("Правовая форма клиента не найдена!");
            }
            
            var loanSubject = await _loanSubjectRepository.GetByCodeAsync(command.LoanSubjectCode);
            if (loanSubject is null)
            {
                throw new ArgumentException("Субъект не найден!");
            }

            var documentType = await _clientDocumentTypeRepository.GetByCode(command.ClientDocumentTypeCode);
            if (documentType is null)
            {
                throw new PawnshopApplicationException("Тип документа не найден!");
            }

            var documentProvider = await _documentProviderRepository.GetByCode(command.DocumentProviderCode);
            if (documentProvider is null)
            {
                throw new PawnshopApplicationException("ClientDocumentProvider не найден!");
            }

            var user = await _userRepository.GetAsync(command.AuthorId);
            if (user is null)
            {
                throw new PawnshopApplicationException("Пользователь не найден!");
            }

            var fullName = $"{command.Surname} {command.Name}";
            if (!string.IsNullOrEmpty(command.Patronymic))
            {
                fullName = $"{fullName} {command.Patronymic}";
            }

            var citizenship = _countryRepository.Get(command.CitizenshipId);
            if (citizenship is null)
            {
                throw new PawnshopApplicationException("Гражданство не найдено!");
            }
            
            var document = new ClientDocument
            {
                Number = command.DocumentNumber,
                ProviderId = documentProvider.Id,
                ProviderName = documentProvider.Name,
                Date = command.DocumentIssueDate.Date,
                DateExpire = command.DocumentExpireDate.Date,
                AuthorId = user.Id,
                Author = user,
                BirthPlace = command.BirthPlace,
                CreateDate = DateTime.Now,
                TypeId = documentType.Id,
                DocumentType = new ClientDocumentType
                {
                    Id = documentType.Id,
                    Name = documentType.Name,
                    NameKaz = documentType.NameKaz,
                    Code = documentType.Code,
                    Disabled = documentType.Disabled,
                    IsIndividual = documentType.IsIndividual,
                    HasSeries = documentType.HasSeries,
                    NumberPlaceholder = documentType.NumberPlaceholder,
                    SeriesPlaceholder = documentType.SeriesPlaceholder,
                    ProviderPlaceholder = documentType.ProviderPlaceholder,
                    BirthPlacePlaceholder = documentType.BirthPlacePlaceholder,
                    DatePlaceholder = documentType.DatePlaceholder,
                    DateExpirePlaceholder = documentType.DateExpirePlaceholder,
                    NumberMask = documentType.NumberMask,
                    NumberMaskError = documentType.NumberMaskError,
                    CBId = documentType.CBId
                }
            };
            
            var client = new Client
            {
                AuthorId = command.AuthorId,
                LegalFormId = legalForm.Id,
                CreateDate = DateTime.Now,
                Name = command.Name,
                Surname = command.Surname,
                FullName = fullName,
                Patronymic = command.Patronymic,
                IdentityNumber = command.IIN,
                IsResident = command.IsResident,
                IsSeller = command.IsSeller,
                CitizenshipId = command.CitizenshipId,
                Citizenship = citizenship,
                BeneficiaryCode = command.BeneficiaryCode,
                BirthDay = command.BirthDay.Date,
                IsMale = command.IsMale,
                MobilePhoneList = new List<ClientContact> {command.Contact},
                Documents = new List<ClientDocument> {document},
                LegalForm = new ClientLegalForm
                {
                    Id = legalForm.Id,
                    Code = legalForm.Code,
                    Name = legalForm.Name,
                    NameKaz = legalForm.NameKaz,
                    Abbreviation = legalForm.Abbreviation,
                    AbbreviationKaz = legalForm.AbbreviationKaz,
                    IsIndividual = legalForm.IsIndividual,
                    CBId = legalForm.CBId,
                    HasIINValidation = legalForm.HasIINValidation,
                    HasBirthDayValidation = legalForm.HasBirthDayValidation
                }
            };

            _clientModelValidateService.ValidateMerchantClientModel(client);
            
            using (var transaction = _clientRepository.BeginTransaction())
            {
                _clientRepository.Insert(client);
                var contractLoanSubject = new ContractLoanSubject
                {
                    SubjectId = loanSubject.Id,
                    ContractId = command.ContractId,
                    ClientId = client.Id,
                    AuthorId = client.AuthorId,
                    CreateDate = DateTime.Now
                };
                
                await _contractLoanSubjectRepository.InsertAsync(contractLoanSubject);
            
                transaction.Commit();
            }

            return client.Id;
        }
    }
}
