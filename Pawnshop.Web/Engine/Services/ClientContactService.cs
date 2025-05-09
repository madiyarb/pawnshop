using Newtonsoft.Json;
using Pawnshop.Core.Exceptions;
using Pawnshop.Core.Queries;
using Pawnshop.Core.Utilities;
using Pawnshop.Core;
using Pawnshop.Data.Access;
using Pawnshop.Data.Models.Audit;
using Pawnshop.Data.Models.Clients;
using Pawnshop.Data.Models.Domains;
using Pawnshop.Web.Engine.Audit;
using Pawnshop.Web.Engine.Services.Interfaces;
using Pawnshop.Web.Models.Clients;
using Pawnshop.Web.Models.Domains.AdditionalData;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System;

namespace Pawnshop.Web.Engine.Services
{
    public class ClientContactService : IClientContactService
    {
        private readonly ClientRepository _clientRepository;
        private readonly ClientContactRepository _clientContactRepository;
        private readonly EventLog _eventLog;
        private readonly IDomainService _domainService;
        private readonly IVerificationService _verificationService;
        private readonly ISessionContext _sessionContext;


        public ClientContactService(ClientRepository clientRepository, ClientContactRepository clientContactRepository,
            EventLog eventLog, IDomainService domainService, IVerificationService verificationService, ISessionContext sessionContext)
        {
            _clientContactRepository = clientContactRepository;
            _clientRepository = clientRepository;
            _eventLog = eventLog;
            _domainService = domainService;
            _verificationService = verificationService;
            _sessionContext = sessionContext;
        }

        public List<ClientContact> GetList(int clientId, string code = null)
        {
            if (clientId <= 0)
                throw new ArgumentNullException(nameof(clientId));

            GetClient(clientId);
            Domain contactTypeDomain = _domainService.GetDomain(Constants.DOMAIN_CONTACT_TYPE_CODE);
            DomainValue domainValue = null;
            if (!string.IsNullOrWhiteSpace(code))
                domainValue = _domainService.GetDomainValue(contactTypeDomain.Code, code);

            List<ClientContact> clientContacts = _clientContactRepository.List(new ListQuery(), new { ClientId = clientId, ContactTypeId = domainValue?.Id });
            clientContacts = clientContacts.OrderByDescending(x => x.IsDefault).ThenBy(x => x.CreateDate).ToList();
            return clientContacts;
        }

        public List<ClientContact> Save(int clientId, List<ClientContactDto> clientContacts, string otp)
        {
            if (clientId <= 0)
                throw new ArgumentNullException(nameof(clientId));

            if (clientContacts == null)
                throw new ArgumentNullException(nameof(clientId));

            Dictionary<int, ClientContact> contactsFromDB = _clientContactRepository.List(new ListQuery(), new { ClientId = clientId }).ToDictionary(c => c.Id, c => c);
            HashSet<int> uniqueContactIds = clientContacts.Where(c => c.Id != default).Select(c => c.Id).ToHashSet();
            if (uniqueContactIds.Except(contactsFromDB.Keys).Any())
                throw new PawnshopApplicationException("Обнаружены Id контактов не принадлежащие к данному клиенту");

            ValidateDtoContacts(clientId, clientContacts);
            var syncedContacts = new List<ClientContact>();
            bool needVerification = false;
            foreach (ClientContactDto clientContact in clientContacts)
            {
                ClientContact contactFromDB = null;
                if (!contactsFromDB.TryGetValue(clientContact.Id, out contactFromDB))
                {
                    contactFromDB = new ClientContact
                    {
                        Address = clientContact.Address,
                        ClientId = clientId,
                        ContactTypeId = clientContact.ContactTypeId.Value,
                        IsDefault = clientContact.IsDefault,
                        AuthorId = _sessionContext.UserId,
                        SendUkassaCheck = clientContact.SendUkassaCheck,
                        ContactCategoryId = clientContact.ContactCategoryId.Value,
                        ContactCategoryCode = clientContact.ContactCategoryCode,
                        SourceId = clientContact.SourceId.Value,
                        Note = clientContact.Note,
                        IsActual = clientContact.IsActual
                    };

                    if (clientContact.IsDefault)
                        needVerification = true;
                }
                else
                {
                    if (CheckContactChangedFromDBModel(clientContact, contactFromDB))
                    {
                        // Если проставляем основной контакт записи не являющимся основным контактом 
                        // или если основной контакт меняется, то требуем верификацию,
                        // то просим верификация
                        if (clientContact.IsDefault && (!contactFromDB.IsDefault
                            || contactFromDB.Address != clientContact.Address))
                            needVerification = true;

                        contactFromDB.Address = clientContact.Address;
                        contactFromDB.ClientId = clientId;
                        contactFromDB.ContactTypeId = clientContact.ContactTypeId.Value;
                        contactFromDB.IsDefault = clientContact.IsDefault;
                        contactFromDB.VerificationExpireDate = null;
                        contactFromDB.SendUkassaCheck = clientContact.SendUkassaCheck;
                        contactFromDB.SourceId = clientContact.SourceId;
                        contactFromDB.ContactCategoryId = clientContact.ContactCategoryId != null ? clientContact.ContactCategoryId.Value : 0;
                        contactFromDB.ContactCategoryCode = clientContact.ContactCategoryCode;
                        contactFromDB.Note = clientContact.Note;
                        contactFromDB.IsActual = clientContact.IsActual;
                        contactFromDB.isChanged = true;
                    }

                    // удаляем из списка контакт, который будет занесен в syncedContacts
                    contactsFromDB.Remove(clientContact.Id);
                }

                syncedContacts.Add(contactFromDB);
            }

            if (needVerification && otp == null)
                throw new PawnshopApplicationException("Не был введен код верификации(OTP)");

            if (contactsFromDB.Count > 0 || syncedContacts.Count > 0)
            {
                if (syncedContacts.Any(x => x.Id == default
                    && (x.ContactCategoryCode == Constants.DOMAIN_VALUE_CONTACT_CONTRACT || x.ContactCategoryCode == string.Empty))
                    && (!_sessionContext.Permissions.Contains(Permissions.ClientManage)))
                    throw new PawnshopApplicationException("У вас нет прав на добавление записи в основных контактах");

                if ((contactsFromDB.Any(x => x.Value.ContactCategoryCode == Constants.DOMAIN_VALUE_CONTACT_CONTRACT)
                    || syncedContacts.Any(x => x.Id != default
                                            && x.isChanged && (x.ContactCategoryCode == Constants.DOMAIN_VALUE_CONTACT_CONTRACT
                                            || x.ContactCategoryCode == string.Empty)))
                    && !_sessionContext.Permissions.Contains(Permissions.ClientManage))
                    throw new PawnshopApplicationException("У вас нет прав на изменения данных в основных контактах");

                if (syncedContacts.Any(x => x.Id == default
                    && x.ContactCategoryCode == Constants.DOMAIN_VALUE_CONTACT_ACTUALIZED)
                    && !_sessionContext.Permissions.Contains(Permissions.ClientContactActualizedAdd))
                    throw new PawnshopApplicationException("У вас нет прав на добавление записи в актуализированных контактах");

                if (((contactsFromDB.Any(x => x.Value.ContactCategoryCode == Constants.DOMAIN_VALUE_CONTACT_ACTUALIZED))
                    || syncedContacts.Any(x => x.Id != default && x.isChanged && x.ContactCategoryCode == Constants.DOMAIN_VALUE_CONTACT_ACTUALIZED))
                    && !_sessionContext.Permissions.Contains(Permissions.ClientContactActualizedManage))
                    throw new PawnshopApplicationException("У вас нет прав на изменения данных в актуализированных контактах");

                using (var transaction = _clientContactRepository.BeginTransaction())
                {
                    // Удаляем ненужные контакты
                    foreach ((int key, ClientContact clientContact) in contactsFromDB)
                    {
                        _clientContactRepository.Delete(key);
                        _eventLog.Log(EventCode.ClientContactDeleted, EventStatus.Success, EntityType.ClientContact, clientContact.Id, JsonConvert.SerializeObject(clientContact));
                    }

                    foreach (ClientContact clientContact in syncedContacts)
                    {
                        if (clientContact.Id == default)
                        {
                            _clientContactRepository.Insert(clientContact);
                            _clientContactRepository.LogChanges(clientContact, _sessionContext.UserId);
                            _eventLog.Log(EventCode.ClientContactCreated, EventStatus.Success, EntityType.ClientContact, clientContact.Id, JsonConvert.SerializeObject(clientContact));
                        }
                        else if (clientContact.isChanged)
                        {
                            _clientContactRepository.Update(clientContact);
                            _clientContactRepository.LogChanges(clientContact, _sessionContext.UserId, true);
                            _eventLog.Log(EventCode.ClientContactUpdated, EventStatus.Success, EntityType.ClientContact, clientContact.Id, JsonConvert.SerializeObject(clientContact));
                        }
                    }

                    if (needVerification)
                        _verificationService.Verify(otp, clientId);

                    transaction.Commit();
                }
            }
            syncedContacts = syncedContacts.OrderByDescending(x => x.IsDefault).ThenBy(x => x.CreateDate).ToList();
            return syncedContacts;
        }

        public List<ClientContact> SaveMerchant(int clientId, List<ClientContactDto> clientContacts)
        {
            if (clientId <= 0)
                throw new ArgumentNullException(nameof(clientId));

            if (clientContacts == null)
                throw new ArgumentNullException(nameof(clientId));

            Dictionary<int, ClientContact> contactsFromDB = _clientContactRepository.List(new ListQuery(), new { ClientId = clientId }).ToDictionary(c => c.Id, c => c);
            HashSet<int> uniqueContactIds = clientContacts.Where(c => c.Id != default).Select(c => c.Id).ToHashSet();
            if (uniqueContactIds.Except(contactsFromDB.Keys).Any())
                throw new PawnshopApplicationException("Обнаружены Id контактов не принадлежащие к данному клиенту");

            ValidateDtoContacts(clientId, clientContacts);
            var syncedContacts = new List<ClientContact>();
            foreach (ClientContactDto clientContact in clientContacts)
            {
                ClientContact contactFromDB = null;
                if (!contactsFromDB.TryGetValue(clientContact.Id, out contactFromDB))
                {
                    contactFromDB = new ClientContact
                    {
                        Address = clientContact.Address,
                        ClientId = clientId,
                        ContactTypeId = clientContact.ContactTypeId.Value,
                        IsDefault = clientContact.IsDefault,
                        AuthorId = _sessionContext.UserId,
                        SendUkassaCheck = clientContact.SendUkassaCheck
                    };
                }
                else
                {
                    if (CheckContactChangedFromDBModel(clientContact, contactFromDB))
                    {
                        contactFromDB.Address = clientContact.Address;
                        contactFromDB.ClientId = clientId;
                        contactFromDB.ContactTypeId = clientContact.ContactTypeId.Value;
                        contactFromDB.IsDefault = clientContact.IsDefault;
                        contactFromDB.VerificationExpireDate = null;
                        contactFromDB.SendUkassaCheck = clientContact.SendUkassaCheck;
                    }
                    // удаляем из списка контакт, который будет занесен в syncedContacts
                    contactsFromDB.Remove(clientContact.Id);
                }

                syncedContacts.Add(contactFromDB);
            }

            if (contactsFromDB.Count > 0 || syncedContacts.Count > 0)
                using (var transaction = _clientContactRepository.BeginTransaction())
                {
                    // Удаляем ненужные контакты
                    foreach ((int key, ClientContact clientContact) in contactsFromDB)
                    {
                        _clientContactRepository.Delete(key);
                        _eventLog.Log(EventCode.ClientContactDeleted, EventStatus.Success, EntityType.ClientContact, clientContact.Id, JsonConvert.SerializeObject(clientContact));
                    }

                    foreach (ClientContact clientContact in syncedContacts)
                    {
                        if (clientContact.Id != default)
                        {
                            _clientContactRepository.Update(clientContact);
                            _clientContactRepository.LogChanges(clientContact, _sessionContext.UserId, true);
                            _eventLog.Log(EventCode.ClientContactUpdated, EventStatus.Success, EntityType.ClientContact, clientContact.Id, JsonConvert.SerializeObject(clientContact));
                        }
                        else
                        {
                            _clientContactRepository.Insert(clientContact);
                            _clientContactRepository.LogChanges(clientContact, _sessionContext.UserId);
                            _eventLog.Log(EventCode.ClientContactCreated, EventStatus.Success, EntityType.ClientContact, clientContact.Id, JsonConvert.SerializeObject(clientContact));
                        }
                    }

                    transaction.Commit();
                }

            return syncedContacts;
        }

        public void ValidateDtoContacts(int clientId, List<ClientContactDto> contacts)
        {
            if (contacts == null)
                throw new ArgumentNullException(nameof(contacts));

            var errors = new HashSet<string>();
            // создадим словари с маппингом code => id
            Dictionary<int, DomainValue> contactTypeDomainValuesDict = _domainService.GetDomainValues(Constants.DOMAIN_CONTACT_TYPE_CODE).ToDictionary(dv => dv.Id, dv => dv);
            Dictionary<int, ContactTypeAdditionalData> contactTypeAdditionalDataDict = ProcessAndValidateContactTypeDomainValues(contactTypeDomainValuesDict.Values);
            // проверим на уникальность
            if (contacts.GroupBy(c => c.Address, StringComparer.InvariantCultureIgnoreCase).Any(c => c.Count() > 1))
                errors.Add("Контакты должны быть уникальными, обнаружены одинаковые контакты");

            // проверим на уникальность Id
            if (contacts.Where(c => c.Id != 0).GroupBy(c => c.Id).Any(c => c.Count() > 1))
                errors.Add("Обнаружены несколько записей с одним Id, обратитесь в тех. поддержку");

            // проверим на null значения в поле ContactTypeId
            bool nullContactTypeExists = contacts.Any(c => !c.ContactTypeId.HasValue);
            if (nullContactTypeExists)
                errors.Add($"Не все контакты имеют заполненный {nameof(ClientContactDto.ContactTypeId)}");

            if (errors.Count > 0)
                throw new PawnshopApplicationException(errors.ToArray());

            // проверим чтобы значения ContactTypeCode были валидными
            HashSet<int> uniqueContactTypesIdsFromContacts = contacts.Select(c => c.ContactTypeId.Value).ToHashSet();
            if (!uniqueContactTypesIdsFromContacts.IsSubsetOf(contactTypeDomainValuesDict.Keys))
                errors.Add($"Не все контакты имеют правильный {nameof(ClientContactDto.ContactTypeId)}");

            var actualDomainValue = _domainService.GetDomainValues(Constants.DOMAIN_CONTACT_CATEGORY).Where(x => x.Code == Constants.DOMAIN_VALUE_CONTACT_ACTUALIZED).FirstOrDefault();
            if (contacts.Where(x => x.ContactCategoryId == actualDomainValue.Id && !x.SourceId.HasValue).Any())
                errors.Add("Не все актуализированные контакты имеют заполненный источник");

            if (errors.Count > 0)
                throw new PawnshopApplicationException(errors.ToArray());

            // проверим количество основных номеров
            List<ClientContactDto> defaultClientContacts = contacts.Where(c => c.IsDefault).ToList();
            if (defaultClientContacts.Count > 0)
            {
                if (defaultClientContacts.Count > 1)
                    errors.Add("Нельзя выбрать более одного основого контакта");
                else
                {
                    ClientContactDto defaultClientContact = defaultClientContacts.Single();
                    DomainValue contactTypeDomainValue = contactTypeDomainValuesDict[defaultClientContact.ContactTypeId.Value];
                    if (contactTypeDomainValue.Code != Constants.DOMAIN_VALUE_MOBILE_PHONE_CODE)
                        errors.Add("Нельзя выбрать основным номером контакт не являющийся номером мобильного телефона");
                }
            }

            if (errors.Count > 0)
                throw new PawnshopApplicationException(errors.ToArray());

            foreach (ClientContactDto contact in contacts)
            {
                DomainValue contactTypeDomainValue = contactTypeDomainValuesDict[contact.ContactTypeId.Value];
                ContactTypeAdditionalData additionalData = null;
                if (contactTypeAdditionalDataDict.TryGetValue(contact.ContactTypeId.Value, out additionalData))
                {
                    if (!Regex.IsMatch(contact.Address, additionalData.Regex))
                        errors.Add(string.Format(additionalData.ErrorMessageTemplate, contact.Address));
                }
                else
                {
                    switch (contactTypeDomainValue.Code)
                    {
                        case Constants.DOMAIN_VALUE_EMAIL_CODE:
                            if (!RegexUtilities.IsValidEmail(contact.Address))
                                errors.Add($"{contact.Address} не является электронной почтой");

                            break;

                        case Constants.DOMAIN_VALUE_MOBILE_PHONE_CODE:
                        case Constants.DOMAIN_VALUE_WORK_PHONE_CODE:
                        case Constants.DOMAIN_VALUE_HOME_PHONE_CODE:
                            if (!RegexUtilities.IsValidPhone(contact.Address))
                                errors.Add($"{contact.Address} не является телефонным номером");

                            break;
                    }
                }
            }

            if (errors.Count > 0)
                throw new PawnshopApplicationException(errors.ToArray());
        }

        private Dictionary<int, ContactTypeAdditionalData> ProcessAndValidateContactTypeDomainValues(IEnumerable<DomainValue> domainValues)
        {
            if (domainValues == null)
                throw new ArgumentNullException(nameof(domainValues));

            var systemErrors = new List<string>();
            var contactTypeAdditionalDataDict = new Dictionary<int, ContactTypeAdditionalData>();
            foreach (DomainValue domainValue in domainValues)
            {
                if (domainValue.AdditionalData != null)
                    try
                    {
                        ContactTypeAdditionalData additionalData = JsonConvert.DeserializeObject<ContactTypeAdditionalData>(domainValue.AdditionalData);
                        if (string.IsNullOrWhiteSpace(additionalData.ErrorMessageTemplate))
                            systemErrors.Add($"Обнаружен пустой шаблон текста ошибки валидации для значения домена '{Constants.DOMAIN_CONTACT_TYPE_CODE}'.'{domainValue.Code}'");
                        else
                        {
                            try
                            {
                                // проверяем чтобы string.Format отработал без исключений
                                string.Format(additionalData.ErrorMessageTemplate, string.Empty);
                            }
                            catch
                            {
                                systemErrors.Add($"Шаблон текста ошибки валидации для значения домена '{Constants.DOMAIN_CONTACT_TYPE_CODE}'.'{domainValue.Code}' имеет некорректный формат, функция string.Format отрабатывает неправильно");
                            }
                        }
                        try
                        {
                            new Regex(additionalData.Regex);
                            contactTypeAdditionalDataDict[domainValue.Id] = additionalData;
                        }
                        catch
                        {
                            systemErrors.Add($"Обнаружены неправильное регулярное выражение валидации для значения домена '{Constants.DOMAIN_CONTACT_TYPE_CODE}'.'{domainValue.Code}'");
                        }
                    }
                    catch
                    {
                        systemErrors.Add($"Обнаружены неправильные настройки валидации для значения домена '{Constants.DOMAIN_CONTACT_TYPE_CODE}'.'{domainValue.Code}'");
                    }
            }

            if (systemErrors.Count > 0)
                throw new PawnshopApplicationException(systemErrors.ToArray());

            return contactTypeAdditionalDataDict;
        }

        private Client GetClient(int clientId)
        {
            Client client = _clientRepository.GetOnlyClient(clientId);
            if (client == null)
                throw new PawnshopApplicationException("Клиент не найден");

            return client;
        }

        public List<ClientContact> GetMobilePhoneContacts(int clientId)
        {
            return GetList(clientId, Constants.DOMAIN_VALUE_MOBILE_PHONE_CODE);
        }

        public List<ClientContact> GetWorkPhoneContacts(int clientId)
        {
            return GetList(clientId, Constants.DOMAIN_VALUE_WORK_PHONE_CODE);
        }

        public List<ClientContact> GetHomePhoneContacts(int clientId)
        {
            return GetList(clientId, Constants.DOMAIN_VALUE_HOME_PHONE_CODE);
        }

        public List<ClientContact> GetEmailContacts(int clientId)
        {
            return GetList(clientId, Constants.DOMAIN_VALUE_EMAIL_CODE);
        }

        private bool CheckContactChangedFromDBModel(ClientContactDto clientContactDto, ClientContact clientContactFromDB)
        {
            if (clientContactDto == null)
                throw new ArgumentNullException(nameof(clientContactDto));

            if (clientContactFromDB == null)
                throw new ArgumentNullException(nameof(clientContactFromDB));

            if (clientContactDto.Id != clientContactFromDB.Id)
                throw new InvalidOperationException($"{nameof(clientContactDto)}.{nameof(clientContactDto.Id)} должен быть равен {nameof(clientContactFromDB)}.{nameof(clientContactFromDB.Id)}");

            return clientContactFromDB.Address != clientContactDto.Address ||
                   clientContactFromDB.ContactTypeId != clientContactDto.ContactTypeId.Value ||
                   clientContactFromDB.IsDefault != clientContactDto.IsDefault ||
                   clientContactFromDB.SendUkassaCheck != clientContactDto.SendUkassaCheck ||
                   clientContactFromDB.IsActual != clientContactDto.IsActual ||
                   clientContactFromDB.Note != clientContactDto.Note ||
                   clientContactFromDB.SourceId != clientContactDto.SourceId;
        }

        public void UpdateUkassaCheckReceive(int contactId, bool receive)
        {
            _clientContactRepository.UpdateUkassaCheckReceive(contactId, receive);
        }

        public void SaveWithoutChecks(ClientContact clientContact)
        {
            if (clientContact.Id > 0)
                _clientContactRepository.Update(clientContact);
            else
                _clientContactRepository.Insert(clientContact);
        }

        public ClientContact Find(object query)
        {
            return _clientContactRepository.Find(query);
        }

        public void Delete(int id) => _clientContactRepository.Delete(id);

        public void Update(ClientContact entity) => _clientContactRepository.Update(entity);


        public int? GetClientIdByDefaultPhone(string phoneNumber) {
            return _clientContactRepository.GetClientIdByDefaultPhone(phoneNumber);
        }
    }
}
