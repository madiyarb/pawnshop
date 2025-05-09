using Pawnshop.Core;
using Pawnshop.Core.Exceptions;
using Pawnshop.Core.Extensions;
using Pawnshop.Data.Access;
using Pawnshop.Data.Models.Clients;
using Pawnshop.Data.Models.Contracts.Actions;
using Pawnshop.Data.Models.Dictionaries;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pawnshop.Services.Clients
{
    public class ClientBlackListService : IClientBlackListService
    {
        private readonly ClientsBlackListRepository _clientsBlackListRepository;
        private readonly BlackListReasonRepository _blackListReasonRepository;
        private readonly ISessionContext _sessionContext;
        private readonly VehicleBlackListRepository _vehicleBlackListRepository;
        private readonly IClientService _clientService;
        public ClientBlackListService(ClientsBlackListRepository clientsBlackListRepository,
            BlackListReasonRepository blackListReasonRepository, ISessionContext sessionContext,
            VehicleBlackListRepository vehicleBlackListRepository, IClientService clientService)
        {
            _clientsBlackListRepository = clientsBlackListRepository;
            _blackListReasonRepository = blackListReasonRepository;
            _sessionContext = sessionContext;
            _vehicleBlackListRepository = vehicleBlackListRepository;
            _clientService = clientService;
        }

        public List<ClientsBlackListDto> GetAddedList(int clientId)
        {
            if (clientId <= 0)
                throw new ArgumentNullException(nameof(clientId));

            List<ClientsBlackList> clientsBlackList = _clientsBlackListRepository.GetAddedListByClientId(clientId);

            return clientsBlackList.Select(p => new ClientsBlackListDto
            {
                Id = p.Id,
                ReasonId = p.ReasonId,
                AddReason = p.AddReason,
                AddedAt = p.AddedAt,
                AddedBy = p.AddedBy,
                AddedFile = p.AddedFile,
                RemoveReason = p.RemoveReason,
                RemoveDate = p.RemoveDate,
                RemovedBy = p.RemovedBy,
                RemovedFile = p.RemovedFile,
                BlackListReason = p.BlackListReason
            }).ToList();
        }

        public List<ClientsBlackList> SaveBlackList(int clientId, List<ClientsBlackListDto> blackList)
        {
            if (blackList == null)
                throw new ArgumentNullException(nameof(blackList));

            var hasDuplicatedReasons = blackList.GroupBy(p => p.ReasonId).Any(e => e.Count() > 1);

            if (hasDuplicatedReasons)
                throw new PawnshopApplicationException($"Вы можете выбрать только одну причину включения в черный список клиентов");

            List<ClientsBlackList> blackListFromDB = _clientsBlackListRepository.GetAddedListByClientId(clientId);
            HashSet<int> uniqueIdsFromAssets = blackList.Where(e => e.Id != default).Select(e => e.Id).ToHashSet();
            Dictionary<int, ClientsBlackList> blackListFromDBDict = blackListFromDB.ToDictionary(e => e.Id, e => e);
            if (!uniqueIdsFromAssets.IsSubsetOf(blackListFromDBDict.Keys))
                throw new PawnshopApplicationException($"В аргументе {nameof(blackList)} присутствуют несуществующие или не принадлежащие Id данного клиента по черному списку");

            var clientsBlackList = new List<ClientsBlackList>();

            foreach (ClientsBlackListDto blackListItem in blackList)
            {
                BlackListReason blackListReasonWithFilesNeeded = _blackListReasonRepository.Get(blackListItem.ReasonId);

                if (blackListItem.AddedAt > DateTime.Now)
                    throw new PawnshopApplicationException($"Дата добавления не может быть больше текущей даты");

                if (blackListItem.AddedFile == null && blackListReasonWithFilesNeeded != null
                                                    && blackListReasonWithFilesNeeded.MustHaveAddedFile == true)
                    throw new PawnshopApplicationException($"Для включения клиента в черный список по данной причине необходимо добавить файл (основание)");

                if (blackListItem.RemoveReason != null && blackListItem.RemovedFile == null &&
                    blackListReasonWithFilesNeeded != null && blackListReasonWithFilesNeeded.MustHaveRemovedFile == true)
                    throw new PawnshopApplicationException($"Для исключения клиента из черного списка по данной причине необходимо добавить файл (основание)");

                ClientsBlackList blackListItemFromDB = null;
                if (!blackListFromDBDict.TryGetValue(blackListItem.Id, out blackListItemFromDB))
                {
                    blackListItemFromDB = new ClientsBlackList
                    {
                        AddedAt = blackListItem.AddedAt,
                        AddedBy = _sessionContext.UserId,
                        AddReason = blackListItem.AddReason,
                        ClientId = clientId,
                        ReasonId = blackListItem.ReasonId
                    };

                    if (blackListItem.AddedFile != null)
                    {
                        blackListItemFromDB.AddedFileRowId = blackListItem.AddedFile.Id;
                        blackListItemFromDB.AddedFile = blackListItem.AddedFile;
                    }
                }
                else
                {
                    blackListItemFromDB.AddReason = blackListItem.AddReason;
                    blackListItemFromDB.ReasonId = blackListItem.ReasonId;
                    blackListItemFromDB.AddedBy = blackListItem.AddedBy;
                    blackListItemFromDB.RemoveDate = blackListItem.RemoveDate;
                    blackListItemFromDB.RemoveReason = blackListItem.RemoveReason;

                    if (blackListItemFromDB.RemoveReason != null && blackListItemFromDB.RemovedBy == null)
                        blackListItemFromDB.RemovedBy = _sessionContext.UserId;
                    else
                        blackListItemFromDB.RemovedBy = blackListItem.RemovedBy;

                    if (blackListItem.AddedFile != null)
                    {
                        blackListItemFromDB.AddedFileRowId = blackListItem.AddedFile.Id;
                        blackListItemFromDB.AddedFile = blackListItem.AddedFile;
                    }
                    else
                    {
                        blackListItemFromDB.AddedFileRowId = null;
                        blackListItemFromDB.AddedFile = null;
                    }

                    if (blackListItem.RemovedFile != null)
                    {
                        blackListItemFromDB.RemovedFileRowId = blackListItem.RemovedFile.Id;
                        blackListItemFromDB.RemovedFile = blackListItem.RemovedFile;
                    }
                    else
                    {
                        blackListItemFromDB.RemovedFileRowId = null;
                        blackListItemFromDB.RemovedFile = null;
                    }

                    blackListFromDBDict.Remove(blackListItem.Id);
                }

                clientsBlackList.Add(blackListItemFromDB);
            }

            var hasDuplicatedReasonsWithDB = clientsBlackList.GroupBy(p => p.ReasonId).Any(e => e.Count() > 1);

            if (hasDuplicatedReasonsWithDB)
                throw new PawnshopApplicationException($"Клиент уже имеется по данной причине в черном списке клиентов");

            // если есть что менять, то вызываем транзакцию
            if (clientsBlackList.Count > 0 || blackListFromDBDict.Count > 0)
                using (var transaction = _clientsBlackListRepository.BeginTransaction())
                {
                    // удаляем ненужные добавления в черный список клиентов 
                    foreach ((int id, ClientsBlackList _) in blackListFromDBDict)
                    {
                        if (_sessionContext.ForSupport == true)
                            _clientsBlackListRepository.Delete(id);
                        else
                            throw new PawnshopApplicationException($"Клиент уже имеется по данной причине в черном списке клиентов");
                    }

                    foreach (ClientsBlackList blackListItem in clientsBlackList)
                    {
                        if (blackListItem.Id == default)
                        {
                            _clientsBlackListRepository.Insert(blackListItem);
                            _clientsBlackListRepository.LogChanges(blackListItem, _sessionContext.UserId, true);
                        }
                        else
                        {
                            _clientsBlackListRepository.Update(blackListItem);
                            _clientsBlackListRepository.LogChanges(blackListItem, _sessionContext.UserId);
                        }
                    }

                    transaction.Commit();
                }

            return clientsBlackList;
        }

        public List<ClientsBlackListDisplayedDto> GetDisplayedList(int clientId)
        {
            var list = _clientsBlackListRepository.GetDisplayedListByClientId(clientId);

            return list.Select(p => new ClientsBlackListDisplayedDto
            {
                AddReason = p.Name
            }).ToList();
        }

        public bool CheckClientIsInBlackList(int clientId, ContractActionType actionType, int? contractId = null)
        {
            List<ClientsBlackList> blackList = _clientsBlackListRepository.GetAddedListByClientId(clientId)?.Where(p => p.RemoveReason == null).ToList();

            if (blackList != null && blackList.Count > 0)
            {
                if (blackList.Any(p => p.BlackListReason == null))
                    throw new PawnshopApplicationException("Не найдена причина черного списка");

                var canCreateNewContracts = actionType == ContractActionType.PartialPayment;

                if (!canCreateNewContracts && !blackList.All(p => p.BlackListReason.AllowNewContracts))
                    throw new PawnshopApplicationException("Клиент в черном списке, действие заблокировано");

                if (!canCreateNewContracts && contractId.HasValue && blackList.All(p => p.BlackListReason.AllowNewContracts))
                {
                    var contractPositions = _vehicleBlackListRepository.GetContractPositionsByContractId(contractId.Value);
                    foreach (var contractPosition in contractPositions)
                    {
                        if (contractPosition.CategoryId != Constants.WITHOUT_DRIVE_RIGHT_CATEGORY)
                            throw new PawnshopApplicationException("Клиент в черном списке, разрешена только категория без права вождения");
                    }
                }

                return false;
            }

            return true;
        }

        public bool IsClientInBlackList(int clientId)
        {
            List<ClientsBlackList> clientsBlackList = _clientsBlackListRepository.GetAddedListByClientId(clientId);

            bool IsInBlacklist = false;

            if (clientsBlackList.Count > 0)
                IsInBlacklist = clientsBlackList.Any(p => p.AddReason != null && p.RemoveReason == null);

            return IsInBlacklist;
        }

        public ClientsBlackList Find(object query) => _clientsBlackListRepository.Find(query);

        public List<ClientsBlackList> GetClientsBlackListsByClientId(int clientId) => _clientsBlackListRepository.GetClientsBlackListsByClientId(clientId);
        
        public async Task<List<ClientsBlackList>> GetClientsBlackListsByClientIdAsync(int clientId) => 
            await _clientsBlackListRepository.GetClientsBlackListsByClientIdAsync(clientId);

        public void Save(ClientsBlackList entity)
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));
            if (entity.Id > 0)
            {
                _clientsBlackListRepository.Update(entity);
                _clientsBlackListRepository.LogChanges(entity, _sessionContext.IsInitialized ? _sessionContext.UserId : Constants.ADMINISTRATOR_IDENTITY);
            }
            else
            {
                _clientsBlackListRepository.Insert(entity);
                _clientsBlackListRepository.LogChanges(entity, _sessionContext.IsInitialized ? _sessionContext.UserId : Constants.ADMINISTRATOR_IDENTITY);
            }
        }

        public async Task InsertIntoBlackListAsync(IEnumerable<string> iinList)
        {
            var userId = Constants.ADMINISTRATOR_IDENTITY;
            if (!iinList.Any())
                return;

            if(_sessionContext.IsInitialized)
                userId = _sessionContext.UserId;

            var reason = await _blackListReasonRepository.GetAsync(Constants.BLACKLIST_REASON_CODE_SOLDIER);
            foreach (var i in iinList)
            {
                var clientId = await _clientService.GetClientIdAsync(i);
                if (clientId == 0)
                    continue;

                if (!IsClientInBlackList(clientId))
                {
                    var item = new ClientsBlackList()
                    {
                        ClientId = clientId,
                        ReasonId = reason.Id,
                        AddedBy = userId,
                        AddReason = "Реструктуризированный клиент",
                        AddedAt = DateTime.Now
                    };
                    Save(item);
                }
            }
        }

        public async Task InsertIntoBlackListAsync(int clientId)
        {
            var userId = Constants.ADMINISTRATOR_IDENTITY;
            if (_sessionContext.IsInitialized)
                userId = _sessionContext.UserId;

            var reason = await _blackListReasonRepository.GetAsync(Constants.BLACKLIST_REASON_CODE_SOLDIER);

            if (clientId == 0)
                return;

            if (!IsClientInBlackList(clientId))
            {
                var item = new ClientsBlackList()
                {
                    ClientId = clientId,
                    ReasonId = reason.Id,
                    AddedBy = userId,
                    AddReason = "Реструктуризированный клиент",
                    AddedAt = DateTime.Now
                };
                Save(item);
            }
        }
    }
}
