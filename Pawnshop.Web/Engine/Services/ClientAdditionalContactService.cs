using Pawnshop.Core.Exceptions;
using Pawnshop.Core.Utilities;
using Pawnshop.Core;
using Pawnshop.Data.Access;
using Pawnshop.Data.Models.Clients;
using Pawnshop.Services.Clients;
using Pawnshop.Web.Engine.Services.Interfaces;
using Pawnshop.Web.Models.Clients.Profiles;
using System.Collections.Generic;
using System.Linq;
using System;
using DocumentFormat.OpenXml.Office2010.Excel;

namespace Pawnshop.Web.Engine.Services
{
    public class ClientAdditionalContactService : IClientAdditionalContactService
    {
        private readonly ClientAdditionalContactRepository _clientAdditionalContactRepository;
        private readonly IDomainService _domainService;
        private readonly IClientService _clientService;
        private readonly ISessionContext _sessionContext;

        public ClientAdditionalContactService(
            ClientAdditionalContactRepository clientAdditionalContactRepository,
            IDomainService domainService,
            IClientService clientService,
            ISessionContext sessionContext
            )
        {
            _clientAdditionalContactRepository = clientAdditionalContactRepository;
            _domainService = domainService;
            _clientService = clientService;
            _sessionContext = sessionContext;

        }

        public List<ClientAdditionalContact> Get(int clientId)
        {
            _clientService.CheckClientExists(clientId);
            List<ClientAdditionalContact> additionalContacts = _clientAdditionalContactRepository.GetListByClientId(clientId);
            return additionalContacts;
        }

        public List<ClientAdditionalContact> Save(int clientId, List<ClientAdditionalContactDto> additionalContacts)
        {
            if (additionalContacts == null)
                throw new ArgumentNullException(nameof(additionalContacts));

            Validate(additionalContacts);
            List<ClientAdditionalContact> additionalContactsFromDB = Get(clientId);
            HashSet<int> additionalContactsUniqueIds = additionalContacts.Where(e => e.Id != default).Select(e => e.Id).ToHashSet();
            Dictionary<int, ClientAdditionalContact> additionalContactsFromDBDict = additionalContactsFromDB.ToDictionary(e => e.Id, e => e);
            if (!additionalContactsUniqueIds.IsSubsetOf(additionalContactsFromDBDict.Keys))
                throw new PawnshopApplicationException($"В аргументе {nameof(additionalContacts)} присутствуют несуществующие или не принадлежащие Id доп. контактов данного клиента");

            var syncList = new List<ClientAdditionalContact>();
            foreach (ClientAdditionalContactDto contact in additionalContacts)
            {
                ClientAdditionalContact contactFromDB = null;
                if (!additionalContactsFromDBDict.TryGetValue(contact.Id, out contactFromDB))
                {
                    contactFromDB = new ClientAdditionalContact
                    {
                        ClientId = clientId,
                        PhoneNumber = contact.PhoneNumber,
                        ContactOwnerTypeId = contact.ContactOwnerTypeId.Value,
                        ContactOwnerFullname = contact.ContactOwnerFullname,
                        AuthorId = _sessionContext.UserId,
                        IsMainPayer = contact.IsMainPayer
                    };
                }
                else
                {
                    contactFromDB.PhoneNumber = contact.PhoneNumber;
                    contactFromDB.ContactOwnerTypeId = contact.ContactOwnerTypeId.Value;
                    contactFromDB.ContactOwnerFullname = contact.ContactOwnerFullname;
                    contactFromDB.IsMainPayer = contact.IsMainPayer;
                    additionalContactsFromDBDict.Remove(contact.Id);
                }

                syncList.Add(contactFromDB);
            }

            // если есть что менять, то вызываем транзакцию
            if (syncList.Count > 0 || additionalContactsFromDBDict.Count > 0)
                using (var transaction = _clientAdditionalContactRepository.BeginTransaction())
                {
                    // удаляем ненужные места работ
                    foreach ((int id, ClientAdditionalContact _) in additionalContactsFromDBDict)
                    {
                        _clientAdditionalContactRepository.Delete(id);
                    }

                    foreach (ClientAdditionalContact contact in syncList)
                    {
                        if (contact.Id == default)
                        {
                            _clientAdditionalContactRepository.Insert(contact);
                            _clientAdditionalContactRepository.LogChanges(contact, _sessionContext.UserId, true);
                        }
                        else
                        {
                            if (!contact.Equals(additionalContactsFromDB.FirstOrDefault(cont => cont.Id == contact.Id)))
                            {
                                _clientAdditionalContactRepository.Update(contact);
                                _clientAdditionalContactRepository.LogChanges(contact, _sessionContext.UserId);
                            }
                        }
                    }

                    transaction.Commit();
                }

            return syncList;
        }

        public void SaveFromMobile(int clientId, List<ClientAdditionalContactDto> additionalContacts)
        {
            Validate(additionalContacts);

            var syncList = new List<ClientAdditionalContact>();
            var additionalContactsFromDB = Get(clientId);

            foreach (var contact in additionalContacts)
            {
                if (additionalContactsFromDB.FirstOrDefault(x => x.PhoneNumber == contact.PhoneNumber) is ClientAdditionalContact dbContact)
                {
                    dbContact.ContactOwnerFullname = contact.ContactOwnerFullname;
                    dbContact.ContactOwnerTypeId = contact.ContactOwnerTypeId.Value;
                    dbContact.AuthorId = Constants.ADMINISTRATOR_IDENTITY;
                    dbContact.FromContactList = contact.FromContactList;
                    syncList.Add(dbContact);
                }
                else
                {
                    syncList.Add(new ClientAdditionalContact
                    {
                        AuthorId = Constants.ADMINISTRATOR_IDENTITY,
                        ClientId = clientId,
                        ContactOwnerFullname = contact.ContactOwnerFullname,
                        ContactOwnerTypeId = contact.ContactOwnerTypeId.Value,
                        CreateDate = DateTime.UtcNow,
                        FromContactList = contact.FromContactList,
                        PhoneNumber = contact.PhoneNumber,
                    });
                }
            }

            additionalContactsFromDB.ForEach(x =>
            {
                if (!syncList.Any(s => s.Id == x.Id))
                    _clientAdditionalContactRepository.Delete(x.Id);
            });

            foreach (ClientAdditionalContact contact in syncList)
            {
                if (contact.Id == default)
                {
                    _clientAdditionalContactRepository.Insert(contact);
                    _clientAdditionalContactRepository.LogChanges(contact, Constants.ADMINISTRATOR_IDENTITY, true);
                }
                else
                {
                    _clientAdditionalContactRepository.Update(contact);
                    _clientAdditionalContactRepository.LogChanges(contact, Constants.ADMINISTRATOR_IDENTITY);
                }
            }
        }

        private void Validate(List<ClientAdditionalContactDto> additionalContacts)
        {
            if (additionalContacts == null)
                throw new ArgumentNullException(nameof(additionalContacts));

            var errors = new HashSet<string>();
            HashSet<int> contactOwnerTypeDomainValueIds = _domainService.GetDomainValues(Constants.CONTACT_OWNER_TYPE_DOMAIN).Select(v => v.Id).ToHashSet();
            var uniquePhoneNumbers = new HashSet<string>();
            foreach (ClientAdditionalContactDto contact in additionalContacts)
            {
                if (contact == null)
                    errors.Add($"Обнаружен пустой объект в списке {nameof(additionalContacts)}");
                else
                {
                    if (!contact.ContactOwnerTypeId.HasValue)
                        errors.Add($"Поле {nameof(contact.ContactOwnerTypeId)} не должно быть пустым");
                    else if (!contactOwnerTypeDomainValueIds.Contains(contact.ContactOwnerTypeId.Value))
                        errors.Add($"Поле {nameof(ClientAdditionalContact.ContactOwnerTypeId)} имеет неверное значение");
                    if (string.IsNullOrWhiteSpace(contact.ContactOwnerFullname))
                        errors.Add($"Поле {nameof(ClientAdditionalContact.ContactOwnerFullname)} не может быть пустым");
                    if (string.IsNullOrWhiteSpace(contact.PhoneNumber))
                        errors.Add($"Поле {nameof(ClientAdditionalContact.PhoneNumber)} не может быть пустым");
                    else
                    {
                        if (!RegexUtilities.IsValidKazakhstanPhone(contact.PhoneNumber))
                        {
                            errors.Add($"Значение поля {nameof(contact.PhoneNumber)} - '{contact.PhoneNumber}' не является телефонным номером");
                        }
                        else
                        {
                            if (uniquePhoneNumbers.Contains(contact.PhoneNumber))
                                errors.Add("Обнаружены одинаковые номера телефонов в дополнительных контактах");

                            uniquePhoneNumbers.Add(contact.PhoneNumber);
                        }
                    }
                }
            }

            if (errors.Count > 0)
                throw new PawnshopApplicationException(errors.ToArray());
        }
    }
}
