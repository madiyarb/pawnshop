using Pawnshop.Core;
using Pawnshop.Core.Exceptions;
using Pawnshop.Core.Queries;
using Pawnshop.Data.Access;
using Pawnshop.Data.Models.Clients;
using Pawnshop.Data.Models.Clients.ClientIncomeHistory;
using Pawnshop.Data.Models.Dictionaries;
using Pawnshop.Services.Domains;
using Pawnshop.Services.Models.Clients;
using Pawnshop.Services.Models.List;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Pawnshop.Services.Clients
{
    public class ClientIncomeService : IClientIncomeService
    {
        private readonly ClientIncomeRepository _clientIncomeRepository;
        private readonly IClientService _clientService;
        private readonly ISessionContext _sessionContext;
        private readonly IDomainService _domainService;
        private readonly ClientIncomeCalculationSettingRepository _clientIncomeCalculationSettingRepository;
        private readonly NotionalRateRepository _notionalRateRepository;
        private readonly IClientAdditionalIncomeService _clientAdditionalIncomeService;
        private readonly IClientExpenseService _clientExpenseService;

        public ClientIncomeService(ClientIncomeRepository clientIncomeRepository,
                                   IClientService clientService,
                                   ISessionContext sessionContext,
                                   IDomainService domainService,
                                   ClientIncomeCalculationSettingRepository clientIncomeCalculationSettingRepository,
                                   NotionalRateRepository notionalRateRepository,
                                   IClientAdditionalIncomeService clientAdditionalIncomeService,
                                   IClientExpenseService clientExpenseService
                                   )
        {
            _clientIncomeRepository = clientIncomeRepository;
            _clientService = clientService;
            _sessionContext = sessionContext;
            _domainService = domainService;
            _clientIncomeCalculationSettingRepository = clientIncomeCalculationSettingRepository;
            _notionalRateRepository = notionalRateRepository;
            _clientAdditionalIncomeService = clientAdditionalIncomeService;
            _clientExpenseService = clientExpenseService;
        }

        public List<ClientIncome> GetClientIncomes(int clientId)
        {
            _clientService.CheckClientExists(clientId);
            var incomes = _clientIncomeRepository.GetListByClientId(clientId);
            return incomes;
        }

        public List<ClientIncome> GetClientIncomes(int clientId, int incomeType)
        {
            _clientService.CheckClientExists(clientId);
            var incomeIdsToDelete = _clientIncomeRepository.GetNotActualIncomesByClientId(clientId, incomeType);

            foreach (var incomeId in incomeIdsToDelete)
            {
                _clientIncomeRepository.Delete(
                    incomeId,
                    Constants.ADMINISTRATOR_IDENTITY,
                    Constants.AUTO_DELETE_MESSAGE);
            }

            var incomes = _clientIncomeRepository.GetListByClientIdAndIncomeType(clientId, incomeType);
            return incomes;
        }

        public void DeleteIncome(int id)
        {
            if (id > 0)
            {
                using (var transaction = _clientIncomeRepository.BeginTransaction())
                {
                    var income = _clientIncomeRepository.Get(id);
                    _clientIncomeRepository.Delete(
                        income,
                        _sessionContext.IsInitialized ?
                            _sessionContext.UserId :
                            Constants.ADMINISTRATOR_IDENTITY);
                    transaction.Commit();
                }
            }
        }

        public async Task<ListModel<ClientIncomeHistory>> GetHistoryFiltered(int clientId, ClientIncomeHistoryQuery query)
        {
            if (clientId == default)
            {
                throw new ArgumentNullException(nameof(clientId));
            }

            var clientIncomeLogs = await _clientIncomeRepository.GetHistoryByFilterData(clientId, query);
            var clientIncomeCnt = await _clientIncomeRepository.GetHistoryCountByFilterData(clientId, query);

            return new ListModel<ClientIncomeHistory>
            {
                List = clientIncomeLogs,
                Count = clientIncomeCnt
            };
        }

        public List<ClientIncome> Save(int clientId, List<ClientIncomeDto> incomes, IncomeType incomeType)
        {
            if (incomes == null)
                throw new ArgumentNullException(nameof(incomes));

            Validate(incomes, incomeType);
            var clientIncomesFromDB = GetClientIncomes(clientId, (int)incomeType);
            Dictionary<int, ClientIncome> clientIncomesFromDBDict = clientIncomesFromDB.ToDictionary(e => e.Id, e => e);
            var syncList = new List<ClientIncome>();


            using (var transaction = _clientIncomeRepository.BeginTransaction())
            {
                foreach (ClientIncome incomeFromDB in clientIncomesFromDB)
                {
                    ClientIncomeDto correspondingDto = incomes.FirstOrDefault(e => e.Id == incomeFromDB.Id);

                    if (correspondingDto != null && clientIncomesFromDBDict.ContainsKey(incomeFromDB.Id))
                    {
                        if (ClientIncomeDto.HaveUpdates(incomeFromDB, correspondingDto))
                        {
                            incomeFromDB.IncomeType = correspondingDto.IncomeType;
                            incomeFromDB.ConfirmationDocumentTypeId = correspondingDto.ConfirmationDocumentTypeId;
                            incomeFromDB.FileRowId = correspondingDto.FileRowId.Value;
                            incomeFromDB.IncomeTurns = correspondingDto.IncomeTurns;
                            incomeFromDB.MonthQuantity = correspondingDto.MonthQuantity;
                            incomeFromDB.IncomeAmount = correspondingDto.IncomeAmount;

                            _clientIncomeRepository.Update(
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

                foreach (ClientIncomeDto incomeDto in incomes)
                {
                    if (incomeDto.Id == default)
                    {
                        ClientIncome incomeFromDB = new ClientIncome
                        {
                            ClientId = clientId,
                            IncomeType = incomeDto.IncomeType,
                            ConfirmationDocumentTypeId = incomeDto.ConfirmationDocumentTypeId,
                            FileRowId = incomeDto.FileRowId.Value,
                            IncomeTurns = incomeDto.IncomeTurns,
                            MonthQuantity = incomeDto.MonthQuantity,
                            IncomeAmount = incomeDto.IncomeAmount,
                            AuthorId = _sessionContext.IsInitialized ?
                                            _sessionContext.UserId :
                                            Constants.ADMINISTRATOR_IDENTITY
                        };

                        _clientIncomeRepository.Insert(
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

        private string GetDomainCodeByIncomeType(List<ClientIncomeDto> incomes, IncomeType incomeType)
        {

            return incomeType == IncomeType.Formal ? Constants.FORMAL_INCOME_DOCTYPES : Constants.INFORMAL_INCOME_DOCTYPES;
        }

        public void Validate(List<ClientIncomeDto> incomes, IncomeType incomeType)
        {
            if (incomes == null)
                throw new ArgumentNullException(nameof(incomes));

            var domainCode = GetDomainCodeByIncomeType(incomes, incomeType);

            HashSet<int> incomeTypeDomainValueIds = _domainService.GetDomainValues(domainCode).Select(v => v.Id).ToHashSet();
            var uniqueIncomeTypes = new HashSet<int>();
            var errors = new HashSet<string>();
            foreach (var income in incomes)
            {
                if (income == null)
                    errors.Add($"В аргументе {nameof(incomes)} присутствуют пустые элементы");
                else
                {
                    if (income.IncomeType != incomeType)
                        errors.Add($"Поле {nameof(income.IncomeType)} не соответствует сохраняемому типу {incomeType}");

                    if (income.ConfirmationDocumentTypeId == 0)
                        errors.Add($"Поле {nameof(income.ConfirmationDocumentTypeId)} не должно быть пустым");
                    else
                    {
                        if (!incomeTypeDomainValueIds.Contains(income.ConfirmationDocumentTypeId))
                            errors.Add($"Поле {nameof(income.ConfirmationDocumentTypeId)} имеет неверное значение");
                        //if (uniqueIncomeTypes.Contains(income.ConfirmationDocumentTypeId))
                        //    errors.Add($"Поле {nameof(income.ConfirmationDocumentTypeId)} должно быть уникальным");

                        uniqueIncomeTypes.Add(income.ConfirmationDocumentTypeId);
                    }

                    if (income.IncomeTurns <= 0)
                        errors.Add($"Поле {nameof(income.IncomeTurns)} должно быть положительным числом");

                    if (income.MonthQuantity <= 0)
                        errors.Add($"Поле {nameof(income.MonthQuantity)} должно быть положительным числом");

                    if (income.IncomeAmount <= 0)
                        errors.Add($"Поле {nameof(income.IncomeAmount)} должно быть положительным числом");

                    if (!income.FileRowId.HasValue && income.FileRowId.Value != 0)
                        errors.Add($"Поле {nameof(income.FileRowId)} не должно быть пустым");
                }
            }

            if (errors.Count > 0)
                throw new PawnshopApplicationException(errors.ToArray());
        }

        private decimal GetIncomeCalcRate(ClientIncomeCalculationSetting clientIncomeCalculationSetting)
        {
            return clientIncomeCalculationSetting.Rate / 100;
        }

        public ClientIncomeDto CalcIncomeAmount(ClientIncomeDto request)
        {
            _clientService.CheckClientExists(request.ClientId);

            var notionalRate = _clientIncomeCalculationSettingRepository.Find(new { DocumentTypeId = request.ConfirmationDocumentTypeId });
            var incomeCalcRate = GetIncomeCalcRate(notionalRate);

            request.IncomeAmount = Math.Round((request.IncomeTurns / request.MonthQuantity) / incomeCalcRate, 2);

            return request;
        }

        public decimal GetTotalFormalIncome(int clientId)
        {
            var clientIncomes = GetClientIncomes(clientId);
            var formalIncomesSum = clientIncomes.Where(x => x.IncomeType == IncomeType.Formal).Select(x => x.IncomeAmount).Sum();

            return formalIncomesSum;
        }

        public decimal GetTotalInformalApprovedIncome(int clientId)
        {
            var clientIncomes = GetClientIncomes(clientId);
            var informalApprovedIncomes = clientIncomes.Where(x => x.IncomeType == IncomeType.Informal).Select(x => x.IncomeAmount).Sum();

            return informalApprovedIncomes;
        }

        public decimal GetTotalInformalUnapprovedIncome(int clientId)
        {
            var clientAdditionalIncomes = _clientAdditionalIncomeService.Get(clientId);
            var clientAdditionalIncomesSum = clientAdditionalIncomes.Select(x => x.Amount).Sum();

            return clientAdditionalIncomesSum;
        }

        public NotionalRate GetNotionalRate(string domainCode, string domainValuesCode)
        {
            var domainValue = _domainService.GetDomainValue(domainCode, domainValuesCode);
            return _notionalRateRepository.GetByTypeOfLastYear(domainValue.Id);
        }

        private decimal GetNotionalRateValue(string domainCode, string domainValuesCode)
        {
            var domainValue = _domainService.GetDomainValue(domainCode, domainValuesCode);
            var notionalRate = _notionalRateRepository.GetByTypeOfLastYear(domainValue.Id);
            return notionalRate.RateValue;
        }

        public decimal GetFamilyDebt(int clientId)
        {
            int underageCount = 0;
            var clientProfile = _clientService.GetClientProfile(clientId);
            if (clientProfile != null)
                underageCount = clientProfile.UnderageDependentsCount.HasValue ? clientProfile.UnderageDependentsCount.Value : 0;

            var notionalRate = GetNotionalRateValue(Constants.NOTIONAL_RATE_TYPES, Constants.NOTIONAL_RATE_TYPES_VPM);

            return notionalRate + Constants.LIVING_WAGE_4_UNDERAGE * notionalRate * underageCount;
        }

        public decimal GetTotalFamilyDebt(int clientId)
        {
            int underageCount = 0;
            var clientProfile = _clientService.GetClientProfile(clientId);
            if (clientProfile != null)
            {
                underageCount = clientProfile.AdultDependentsCount.HasValue ? clientProfile.AdultDependentsCount.Value : 0;
                underageCount += clientProfile.UnderageDependentsCount.HasValue ? clientProfile.UnderageDependentsCount.Value : 0;
            }
            var notionalRate = GetNotionalRateValue(Constants.NOTIONAL_RATE_TYPES, Constants.NOTIONAL_RATE_TYPES_VPM);

            return notionalRate + Constants.LIVING_WAGE_4_UNDERAGE * notionalRate * underageCount;
        }

        public decimal GetClientExpenses(int clientId)
        {
            decimal clientExpenseAmt = 0;
            var clientExpense = _clientExpenseService.Get(clientId);
            if (clientExpense != null)
                clientExpenseAmt = (decimal)((clientExpense.Housing.HasValue ? clientExpense.Housing : 0) + (clientExpense.Vehicle.HasValue ? clientExpense.Vehicle : 0) + (clientExpense.Other.HasValue ? clientExpense.Other : 0));

            return clientExpenseAmt;
        }

        public decimal GetClientAllLoanExpense(int clientId)
        {
            var clientExpense = _clientExpenseService.Get(clientId);
            if (clientExpense != null)
                return clientExpense.AllLoan.HasValue ? clientExpense.AllLoan.Value : 0;

            return 0m;
        }

        public ClientExpense GetClientFullExpenses(int clientId)
        {
            return _clientExpenseService.Get(clientId);
        }

        public void SaveClientExpense(int clientId, decimal fcbDebt, decimal familyDebt, decimal avgPaymentToday)
        {
            using (var transaction = _clientExpenseService.BeginClientExpenseTransaction())
            {
                var clientExpense = _clientExpenseService.Get(clientId);

                if (clientExpense != null)
                {
                    clientExpense.Loan = (int)fcbDebt;
                    clientExpense.Family = (int)familyDebt;

                    var clientExpenseDto = new ClientExpenseDto()
                    {
                        Loan = (int)fcbDebt,
                        AllLoan = clientExpense.AllLoan,//ToDo здесь надо подставлять значение со шлюза
                        Other = clientExpense.Other,
                        Housing = clientExpense.Housing,
                        Family = (int)familyDebt,
                        Vehicle = clientExpense.Vehicle,
                        AvgPaymentToday = avgPaymentToday
                    };

                    _clientExpenseService.Save(clientId, clientExpenseDto);
                }

                transaction.Commit();
            }
        }

        public void RemoveIncomesAfterSign(int contractId, int clientId)
        {
            using var transaction = _clientIncomeRepository.BeginTransaction();
            var incomeListByClientId = _clientIncomeRepository
                                                    .GetListByClientId(clientId);

            foreach (var income in incomeListByClientId)
            {
                income.ContractId = contractId;
                income.DeleteDate = DateTime.Now;
                _clientIncomeRepository.Update(
                    income,
                    Constants.ADMINISTRATOR_IDENTITY,
                    Constants.INCOME_LINKED_CONTRACT_SIGNED,
                    OperationType.Delete);
            }

            transaction.Commit();
        }
    }
}
