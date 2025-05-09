using Microsoft.CodeAnalysis.Operations;
using Pawnshop.AccountingCore.Abstractions;
using Pawnshop.AccountingCore.Models;
using Pawnshop.Core;
using Pawnshop.Core.Exceptions;
using Pawnshop.Core.Extensions;
using Pawnshop.Core.Impl;
using Pawnshop.Core.Queries;
using Pawnshop.Data.Access;
using Pawnshop.Data.Models.Audit;
using Pawnshop.Data.Models.Contracts;
using Pawnshop.Services.Contracts;
using Pawnshop.Services.Models.Filters;
using Pawnshop.Services.Models.List;
using Pawnshop.Services.PenaltyLimit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace Pawnshop.Services.AccountingCore
{
    /// <summary>
    ///  Сервис для начисления платежей на внебалансовые счета договоров с исп. надписью, начиная с 01/10/2021 по сегодняшний день
    /// </summary>
    public class InscriptionOffBalanceAdditionService : IInscriptionOffBalanceAdditionService
    {
        private readonly IPenaltyAccrual _penaltyAccrualService;
        private readonly IInterestAccrual _interestAccrual;
        private readonly ISessionContext _sessionContext;
        private readonly IPenaltyLimitAccrualService _penaltyLimitAccrualService;
        private readonly IPenaltyRateService _penaltyRateService;
        private readonly IContractPaymentScheduleService _contractPaymentScheduleService;
        private readonly ITakeAwayToDelay _takeAwayToDelay;
        private readonly IContractRateService _contractRateService;
        private readonly ContractRateRepository _contractRateRepository;
        private readonly IContractService _contractService;
        private readonly ContractRepository _contractRepository;
        private readonly IEventLog _eventLog;
        private readonly IAccountService _accountService;
        private readonly IAccountRecordService _accountRecordService;

        private readonly DateTime startDate = Constants.INSCRIPTION_SERVICE_OFFBALANCE_ACCOUNT_ADDITION_START_DATE;
        private DateTime endDate = DateTime.Today.Date;
        private const string OFFBALANCE_ENDING = "_OFFBALANCE";
        private readonly DateTime january2022BlackoutStartDate = new DateTime(2022, 1, 5);
        private readonly DateTime january2022BlackoutEndDate = new DateTime(2022, 1, 18);

        public InscriptionOffBalanceAdditionService(IPenaltyAccrual penaltyAccrual, IInterestAccrual interestAccrual, ISessionContext sessionContext, IPenaltyLimitAccrualService penaltyLimitAccrualService, IPenaltyRateService penaltyRateService, IContractPaymentScheduleService contractPaymentScheduleService,
            ITakeAwayToDelay takeAwayToDelay, IContractRateService contractRateService, ContractRateRepository contractRateRepository, IContractService contractService, ContractRepository contractRepository, IEventLog eventLog, IAccountService accountService,
            IAccountRecordService accountRecordService)
        {
            _penaltyAccrualService = penaltyAccrual;
            _interestAccrual = interestAccrual;
            _sessionContext = sessionContext;
            _penaltyLimitAccrualService = penaltyLimitAccrualService;
            _penaltyRateService = penaltyRateService;
            _contractPaymentScheduleService = contractPaymentScheduleService;
            _takeAwayToDelay = takeAwayToDelay;
            _contractRateService = contractRateService;
            _contractRateRepository = contractRateRepository;
            _contractService = contractService;
            _contractRepository = contractRepository;
            _eventLog = eventLog;
            _accountService = accountService;
            _accountRecordService = accountRecordService;
        }

        public async Task AddOffbalancePaymentForContractsWithInscription(int branchId, DateTime? inputEndDate = null)
        {
            if (!inputEndDate.HasValue)
                throw new PawnshopApplicationException("Не указан параметр endDate. Укажите параметр и повторите");

            endDate = inputEndDate.Value;

            var contractIds = _contractService.GetOffbalanceAdditionContractIds(startDate, branchId).OrderBy(x => x);

            _eventLog.Log(EventCode.InscriptionOffBalanceForBranch, EventStatus.Success, EntityType.Group, branchId, uri: "/api/contractAction/OffBalanceAdditionForBranch", userId: _sessionContext.UserId, requestData: $"Для филиала с айди {branchId} найдено {contractIds.Count()}  договоров для начисления");

            var errorIds = new List<int>();
            
            foreach ( var id in contractIds )
            {
                try
                {
                    using (var transaction = _contractRepository.BeginTransaction())
                    {                        
                        await ProcessContract(id);
                        //_eventLog.Log(EventCode.InscriptionOffBalanceForBranch, EventStatus.Success, EntityType.Group, branchId, uri: "/api/contractAction/OffBalanceAdditionForBranch", userId: _sessionContext.UserId);
                        transaction.Commit();
                    }
                }
                catch (Exception ex)
                {
                    _eventLog.Log(EventCode.InscriptionOffBalanceForContract, EventStatus.Failed, EntityType.Group, branchId, uri: "/api/contractAction/OffBalanceAdditionForBranch", userId: _sessionContext.UserId, responseData: ex.Message);
                    errorIds.Add(id);
                    continue;
                }
            }

            if(errorIds.Count > 0)
            {
                _eventLog.Log(EventCode.InscriptionOffBalanceForBranch, EventStatus.Failed, EntityType.Group, branchId, uri: "/api/contractAction/OffBalanceAdditionForBranch", userId: _sessionContext.UserId, responseData: "Неудачное начисление по следующим договорам : "  + String.Join(", ", errorIds));
                throw new PawnshopApplicationException("Список договоров, где не начислилась пеня на внебалансе : " + String.Join(", ", errorIds));
            }
            else
            {
                _eventLog.Log(EventCode.InscriptionOffBalanceForBranch, EventStatus.Success, EntityType.Group, branchId, uri: "/api/contractAction/OffBalanceAdditionForBranch", userId: _sessionContext.UserId, responseData: $"Удачное зачисление для всех договоров филиала с айди {branchId}");
            }
        }

        /// <summary>
        /// Добавление начисления для специфичного договора - изначально для теста
        /// </summary>
        /// <param name="contractId"></param>
        public async Task AddOffBalanceForSpecificContract(int contractId, DateTime? inputEndDate = null)
        {
            if (inputEndDate.HasValue)
                endDate = inputEndDate.Value;
            else
                endDate = DateTime.Today.Date;

            var contractIds = new List<int>();
            contractIds.Add(contractId);
            
            _eventLog.Log(EventCode.InscriptionOffBalanceForContract, EventStatus.Success, EntityType.Contract, contractId, uri: "/api/contractAction/testOffBalanceAddition", userId: _sessionContext.UserId, requestData: $"Договор {contractId} отрабатывает до даты {inputEndDate ?? DateTime.Today}");

            try
            {
                using (var transaction = _contractRepository.BeginTransaction())
                {
                    foreach (var id in contractIds)
                    {
                        await ProcessContract(id);
                    }
                    transaction.Commit();
                }
            }catch(Exception ex)
            {
                _eventLog.Log(EventCode.InscriptionOffBalanceForContract, EventStatus.Failed, EntityType.Contract, contractId, uri: "/api/contractAction/testOffBalanceAddition", userId: _sessionContext.UserId, responseData: ex.Message);
                throw new PawnshopApplicationException(ex.Message);
            }

        }

        //to avoid nested try catch for better readability
        private async Task ProcessContract(int contractId)
        {
            try
            {
                var contract = _contractService.Get(contractId);

                if (!contract.UsePenaltyLimit || !contract.IsOffBalance)
                    throw new PawnshopApplicationException("Договор не соответствует для автоматического начисления пени");

                //проверяем ставки по начислению пени
                CheckAndSetDecreasedPenyRates(contract);
                SetPenyRateZeroForJanuary2022(contract);

                if (IsShortDiscrete(contract))
                {
                    AddOffBalanceForShortDiscrete(contract);
                }
                else
                {
                    AddOffBalanceForAnnuityOrProductDiscrete(contract);
                }
                _eventLog.Log(EventCode.InscriptionOffBalanceForContract, EventStatus.Success, EntityType.Contract, contract.Id, userId: _sessionContext.UserId, requestData: $"Договор {contractId} отработал начисление пени на внебалансе до {endDate}");
            }
            catch (Exception ex)
            {
                _eventLog.Log(EventCode.InscriptionOffBalanceForContract, EventStatus.Failed, EntityType.Contract, contractId, userId: _sessionContext.UserId);
                throw new PawnshopApplicationException($"Неудачное начисление для договора с айди {contractId}. Ошибка: {ex.Message}. Stack trace: {ex.StackTrace}");
            }
        }

        private bool IsShortDiscrete(Contract contract)
        {
            return contract.PercentPaymentType == Pawnshop.AccountingCore.Models.PercentPaymentType.EndPeriod && contract.LoanPeriod == Constants.SELLING_DISCRETE_LOAN_PERIOD;
        }

        /// <summary>
        /// Внебалансовые начисления для договоров - старых коротких дискретов на 30 дней
        /// </summary>
        /// <param name="contract">Договор с информацией о нем</param>
        private void AddOffBalanceForShortDiscrete(Contract contract)
        {

            var scheduleItemWithNoActualDate = _contractPaymentScheduleService.GetListByContractId(contract.Id).Where(x => x.ActualDate == null).OrderBy(x => x.Date).FirstOrDefault();
            var penaltyLimitCursorDate = GetPenaltyLimitCursorDateForFirstAccrual(contract);

            //начисление процентов и вынос на просрочку на контрольную дату
            // если вынос на просрочку уже был, то не делаем действие
            if (!contract.Actions.Any(x => x.ActionType == Data.Models.Contracts.Actions.ContractActionType.MoveToOverdue 
                                           && x.Date == scheduleItemWithNoActualDate.Date))
                _takeAwayToDelay.TakeAwayToDelay(contract, scheduleItemWithNoActualDate.Date, scheduleItemWithNoActualDate.Date, Constants.ADMINISTRATOR_IDENTITY);

            //начисление процентов на дату окончания отработки сервиса
            _interestAccrual.ManualInterestAccrualOnOverdueDebt(contract, Constants.ADMINISTRATOR_IDENTITY, endDate);

            AddOffBalancePeny(contract, penaltyLimitCursorDate);
        }


        /// <summary>
        /// Внебалансовые начисления для договоров - продуктовых и аннуитетов
        /// </summary>
        /// <param name="contract"></param>
        private void AddOffBalanceForAnnuityOrProductDiscrete(Contract contract)
        {
            var penaltyLimitCursorDate = GetPenaltyLimitCursorDateForFirstAccrual(contract);
            AddOffBalancePeny(contract, penaltyLimitCursorDate);
        }

        //начисление пени на внебалансе
        private void AddOffBalancePeny(Contract contract, DateTime penaltyLimitCursorDate)
        {
            var accounts = _accountService.List(new Models.List.ListQueryModel<AccountFilter>() { Page = null, Model = new AccountFilter() { ContractId = contract.Id } }).List;
            var penaltyLimitAccount = accounts.Where(x => x.AccountSetting.Code == Constants.ACCOUNT_SETTING_PENALTY_LIMIT).FirstOrDefault();
            var penaltyLimitAccountRecords = _accountRecordService.List(new ListQueryModel<AccountRecordFilter>() { Model = new AccountRecordFilter() { AccountId = penaltyLimitAccount.Id }, Page = new Page() { Limit = int.MaxValue - 1 }  }).List;
            var offbalancePenyProfitAccount = accounts.Where(x => x.AccountSetting.Code == Constants.ACCOUNT_SETTING_PENY_PROFIT + OFFBALANCE_ENDING).FirstOrDefault();
            var offbalancePenyAccountAccount = accounts.Where(x => x.AccountSetting.Code == Constants.ACCOUNT_SETTING_PENY_ACCOUNT + OFFBALANCE_ENDING).FirstOrDefault();
            var offbalancePenyProfitAccountRecords = _accountRecordService.List(new ListQueryModel<AccountRecordFilter>() { Model = new AccountRecordFilter() { AccountId = offbalancePenyProfitAccount.Id }, Page = new Page() { Limit = int.MaxValue - 1 } }).List;
            var offbalancePenyAccountAccountRecords = _accountRecordService.List(new ListQueryModel<AccountRecordFilter>() { Model = new AccountRecordFilter() { AccountId = offbalancePenyAccountAccount.Id }, Page = new Page() { Limit = int.MaxValue - 1 } }).List;
            //когда график по начислению закончился, доначисляем пеню и лимит каждый год
            while (penaltyLimitCursorDate < endDate)
            {
                //если есть история начисления пени на эту дату, то не начисляем
                if (offbalancePenyProfitAccountRecords == null || offbalancePenyAccountAccountRecords == null ||
                    !(offbalancePenyAccountAccountRecords.Any(x => x.Date.Date == penaltyLimitCursorDate.AddDays(-1)) 
                    || offbalancePenyProfitAccountRecords.Any(x => x.Date.Date == penaltyLimitCursorDate.AddDays(-1))))
                    _penaltyAccrualService.Execute(contract, penaltyLimitCursorDate.AddDays(-1), Constants.ADMINISTRATOR_IDENTITY);

                //если есть история по начислению лимита пени на дату, то не начисляем
                if(!penaltyLimitAccountRecords.Any(x => x.Date.Date == penaltyLimitCursorDate.Date))
                    _penaltyLimitAccrualService.ManualPenaltyLimitAccrual(contract, penaltyLimitCursorDate, Constants.ADMINISTRATOR_IDENTITY);
                penaltyLimitCursorDate = penaltyLimitCursorDate.AddYears(1);
            }
            //доначисляем пеню до дня окончания отработки сервиса
            _penaltyAccrualService.Execute(contract, endDate, Constants.ADMINISTRATOR_IDENTITY);
        }

        private DateTime GetPenaltyLimitCursorDateForFirstAccrual(Contract contract)
        {
            var penaltyLimitCursorDate = contract.ParentId != null ? _contractService.GetOnlyContract(contract.ParentId.Value).ContractDate : contract.ContractDate;
            var scheduleItemWithNoActualDate = _contractPaymentScheduleService.GetListByContractId(contract.Id).Where(x => x.ActualDate == null).OrderBy(x => x.Date).FirstOrDefault();

            if (scheduleItemWithNoActualDate == null)
                throw new PawnshopApplicationException($"Не найден график с неоплаченным платежом для договора {contract.ContractNumber}");

            while(penaltyLimitCursorDate.Date <= scheduleItemWithNoActualDate.Date)
            {
                penaltyLimitCursorDate = penaltyLimitCursorDate.AddYears(1);
            }

            return penaltyLimitCursorDate;
            
        }

        //уменьшение ставки пени на 90-ый день просрочки
        private void CheckAndSetDecreasedPenyRates(Contract contract)
        {
            var hasDecreasedPenyProfitRate = contract.ContractRates.Where(x => x.RateSetting.Code == Constants.ACCOUNT_SETTING_PENY_PROFIT && x.Rate == Constants.NBRK_PENALTY_RATE).FirstOrDefault() != null;
            var hasDecreasedPenyAccountRate = contract.ContractRates.Where(x => x.RateSetting.Code == Constants.ACCOUNT_SETTING_PENY_ACCOUNT && x.Rate == Constants.NBRK_PENALTY_RATE).FirstOrDefault() != null;
            if (!(hasDecreasedPenyAccountRate && hasDecreasedPenyProfitRate))
            {
                var schedulePayment = contract.PaymentSchedule.Where(x => x.ActualDate == null).OrderBy(x => x.Date).FirstOrDefault();
                var dateForPenaltyDecrease = schedulePayment.Date.AddDays(Constants.NBRK_PENALTY_DECREASE_PERIOD_FROM);

                if (dateForPenaltyDecrease > DateTime.Today.Date)
                    return;

                _penaltyRateService.IncreaseOrDecreaseRateManualy(contract, dateForPenaltyDecrease.Date, Constants.ADMINISTRATOR_IDENTITY, false);
                contract.ContractRates = _contractRateRepository.List(new ListQuery(), new { ContractId = contract.Id });
            }
        }

        #region уменьшение ставок пени для январьских событий 2022 года
        // уменьшение ставок пени для январьских событий 2022 года
        private void SetPenyRateZeroForJanuary2022(Contract contract)
        {

            //if (contract.ContractDate > january2022BlackoutEndDate)
            //    return;

            if (contract.ContractRates.Any(x => x.Rate == 0 && x.Date == january2022BlackoutStartDate.Date))
                return;

            var normalPenyProfitRate = contract.ContractRates.Where(x => x.RateSetting.Code == Constants.ACCOUNT_SETTING_PENY_PROFIT && x.Rate != Constants.NBRK_PENALTY_RATE).OrderByDescending(x => x.Date).FirstOrDefault();
            var normalPenyAccountRate = contract.ContractRates.Where(x => x.RateSetting.Code == Constants.ACCOUNT_SETTING_PENY_ACCOUNT && x.Rate != Constants.NBRK_PENALTY_RATE).OrderByDescending(x => x.Date).FirstOrDefault();
            var decreasedPenyProfitRate = contract.ContractRates.Where(x => x.RateSetting.Code == Constants.ACCOUNT_SETTING_PENY_PROFIT && x.Rate == Constants.NBRK_PENALTY_RATE).OrderByDescending(x => x.Date).FirstOrDefault();
            var decreasedPenyAccountRate = contract.ContractRates.Where(x => x.RateSetting.Code == Constants.ACCOUNT_SETTING_PENY_ACCOUNT && x.Rate == Constants.NBRK_PENALTY_RATE).OrderByDescending(x => x.Date).FirstOrDefault();

            #region добавление нулевой пени для январьских событий

            ContractRate zeroProfitPenyRate = new ContractRate
            {
                ContractId = contract.Id,
                Rate = 0,
                Date = january2022BlackoutStartDate.Date,
                RateSetting = normalPenyProfitRate.RateSetting,
                RateSettingId = normalPenyProfitRate.RateSettingId,
                CreateDate = DateTime.Now,
                AuthorId = Constants.ADMINISTRATOR_IDENTITY
            };

            ContractRate zeroAccountPenyRate = new ContractRate
            {
                ContractId = contract.Id,
                Rate = 0,
                Date = january2022BlackoutStartDate.Date,
                RateSetting = normalPenyAccountRate.RateSetting,
                RateSettingId = normalPenyAccountRate.RateSettingId,
                CreateDate = DateTime.Now,
                AuthorId = Constants.ADMINISTRATOR_IDENTITY
            };

            contract.ContractRates.Add(zeroAccountPenyRate);
            contract.ContractRates.Add(zeroProfitPenyRate);

            #endregion

            #region восстановление ставок пени для январьских событий

            // если нет уменьшенной ставки, то восстанавливаем процент пени по договору
            if(decreasedPenyProfitRate == null || decreasedPenyAccountRate == null)
            {
                ContractRate profitRate = normalPenyProfitRate.Clone();
                ContractRate accountRate = normalPenyAccountRate.Clone();
                SetNewPenyRatesForContract(profitRate, accountRate, contract);
            }
            // если ставка пени была на дату восстановления начисления пени, то ничего не делаем, так как ставка уже стоит на правильный день
            else if (decreasedPenyProfitRate.Date.Date == january2022BlackoutEndDate.AddDays(1).Date || decreasedPenyAccountRate.Date.Date == january2022BlackoutEndDate.AddDays(1).Date || normalPenyAccountRate.Date.Date == january2022BlackoutEndDate.AddDays(1).Date || normalPenyProfitRate.Date.Date == january2022BlackoutEndDate.AddDays(1).Date)
            {
            }
            //если снижение процента пени было после конца январьских событий, то восстанавливаем процент пени по договору до снижения
            else if (decreasedPenyProfitRate.Date.Date >= january2022BlackoutEndDate.Date || decreasedPenyAccountRate.Date.Date >= january2022BlackoutEndDate.Date)
            {
                ContractRate profitRate = normalPenyProfitRate.Clone();
                ContractRate accountRate = normalPenyAccountRate.Clone();
                SetNewPenyRatesForContract(profitRate, accountRate, contract);
            }
            // если снижение процента пени было до конца январьских событий, то восстанавливаем сниженный процент пени
            else if (decreasedPenyAccountRate.Date.Date <= january2022BlackoutEndDate.Date || january2022BlackoutEndDate.Date.Date <= january2022BlackoutStartDate.Date)
            {
                //добавление возвращения ставки для январьских событий
                ContractRate profitRate = decreasedPenyProfitRate.Clone();
                ContractRate accountRate = decreasedPenyAccountRate.Clone();
                SetNewPenyRatesForContract(profitRate, accountRate, contract);

                //если сниженная ставка была добавлена во время приостановки начислений во время январьских событий, то удаляем её
                if (decreasedPenyAccountRate.Date.Date >= january2022BlackoutStartDate || decreasedPenyProfitRate.Date.Date >= january2022BlackoutStartDate)
                {
                    contract.ContractRates.Remove(decreasedPenyProfitRate);
                    contract.ContractRates.Remove(decreasedPenyAccountRate);
                }
            }
            #endregion

            bool? isFloatingDiscrete = contract.Setting?.IsFloatingDiscrete ?? null;

            _contractRateService.DeleteAndInsert(contract.ContractRates, isFloatingDiscrete);
        }

        private void SetNewPenyRatesForContract(ContractRate profitRate, ContractRate accountRate, Contract contract)
        {
            profitRate.Id = 0;
            accountRate.Id = 0;
            profitRate.CreateDate = DateTime.Now;
            accountRate.CreateDate = DateTime.Now;
            profitRate.Date = january2022BlackoutEndDate.AddDays(1).Date;
            accountRate.Date = january2022BlackoutEndDate.AddDays(1).Date;
            contract.ContractRates.Add(profitRate);
            contract.ContractRates.Add(accountRate);
        }
        #endregion
    }
}