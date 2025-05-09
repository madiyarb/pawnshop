using Pawnshop.Core;
using Pawnshop.Core.Exceptions;
using Pawnshop.Core.Queries;
using Pawnshop.Data.Access;
using Pawnshop.Data.Models.Clients;
using Pawnshop.Data.Models.Clients.ClientAdditionalIncomeHistory;
using Pawnshop.Services.Domains;
using Pawnshop.Services.Models.Clients;
using Pawnshop.Services.Models.List;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Pawnshop.Services.Clients
{
    public class ClientAdditionalIncomeService : IClientAdditionalIncomeService
    {
        private readonly IDomainService _domainService;
        private readonly IClientService _clientService;
        private readonly ISessionContext _sessionContext;
        private readonly ClientAdditionalIncomeRepository _clientAdditionalIncomeRepository;
        public ClientAdditionalIncomeService(IDomainService domainService, IClientService clientService,
            ClientAdditionalIncomeRepository clientAdditionalIncomeRepository, ISessionContext sessionContext
            )
        {
            _domainService = domainService;
            _clientService = clientService;
            _clientAdditionalIncomeRepository = clientAdditionalIncomeRepository;
            _sessionContext = sessionContext;
        }

        public List<ClientAdditionalIncome> Get(int clientId)
        {
            _clientService.CheckClientExists(clientId);

            var incomeIdsToDelete = _clientAdditionalIncomeRepository.GetNotActualIncomesIds(clientId);

            foreach (var incomeId in incomeIdsToDelete)
            {
                _clientAdditionalIncomeRepository.Delete(
                    incomeId,
                    Constants.ADMINISTRATOR_IDENTITY,
                    Constants.AUTO_DELETE_MESSAGE);
            }

            List<ClientAdditionalIncome> incomes = _clientAdditionalIncomeRepository.GetListByClientId(clientId);
            return incomes;
        }

        public ClientAdditionalIncome GetBusinessIncome(int clientId)
        {
            _clientService.CheckClientExists(clientId);

            var incomeIdsToDelete = _clientAdditionalIncomeRepository.GetNotActualIncomesIds(clientId);

            foreach (var incomeId in incomeIdsToDelete)
            {
                _clientAdditionalIncomeRepository.Delete(
                    incomeId,
                    Constants.ADMINISTRATOR_IDENTITY,
                    Constants.AUTO_DELETE_MESSAGE);
            }

            List<ClientAdditionalIncome> incomes = _clientAdditionalIncomeRepository.GetListByClientId(clientId);
            return incomes.Find(x => x.TypeId == _domainService.GetDomainValue(Constants.INCOME_TYPE_DOMAIN, Constants.BUSINESS_LOAN_PURPOSE).Id);
        }

        public List<ClientAdditionalIncome> Save(int clientId, List<ClientAdditionalIncomeDto> incomes)
        {
            if (incomes == null)
                throw new ArgumentNullException(nameof(incomes));

            Validate(incomes);
            List<ClientAdditionalIncome> additionalIncomesFromDB = Get(clientId);
            Dictionary<int, ClientAdditionalIncome> additionalIncomesFromDBDict = additionalIncomesFromDB.ToDictionary(e => e.Id, e => e);
            var syncList = new List<ClientAdditionalIncome>();

            using (var transaction = _clientAdditionalIncomeRepository.BeginTransaction())
            {
                foreach (ClientAdditionalIncome incomeFromDB in additionalIncomesFromDB)
                {
                    ClientAdditionalIncomeDto correspondingDto = incomes.FirstOrDefault(e => e.Id == incomeFromDB.Id);

                    if (correspondingDto != null && additionalIncomesFromDBDict.ContainsKey(incomeFromDB.Id))
                    {
                        if (ClientAdditionalIncomeDto.HaveUpdates(incomeFromDB, correspondingDto))
                        {
                            incomeFromDB.TypeId = correspondingDto.TypeId.Value;
                            incomeFromDB.Amount = correspondingDto.Amount.Value;

                            _clientAdditionalIncomeRepository.Update(
                                incomeFromDB,
                                _sessionContext.IsInitialized ?
                                    _sessionContext.UserId :
                                    Constants.ADMINISTRATOR_IDENTITY);
                        }

                        syncList.Add(incomeFromDB);
                    }
                    else
                    {
                        DeleteIncome(incomeFromDB.Id);
                    }
                }

                transaction.Commit();
            }

            using (var transaction = _clientAdditionalIncomeRepository.BeginTransaction())
            {
                foreach (ClientAdditionalIncomeDto incomeDto in incomes)
                {
                    if (incomeDto.Id == default)
                    {
                        ClientAdditionalIncome incomeFromDB = new ClientAdditionalIncome
                        {
                            ClientId = clientId,
                            TypeId = incomeDto.TypeId.Value,
                            Amount = incomeDto.Amount.Value,
                            AuthorId = _sessionContext.IsInitialized ?
                                            _sessionContext.UserId :
                                            Constants.ADMINISTRATOR_IDENTITY,
                        };

                        _clientAdditionalIncomeRepository.Insert(
                            incomeFromDB,
                            _sessionContext.IsInitialized ?
                                            _sessionContext.UserId :
                                            Constants.ADMINISTRATOR_IDENTITY);

                        syncList.Add(incomeFromDB);
                    }
                }

                transaction.Commit();
            }

            return syncList;
        }

        public void Validate(List<ClientAdditionalIncomeDto> incomes)
        {
            if (incomes == null)
                throw new ArgumentNullException(nameof(incomes));

            List<int> incomeTypeDomainValueIds = _domainService.GetDomainValues(Constants.INCOME_TYPE_DOMAIN).Select(v => v.Id).ToList();
            var uniqueIncomeTypes = new HashSet<int>();
            var errors = new HashSet<string>();
            foreach (ClientAdditionalIncomeDto income in incomes)
            {
                if (income == null)
                    errors.Add($"В аргументе {nameof(incomes)} присутствуют пустые элементы");
                else
                {
                    if (!income.TypeId.HasValue)
                        errors.Add($"Поле {nameof(income.TypeId)} не должно быть пустым");
                    else
                    {
                        if (!incomeTypeDomainValueIds.Contains(income.TypeId.Value))
                            errors.Add($"Поле {nameof(income.TypeId)} имеет неверное значение");
                        if (uniqueIncomeTypes.Contains(income.TypeId.Value))
                            errors.Add($"Поле {nameof(income.TypeId)} должно быть уникальным");

                        uniqueIncomeTypes.Add(income.TypeId.Value);
                    }

                    if (!income.Amount.HasValue)
                        errors.Add($"Поле {nameof(income.Amount)} не должно быть пустым");
                    else if (income.Amount <= 0)
                        errors.Add($"Поле {nameof(income.Amount)} должно быть положительным числом");
                }
            }

            if (errors.Count > 0)
                throw new PawnshopApplicationException(errors.ToArray());
        }

        public void DeleteIncome(int id)
        {
            if (id > 0)
            {
                using (var transaction = _clientAdditionalIncomeRepository.BeginTransaction())
                {
                    var additionalIncome = _clientAdditionalIncomeRepository.Get(id);
                    _clientAdditionalIncomeRepository.Delete(
                        additionalIncome,
                        _sessionContext.IsInitialized ?
                            _sessionContext.UserId :
                            Constants.ADMINISTRATOR_IDENTITY);
                    transaction.Commit();
                }
            }
        }

        public async Task<ListModel<ClientAdditionalIncomeHistory>> GetHistoryFiltered(int clientId, ClientAdditionalIncomeHistoryQuery query)
        {
            if (clientId == default)
            {
                throw new ArgumentNullException(nameof(clientId));
            }

            var clientAdditionalIncomeHistories = await _clientAdditionalIncomeRepository.GetHistoryByFilterData(clientId, query);
            var clientAdditionalIncomeHistoriesCnt = await _clientAdditionalIncomeRepository.GetHistoryCountByFilterData(clientId, query);

            return new ListModel<ClientAdditionalIncomeHistory>
            {
                List = clientAdditionalIncomeHistories,
                Count = clientAdditionalIncomeHistoriesCnt
            };
        }

        public void RemoveAdditionalIncomesAfterSign(int contractId, int clientId)
        {
            using var transaction = _clientAdditionalIncomeRepository.BeginTransaction();
            var incomeListByClientId = _clientAdditionalIncomeRepository.GetListByClientId(clientId);

            foreach (var income in incomeListByClientId)
            {
                income.ContractId = contractId;
                income.DeleteDate = DateTime.Now;
                _clientAdditionalIncomeRepository.Update(
                    income,
                    Constants.ADMINISTRATOR_IDENTITY,
                    Constants.INCOME_LINKED_CONTRACT_SIGNED,
                    OperationType.Delete);
            }

            transaction.Commit();
        }
    }
}
