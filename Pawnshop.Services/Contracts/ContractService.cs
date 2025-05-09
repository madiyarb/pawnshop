using Pawnshop.AccountingCore.Models;
using Pawnshop.Core.Exceptions;
using Pawnshop.Core.Extensions;
using Pawnshop.Core.Queries;
using Pawnshop.Core;
using Pawnshop.Data.Access;
using Pawnshop.Data.Models.Contracts;
using Pawnshop.Data.Models.Dictionaries;
using Pawnshop.Data.Models.Insurances;
using Pawnshop.Data.Models.LoanSettings;
using Pawnshop.Services.AccountingCore;
using Pawnshop.Services.Cars;
using Pawnshop.Services.Positions;
using Pawnshop.Services.Models.Calculation.APR;
using Pawnshop.Services.Models.Contracts;
using Pawnshop.Services.Models.Filters;
using Pawnshop.Services.Models.List;
using Pawnshop.Services.Models.Vehicle;
using static Pawnshop.Services.Contracts.LoanFinancePlans.LoanFinancePlanSerivce;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System;
using Pawnshop.Data.Models.Base;
using Pawnshop.Services.Insurance;
using Microsoft.Data.SqlClient;
using Pawnshop.Data.Models.MobileApp;
using Pawnshop.Data.Models.Contracts.Actions;
using Pawnshop.Services.Clients;
using Pawnshop.Data.Models.Audit;
using Pawnshop.Services.Audit;
using Pawnshop.Data.Access.AccountingCore;
using Pawnshop.Services.PaymentSchedules;
using Pawnshop.Services.Domains;
using Pawnshop.Data.Models.Restructuring;
using Pawnshop.Services.ClientDeferments.Interfaces;

namespace Pawnshop.Services.Contracts
{
    public class ContractService : IContractService
    {
        private readonly ContractRepository _contractRepository;
        private readonly AccountRepository _accountRepository;
        private readonly IAccountService _accountService;
        private readonly LoanPercentRepository _loanPercentRepository;
        private readonly ContractNumberCounterRepository _counterRepository;
        private readonly AnnuitySettingRepository _annuitySettingRepository;
        private readonly IContractPeriodVehicleLiquidityService _contractPeriodVehicleLiquidityService;
        private readonly ICarService _carService;
        private readonly IMachineryService _machineryService;
        private readonly DomainValueRepository _domainValueRepository;
        private readonly IPositionSubjectService _positionSubjectService;
        private readonly ContractPaymentScheduleRepository _contractPaymentScheduleRepository;
        private readonly IPositionService _positionService;
        private readonly UserRepository _userRepository;
        private readonly CarRepository _carRepository;
        private readonly IClientBlackListService _clientBlackListService;
        private readonly CategoryRepository _categoryRepository;
        private readonly IParkingActionService _parkingActionService;
        private readonly EventLogService _eventLogService;
        private readonly ISessionContext _sessionContext;
        private readonly TypeRepository _typeRepository;
        private readonly IPaymentScheduleService _paymentScheduleService;
        private readonly IDomainService _domainService;
        private readonly IClientIncomeService _clientIncomeService;
        private readonly IClientService _clientService;
        private readonly IClientDefermentService _clientDefermentService;
        private readonly GroupRepository _groupRepository;
        private readonly ContractKdnCalculationLogRepository _contractKdnCalculationLogRepository;

        private readonly CreditLineRepository _creditLineRepository;
        private readonly ParkingStatusRepository _parkingStatusRepository;
        private readonly ContractActionRepository _contractActionRepository;
        private readonly ContractExpenseRepository _contractExpenseRepository;

        public ContractService(
            ContractRepository contractRepository,
            IAccountService accountService,
            LoanPercentRepository loanPercentRepository,
            ContractNumberCounterRepository counterRepository,
            AnnuitySettingRepository annuitySettingRepository,
            IContractPeriodVehicleLiquidityService contractPeriodVehicleLiquidityService,
            ICarService carService,
            IMachineryService machineryService,
            DomainValueRepository domainValueRepository,
            IPositionSubjectService positionSubjectService,
            ContractPaymentScheduleRepository contractPaymentScheduleRepository,
            IPositionService positionService,
            UserRepository userRepository,
            CarRepository carRepository,
            IClientBlackListService clientBlackListService,
            CategoryRepository categoryRepository,
            IParkingActionService parkingActionService,
            ISessionContext sessionContext,
            EventLogService eventLogService,
            IDomainService domainService,
            IClientIncomeService clientIncomeService,
            IClientService clientService,
            IClientDefermentService clientDefermentService,
            TypeRepository typeRepository,
            IPaymentScheduleService paymentScheduleService,
            CreditLineRepository creditLineRepository,
            ContractKdnCalculationLogRepository contractKdnCalculationLogRepository,
            ParkingStatusRepository parkingStatusRepository,
            ContractActionRepository contractActionRepository,
            ContractExpenseRepository contractExpenseRepository,
            AccountRepository accountRepository)
        {
            _contractRepository = contractRepository;
            _accountService = accountService;
            _loanPercentRepository = loanPercentRepository;
            _counterRepository = counterRepository;
            _annuitySettingRepository = annuitySettingRepository;
            _contractPeriodVehicleLiquidityService = contractPeriodVehicleLiquidityService;
            _carService = carService;
            _machineryService = machineryService;
            _domainValueRepository = domainValueRepository;
            _positionSubjectService = positionSubjectService;
            _contractPaymentScheduleRepository = contractPaymentScheduleRepository;
            _positionService = positionService;
            _userRepository = userRepository;
            _carRepository = carRepository;
            _clientBlackListService = clientBlackListService;
            _categoryRepository = categoryRepository;
            _parkingActionService = parkingActionService;
            _sessionContext = sessionContext;
            _eventLogService = eventLogService;
            _typeRepository = typeRepository;
            _paymentScheduleService = paymentScheduleService;
            _domainService = domainService;
            _clientIncomeService = clientIncomeService;
            _clientService = clientService;
            _clientDefermentService = clientDefermentService;
            _creditLineRepository = creditLineRepository;
            _contractKdnCalculationLogRepository = contractKdnCalculationLogRepository;
            _parkingStatusRepository = parkingStatusRepository;
            _contractActionRepository = contractActionRepository;
            _contractExpenseRepository = contractExpenseRepository;
            _accountRepository = accountRepository;
        }

        public IDbTransaction BeginContractTransaction()
        {
            return _contractRepository.BeginTransaction();
        }

        public void Delete(int id) => _contractRepository.Delete(id);

        public Contract Find(ContractFilter filter) => _contractRepository.Find(filter);

        public Contract Get(int id, DateTime? date = null)
        {
            if (!date.HasValue)
                date = DateTime.Now;

            Contract contract = _contractRepository.Get(id);
            if (contract != null)
            {
                decimal accountBalance = GetAccountBalance(contract.Id, date);
                decimal overdueBalance = GetOverdueAccountBalance(contract.Id, date);
                contract.LeftLoanCost = accountBalance + overdueBalance;
            }
            this.FillPositionSubjectsAndHasPledge(contract);

            return contract;
        }

        public async Task<Contract> GetAsync(int id, DateTime? date = null)
        {
            if (!date.HasValue)
                date = DateTime.Now;

            Contract contract = _contractRepository.Get(id);
            if (contract != null)
            {
                decimal accountBalance = GetAccountBalance(contract.Id, date);
                decimal overdueBalance = GetOverdueAccountBalance(contract.Id, date);
                contract.LeftLoanCost = accountBalance + overdueBalance;
            }

            return contract;
        }

        public async Task<Contract> GetOnlyContractAsync(int contractId, bool withoutCheckContract = false)
        {
            Contract contract = await _contractRepository.GetOnlyContractAsync(contractId);
            if (!withoutCheckContract && (contract == null || contract.DeleteDate.HasValue))
                throw new PawnshopApplicationException($"Договор с идентификатором {contractId} не найден");

            return contract;
        }

        public ListModel<Contract> ListWithCount(ListQueryModel<ContractFilter> listQuery)
        {
            return new ListModel<Contract>
            {
                List = _contractRepository.List(listQuery, listQuery.Model),
                Count = _contractRepository.Count(listQuery, listQuery.Model)
            };
        }

        public List<ContractPaymentSchedule> GetOnlyPaymentSchedule(int ContractId) =>
            _contractRepository.GetOnlyPaymentSchedule(ContractId);

        public List<Contract> List(ContractFilter filter) => _contractRepository.List(new ListQuery { Page = null }, filter);

        public int Count(ContractFilter filter) => _contractRepository.Count(new ListQuery { Page = null }, filter);

        public Contract Save(Contract model)
        {
            if (model.Id > 0) _contractRepository.Update(model);
            else _contractRepository.Insert(model);

            return model;
        }

        public Task<List<Contract>> FindForProcessingAsync(int clientId) => Task.Run(() => _contractRepository.FindForProcessing(clientId));

        public Task<List<Contract>> FindCreditLinesForProcessing(int clientId) => Task.Run(() => _contractRepository.FindCreditLinesForProcessing(clientId));

        public decimal GetPrepaymentBalance(int contractId, DateTime? date = null)
        {
            return GetContractAccountBalance(contractId, Constants.ACCOUNT_SETTING_DEPO, date);
        }

        public decimal GetReceivableOnlinePaymentBalance(int contractId, DateTime? date = null)
        {
            return GetContractAccountBalance(contractId, Constants.ACCOUNT_SETTING_RECEIVABLE_ONLINEPAYMENT, date);
        }

        public decimal GetExtraExpensesCost(int contractId, DateTime? date = null)
        {
            return GetContractAccountBalance(contractId, Constants.ACCOUNT_SETTING_EXPENSE, date);
        }

        public decimal GetContractAccountBalance(int contractId, string accountSettingCode, DateTime? date = null)
        {
            if (!date.HasValue)
                date = DateTime.Now;

            Contract contract = _contractRepository.GetOnlyContract(contractId);
            if (contract == null || contract.DeleteDate.HasValue)
                throw new PawnshopApplicationException($"Договор с идентификатором {contractId} не найден");

            var accountListQueryModel = new ListQueryModel<AccountFilter>
            {
                Page = null,
                Model = new AccountFilter
                {
                    ContractId = contract.Id,
                    IsOpen = true,
                    SettingCodes = new List<string> { accountSettingCode }.ToArray()
                }
            };
            ListModel<Account> accountListModel = _accountService.List(accountListQueryModel);
            if (accountListModel == null)
                throw new PawnshopApplicationException($"Ожидалось что {nameof(accountListModel)} не будет null");

            if (accountListModel.List == null)
                throw new PawnshopApplicationException($"Ожидалось что {nameof(accountListModel)}.{nameof(accountListModel.List)} не будет null");

            List<Account> accounts = accountListModel.List;
            Account account = accounts.Find(x => x.AccountSetting.Code == accountSettingCode && !x.DeleteDate.HasValue && !x.CloseDate.HasValue);
            if (account == null)
                return 0;

            return _accountService.GetAccountBalance(account.Id, date.Value);
        }

        public decimal GetAccountBalance(int contractId, DateTime? date = null)
        {
            return GetContractAccountBalance(contractId, Constants.ACCOUNT_SETTING_ACCOUNT, date);
        }

        public decimal GetDepoMerchantBalance(int contractId, DateTime? date = null)
        {
            return GetContractAccountBalance(contractId, Constants.ACCOUNT_SETTING_DEPO_MERCHANT, date);
        }

        public decimal GetOverdueAccountBalance(int contractId, DateTime? date = null)
        {
            return GetContractAccountBalance(contractId, Constants.ACCOUNT_SETTING_OVERDUE_ACCOUNT, date);
        }

        public decimal GetProfitBalance(int contractId, DateTime? date = null)
        {
            return GetContractAccountBalance(contractId, Constants.ACCOUNT_SETTING_PROFIT, date);
        }

        public decimal GetOverdueProfitBalance(int contractId, DateTime? date = null)
        {
            return GetContractAccountBalance(contractId, Constants.ACCOUNT_SETTING_OVERDUE_PROFIT, date);
        }

        public decimal GetPenyAccountBalance(int contractId, DateTime? date = null)
        {
            return GetContractAccountBalance(contractId, Constants.ACCOUNT_SETTING_PENY_ACCOUNT, date);
        }

        public decimal GetPenyProfitBalance(int contractId, DateTime? date = null)
        {
            return GetContractAccountBalance(contractId, Constants.ACCOUNT_SETTING_PENY_PROFIT, date);
        }

        public decimal GetPenaltyLimitBalance(int contractId, DateTime? date = null) =>
            GetContractAccountBalance(contractId, Constants.ACCOUNT_SETTING_PENALTY_LIMIT, date);

        /// <summary>
        /// Получить сумму общей задолженности договора
        /// </summary>
        /// <param name="contractId">Идентификатор договора</param>
        /// <param name="date">Дата</param>
        /// <returns></returns>
        public decimal GetTotalDue(int contractId, DateTime? date = null)
        {
            if (!date.HasValue)
                date = DateTime.Now;

            decimal totalDue = 0;
            totalDue += GetExtraExpensesCost(contractId, date);
            totalDue += GetAccountBalance(contractId, date);
            totalDue += GetOverdueAccountBalance(contractId, date);
            totalDue += GetProfitBalance(contractId, date);
            totalDue += GetOverdueProfitBalance(contractId, date);
            totalDue += GetPenyAccountBalance(contractId, date);
            totalDue += GetPenyProfitBalance(contractId, date);

            return totalDue;
        }

        public List<Contract> GetContractsByPaymentScheduleFilter(DateTime? fromDate, DateTime? endDate, IEnumerable<ContractStatus> contractStatuses, IEnumerable<PercentPaymentType> neededPercentPaymentTypes = null)
        {
            if (!fromDate.HasValue && !endDate.HasValue)
                throw new ArgumentException($"Один из аргументов {nameof(fromDate)}, {nameof(endDate)} должен быть заполнен");

            return _contractRepository.GetContractsByPaymentScheduleFilter(fromDate, endDate, contractStatuses, neededPercentPaymentTypes);
        }

        public void AllowContractPrepaymentReturn(Contract contract)
        {
            switch (contract.Status)
            {
                case ContractStatus.Signed: throw new PawnshopApplicationException("Возврат предоплаты по действующему договору невозможен");

                case ContractStatus.BoughtOut:
                case ContractStatus.SoldOut:
                case ContractStatus.Disposed:
                    {
                        if (_contractRepository.List(new ListQuery { Page = null }, new { contract.ClientId }).Any(c => c.Status == ContractStatus.Signed || c.Status == ContractStatus.SoldOut))
                            throw new PawnshopApplicationException("Возврат предоплаты не возможен, так как у клиента есть действующие договора");
                        break;
                    }
            }
        }

        public decimal GetDebtAndDebtOverdueBalanceForClient(int clientId, string productCode)
        {
            decimal debtCostLeftFromAllContracts = 0;
            var сontractsByClient = _contractRepository.GetContractsAccordingProductTypeForClient(productCode, clientId);

            сontractsByClient.ForEach(c =>
            {
                debtCostLeftFromAllContracts += GetContractAccountBalance(c.Id, Constants.ACCOUNT_SETTING_ACCOUNT, DateTime.Now);
                debtCostLeftFromAllContracts += GetContractAccountBalance(c.Id, Constants.ACCOUNT_SETTING_OVERDUE_ACCOUNT, DateTime.Now);
            });

            return debtCostLeftFromAllContracts;
        }

        public LoanPercentSetting GetProductSettings(int settingId) => _loanPercentRepository.Get(settingId);

        public decimal GetAvailableBalanceForDAMU(AvailBalanceRequest availBalancerequest)
        {
            decimal debtCostLeftFromAllContracts = GetDebtAndDebtOverdueBalanceForClient(availBalancerequest.ClientId, Constants.PRODUCT_DAMU);
            decimal loanCostTo = GetProductSettings(availBalancerequest.SettingId).LoanCostTo;

            return loanCostTo - debtCostLeftFromAllContracts;
        }

        public Contract GetOnlyContract(int contractId, bool withoutCheckContract = false)
        {
            Contract contract = _contractRepository.GetOnlyContract(contractId);
            if (!withoutCheckContract && (contract == null || contract.DeleteDate.HasValue))
                throw new PawnshopApplicationException($"Договор с идентификатором {contractId} не найден");
            return contract;
        }

        public void ContractStatusUpdate(int contractId, ContractStatus status)
        {
            Contract contract = _contractRepository.GetOnlyContract(contractId);
            if (contract == null || contract.DeleteDate.HasValue)
                throw new PawnshopApplicationException($"Договор с идентификатором {contractId} не найден");

            _contractRepository.ContractStatusUpdate(contractId, status);

        }

        public LoanPercentSetting GetSettingForContract(Contract contract, int? settingId)
        {
            if (!settingId.HasValue)
                return GetSettingForContract(contract);
            else
                return GetSettingById(settingId.Value);
        }

        public LoanPercentSetting GetSettingById(int settingId)
        {
            var setting = _loanPercentRepository.Get(settingId);

            if (setting == null)
                throw new PawnshopApplicationException($"Не найден шаблон настроек для Id {settingId}. Обратитесь к администратору.");

            return setting;
        }

        public LoanPercentSetting GetSettingForContract(Contract contract, decimal? loanCost = null)
        {
            var setting = contract.SettingId.HasValue ? contract.Setting : _loanPercentRepository.Find(new
            {
                contract.BranchId,
                contract.CollateralType,
                contract.Client.CardType,
                LoanCost = loanCost ?? contract.LoanCost,
                contract.LoanPeriod,
                IsProduct = false,
                IsActual = true
            }) ?? _loanPercentRepository.Find(new
            {
                contract.BranchId,
                contract.CollateralType,
                contract.Client.CardType,
                contract.LoanPeriod,
                IsProduct = false,
                IsActual = true
            });

            if (setting == null)
                throw new PawnshopApplicationException($"Не найден шаблон настроек для договора {contract.ContractNumber}. Обратитесь к администратору.");

            return setting;
        }

        public List<Contract> GetContractsForDecreasePenaltyRates(DateTime? date)
        {
            if (!date.HasValue)
                throw new ArgumentException($"Входящий параметр {nameof(date)} должен быть заполнен");

            return _contractRepository.GetContractsForDecreasePenaltyRates((DateTime)date);
        }

        public decimal GetChildContractCost(int contractId, DateTime actionDate, decimal actionCost)
        {
            decimal parentContractDebtCost = 0;
            parentContractDebtCost += GetAccountBalance(contractId, actionDate);
            parentContractDebtCost += GetOverdueAccountBalance(contractId, actionDate);

            decimal childContractCost = actionCost + parentContractDebtCost;
            decimal prepaymentBalance = GetPrepaymentBalance(contractId, actionDate);

            if (prepaymentBalance > 0 && prepaymentBalance > childContractCost)
                throw new PawnshopApplicationException($"Сумма аванса {prepaymentBalance} больше задолженности по договору {childContractCost}. Воспользуйтесь операцией ОПЛАТА");

            return childContractCost;
        }

        public string GenerateContractNumber(DateTime contractDate, int branchId, string numberCode)
        {
            return _counterRepository.Next(contractDate.Year, branchId, numberCode);
        }

        public decimal GetPreApprovedAmount(int modelId, int releaseYear)
        {
            var listEstimatedCosts = _contractRepository.GetEsimatedCostsForPreAprovedAmount(modelId, releaseYear);

            if (listEstimatedCosts is null || !listEstimatedCosts.Any())
                throw new PawnshopApplicationException($"Не найдены договора для модели с id = {modelId} и годом = {releaseYear}");

            decimal index = (((decimal)listEstimatedCosts.Count + 1) / 2) - 1;

            if (index % 1 != 0)
            {
                index = Math.Truncate(index);

                var firstNumber = listEstimatedCosts[(int)index];
                var secondNumber = listEstimatedCosts[(int)index + 1];

                return (firstNumber + secondNumber) / 2;
            }

            return listEstimatedCosts[(int)index];
        }

        public Boolean AreAllRelatedContractsBoughtOut(int contractId)
        {
            /*
             *  1) if current contract is in closedParents
             *  2) if current contract has closedParents
             *   2.1) if there are other contracts with ClosedParents
             */
            var contract = _contractRepository.GetOnlyContract(contractId);
            if (contract.Status != ContractStatus.BoughtOut)
                return false;

            if (contract.ClosedParentId != null)
            {
                var parent = _contractRepository.GetOnlyContract((int)contract.ClosedParentId);
                if (parent.Status != ContractStatus.BoughtOut)
                    return false;

                var relatedContracts = _contractRepository.GetChildrenContracts(parent.Id);
                foreach (var c in relatedContracts)
                {
                    if (c.Status != ContractStatus.BoughtOut)
                        return false;
                }

            }
            else
            {
                var childrenContracts = _contractRepository.GetChildrenContracts(contractId);

                foreach (var child in childrenContracts)
                {
                    if (child.Status != ContractStatus.BoughtOut)
                    {
                        return false;
                    }
                }
            }
            return true;
        }

        public ContractModel ChangeForPensioner(ContractModel model)
        {
            var policeRequest = model.PoliceRequests.OrderByDescending(t => t.CreateDate)
    .FirstOrDefault(t => t.Status != InsuranceRequestStatus.Rejected);

            if (policeRequest == null)
                throw new PawnshopApplicationException("Не найдена заявка на страхование");

            policeRequest.IsInsuranceRequired = false;
            policeRequest.CancelReason = "Пенсионер";

            if (policeRequest.RequestData.InsurancePremium != 0)
                policeRequest.RequestData.AmountToDecrease4Pensioner = policeRequest.RequestData.InsurancePremium;

            model.Contract.LoanCost = policeRequest.RequestData.LoanCost - (policeRequest.RequestData.AmountToDecrease4Pensioner ?? 0);

            policeRequest.CancelDate = DateTime.Now;
            policeRequest.CancelledUserId = Constants.ADMINISTRATOR_IDENTITY;
            policeRequest.RequestData.InsurancePremium = 0;

            return model;
        }

        public void SetBuyoutReasonForContractAction(Contract contract)
        {
            if (contract.Status == ContractStatus.BoughtOut)
            {
                foreach (var action in contract.Actions)
                {
                    action.BuyoutReasonId = contract.BuyoutReasonId;
                }
            }
        }

        public List<Contract> GetActiveContractsByClientId(int clientId, int contractId)
        {
            return _contractRepository.GetContractsByClientIdAndContractId(clientId, contractId, new List<ContractStatus> { ContractStatus.Signed });
        }

        public List<Contract> GetActiveContractsByClientId(int clientId)
        {
            return _contractRepository.GetContractsByClientId(clientId, new List<ContractStatus> { ContractStatus.Signed });
        }

        public Contract FillPositions4Contract(int contractId)
        {
            return _contractRepository.GetContractPositions(contractId);
        }

        public int GetPaymentsCount(Contract contract)
        {
            var beginDate = contract.SignDate?.Date ?? contract.ContractDate.Date;
            var maturityDate = contract.MaturityDate;
            return maturityDate.MonthDifferenceKdn(beginDate);
        }

        public void CheckSchedule(Contract contract, bool isChangeCD = false)
        {
            if (Math.Round(contract.PaymentSchedule.LastOrDefault().DebtLeft) != 0) throw new PawnshopApplicationException("Ошибка графика - остаток основного долга в конце не равен 0");
            object query = new { CollateralType = contract.CollateralType };
            var setting = _annuitySettingRepository.Find(query);

            DateTime minDate = contract.ContractDate.AddMonths(1);
            DateTime maxDate = contract.ContractDate.AddMonths(1);

            if (setting != null)
            {
                minDate = contract.ContractDate.AddDays(setting.MinDayCount);
                maxDate = contract.ContractDate.AddDays(setting.MaxDayCount);

                if (setting.CertainDay != null && setting.CertainDay > 0)
                {
                    DateTime _firstDate = new DateTime(minDate.Year, minDate.Month, (int)setting.CertainDay);
                    DateTime _secondDate = new DateTime(maxDate.Year, maxDate.Month, (int)setting.CertainDay);

                    if (DateTime.Compare(_firstDate.Date, minDate.Date) >= 0 && DateTime.Compare(_firstDate.Date, maxDate.Date) <= 0)
                    {
                        minDate = _firstDate;
                        maxDate = _firstDate;
                    }

                    if (DateTime.Compare(_secondDate.Date, minDate.Date) >= 0 && DateTime.Compare(_secondDate.Date, maxDate.Date) <= 0)
                    {
                        minDate = _secondDate;
                        maxDate = _secondDate;
                    }
                }
            }

            if (contract.FirstPaymentDate == null)
            {
                contract.FirstPaymentDate = contract.PaymentSchedule.FirstOrDefault().Date.Date;
            }

            if (!isChangeCD && contract.ContractClass != ContractClass.Tranche && contract.Id > 0 &&
                (contract.FirstPaymentDate.Value.Date < minDate.Date || contract.FirstPaymentDate.Value.Date > maxDate.Date))
                throw new PawnshopApplicationException("Ошибка графика - дата первого платежа находится вне диапазона дат");

            if (contract.PaymentSchedule.Where(x => x.Period <= 0).Count() > 0) throw new PawnshopApplicationException("Ошибка графика - период не может быть равен 0");

            if (contract.PaymentSchedule.FirstOrDefault().PercentCost ==
                contract.PaymentSchedule.LastOrDefault().PercentCost)
            {
                contract.AnnuityType = AnnuityType.EqualPayments;
            }
            else if (contract.PaymentSchedule.FirstOrDefault().PercentCost !=
                     contract.PaymentSchedule.LastOrDefault().PercentCost &&
                     contract.PaymentSchedule.FirstOrDefault().Date.Date != contract.ContractDate.Date.AddMonths(1))
            {
                contract.AnnuityType = AnnuityType.AnnuityWithDateSelect;
            }
            else
            {
                contract.AnnuityType = AnnuityType.Annuity;
            }
        }

        public void CheckNumberOfPositionsForContractCarCollateralType(Contract contract)
        {
            if (contract.CollateralType == CollateralType.Car && contract.Positions.Count > Constants.MAX_NUMBER_OF_POSITIONS_WITH_CAR_COLLATERAL_TYPE_CONTRACT)
            {
                throw new PawnshopApplicationException($"Количество позиций по договору не должно превышать {Constants.MAX_NUMBER_OF_POSITIONS_WITH_CAR_COLLATERAL_TYPE_CONTRACT}");
            }
        }

        public void CheckPositionsForDuplicates(Contract contract)
        {
            if (contract.Positions.Count() != contract.Positions.Select(p => p.PositionId).Distinct().Count())
            {
                throw new PawnshopApplicationException("Залог не должен повторяться");
            }
        }

        public void CheckFirstPositionCollateralTypeSameAsContractCollateralType(Contract contract)
        {
            if (contract.CollateralType != null && contract.Positions != null && contract.Positions.Count() > 0 &&
                contract.CollateralType != contract.Positions.FirstOrDefault().Position.CollateralType)
            {
                throw new PawnshopApplicationException("Вид залога контракта должен быть одинаков с видом залога первой позиции");
            }
        }

        public async Task FillCollateralCostForContractPositions(Contract contract)
        {
            foreach (var pos in contract.Positions)
            {
                if (pos.Position.CollateralType == CollateralType.Realty)
                {
                    Realty realty = (Realty)pos.Position;
                    var realtyType = _domainValueRepository.Get(realty.RealtyTypeId);
                    var code = realtyType.Code;
                    var productSetting = contract.Setting;
                    var requiredLTVForPosition = productSetting.ProductTypeLTVs?.Where(x => x.SubCollateralType.Code == code).FirstOrDefault();

                    if (requiredLTVForPosition == null)
                        throw new PawnshopApplicationException($"Для данного продукта недвижимости не настроен LTV на следующий тип недвижимости : {realtyType.Name}. Обратитесь в поддержку");

                    pos.CollateralCost = Decimal.Multiply((decimal)pos.EstimatedCost, (decimal)requiredLTVForPosition.LTV);
                }
            }
        }

        //метод для заполнения номера договора позиций
        public void FillPositionContractNumbers(Contract contract)
        {
            if (contract.Positions.Count == 1)
            {
                contract.Positions.FirstOrDefault().PositionContractNumber = contract.ContractNumber;
                return;
            }

            int counter = 1;
            foreach (var position in contract.Positions)
            {
                position.PositionContractNumber = contract.ContractNumber + "/" + counter;
                counter++;
            }
        }

        public List<ContractPosition> FillPositionContractNumbers(Contract contract, List<ContractPosition> positions)
        {
            var updatedPositions = new List<ContractPosition>();

            if (positions.Count() == 1)
            {
                var position = positions.FirstOrDefault();
                position.PositionContractNumber = contract.ContractNumber;
                updatedPositions.Add(position);
            }
            else
            {
                int counter = 1;
                foreach (var position in positions)
                {
                    position.PositionContractNumber = contract.ContractNumber + "/" + counter;
                    updatedPositions.Add(position);
                    counter++;
                }
            }

            return updatedPositions;
        }

        public PositionDetails GetContractPositionDetail(ContractPosition contractPosition)
        {
            string mark = $"Не присвоена марка для позиции с Id {contractPosition.PositionId}";
            string model = $"Не присвоена модель для позиции с Id {contractPosition.PositionId}";
            string vehicleType = "автомобиля";
            bool isDisabled = false;
            int vehicleMarkId = 0, vehicleModelId = 0, releaseYear = 0;

            if (contractPosition.Position.CollateralType == CollateralType.Car)
            {
                vehicleType = "автомобиля";
                Car car = null;
                if (contractPosition.Position is Car)
                    car = (Car)contractPosition.Position;

                if (contractPosition.Position is Position)
                    car = _carService.Get(contractPosition.Position.Id);

                mark = car.Mark;
                model = car.Model;
                isDisabled = car.VehicleModel.IsDisabled;
                vehicleMarkId = car.VehicleMarkId;
                vehicleModelId = car.VehicleModelId;
                releaseYear = car.ReleaseYear;
            };

            if (contractPosition.Position.CollateralType == CollateralType.Machinery)
            {
                vehicleType = "спецтехники";
                Machinery machinery = null;
                if (contractPosition.Position is Car)
                    machinery = (Machinery)contractPosition.Position;

                if (contractPosition.Position is Position)
                    machinery = _machineryService.Get(contractPosition.Position.Id);

                mark = machinery.Mark;
                model = machinery.Model;
                isDisabled = machinery.VehicleModel.IsDisabled;
                vehicleMarkId = machinery.VehicleMarkId;
                vehicleModelId = machinery.VehicleModelId;
                releaseYear = machinery.ReleaseYear;
            };

            return new PositionDetails()
            {
                Mark = mark,
                Model = model,
                VehicleType = vehicleType,
                IsDisabled = isDisabled,
                VehicleMarkId = vehicleMarkId,
                VehicleModelId = vehicleModelId,
                ReleaseYear = releaseYear
            };
        }

        public void CheckPositionLiquidity(Contract contract)
        {
            foreach (var position in contract.Positions)
            {
                if (position.Position.CollateralType != CollateralType.Car && position.Position.CollateralType != CollateralType.Machinery) return;

                var contractPositionDetails = GetContractPositionDetail(position);

                int liquidity = _contractPeriodVehicleLiquidityService.GetLiquidityByVehicleMarkAndModel(contractPositionDetails.ReleaseYear, contractPositionDetails.VehicleMarkId, contractPositionDetails.VehicleModelId);

                if (liquidity == 0)
                    throw new PawnshopApplicationException(
                        $"Ликвидность для {contractPositionDetails.VehicleType} марки {contractPositionDetails.Mark} и модели {contractPositionDetails.Model} не определена");
            }
        }

        private void CheckMaxMonthsCount(Contract contract, int maxMonthsCount, PositionDetails positionDetails)
        {
            int periodType = contract.Setting != null ? (int)contract.Setting.ContractPeriodFromType : (int)PeriodType.Month;

            int contractMonthCount = contract.LoanPeriod / periodType;

            if (contractMonthCount > maxMonthsCount)
                throw new PawnshopApplicationException($"Указанный в заявке срок кредита {contractMonthCount} мес. превышает для автотранспорта {positionDetails.Mark}, {positionDetails.Model}, {positionDetails.ReleaseYear} максимально разрешенный срок - {maxMonthsCount} мес.");
        }

        public void CheckMaxPossibleContractPeriod(Contract contract)
        {
            if (contract.Status >= ContractStatus.Signed || contract.PartialPaymentParentId != null) return;

            if (contract.ContractClass == ContractClass.CreditLine || contract.ContractClass == ContractClass.Tranche ||
                contract.Setting.ContractClass == ContractClass.CreditLine || contract.Setting.ContractClass == ContractClass.Tranche)
            {
                if (contract.Id == 0)
                {
                    var contractSetting = _loanPercentRepository.Get(contract.SettingId.Value);
                    if (contractSetting != null && !contractSetting.IsLiquidityOn)
                        return;
                }
                else
                {
                    var contractSetting = GetContractSettings(contract.Id);
                    if (!contractSetting.IsLiquidityOn)
                        return;
                }
            }

            foreach (var position in contract.Positions)
            {
                if (position.Position.CollateralType != CollateralType.Car && position.Position.CollateralType != CollateralType.Machinery) return;

                var contractPositionDetails = GetContractPositionDetail(position);

                var maxPossibleContractPeriod = _contractPeriodVehicleLiquidityService.GetPeriodByLiquidity(contractPositionDetails.ReleaseYear, contractPositionDetails.VehicleMarkId, contractPositionDetails.VehicleModelId);

                CheckMaxMonthsCount(contract, maxPossibleContractPeriod.MaxMonthsCount, contractPositionDetails);
            }
        }

        public void FillPositionSubjectsAndHasPledge(Contract contract)
        {

            foreach (var position in contract.Positions)
            {
                if (contract.Status < ContractStatus.Signed)
                {
                    position.Position.PositionSubjects = _positionSubjectService.GetSubjectsForPosition(position.PositionId);
                }
                else
                {
                    if (position.Position.CollateralType != CollateralType.Realty)
                        continue;

                    var positionSubjects = _positionSubjectService.GetPositionSubjectsForPositionAndDate(position.PositionId, contract.SignDate ?? contract.ContractDate).Result;
                    var mainPledger = positionSubjects.Where(x => x.Subject.Code == Constants.MAIN_PLEDGER_CODE).FirstOrDefault();
                    if (mainPledger != null)
                        position.Position.Client = mainPledger.Client;

                    position.Position.PositionSubjects = positionSubjects.Where(x => x.Subject.Code == Constants.PLEDGER_CODE).ToList();
                }
                position.Position.HasUsedPledge = _positionService.HasUsedPledge(position.PositionId).Result;
            }
        }

        public async Task<int> GetCreditLineId(int contractId)
        {
            return await _contractRepository.GetCreditLineId(contractId);
        }

        public async Task<decimal> GetCreditLineLimit(int contractId)
        {
            return GetContractAccountBalance(contractId, Constants.ACCOUNT_SETTING_CREDIT_LINE_LIMIT);
        }

        public async Task<int> GetActiveTranchesCount(int trancheContractId)
        {
            return await _contractRepository.GetActiveTranchesCount(trancheContractId);
        }

        public async Task<List<Contract>> GetAllSignedTranches(int creditLineContractId)
        {
            return await _contractRepository.GetAllSignedTranches(creditLineContractId);
        }

        public async Task<List<Contract>> GetAllTranchesAsync(int creditLineContractId)
        {
            return await _contractRepository.GetAllSignedTranches(creditLineContractId, false);
        }

        public IList<ContractTrancheInfo> GetTranches(int creditLineId)
        {
            var tranchesList = _contractRepository.GetTranchesAndBalanceByCreditLineId(creditLineId);

            if (!tranchesList.Any())
                return new List<ContractTrancheInfo>();

            var tranchesIds = tranchesList.Select(x => x.Id).ToList();

            var tranchesBalance = _contractRepository.GetBalances(tranchesIds);
            var restructedTranchebalance = _accountRepository.GetWithRestructedBalances(tranchesIds);

            foreach (var tranche in tranchesList)
            {
                var clientDeferment = _clientDefermentService.GetActiveDeferment(tranche.Id);
                IList<ContractBalance> currentBalance = clientDeferment == null
                ? new List<ContractBalance> { tranchesBalance.FirstOrDefault(x => x.ContractId == tranche.Id) }
                : new List<ContractBalance> { restructedTranchebalance.FirstOrDefault(x => x.ContractId == tranche.Id) };

                var balance = currentBalance.FirstOrDefault(); 

                if (balance == null)
                {
                    tranche.AmortizedDebtInfo = new AmortizedDebtInfo();
                    tranche.UrgentDebt = new DebtInfo();
                    tranche.OverdueDebt = new DebtInfo();
                    tranche.TotalDebt = new DebtInfo();
                    continue;
                }

                if (clientDeferment != null) {
                    tranche.AmortizedDebtInfo = new AmortizedDebtInfo
                    {
                        DefermentProfit = balance.DefermentProfit,
                        AmortizedProfit = balance.AmortizedProfit,
                        AmortizedDebtPenalty = balance.AmortizedDebtPenalty,
                        AmortizedLoanPenalty = balance.AmortizedLoanPenalty,
                        ClientDeferment = clientDeferment
                    }; 
                } else 
                {
                    tranche.AmortizedDebtInfo = new AmortizedDebtInfo();
                }

                tranche.UrgentDebt = new DebtInfo
                {
                    PrincipalDebt = balance.AccountAmount,
                    Profit = balance.ProfitAmount
                };

                tranche.OverdueDebt = new DebtInfo
                {
                    PrincipalDebt = balance.OverdueAccountAmount,
                    Profit = balance.OverdueProfitAmount,
                    Penalty = balance.PenyAmount
                };

                tranche.TotalDebt = new DebtInfo
                {
                    PrincipalDebt = balance.TotalAcountAmount,
                    Profit = balance.TotalProfitAmount,
                    Penalty = balance.PenyAmount
                };

                tranche.ExpenseAmount = balance.ExpenseAmount;
                tranche.PrepaymentBalance = balance.PrepaymentBalance;
                tranche.TotalRepaymentAmount = balance.TotalRepaymentAmount;
                tranche.TotalRedemptionAmount = balance.TotalRedemptionAmount;
            }

            return tranchesList;
        }

        public async Task CalculateAPR(Contract contract)
        {
            double loanCostDouble = Convert.ToDouble(contract.LoanCost);
            var calculator = new APRCalculator(loanCostDouble);

            if (contract.ContractClass == ContractClass.CreditLine)
                return;

            if (contract.PercentPaymentType == PercentPaymentType.EndPeriod)
            {
                calculator.AddInstalment(
                    (double)(contract.LoanCost + (contract.LoanPercentCost * contract.LoanPeriod)),
                    (contract.MaturityDate - contract.ContractDate).Days + (contract.Locked ? 1 : 0));
                contract.APR = (decimal)calculator.Calculate();
            }

            contract.PaymentSchedule.ToList().ForEach(payment =>
            {
                calculator.AddInstalment((double)(payment.DebtCost + payment.PercentCost), (payment.Date - contract.ContractDate).Days);
            });

            contract.APR = (decimal)calculator.Calculate();
        }

        public decimal CalculateAPRAfterRestructuring(CalculateAPRModel model)
        {
            var calculator = new APRCalculator(model.LoanCost);
            model.ScheduleData.ForEach(payment =>
            {
                calculator.AddInstalment(payment.SchedulePaymentAmount, payment.ScheduleDays);
            });
            var apr = (decimal)calculator.Calculate();

            return apr;
        }

        public async Task<List<ContractPosition>> GetPositionsByContractIdAsync(int contractId)
        {
            return await _contractRepository.GetPositionsByContractIdAsync(contractId);
        }

        public List<ContractPosition> GetPositionsByContractId(int contractId)
        {
            return _contractRepository.GetPositionsByContractId(contractId);
        }

        public async Task<Contract> GetNonCreditLineByNumberAsync(string contractNumber)
        {
            return await _contractRepository.GetNonCreditLineByNumberAsync(contractNumber);
        }

        public async Task<Contract> GetCreditLineByNumberAsync(string contractNumber)
        {
            return await _contractRepository.GetCreditLineByNumberAsync(contractNumber);
        }

        public AnnuityType GetAnnuityType(Contract contract)
        {
            if (contract.PaymentSchedule.FirstOrDefault().PercentCost ==
                contract.PaymentSchedule.LastOrDefault().PercentCost)
                return AnnuityType.EqualPayments;

            if (contract.PaymentSchedule.FirstOrDefault().PercentCost !=
                contract.PaymentSchedule.LastOrDefault().PercentCost &&
                contract.PaymentSchedule.FirstOrDefault().Date.Date != contract.ContractDate.Date.AddMonths(1))
                return AnnuityType.AnnuityWithDateSelect;

            return AnnuityType.Annuity;
        }

        public async Task<IEnumerable<Contract>> GetContractsByVinAsync(string vin)
        {
            return await _contractRepository.GetContractsByVinAsync(vin);
        }

        public IList<ContractBalance> GetBalances(IList<int> contractIds)
        {
            return _contractRepository.GetBalances(contractIds);
        }

        public async Task<IEnumerable<ContractBalance>> GetBalancesAsync(IList<int> contractIds)
        {
            return await _contractRepository.GetBalancesAsync(contractIds);
        }

        public async Task<ContractBalance> GetBalance(int contractId)
        {
            return (await _contractRepository.GetBalancesAsync(new List<int> { contractId })).SingleOrDefault();
        }

        public async Task<IEnumerable<Contract>> GetListForOnlineByIinAsync(string iin)
        {
            return await _contractRepository.GetListForOnlineByIinAsync(iin);
        }

        public async Task<IEnumerable<Contract>> GetHistoryForOnlineByIinAsync(string iin)
        {
            return await _contractRepository.GetHistoryForOnlineByIinAsync(iin);
        }

        public async Task<bool> HasPartialPaymentAsync(int id)
        {
            return await _contractRepository.HasPartialPaymentAsync(id);
        }

        public async Task<int> GetTranchesCount(int creditLineContractId)
        {
            return await _contractRepository.GetTranchesCount(creditLineContractId);
        }

        public async Task<string> GetWaitPayTypeOperationCode(int contractId)
        {
            return await _contractRepository.GetWaitPayTypeOperationCode(contractId);
        }

        public async Task<IEnumerable<Contract>> GetTrancheListForOnlineByCreditLineIdAsync(int creditLineId)
        {
            return await _contractRepository.GetTrancheListForOnlineByCreditLineIdAsync(creditLineId);
        }


        public async Task<OverdueForCrm> GetOverdueForCrmAsync(string contractNumber)
        {
            return await _contractRepository.GetOverdueForCrmAsync(contractNumber);
        }

        public bool IsOnline(int contractId)
        {
            return _contractRepository.IsOnline(contractId);
        }

        public async Task CreditLineFillConsolidateSchedule(Contract contract, bool includeOffBalance = true)
        {
            if (contract.ContractClass != ContractClass.CreditLine)
                return;

            var tranches = await GetAllSignedTranches(contract.Id);

            if (includeOffBalance == false)
            {
                tranches = tranches.Where(tranche => tranche.IsOffBalance == false).ToList();
            }

            var tasks = new List<Task>();

            tranches.ForEach(x =>
                tasks.Add(Task.Run(() =>
                {
                    x.PaymentSchedule = _contractPaymentScheduleRepository.GetContractPaymentSchedules(x.Id).Result.ToList();
                })));

            Task.WaitAll(tasks.ToArray());

            var index = 1;

            var groupByDate = tranches.SelectMany(x => x.PaymentSchedule).GroupBy(x => x.Date).OrderBy(x => x.Key).ToList();

            groupByDate.ForEach(x =>
            {
                var row = new ContractPaymentSchedule
                {
                    Id = index,
                    ContractId = contract.Id,
                    Date = x.Key,
                    ActualDate = x.Max(x => x.ActualDate),
                    DebtLeft = x.Sum(x => x.DebtLeft),
                    DebtCost = x.Sum(x => x.DebtCost),
                    PercentCost = x.Sum(x => x.PercentCost),
                    PenaltyCost = x.Sum(x => x.PenaltyCost),
                    CreateDate = x.Min(x => x.CreateDate),
                    ActionId = x.FirstOrDefault()?.ActionId,
                    Canceled = x.Min(x => x.Canceled),
                    Prolongated = x.Min(x => x.Prolongated),
                    Period = x.Min(x => x.Period),
                    Status = x.Max(x => x.Status),
                    TranchesSchedulePayments = x.Select(x => x).ToList()
                };

                contract.PaymentSchedule.Add(row);
                index++;
            });
        }

        public async Task CheckAndChangeStatusForRealtyContractsOnSave(Contract contract)
        {
            if (contract.ContractClass == ContractClass.CreditLine || contract.ContractClass == ContractClass.Tranche)
                return;

            if (contract.CollateralType != CollateralType.Realty)
                return;

            if (contract.Status == ContractStatus.Confirmed)
                contract.Status = ContractStatus.AwaitForConfirmation;
        }

        public List<int> GetOffbalanceAdditionContractIds(DateTime inscriptionOnOrAfter, int? branchId = null)
        {
            if (branchId.HasValue)
                return _contractRepository.GetContractsForInscriptionOffBalanceAdditionService(inscriptionOnOrAfter, branchId.Value);

            return _contractRepository.GetContractsForInscriptionOffBalanceAdditionService(inscriptionOnOrAfter);
        }

        public async Task<IEnumerable<Contract>> FindListByIdentityNumberAsync(string iin)
        {
            return await _contractRepository.FindListByIdentityNumberAsync(iin);
        }

        public async Task<Contract> CreateFirstTranche(Contract creditLine, FirstTranche firstTranche, int authorId,
            IInsurancePremiumCalculator insurancePremiumCalculator,
            IInsurancePoliceRequestService insurancePoliceRequestService,
            IContractPaymentScheduleService contractPaymentScheduleService,
            IContractKdnService contractKdnService,
            bool isMobApp = false)
        {
            if (creditLine.ContractClass != ContractClass.CreditLine)
                return null;

            var signedTranches = await _contractRepository.GetAllSignedTranches(creditLine.Id);

            if (signedTranches.Any())
                return null;

            if (creditLine.LoanCost < firstTranche.LoanCost)
                throw new PawnshopApplicationException($"Сумма транша превышает сумму кредитного лимита: {creditLine.LoanCost} < {firstTranche.LoanCost}.");

            if (!firstTranche.SettingId.HasValue)
                throw new PawnshopApplicationException("Требуется выбрать продукт для транша.");

            var trancheProduct = _loanPercentRepository.Get(firstTranche.SettingId.Value);

            if (trancheProduct == null || trancheProduct.ParentId != creditLine.SettingId)
                throw new PawnshopApplicationException("Выбран некорректный продукт.");

            if (firstTranche.LoanPeriod == 0)
                throw new PawnshopApplicationException("Требуется указать период транша.");

            var maturityDate = DateTime.Now.Date.AddMonths(firstTranche.LoanPeriod / (int)trancheProduct.ContractPeriodFromType);

            if (maturityDate > creditLine.MaturityDate)
                throw new PawnshopApplicationException("Срок действия транша не может быть больше срока действия кредитной линии.");

            if (trancheProduct.IsLiquidityOn && (firstTranche.LoanPeriod > trancheProduct.PossibleContractPeriods.Max(x => x.LoanPeriod)
                || firstTranche.LoanPeriod < trancheProduct.PossibleContractPeriods.Min(x => x.LoanPeriod)))
                throw new PawnshopApplicationException("Выбран некорректный срок займа.");

            var tranche = await _contractRepository.GetFirstTranche(creditLine.Id);
            var trancheId = 0;
            DateTime? firstPaymentDate = null;

            if (tranche != null)
            {
                trancheId = tranche.Id;
                firstPaymentDate = tranche.FirstPaymentDate;
            }

            tranche = _contractRepository.GetOnlyContract(creditLine.Id);
            tranche.Id = trancheId;
            tranche.ContractClass = trancheProduct.ContractClass;
            tranche.ContractNumber = $"{creditLine.ContractNumber}-T001";
            tranche.ContractTypeId = trancheProduct.ContractTypeId;
            tranche.CreditLineId = creditLine.Id;
            tranche.LoanCost = firstTranche.LoanCost;
            tranche.LoanPercent = trancheProduct.LoanPercent;
            tranche.LoanPercentCost = Math.Round(tranche.LoanCost * trancheProduct.LoanPercent / 100, 4, MidpointRounding.AwayFromZero);
            tranche.LoanPeriod = firstTranche.LoanPeriod;
            tranche.MaturityDate = maturityDate;
            tranche.MaxCreditLineCost = creditLine.LoanCost;
            tranche.PercentPaymentType = PercentPaymentType.Product;
            tranche.ProductTypeId = trancheProduct.ProductTypeId;
            tranche.Setting = trancheProduct;
            tranche.SettingId = trancheProduct.Id;
            tranche.UsePenaltyLimit = trancheProduct.UsePenaltyLimit;
            tranche.FirstPaymentDate = firstPaymentDate ?? creditLine.FirstPaymentDate;

            tranche.Subjects = _contractRepository.GetContractLoanSubjects(tranche.Id);

            var newSubjects = creditLine.Subjects?
                .Where(x => !tranche.Subjects.Any(f => f.ClientId == x.ClientId))
                .Select(x => new ContractLoanSubject
                {
                    AuthorId = x.AuthorId,
                    ClientId = x.ClientId,
                    CreateDate = DateTime.Now,
                    SubjectId = x.SubjectId,
                    Subject = x.Subject,
                });

            tranche.Subjects.ForEach(x =>
            {
                var subject = creditLine.Subjects.FirstOrDefault(s => s.ClientId == x.ClientId);
                if (subject != null)
                    x.DeleteDate = subject.DeleteDate;
                else
                    x.DeleteDate = DateTime.Now;
            });

            if (newSubjects != null && newSubjects.Any())
                tranche.Subjects.AddRange(newSubjects);

            if (trancheProduct.IsFloatingDiscrete)
            {
                tranche.AnnuityType = null;
                _paymentScheduleService.BuildWithContract(tranche);
            }
            else
            {
                _paymentScheduleService.BuildWithContract(tranche);
                CheckSchedule(tranche);
            }

            tranche.FirstPaymentDate = tranche.FirstPaymentDate ?? tranche.PaymentSchedule.FirstOrDefault().Date;
            tranche.NextPaymentDate = tranche.PercentPaymentType == PercentPaymentType.EndPeriod
                ? tranche.MaturityDate
                : tranche.PaymentSchedule.Where(x => x.ActionId == null && x.Canceled == null).Min(x => x.Date);
            await CalculateAPR(tranche);

            if (!isMobApp)
            {
                await _paymentScheduleService.CheckPayDayFromContract(tranche);
            }

            if (tranche.Id == 0)
            {
                tranche.ContractRates = trancheProduct.LoanSettingRates
                    .Where(x => x.RateSetting.Code == Constants.ACCOUNT_SETTING_PENY_ACCOUNT || x.RateSetting.Code == Constants.ACCOUNT_SETTING_PENY_PROFIT)
                    .Select(x =>
                        new ContractRate
                        {
                            AuthorId = Constants.ADMINISTRATOR_IDENTITY,
                            CreateDate = DateTime.Now,
                            Date = DateTime.Now,
                            Rate = x.Rate,
                            RateSettingId = x.RateSettingId,
                        })
                    .ToList();

                _contractRepository.Insert(tranche);
            }
            else
            {
                _contractRepository.Update(tranche);
            }

            tranche = _contractRepository.Get(tranche.Id);
            firstTranche.Id = tranche.Id;

            if (trancheProduct.IsInsuranceAvailable)
            {
                var policyRequest = insurancePoliceRequestService.GetLatestPoliceRequest(tranche.Id);
                if (policyRequest == null || policyRequest.CreateDate.Date != DateTime.Now.Date ||
                    tranche.LoanCost != (policyRequest.RequestData.LoanCost - policyRequest.RequestData.InsurancePremium))
                {
                    insurancePoliceRequestService.DeletePoliceRequestsForContract(tranche.Id);
                    var insuranceCompanyId = trancheProduct.InsuranceCompanies.FirstOrDefault().InsuranceCompanyId;
                    var insuranceDataResult = insurancePremiumCalculator.GetInsuranceDataV2(tranche.LoanCost, insuranceCompanyId, trancheProduct.Id);
                    var insurancePremium = insuranceDataResult?.InsurancePremium ?? 0;

                    tranche.LoanCost += insurancePremium;

                    if (insurancePremium == 0)
                        return tranche;

                    var contractModel = new ContractModel
                    {
                        Contract = tranche,
                        PoliceRequests = new List<InsurancePoliceRequest>
                        {
                            new InsurancePoliceRequest
                            {
                                IsInsuranceRequired = true,
                                InsuranceCompanyId = insuranceCompanyId,
                                RequestData = new InsuranceRequestData
                                {
                                    LoanCost = tranche.LoanCost,
                                    InsurancePremium = insurancePremium
                                }
                            }
                        }
                    };

                    var insurancePoliceRequest = insurancePoliceRequestService.GetPoliceRequest(contractModel);

                    if (contractModel.Contract.Client.IsPensioner)
                        contractModel = ChangeForPensioner(contractModel);

                    if (creditLine.LoanCost < tranche.LoanCost)
                        throw new PawnshopApplicationException($"Сумма транша превышает сумму кредитного лимита: {creditLine.LoanCost} < {tranche.LoanCost}");

                    if (insurancePoliceRequest.ContractId == 0)
                        insurancePoliceRequestService.SetContractIdAndNumber(insurancePoliceRequest, tranche);

                    insurancePoliceRequestService.Save(insurancePoliceRequest);
                    _contractRepository.Update(tranche);
                }
            }

            if (trancheProduct.IsFloatingDiscrete)
            {
                tranche.AnnuityType = null;
                _paymentScheduleService.BuildWithContract(tranche);
            }
            else
            {
                _paymentScheduleService.BuildWithContract(tranche);
                CheckSchedule(tranche);
            }

            if (contractPaymentScheduleService.IsNeedUpdatePaymentSchedule(tranche.PaymentSchedule, tranche.Id))
                contractPaymentScheduleService.Save(tranche.PaymentSchedule, tranche.Id, authorId);

            var user = _userRepository.Get(authorId);
            contractKdnService.FillKdnModel(tranche, user);
            return tranche;
        }

        public bool ReissueAutocredit(ReissueCarModel model)
        {
            Contract contract = _contractRepository.Find(new { model.AppId })
                ?? throw new PawnshopApplicationException($"По данному AppId = {model.AppId} договор не найден");

            contract = _contractRepository.Get(contract.Id);

            if (contract.Status != ContractStatus.PositionRegistration)
                throw new PawnshopApplicationException($"Договор не в ожидании переоформления");

            foreach (var position in contract.Positions)
            {
                var car = (Car)position.Position;
                car.TransportNumber = model.TransportNumber;
                car.TechPassportDate = model.TechPassportDate;
                car.TechPassportNumber = model.TechPassportNumber;

                position.Position = car;
            }

            contract.Status = ContractStatus.Reissued;

            using (var transaction = _contractRepository.BeginTransaction())
            {
                contract.Positions.ForEach(position =>
                {
                    try
                    {
                        _carRepository.Update((Car)position.Position);
                    }
                    catch (SqlException e)
                    {
                        if (e.Number == 2627)
                            throw new PawnshopApplicationException(
                                $"Позиция с такими данными (гос. номер: {model.TransportNumber}, техпаспорт: {model.TechPassportNumber}) уже существует");

                        throw new PawnshopApplicationException(e.Message);
                    }
                });

                _contractRepository.Update(contract);
                transaction.Commit();
            }

            return true;
        }

        public bool CheckBlackListOnActionType(Contract contract, ContractActionType actionType)
        {
            if (contract.ClientId <= 0)
                throw new PawnshopApplicationException("Идентификатор клиента не может быть равен или меньше нуля");

            if (contract.Positions.FirstOrDefault().Category.Code != Constants.WITH_DRIVE_RIGHT_CATEGORY_CODE)
                return false;

            foreach (var subject in contract.Subjects)
            {
                if (subject.Subject.Code == Constants.LOAN_SUBJECT_MERCHANT)
                    continue;
                CheckClient(actionType, subject.ClientId, subject.Subject.Name);
            }

            CheckClient(actionType, contract.ClientId, "Клиент");

            return false;
        }

        private void CheckClient(ContractActionType actionType, int clientId, string clientText)
        {
            var blackList = _clientBlackListService.GetClientsBlackListsByClientId(clientId);
            switch (actionType)
            {
                case ContractActionType.PartialPayment:
                    {
                        if (blackList != null && blackList.Any(x => x.BlackListReason.PartialPaymentWithDrive))
                            throw new PawnshopApplicationException($"{clientText} в черном списке для операций ЧДП с правом вождения");

                        break;
                    }
                case ContractActionType.Sign:
                    {
                        if (blackList != null && blackList.Any(x => x.BlackListReason.AllowNewContractsWithDrive))
                            throw new PawnshopApplicationException($"{clientText} в черном списке для операций Подписания с правом вождения");

                        break;
                    }
                case ContractActionType.Addition:
                    {
                        if (blackList != null && blackList.Any(x => x.BlackListReason.AdditionNewContractWithDrive))
                            throw new PawnshopApplicationException($"{clientText} в черном списке для операций Добора с правом вождения");

                        break;
                    }
            }
        }

        public bool CheckCategoryLimitSum(Contract contract, decimal costSum, bool categoryChanged = true, decimal? additionalLimit = null)
        {
            if (contract == null || contract.ContractClass == ContractClass.Tranche)
                return false;

            var position = contract.Positions.FirstOrDefault();
            if (position == null || position.Category == null)
                throw new PawnshopApplicationException("Отсутствует позиция или категория позиции по договору");

            if (categoryChanged)
            {
                if (position.Category.Code == Constants.WITH_DRIVE_RIGHT_CATEGORY_CODE && costSum > position.MotorCost + additionalLimit)
                    throw new PawnshopApplicationException("Сумма кредита больше сумма без права вождения");
                else if (position.Category.Code == Constants.WITHOUT_DRIVE_RIGHT_CATEGORY_CODE && costSum > position.TurboCost + additionalLimit)
                    throw new PawnshopApplicationException("Сумма кредита больше сумма с правом вождения");
            }
            else
            {
                if (position.Category.Code == Constants.WITHOUT_DRIVE_RIGHT_CATEGORY_CODE && costSum > position.MotorCost + additionalLimit)
                    throw new PawnshopApplicationException("Сумма кредита больше сумма без права вождения");
                else if (position.Category.Code == Constants.WITH_DRIVE_RIGHT_CATEGORY_CODE && costSum > position.TurboCost + additionalLimit)
                    throw new PawnshopApplicationException("Сумма кредита больше сумма с правом вождения");
            }

            return true;
        }

        public bool ChangeCategory(ContractAction action, Contract contract, decimal costSum, decimal? additionalLimit = null)
        {
            if (action == null || contract == null || contract.ContractClass == ContractClass.Tranche)
                return false;

            var position = contract.Positions.FirstOrDefault();
            if (position == null)
                return false;

            CheckBlackListOnActionType(contract, action.ActionType);

            CheckCategoryLimitSum(contract, costSum, true, additionalLimit);

            var limit = new ListQuery();
            limit.Page.Limit = 10000;
            var categoryList = _categoryRepository.List(limit);
            var positionIndex = contract.Positions.IndexOf(position);

            if (position.Category.Code == Constants.WITH_DRIVE_RIGHT_CATEGORY_CODE)
            {
                position.Category = categoryList.FirstOrDefault(x => x.Code == Constants.WITHOUT_DRIVE_RIGHT_CATEGORY_CODE);
                position.CategoryId = position.Category.Id;
            }
            else if (position.Category.Code == Constants.WITHOUT_DRIVE_RIGHT_CATEGORY_CODE)
            {
                position.Category = categoryList.FirstOrDefault(x => x.Code == Constants.WITH_DRIVE_RIGHT_CATEGORY_CODE);
                position.CategoryId = position.Category.Id;
            }

            if (positionIndex != -1)
                contract.Positions[positionIndex] = position;

            _contractRepository.UpdatePositions(contract.Id, contract.Positions.ToArray());
            _parkingActionService.ChangeParkingStatusByCategory(contract, action.Id);

            if (contract.Positions.FirstOrDefault().Category.Code == Constants.WITH_DRIVE_RIGHT_CATEGORY_CODE)
                _eventLogService.Log(EventCode.CategoryChangeWithDrive, EventStatus.Success, EntityType.Contract, contract.Id, userId: _sessionContext.UserId);
            else if (contract.Positions.FirstOrDefault().Category.Code == Constants.WITHOUT_DRIVE_RIGHT_CATEGORY_CODE)
                _eventLogService.Log(EventCode.CategoryChangeWithoutDrive, EventStatus.Success, EntityType.Contract, contract.Id, userId: _sessionContext.UserId);

            return true;
        }

        public bool CancelChangeCategory(Contract contract, ContractAction contractAction)
        {
            if (contract == null || contractAction == null)
                return false;

            if (contractAction.Data != null && !contractAction.Data.CategoryChanged)
                return false;

            if (contract.ContractClass == ContractClass.Tranche)
            {
                var creditLineId = GetCreditLineId(contract.Id).Result;
                contract = Get(creditLineId);
            }

            var limit = new ListQuery();
            limit.Page.Limit = 10000;
            var categoryList = _categoryRepository.List(limit);

            var position = contract.Positions.FirstOrDefault();
            if (position == null)
                return false;

            var positionIndex = contract.Positions.IndexOf(position);

            if (position.Category.Code == Constants.WITHOUT_DRIVE_RIGHT_CATEGORY_CODE)
            {
                var categoryWith = categoryList.FirstOrDefault(x => x.Code == Constants.WITH_DRIVE_RIGHT_CATEGORY_CODE);
                position.Category = categoryWith;
                position.CategoryId = categoryWith.Id;
            }
            else if (position.Category.Code == Constants.WITH_DRIVE_RIGHT_CATEGORY_CODE)
            {
                var categoryWithout = categoryList.FirstOrDefault(x => x.Code == Constants.WITHOUT_DRIVE_RIGHT_CATEGORY_CODE);
                position.Category = categoryWithout;
                position.CategoryId = categoryWithout.Id;
            }

            if (positionIndex != -1)
                contract.Positions[positionIndex] = position;

            _parkingActionService.CancelParkingHistory(contract);
            contractAction.Data.CategoryChanged = false;
            return true;
        }

        public DebtInfo GetDebtInfoByCreditLine(int creditLineId)
        {
            var tranches = _contractRepository.GetAllSignedTranches(creditLineId).Result;
            decimal debt = 0;
            foreach (var tranche in tranches)
            {
                var accountListQueryModel = new ListQueryModel<AccountFilter>
                {
                    Model = new AccountFilter
                    {
                        ContractId = tranche.Id
                    }
                };
                var account = _accountService.List(accountListQueryModel).List
                    .Where(x => (x.Code == "2010" || x.Code == "1152")).FirstOrDefault();
                debt += Math.Abs(account.Balance);
            }
            return new DebtInfo()
            {
                PrincipalDebt = debt
            };
        }

        public (bool isCategoryChange, bool checkLeftLoanCostForLight, bool checkLeftLoanCostForMotor, decimal maxSumByAnaliticsCategory) ChangeCategoryForCredilLineData(Application application,
            bool isCreditLineCategoryMotor, ProdKind prodKind, decimal applicationAdditionalLimit, decimal debt, int settingId)
        {
            var limit = Math.Min(application.EstimatedCost * (applicationAdditionalLimit / 100), Constants.MAX_ADDITIONAL_SUM);
            bool isFromLightToMotor = !isCreditLineCategoryMotor && prodKind == ProdKind.Motor;
            bool isFromMotorToLight = isCreditLineCategoryMotor && prodKind != ProdKind.Motor;
            bool checkLeftLoanCostForLight = debt <= application.LightCost + limit;
            bool checkLeftLoanCostForMotor = debt <= application.MotorCost + limit;

            var cost = isCreditLineCategoryMotor ? application.MotorCost : application.LightCost;
            var maxSumByAnaliticsCategory = cost + limit;

            // смена категории при создании транша
            bool isCategoryChange =
                // из категории "С правом вождения" в "Без права вождения"
                ((isFromLightToMotor)

                // из категории "Без права вождения" в "С правом вождения"
                || (isFromMotorToLight && checkLeftLoanCostForLight))

                // если в заявке оценщик указал машину как "Без права вождения" смена категории запрещается
                && !application.WithoutDriving;

            int minimalLoanCost = _loanPercentRepository.Get(settingId).LoanCostFrom;
            if ((debt > maxSumByAnaliticsCategory && !isCategoryChange) || (!checkLeftLoanCostForLight && !checkLeftLoanCostForMotor)
                || (isFromMotorToLight && !checkLeftLoanCostForLight) || (isFromLightToMotor && !checkLeftLoanCostForMotor)
                || ((maxSumByAnaliticsCategory - debt < minimalLoanCost) && !isFromLightToMotor))
                throw new PawnshopApplicationException($"Превышен доступный остаток кредитной линии по сумме для выбранной категории аналитики");

            return (isCategoryChange, checkLeftLoanCostForLight, checkLeftLoanCostForMotor, maxSumByAnaliticsCategory.Value);
        }

        public int GetPeriodTypeId(DateTime maturityDate)
        {
            if (Math.Abs((DateTime.Now - maturityDate).Days) <= (int)PeriodType.Year)
                return _typeRepository.FindAsync(new { Code = Constants.PERIOD_TYPE_TERMS_SHORT }).Result.Id;

            return _typeRepository.FindAsync(new { Code = Constants.PERIOD_TYPE_TERMS_LONG }).Result.Id;
        }

        public DateTime? GetNextPaymentDateByCreditLineId(int creditLineId)
        {
            var creditLineUpcomingPaymentsDate = _contractRepository.GetUpcomingPaymentsDateByCreditLineId(creditLineId)
                .Select(x => new { Date = x.Key, UpcomingDays = x.Value });

            if (!creditLineUpcomingPaymentsDate.Any())
                return null;

            var rangeMinDate = DateTime.Today.AddDays(Constants.PAYMENT_RANGE_MIN_DAYS);
            var rangeMaxDate = DateTime.Today.AddDays(Constants.PAYMENT_RANGE_MAX_DAYS);

            var rangeValue = creditLineUpcomingPaymentsDate
                .Where(x => x.Date >= rangeMinDate && x.Date <= rangeMaxDate)
                .FirstOrDefault();

            if (rangeValue != null)
                return rangeValue.Date;

            var firstPaymentDate = creditLineUpcomingPaymentsDate
                .Where(x => x.Date < rangeMinDate)
                .FirstOrDefault()?.Date;

            firstPaymentDate = firstPaymentDate?.AddMonths(1);

            return firstPaymentDate;
        }

        public async Task<DateTime?> GetNearestTranchePaymentDateOfCreditLine(int creditLineId)
        {
            return await _contractRepository.GetNearestTranchePaymentDateOfCreditLine(creditLineId);
        }

        public async Task<PaymentAmount> GetPaymentAmount(int id, Contract contract = null)
        {
            if (contract == null)
                contract = _contractRepository.GetOnlyContract(id);

            ContractBalance balance = null;

            if (contract.ContractClass == ContractClass.CreditLine)
            {
                await CreditLineFillConsolidateSchedule(contract, false);

                if (!contract.PaymentSchedule.Any())
                    return new PaymentAmount
                    {
                        Amount = 0,
                        Desc = "There are no open trenches."
                    };

                balance = await GetCreditLineTotalBalance(contract.Id, contract);
            }
            else
            {
                contract.PaymentSchedule = _contractPaymentScheduleRepository.GetListByContractId(id);

                var queryId = new List<int> { contract.Id };
                balance = GetBalances(queryId).FirstOrDefault();
            }

            var lastPaymentToPay = contract.PaymentSchedule?
                .OrderBy(x => x.Date)
                .FirstOrDefault(x => !x.ActionId.HasValue && !x.ActualDate.HasValue);

            var upcomingPayment = contract.PaymentSchedule?
                .OrderBy(x => x.Date)
                .FirstOrDefault(x => !x.ActionId.HasValue && !x.ActualDate.HasValue && x.Date.Date >= DateTime.Now.Date);

            decimal amount = 0;

            if (lastPaymentToPay != null && lastPaymentToPay.Date < DateTime.Today)
            {
                amount = balance?.CurrentDebt ?? 0;
                amount -= balance?.PrepaymentBalance ?? 0;

                if (amount < 0)
                    amount = 0;

                return new PaymentAmount
                {
                    Amount = amount,
                    Desc = "This is an overdue payment",
                };
            }
            else if (upcomingPayment != null)
            {
                if (upcomingPayment.Date > DateTime.Today)
                {

                    amount = upcomingPayment.DebtCost + upcomingPayment.PercentCost + (balance?.PenyAmount ?? 0);
                    amount -= balance?.PrepaymentBalance ?? 0;

                    if (amount < 0)
                    {
                        amount = 0;
                    }

                    return new PaymentAmount
                    {
                        Amount = amount,
                        Desc = "This is an upcoming payment.",
                        NextPaymentDate = upcomingPayment.Date
                    };
                }
                else
                {
                    amount = balance?.CurrentDebt ?? 0;
                    amount -= balance?.PrepaymentBalance ?? 0;

                    if (amount < 0)
                        amount = 0;

                    return new PaymentAmount
                    {
                        Amount = amount,
                        Desc = "This is today's payment.",
                    };
                }
            }

            return new PaymentAmount
            {
                Amount = -1,
                Desc = "Not found amount."
            };
        }

        public async Task<ContractBalance> GetCreditLineTotalBalance(int creditLineId, Contract contract = null)
        {
            if (contract == null)
                contract = _contractRepository.GetOnlyContract(creditLineId);

            var tranches = await GetAllSignedTranches(contract.Id);
            var balanceContractIds = tranches.Select(x => x.Id).ToList();
            balanceContractIds.Add(creditLineId);

            var balances = GetBalances(balanceContractIds);

            return new ContractBalance
            {
                ContractId = creditLineId,
                AccountAmount = balances.Sum(s => s.AccountAmount),
                ProfitAmount = balances.Sum(s => s.ProfitAmount),
                OverdueAccountAmount = balances.Sum(s => s.OverdueAccountAmount),
                OverdueProfitAmount = balances.Sum(s => s.OverdueProfitAmount),
                PenyAmount = balances.Sum(s => s.PenyAmount),
                TotalAcountAmount = balances.Sum(s => s.TotalAcountAmount),
                TotalProfitAmount = balances.Sum(s => s.TotalProfitAmount),
                ExpenseAmount = balances.Sum(s => s.ExpenseAmount),
                PrepaymentBalance = balances.FirstOrDefault(s => s.ContractId == creditLineId)?.PrepaymentBalance ?? 0,
                CurrentDebt = balances.Sum(s => s.CurrentDebt),
                TotalRepaymentAmount = balances.Sum(s => s.TotalRepaymentAmount),
                TotalRedemptionAmount = balances.Sum(s => s.TotalRedemptionAmount),
            };
        }

        public bool IsContractBusinessPurpose(Contract contract)
        {
            return (contract.LoanPurposeId == _domainService.GetDomainValue(Constants.LOAN_PURPOSE_DOMAIN_VALUE, Constants.BUSINESS_LOAN_PURPOSE).Id ||
                contract.LoanPurposeId == _domainService.GetDomainValue(Constants.LOAN_PURPOSE_DOMAIN_VALUE, Constants.INVESTMENTS_LOAN_PURPOSE).Id ||
                contract.LoanPurposeId == _domainService.GetDomainValue(Constants.LOAN_PURPOSE_DOMAIN_VALUE, Constants.CURRENT_ASSETS_LOAN_PURPOSE).Id ||
                contract.LoanPurposeId == _domainService.GetDomainValue(Constants.LOAN_PURPOSE_DOMAIN_VALUE, Constants.INVESTMENTS_AND_CURRENT_ASSETS_LOAN_PURPOSE).Id) &&
                contract.Client.LegalForm.IsIndividual;
        }

        public void ValidateBusinessLoanPurpose(Contract contract)
        {
            var incomes = _clientIncomeService.GetClientIncomes(contract.ClientId);
            if (incomes == null)
                throw new PawnshopApplicationException("У клиента не заполнены доходы!");

            var expenses = _clientIncomeService.GetClientExpenses(contract.ClientId);
            if (expenses == 0)
                throw new PawnshopApplicationException("У клиента не заполнены расходы!");
        }

        public void SaveContractExpertOpinionData(int contractId)
        {
            if (contractId == null)
                throw new ArgumentNullException(nameof(contractId));

            var contract = Get(contractId);
            ValidateBusinessLoanPurpose(contract);

            List<ContractLoanSubject> subjects = new List<ContractLoanSubject>();
            List<ContractPosition> contractPositions = new List<ContractPosition>();

            if (contract.ContractClass == ContractClass.CreditLine)
            {
                subjects = contract.Subjects;
                contractPositions = contract.Positions;
            }
            else if (contract.ContractClass == ContractClass.Tranche)
            {
                var creditLine = Get(contract.CreditLineId.Value);
                subjects = creditLine.Subjects;
                contractPositions = creditLine.Positions;
            }

            var subjectsExpertOpinionData = new List<ClientExpertOpinionData>();
            foreach (var subject in subjects)
            {
                subjectsExpertOpinionData.Add(new ClientExpertOpinionData()
                {
                    ClientData = new ClientData()
                    {
                        FullName = subject.Client.FullName,
                        IIN = subject.Client.IdentityNumber,
                        BirthDate = subject.Client?.BirthDay,
                        Age = _clientService.GetClientAge(subject.Client)
                    },
                    Incomes = _clientIncomeService.GetClientIncomes(subject.ClientId),
                    Expences = _clientIncomeService.GetClientFullExpenses(subject.ClientId)
                });
            }

            var contractPositionData = new List<ContractPositionData>();
            foreach (var position in contractPositions)
            {
                var carPosition = (Car)position.Position;
                contractPositionData.Add(new ContractPositionData()
                {
                    OwnerFullName = position.Position.Client.FullName,
                    PositionDetails = carPosition.Mark + " " + carPosition.Model + " " + carPosition.TransportNumber,
                    ReleaseYear = carPosition.ReleaseYear,
                    EstimatedIncome = position.EstimatedCost
                });
            }

            contract.ContractData.ContractExpertOpinionData = new ContractExpertOpinionData()
            {
                ClientExpertOpinionData = new ClientExpertOpinionData()
                {
                    ClientData = new ClientData()
                    {
                        FullName = contract.Client.FullName,
                        IIN = contract.Client.IdentityNumber,
                        BirthDate = contract.Client?.BirthDay,
                        Age = _clientService.GetClientAge(contract.Client)
                    },
                    Incomes = _clientIncomeService.GetClientIncomes(contract.ClientId),
                    Expences = _clientIncomeService.GetClientFullExpenses(contract.ClientId)
                },
                SubjectsExpertOpinionData = subjectsExpertOpinionData,
                ContractPositionData = contractPositionData
            };
            Save(contract);
        }

        public (bool IsLiquidityOn, bool IsInsuranceAdditionalLimitOn) GetContractSettings(int contractId)
        {
            var contract = GetOnlyContract(contractId);
            (bool IsLiquidityOn, bool IsInsuranceAdditionalLimitOn)? result = null;
            if (contract.ContractClass == ContractClass.CreditLine)
                result = _creditLineRepository.GetCreditLineSettings(contractId);
            else if (contract.ContractClass == ContractClass.Tranche)
                result = _creditLineRepository.GetTrancheSettings(contractId);

            return result.Value;
        }

        // для проверки КДН оффлайн контрактов
        // (для онлайна этот метод не требуется)
        public bool IsKDNPassedForOffline(int contractId)
        {
            var kdnLog = _contractKdnCalculationLogRepository.GetByContractId(contractId);
            var contract = GetOnlyContract(contractId);

            // в ContractKdnCalculationLogs лежат записи только по контрактам с CollateralType.Car
            if (kdnLog != null && !contract.CreatedInOnline && contract.CollateralType == CollateralType.Car)
            {
                return kdnLog.IsKdnPassed;
            }

            // если не CollateralType.Car (к примеру CollateralType.Realty) то пропускаем
            return true;
        }

        public async Task IsCanEditFirstPaymentDate(Contract contract)
        {
            contract.CanEditFirstPaymentDate = false;

            if (contract.ContractClass == ContractClass.CreditLine)
                return;

            if (contract.ContractClass == ContractClass.Credit)
            {
                contract.CanEditFirstPaymentDate = true;
                return;
            }

            var creditLine = _contractRepository.GetOnlyContract(contract.CreditLineId.Value);

            if (creditLine.Status == ContractStatus.Draft)
            {
                contract.CanEditFirstPaymentDate = true;
                return;
            }

            var activeTrancheCount = await _contractRepository.GetActiveTranchesCount(contract.Id);

            if (creditLine.Status == ContractStatus.Signed && activeTrancheCount == 0)
            {
                contract.CanEditFirstPaymentDate = true;
                return;
            }

            return;
        }

        public async Task<bool> CarHasClientAsync(int contractId)
        {
            var car = await _carRepository.GetByContractIdAsync(contractId);

            if (car != null)
            {
                var parkingStatuses = _parkingStatusRepository.List(new Core.Queries.ListQuery());
                var statusAtClient = parkingStatuses.FirstOrDefault(x => x.StatusCode == Constants.AT_CLIENT);

                return !car.ParkingStatusId.HasValue || car.ParkingStatusId == statusAtClient.Id ? true : false;
            }

            return true;
        }

        public async Task<bool> IncompleteActionExistsAsync(int contractId)
        {
            var incompleteActions = await _contractActionRepository.GetIncompleteActions(contractId);
            return incompleteActions.Count > 0;
        }

        public async Task<bool> HasExpenses(int contractId)
        {
            var expenses = await _contractExpenseRepository.GetListByContractIdAsync(contractId);
            var extraExpense = GetExtraExpensesCost(contractId);

            return expenses.Any(x => !x.IsPayed) || extraExpense > 0;
        }

        public async Task UpdatePeriodType(int periodTypeId, int contractId)
        {
            await _contractRepository.UpdatePeriodType(periodTypeId, contractId);
        }
    }
}