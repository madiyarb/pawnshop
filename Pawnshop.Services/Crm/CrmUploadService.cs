using Newtonsoft.Json;
using Pawnshop.Data.Access;
using System;
using Pawnshop.Data.Models.Contracts;
using System.Dynamic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Pawnshop.Core;
using Pawnshop.Core.Exceptions;
using Pawnshop.Data.Models.Clients;
using Pawnshop.Data.Models.Crm;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Linq;
using Pawnshop.Core.Queries;
using Pawnshop.Data.Models.Domains;
using Pawnshop.Services.Domains;
using Pawnshop.Data.Models.Contracts.Actions;
using Pawnshop.Core.Options;
using Microsoft.Extensions.Options;
using Pawnshop.Data.Models.Notifications;
using Pawnshop.Services.Contracts;
using System.Net.Http.Headers;
using Serilog;

namespace Pawnshop.Services.Crm
{
    public class CrmUploadService : ICrmUploadService
    {
        private readonly HttpClient _httpClient;
        private readonly OuterServiceSettingRepository _outerServiceSettings;
        private readonly ClientContactRepository _clientContactRepository;
        private readonly IDomainService _domainService;

        private readonly CrmUploadContractRepository _crmUploadContractRepository;
        private readonly ContractRepository _contractRepository;
        private readonly GroupRepository _groupRepository;
        private readonly OrganizationRepository _organizationRepository;
        private readonly ClientRepository _clientRepository;
        private readonly EnviromentAccessOptions _options;
        private readonly AttractionChannelRepository _attractionChannelRepository;
        private readonly ContractActionRepository _contractActionRepository;
        private readonly NotificationRepository _notificationRepository;

        private readonly CrmPaymentRepository _crmPaymentRepository;
        private readonly IContractService _contractService;
        private readonly ContractPaymentScheduleRepository _contractPaymentScheduleRepository;
        private readonly ContractTransferRepository _contractTransferRepository;
        private readonly InscriptionRepository _inscriptionRepository;
        private readonly CollectionStatusRepository _collectionStatusRepository;
        private readonly CrmStatusesRepository _crmStatusesRepository;

        private readonly Regex _regex = new Regex("[^0-9+7]");

        private readonly ILogger _logger;

        public CrmUploadService(
            HttpClient httpClient,
            OuterServiceSettingRepository outerServiceSettings,
            ClientContactRepository clientContactRepository,
            IDomainService domainService,

            CrmUploadContractRepository crmUploadContractRepository,
            ContractRepository contractRepository,
            GroupRepository groupRepository,
            OrganizationRepository organizationRepository,
            ClientRepository clientRepository,
            IOptions<EnviromentAccessOptions> options,
            AttractionChannelRepository attractionChannelRepository,
            ContractActionRepository contractActionRepository,
            NotificationRepository notificationRepository,

            CrmPaymentRepository crmPaymentRepository,
            IContractService contractService,
            ContractPaymentScheduleRepository contractPaymentScheduleRepository,
            ContractTransferRepository contractTransferRepository,
            InscriptionRepository inscriptionRepository,
            CollectionStatusRepository collectionStatusRepository,
            CrmStatusesRepository crmStatusesRepository,
            ILogger logger)
        {
            _httpClient = httpClient;
            _httpClient.DefaultRequestHeaders.Accept.Clear();
            _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            _httpClient.Timeout = TimeSpan.FromSeconds((double)options.Value.BitrixHttpTimeoutSeconds);
            _outerServiceSettings = outerServiceSettings;
            _clientContactRepository = clientContactRepository;
            _domainService = domainService;

            _crmUploadContractRepository = crmUploadContractRepository;
            _contractRepository = contractRepository;
            _groupRepository = groupRepository;
            _organizationRepository = organizationRepository;
            _clientRepository = clientRepository;
            _options = options.Value;
            _attractionChannelRepository = attractionChannelRepository;
            _contractActionRepository = contractActionRepository;
            _notificationRepository = notificationRepository;

            _crmPaymentRepository = crmPaymentRepository;
            _contractService = contractService;
            _contractPaymentScheduleRepository = contractPaymentScheduleRepository;
            _contractTransferRepository = contractTransferRepository;
            _inscriptionRepository = inscriptionRepository;
            _collectionStatusRepository = collectionStatusRepository;
            _crmStatusesRepository = crmStatusesRepository;
            _logger = logger;
        }

        public async Task<dynamic> BitrixAPI(string url, object filter = null, object select = null, int? objectId = null, object fields = null)
        {
            dynamic rooter = new ExpandoObject();

            if (objectId != null)
                rooter.id = objectId;
            if (fields != null)
                rooter.fields = fields;
            if (filter != null)
                rooter.filter = filter;
            if (select != null)
            {
                rooter.order = new ExpandoObject();
                rooter.order.ID = "DESC";
                rooter.select = select;
            }

            var json = JsonConvert.SerializeObject(rooter);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            _logger.Warning($"Bitrix request. URL: {url}, BODY: {json}");
            var response = await _httpClient.PostAsync(url, content);
            var responseString = await response.Content.ReadAsStringAsync();
            _logger.Warning($"Bitrix response: StatusCode - {response.StatusCode}, ResponseContent - {responseString}");
            if (!response.IsSuccessStatusCode)
            {
                var bitrixError = JsonConvert.DeserializeObject<BitrixError>(responseString);
                if(bitrixError != null && (bitrixError.error_description.Equals("Not found", StringComparison.InvariantCultureIgnoreCase)||
                    bitrixError.error_description.Equals("Contact is not found", StringComparison.InvariantCultureIgnoreCase)))
                {
                    return new { result = false };
                }
                else
                    throw new PawnshopApplicationException($"BitrixAPI. Request:{json}, ResponseCode: {response.StatusCode}, Response: {responseString}");
            }
            return JsonConvert.DeserializeObject<dynamic>(responseString);
        }

        public async Task<List<CrmContact>> SearchContactInCRM(Client client)
        {
            ClientContact defaultContact = _clientContactRepository.Find(new { IsDefault = true, ClientId = client.Id });
            if (string.IsNullOrEmpty(defaultContact?.Address))
                throw new PawnshopApplicationException($"Основной телефон не найден у клиента {client.Id}");

            var crmContacts = new List<CrmContact>();
            dynamic rooter = new ExpandoObject();
            rooter.filter = new ExpandoObject();
            rooter.select = new[] { "ID", "NAME", "SECOND_NAME", "LAST_NAME", "UF_CRM_1554290627253" };

            rooter.filter.UF_CRM_1554290627253 = client.IdentityNumber.Trim();
            rooter.filter.PHONE = defaultContact.Address;
            var url = _outerServiceSettings.Find(new { Code = "CRM_CONTACT_LIST" }).URL;
            var result = await BitrixAPI(url, filter: rooter.filter, select: rooter.select);

            if (result.total > 0)
            {
                foreach (var con in result.result)
                {
                    if (crmContacts.Count == 0 || crmContacts.Find(x => x.Id == (int)con.ID) == null)
                    {
                        crmContacts.Add(new CrmContact()
                        {
                            Id = con.ID,
                            Name = con.NAME,
                            SecondName = con.SECOND_NAME,
                            LastName = con.LAST_NAME,
                            IdentityNumber = con.UF_CRM_1554290627253
                        });
                    }
                }
            }

            return crmContacts;
        }

        public async Task<CrmContact> CreateContactInCRM(Client client)
        {
            CrmContact crmContact = new CrmContact();
            CrmUploadContact contact = new CrmUploadContact();
            List<ClientContact> clientMobilePhoneNumbers = GetMobilePhoneContacts(client.Id);
            List<ClientContact> clientHomePhoneNumbers = GetHomePhoneContacts(client.Id);
            List<ClientContact> clientWorkPhoneNumbers = GetWorkPhoneContacts(client.Id);

            contact.Fields.LAST_NAME = client.LegalForm.IsIndividual ? client.Surname : client.Chief.Surname;
            contact.Fields.NAME = client.LegalForm.IsIndividual ? client.Name : client.Chief.Name;
            contact.Fields.SECOND_NAME = client.LegalForm.IsIndividual ? client.Patronymic : client.Chief.Patronymic;
            contact.Fields.ADDRESS = client.Addresses?.FirstOrDefault()?.FullPathRus ?? "Адрес не найден";
            contact.Fields.ADDRESS_2 = client.Addresses?.FirstOrDefault()?.FullPathRus ?? "Адрес не найден";
            contact.Fields.UF_CRM_1554290627253 = client.IdentityNumber;
            contact.Fields.PHONE = new List<CrmMultifieldItem>();

            contact.Fields.PHONE.AddRange(clientMobilePhoneNumbers.Select(c => new CrmMultifieldItem
            {
                Value = c.Address,
                ValueType = "MOBILE"
            }));
            contact.Fields.PHONE.AddRange(clientHomePhoneNumbers.Select(c => new CrmMultifieldItem
            {
                Value = c.Address,
                ValueType = "HOME"
            }));
            contact.Fields.PHONE.AddRange(clientWorkPhoneNumbers.Select(c => new CrmMultifieldItem
            {
                Value = c.Address,
                ValueType = "WORK"
            }));
            contact.Fields.UF_CRM_1589434063619 = client.Id.ToString();
            
            var result = await BitrixAPI(_outerServiceSettings.Find(new { Code = "CRM_CONTACT_ADD" }).URL, fields: contact.Fields);
            crmContact.Id = (int)result.result;
            return crmContact;
        }

        public async Task<bool> UpdateContactsInCrm(int сrmId, Client client)
        {
            CrmUploadContact _contact = new CrmUploadContact();
            _contact.Fields.LAST_NAME = client.LegalForm.IsIndividual ? client.Surname : client.Chief.Surname;
            _contact.Fields.NAME = client.LegalForm.IsIndividual ? client.Name : client.Chief.Name;
            _contact.Fields.SECOND_NAME = client.LegalForm.IsIndividual ? client.Patronymic : client.Chief.Patronymic;
            _contact.Fields.ADDRESS = client.Addresses?.FirstOrDefault(x => x.AddressType.Code.Contains("RESIDENCE") && x.IsActual)?.FullPathRus ?? "Адрес не найден";
            _contact.Fields.ADDRESS_2 = client.Addresses?.FirstOrDefault(x => x.AddressType.Code.Contains("REGISTRATION") && x.IsActual)?.FullPathRus ?? "Адрес не найден";
            _contact.Fields.UF_CRM_1554290627253 = client.IdentityNumber;
            _contact.Fields.UF_CRM_1589434063619 = client.Id.ToString();

            var response = await BitrixAPI(_outerServiceSettings.Find(new { Code = "CRM_CONTACT_UPDATE" }).URL, objectId: сrmId, fields: _contact.Fields);
            return (bool)response.result;
        }

        public async Task<int> CreateCrmContract(int clientCrmId, Contract contract)
        {
            var status = _crmStatusesRepository.FindStatus(new { CrmName = "WON", StatusTypeId = _crmStatusesRepository.FindStatusType(new { code = "Contract" }).Id }); 
            DateTime signDate = new DateTime(2000, 01, 01);
            var _contractAction = _contractActionRepository.GetLastContractActionByType(contract.Id, ContractActionType.Sign);
            if (_contractAction != null && _contractAction.CreateDate != null)
                signDate = _contractAction.CreateDate;
            CrmContractToUpload contractToUpload = new CrmContractToUpload()
            {
                Title = $"Номер договора {contract.ContractNumber}",
                CategoryId = contract.Branch.BitrixCategoryId.ToString(),
                ClientCrmId = clientCrmId.ToString(),
                Stage = status.GetCrmStage(contract.Branch.BitrixCategoryId),
                ContractId = contract.Id.ToString(),
                LoanCost = contract.LoanCost,
                SignDate = signDate.ToString("dd.MM.yyyy HH:mm:ss"),
                AttractionChannel = _attractionChannelRepository.Get(contract.AttractionChannelId.Value).Name
            };
            
            var result = await BitrixAPI(_outerServiceSettings.Find(new { Code = "CRM_DEAL_ADD" }).URL, fields: contractToUpload);
            _contractRepository.UpdateCrmInfo(contract.Id, (int)result.result);
            return (int)result.result;
        }

        public async Task UpdateCrmContract(Contract contract, CrmContract deal, int? bitrixId = null)
        {
            var status = _crmStatusesRepository.FindStatus(new { CrmName = "WON", StatusTypeId = _crmStatusesRepository.FindStatusType(new { code = "Contract" }).Id });
            DateTime signDate = new DateTime(2000, 01, 01);
            var _contractAction = _contractActionRepository.GetLastContractActionByType(contract.Id, ContractActionType.Sign);
            if (_contractAction != null && _contractAction.CreateDate != null)
                signDate = _contractAction.CreateDate;
            CrmContractToUpload contractToUpload = new CrmContractToUpload()
            {
                Title = $"Номер договора {contract.ContractNumber}",
                CategoryId = contract.Branch.BitrixCategoryId.ToString(),
                ClientCrmId = deal.ContactId.ToString(),
                Stage = status.GetCrmStage(deal.CategoryId),
                ContractId = contract.Id.ToString(),
                LoanCost = contract.LoanCost,
                SignDate = signDate.ToString("dd.MM.yyyy HH:mm:ss"),
                AttractionChannel = _attractionChannelRepository.Get(contract.AttractionChannelId.Value).Name,
                BitrixId = bitrixId.ToString()
            };
            
            await BitrixAPI(_outerServiceSettings.Find(new { Code = "CRM_DEAL_UPDATE" }).URL, objectId: deal.Id, fields: contractToUpload);
            _contractRepository.UpdateCrmInfo(contract.Id, deal.Id);
        }

        public async Task<bool> UpdateDeal(Contract contract, decimal loanCostLeft, decimal loanPercentCost, decimal penaltyPercentCost, decimal prepayment, decimal buyoutAmount, decimal prolongAmount)
        {
            ClientContact defaultContact = _clientContactRepository.Find(new { IsDefault = true, ClientId = contract.ClientId });
            List<Notification> notifications = _notificationRepository.List(new Core.Queries.ListQuery { Page = null }, new { BranchId = contract.BranchId, ClientId = contract.ClientId, MessageType = MessageType.Sms });
            var collectionStatus = _collectionStatusRepository.GetByContractId(contract.Id);

            CrmPaymentUpdate fields = new CrmPaymentUpdate(
                contract: contract,
                categoryId: Constants.BITRIX_CATEGORY,
                notifications: notifications,
                defaultContact: defaultContact?.Address,
                loanCostLeft: loanCostLeft,
                loanPercentCost: loanPercentCost,
                penaltyPercentCost: penaltyPercentCost,
                prepayment: prepayment,
                buyoutAmount: buyoutAmount,
                prolongAmount: prolongAmount,
                overdueContracts: _collectionStatusRepository.GetOverdueContractsByClientId(contract.ClientId).Count,
                collectionStatus: collectionStatus,
                _outerServiceSettings.Find(new { Code = Constants.FINCORE_URL }).URL);

            var responce = await BitrixAPI(_outerServiceSettings.Find(new { Code = "CRM_DEAL_UPDATE" }).URL, objectId: fields.CrmPaymentId, fields: fields);
            return (bool)responce.result;
        }
        
        public async Task CreateDeal(Contract contract, decimal loanCostLeft, decimal loanPercentCost, decimal penaltyPercentCost, decimal prepayment, decimal buyoutAmount, decimal prolongAmount)
        {
            ClientContact defaultContact = _clientContactRepository.Find(new { IsDefault = true, ClientId = contract.ClientId });
            var collectionStatus = _collectionStatusRepository.GetByContractId(contract.Id);
            bool isWon = contract.BuyoutDate.HasValue;

            CrmPaymentCreate fields = new CrmPaymentCreate(
                contract: contract,
                categoryId: Constants.BITRIX_CATEGORY,
                defaultContact: defaultContact?.Address,
                loanCostLeft: loanCostLeft,
                loanPercentCost: loanPercentCost,
                penaltyPercentCost: penaltyPercentCost,
                prepayment: prepayment,
                buyoutAmount: buyoutAmount,
                prolongAmount: prolongAmount,
                overdueContracts: _collectionStatusRepository.GetOverdueContractsByClientId(contract.ClientId).Count,
                collectionStatus: collectionStatus,
                currentStatus: _crmStatusesRepository.FindStatus(new { CrmName = isWon? "WON" : "NEW", StatusTypeId = _crmStatusesRepository.FindStatusType(new { Code = "Payment" }).Id }),
                _outerServiceSettings.Find(new { Code = Constants.FINCORE_URL }).URL
            );

            var bitrixResult = await BitrixAPI(_outerServiceSettings.Find(new { Code = "CRM_DEAL_ADD" }).URL, fields: fields);
            int crmPaymentId = 0;
            try
            {
                crmPaymentId = bitrixResult.result;
            }
            catch
            {
                throw new PawnshopApplicationException($"BitrixCreateDeal. Request:{JsonConvert.SerializeObject(fields)}, Response: {bitrixResult}");
            }

            _contractRepository.UpdateCrmPaymentInfo(contract.Id, crmPaymentId);
        }

        private List<ClientContact> GetMobilePhoneContacts(int clientId)
        {
            return GetList(clientId, Constants.DOMAIN_VALUE_MOBILE_PHONE_CODE);
        }

        private List<ClientContact> GetWorkPhoneContacts(int clientId)
        {
            return GetList(clientId, Constants.DOMAIN_VALUE_WORK_PHONE_CODE);
        }

        private List<ClientContact> GetHomePhoneContacts(int clientId)
        {
            return GetList(clientId, Constants.DOMAIN_VALUE_HOME_PHONE_CODE);
        }

        private List<ClientContact> GetList(int clientId, string code = null)
        {
            Domain contactTypeDomain = _domainService.GetDomain(Constants.DOMAIN_CONTACT_TYPE_CODE);
            DomainValue domainValue = null;
            if (!string.IsNullOrWhiteSpace(code))
                domainValue = _domainService.GetDomainValue(contactTypeDomain.Code, code);

            List<ClientContact> clientContacts = _clientContactRepository.List(new ListQuery(), new { ClientId = clientId, ContactTypeId = domainValue?.Id });
            clientContacts = clientContacts.OrderByDescending(x => x.IsDefault).ThenBy(x => x.CreateDate).ToList();
            return clientContacts;
        }

        public async Task<int> CreateOrUpdateContactInCrm(Client client)
        {
            dynamic rooter = new ExpandoObject();
            rooter.filter = new ExpandoObject();
            ClientContact defaultContact = _clientContactRepository.Find(new { IsDefault = true, ClientId = client.Id });
            if (defaultContact != null) {
                rooter.filter.PHONE = defaultContact.Address;
            }
            rooter.filter.LAST_NAME = client.LegalForm.IsIndividual ? client.Surname : client.Chief.Surname;
            rooter.filter.NAME = client.LegalForm.IsIndividual ? client.Name : client.Chief.Name;
            rooter.filter.SECOND_NAME = client.LegalForm.IsIndividual ? client.Patronymic : client.Chief.Patronymic;
            rooter.filter.ADDRESS = client.Addresses?.FirstOrDefault(x => x.AddressType.Code.Contains("RESIDENCE") && x.IsActual)?.FullPathRus ?? "Адрес не найден";
            rooter.filter.ADDRESS_2 = client.Addresses?.FirstOrDefault(x => x.AddressType.Code.Contains("REGISTRATION") && x.IsActual)?.FullPathRus ?? "Адрес не найден";
            rooter.filter.UF_CRM_1554290627253 = client.IdentityNumber.Trim();
            rooter.filter.UF_CRM_1589434063619 = client.Id.ToString();

            var response = await BitrixAPI(_outerServiceSettings.Find(new { Code = "CRM_CONTACT_ADDORUPDATE" }).URL, filter: rooter.filter);
            return (int)response;
        }
    }

    public class BitrixError
    {
        public string error { get; set; }
        public string error_description { get; set; }
    }
}
