using Pawnshop.Core;
using Pawnshop.Core.Exceptions;
using Pawnshop.Core.Queries;
using Pawnshop.Data.Access;
using Pawnshop.Data.Models.Clients;
using Pawnshop.Data.Models.Domains;
using Pawnshop.Services.Domains;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Pawnshop.Services.Clients
{
    public class ClientEconomicActivityService : IClientEconomicActivityService
    {
        private readonly ClientEconomicActivityRepository _clientEconomicActivityRepository;
        private readonly ClientRepository _clientRepository;
        private readonly IDomainService _domainService;
        private readonly ISessionContext _sessionContext;
        private readonly IClientService _clientService;

        public ClientEconomicActivityService(ClientEconomicActivityRepository clientEconomicActivityRepository, ClientRepository clientRepository,
            IDomainService domainService, ISessionContext sessionContext, IClientService clientService)
        {
            _clientEconomicActivityRepository = clientEconomicActivityRepository;
            _clientRepository = clientRepository;
            _domainService = domainService;
            _sessionContext = sessionContext;
            _clientService = clientService;
        }

        public List<ClientEconomicActivity> GetList(int clientId)
        {
            if (clientId <= 0)
                throw new ArgumentNullException(nameof(clientId));

            _clientService.Get(clientId);

            List<ClientEconomicActivity> clientEconomicActivities = _clientEconomicActivityRepository.List(new ListQuery(), new { ClientId = clientId });

            return clientEconomicActivities;
        }

        public List<ClientEconomicActivity> Save(int clientId, List<ClientEconomicActivity> clientEconomicActivitiesRequest)
        {
            if (clientId <= 0)
                throw new ArgumentNullException(nameof(clientId));

            Dictionary<int, ClientEconomicActivity> clientEconomicActivitiesFromDB = _clientEconomicActivityRepository.List(new ListQuery(), new { ClientId = clientId }).ToDictionary(c => c.Id, c => c);
            ValidateClientEconomicActivity(clientId, clientEconomicActivitiesRequest);

            var syncedClientEconomicActivities = new List<ClientEconomicActivity>();

            foreach (var clientEconomicActivity in clientEconomicActivitiesRequest)
            {
                ClientEconomicActivity clientEconomicActivityFromDB = null;
                if (!clientEconomicActivitiesFromDB.TryGetValue(clientEconomicActivity.Id, out clientEconomicActivityFromDB))
                {
                    clientEconomicActivityFromDB = new ClientEconomicActivity
                    {
                        ClientId = clientId,
                        EconomicActivityTypeId = clientEconomicActivity.EconomicActivityTypeId,
                        CreateDate = DateTime.Now,
                        AuthorId = _sessionContext.UserId,
                        ValueKindId = clientEconomicActivity.ValueKindId
                    };
                }
                else
                {
                    if (CheckEconomicActivitiesChangedFromDBModel(clientEconomicActivity, clientEconomicActivityFromDB))
                    {
                        clientEconomicActivityFromDB.EconomicActivityTypeId = clientEconomicActivity.EconomicActivityTypeId;
                        clientEconomicActivityFromDB.ClientId = clientId;
                        clientEconomicActivityFromDB.ValueKindId = clientEconomicActivity.ValueKindId;
                    }

                    clientEconomicActivitiesFromDB.Remove(clientEconomicActivity.Id);
                }
                syncedClientEconomicActivities.Add(clientEconomicActivityFromDB);
            }

            if (clientEconomicActivitiesFromDB.Count > 0 || syncedClientEconomicActivities.Count > 0)
                using (var transaction = _clientEconomicActivityRepository.BeginTransaction())
                {
                    foreach ((int key, var clientEconomicActivity) in clientEconomicActivitiesFromDB)
                    {
                        _clientEconomicActivityRepository.Delete(key);
                    }

                    foreach (var clientEconomicActivity in syncedClientEconomicActivities)
                    {
                        if (clientEconomicActivity.Id != default)
                        {
                            _clientEconomicActivityRepository.Update(clientEconomicActivity);
                            //_clientEconomicActivityRepository.LogChanges(clientEconomicActivity, _sessionContext.UserId, true);
                        }
                        else
                        {
                            _clientEconomicActivityRepository.Insert(clientEconomicActivity);
                            //_clientEconomicActivityRepository.LogChanges(clientEconomicActivity, _sessionContext.UserId);
                        }
                    }

                    transaction.Commit();
                }

            return syncedClientEconomicActivities;
        }

        private bool CheckEconomicActivitiesChangedFromDBModel(ClientEconomicActivity clientEconomicActivity, ClientEconomicActivity clientEconomicActivityFromDB)
        {
            if (clientEconomicActivity == null)
                throw new ArgumentNullException(nameof(clientEconomicActivity));

            if (clientEconomicActivityFromDB == null)
                throw new ArgumentNullException(nameof(clientEconomicActivityFromDB));

            if (clientEconomicActivity.Id != clientEconomicActivityFromDB.Id)
                throw new InvalidOperationException($"{nameof(clientEconomicActivity)}.{nameof(clientEconomicActivity.Id)} должен быть равен {nameof(clientEconomicActivityFromDB)}.{nameof(clientEconomicActivityFromDB.Id)}");

            return clientEconomicActivityFromDB.EconomicActivityTypeId != clientEconomicActivity.EconomicActivityTypeId ||
                   clientEconomicActivityFromDB.ValueKindId != clientEconomicActivity.ValueKindId;
        }

        private void ValidateClientEconomicActivity(int clientId, List<ClientEconomicActivity> clientEconomicActivities)
        {
            if (clientEconomicActivities == null)
                throw new ArgumentNullException(nameof(clientId));

            var client = _clientService.Get(clientId);
            var errors = new HashSet<string>();

            Dictionary<int, DomainValue> clientEconomicActivitiesDomainValuesDict = _domainService.GetDomainValues(Constants.OKED_KINDS_DOMAIN_VALUE).ToDictionary(dv => dv.Id, dv => dv);

            List<DomainValue> majorClientEconomicActivity = clientEconomicActivitiesDomainValuesDict.Values.Where(x => x.Code.Equals(Constants.OKED_KINDS_MAIN_CODE)).ToList();
            if (majorClientEconomicActivity.Any())
            {
                int majorId = majorClientEconomicActivity.FirstOrDefault().Id;
                var economicActivities = clientEconomicActivities.Where(x => x.ValueKindId == majorId);

                if (economicActivities.Count() > 1)
                    errors.Add("Обнаружены несколько записей с типом ОКЭД Основной");
                else if (economicActivities.Count() == 0)
                    errors.Add("Не обнаружены записи с типом ОКЭД Основной");
            }

            if (clientEconomicActivities.Where(e => e.Id != 0).GroupBy(e => e.Id).Any(e => e.Count() > 1))
                errors.Add("Обнаружены несколько записей с одним Id, обратитесь в тех. поддержку");

            // проверим на null значения в поле EconomicActivityTypeId
            bool nullEconomicActivityExists = clientEconomicActivities.Any(e => e.EconomicActivityTypeId == 0);
            if (nullEconomicActivityExists)
                errors.Add($"Не все ОКЭД имеют заполненный {nameof(ClientEconomicActivity.EconomicActivityTypeId)}");

            // проверим чтобы значения ValueKindId были валидными
            HashSet<int> uniqueEconomicActivitiesIds = clientEconomicActivities.Select(e => e.ValueKindId).ToHashSet();
            if (!uniqueEconomicActivitiesIds.IsSubsetOf(clientEconomicActivitiesDomainValuesDict.Keys))
                errors.Add($"Не все ОКЭД имеют правильный {nameof(ClientEconomicActivity.ValueKindId)}");

            if (errors.Count > 0)
                throw new PawnshopApplicationException(errors.ToArray());
        }
    }
}
