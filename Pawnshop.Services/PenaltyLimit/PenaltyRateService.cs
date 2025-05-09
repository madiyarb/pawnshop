using Pawnshop.AccountingCore.Models;
using Pawnshop.Core;
using Pawnshop.Core.Exceptions;
using Pawnshop.Data.Models.Audit;
using Pawnshop.Data.Models.Contracts;
using Pawnshop.Data.Models.Contracts.Actions;
using Pawnshop.Data.Models.Contracts.Penalty;
using Pawnshop.Services.ClientDeferments.Interfaces;
using Pawnshop.Services.Contracts;
using Pawnshop.Services.Models.Filters;
using Pawnshop.Services.Models.List;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Pawnshop.Services.PenaltyLimit
{
    public class PenaltyRateService : IPenaltyRateService
    {
        private readonly IEventLog _eventLog;
        private readonly IDictionaryWithSearchService<AccrualBase, AccrualBaseFilter> _accrualBaseService;
        private readonly IDictionaryWithSearchService<AccountSetting, AccountSettingFilter> _accountSettingService;
        private readonly IContractRateService _contractRateService;
        private readonly IContractActionService _contractActionService;
        private readonly IClientDefermentService _clientDefermentService;
        static string _increaseStr = "увеличить";
        static string _decreaseStr = "уменьшить";

        public PenaltyRateService(IEventLog eventLog, IDictionaryWithSearchService<AccrualBase, AccrualBaseFilter> accrualBaseService,
                                  IDictionaryWithSearchService<AccountSetting, AccountSettingFilter> accountSettingService,
                                  IContractRateService contractRateService, IContractActionService contractActionService,
                                  IClientDefermentService clientDefermentService)
        {
            _eventLog = eventLog;
            _accrualBaseService = accrualBaseService;
            _accountSettingService = accountSettingService;
            _contractRateService = contractRateService;
            _contractActionService = contractActionService;
            _clientDefermentService = clientDefermentService;
        }

        public void IncreaseOrDecreaseRateManualy(Contract contract, DateTime date, int authorId, bool increase = true)
        {
            if (contract.Status != ContractStatus.Signed && contract.Status != ContractStatus.SoldOut)
                throw new PawnshopApplicationException($"Договор c Id {contract.Id} должен быть подписан или отправлен на реализацию");

            if (!contract.UsePenaltyLimit)
                throw new PawnshopApplicationException($"По Договору c Id {contract.Id} невозможно {(increase ? _increaseStr : _decreaseStr)} ставку пени");

            //if (date.Date < Constants.PENY_LIMIT_DATE)
            //    throw new PawnshopApplicationException($"Дата уменьшения ставки пени должна быть после {Constants.PENY_LIMIT_DATE:dd.MM.yyyy}");

            if (date.Date < contract.ContractDate)
                throw new PawnshopApplicationException($"Дата уменьшения ставки пени должна быть больше даты выдачи Договора {contract.ContractDate:dd.MM.yyyy}");

            switch (increase)
            {
                case true:
                {
                    IncreaseRates(contract, date, Constants.ADMINISTRATOR_IDENTITY);

                    break;
                }
                case false:
                {
                    if (IsDecreaseRateDaysPeriod(contract, date))
                    {
                        var accrualSettings = _accrualBaseService.List(new ListQueryModel<AccrualBaseFilter> { Page = null, Model = new AccrualBaseFilter { AccrualType = AccrualType.PenaltyAccrual, IsActive = true } }).List;
                        DecreaseRates(contract, date, Constants.ADMINISTRATOR_IDENTITY, accrualSettings);
                    }

                    break;
                }
            }
        }

        private bool IsDecreaseRateDaysPeriod(Contract contract, DateTime date)
        {
            if (ActualPenaltyDaysFromContractSchedule(contract, date) == Constants.NBRK_PENALTY_DECREASE_PERIOD_FROM)
                return true;
            
            return false;
        }

        private int ActualPenaltyDaysFromContractSchedule(Contract contract, DateTime date)
        {
            int days = 0;
            var schedulePayment = contract.PaymentSchedule.Where(x => x.ActualDate == null || x.ActualDate > date).OrderBy(x => x.Date).FirstOrDefault();

            if (schedulePayment != null)
            {
                return (date.Date - schedulePayment.Date.Date ).Days;
            }

            return days;
        }

        public void IncreaseRates(Contract contract, DateTime date, int authorId, ContractAction? parentAction = null)
        {
            var defermentInformation = _clientDefermentService.GetDefermentInformation(contract.Id, date);
            if (defermentInformation != null &&
                ((defermentInformation.IsInDefermentPeriod && defermentInformation.Status == Data.Models.Restructuring.RestructuringStatusEnum.Restructured) ||
                defermentInformation.Status == Data.Models.Restructuring.RestructuringStatusEnum.Frozen))
            {
                return;
            }

            if (!contract.UsePenaltyLimit)
                return;

            if (ActualPenaltyDaysFromContractSchedule(contract, date) >= Constants.NBRK_PENALTY_DECREASE_PERIOD_FROM)
                return;

            var accrualSettings = _accrualBaseService.List(new ListQueryModel<AccrualBaseFilter> { Page = null, Model = new AccrualBaseFilter { AccrualType = AccrualType.PenaltyAccrual, IsActive = true } }).List;

            decimal newRate = 0;
            try
            {
                using (var transaction = _contractActionService.BeginContractActionTransaction())
                {
                    var penaltyRateIncreaseAction = new ContractAction
                    {
                        ActionType = ContractActionType.PenaltyRateIncrease,
                        AuthorId = authorId,
                        ContractId = contract.Id,
                        Date = date,
                        CreateDate = DateTime.Now
                    };

                    foreach(var x in accrualSettings)
                    {
                        ContractRate contractRate = _contractRateService.GetRateOnDateByContractAndRateSettingId(contract.Id, (int)x.RateSettingId, date);
                        if (contractRate is null)
                            //throw new NullReferenceException($"Массив процентных ставок пустой по Договору {contract.Id} для SettingId {(int)x.RateSettingId}");
                            continue;

                        if (contractRate.Rate != Constants.NBRK_PENALTY_RATE)
                            return;

                        var contractRates = _contractRateService.GetLastTwoRatesOnDateByContractAndRateSettingId(contract.Id, (int)x.RateSettingId, date);
                        if (!contractRates.Any())
                            throw new NullReferenceException($"Массив процентных ставок пустой по Договору {contract.Id}");

                        if (contractRates.Count() < 2)
                            return;

                        var currentRate = contractRates.First();
                        var beforeRate = contractRates.Last();

                        if (currentRate.Rate >= beforeRate.Rate)
                            return;

                        if (currentRate.Rate < beforeRate.Rate)
                            newRate = beforeRate.Rate;

                        if (penaltyRateIncreaseAction.Id == 0)
                        {
                            penaltyRateIncreaseAction.TotalCost = newRate;
                            penaltyRateIncreaseAction.Cost = newRate;
                            penaltyRateIncreaseAction.Reason = $"Увеличение ставки пени по договору {contract.ContractNumber} с {date:dd.MM.yyyy} на {newRate}";
                            _contractActionService.Save(penaltyRateIncreaseAction);

                            if (parentAction != null)
                            {
                                parentAction.ParentActionId = penaltyRateIncreaseAction.Id;
                                penaltyRateIncreaseAction.ChildActionId = parentAction.Id;
                                _contractActionService.Save(penaltyRateIncreaseAction);
                                _contractActionService.Save(parentAction);
                            }
                        }
                        
                        var newContractRate = new ContractRate
                        {
                            ContractId = contract.Id,
                            Date = date,
                            RateSettingId = (int)x.RateSettingId,
                            Rate = newRate,
                            CreateDate = DateTime.Now,
                            AuthorId = Constants.ADMINISTRATOR_IDENTITY,
                            ActionId = penaltyRateIncreaseAction.Id
                        };
                        List<ContractRate> addedContractRates = new List<ContractRate>() { newContractRate };
                        _contractRateService.DeleteAndInsert(addedContractRates, contract.Setting?.IsFloatingDiscrete);

                        _eventLog.Log(EventCode.ContractPenaltyRateIncrease, EventStatus.Success, EntityType.Contract, contract.Id, $"Увеличение ставки пени для Договора {contract.Id} на {newRate}", userId: authorId);
                    }
                    
                    transaction.Commit();
                };
            }
            catch (Exception e)
            {
                _eventLog.Log(EventCode.ContractPenaltyRateIncrease, EventStatus.Failed, EntityType.Contract, contract.Id, $"Увеличение ставки пени для Договора {contract.Id} на {newRate}", userId: authorId);
                throw;
            }
        }

        public void DecreaseRates(Contract contract, DateTime date, int authorId, List<AccrualBase> accrualSettings)
        {
            var defermentInformation = _clientDefermentService.GetDefermentInformation(contract.Id, date);
            if (defermentInformation != null &&
                ((defermentInformation.IsInDefermentPeriod && defermentInformation.Status == Data.Models.Restructuring.RestructuringStatusEnum.Restructured) ||
                defermentInformation.Status == Data.Models.Restructuring.RestructuringStatusEnum.Frozen))
            {
                return;
            }

            try
            {
                using (var transaction = _contractActionService.BeginContractActionTransaction())
                {
                    var penaltyRateDecreaseAction = new ContractAction
                    {
                        ActionType = ContractActionType.PenaltyRateDecrease,
                        AuthorId = authorId,
                        ContractId = contract.Id,
                        TotalCost = Constants.NBRK_PENALTY_RATE,
                        Cost = Constants.NBRK_PENALTY_RATE,
                        Reason = $"Уменьшение ставки пени по договору {contract.ContractNumber} с {date:dd.MM.yyyy} на {Constants.NBRK_PENALTY_RATE}",
                        Date = date,
                        CreateDate = DateTime.Now
                    };
                    
                    foreach(var accrualSetting in accrualSettings)
                    { 
                        ContractRate contractRate = _contractRateService.GetRateOnDateByContractAndRateSettingId(contract.Id, (int)accrualSetting.RateSettingId, date);
                        if (contractRate is null)
                            continue;

                        if (contractRate.Rate > Constants.NBRK_PENALTY_RATE)
                        {
                            _contractActionService.Save(penaltyRateDecreaseAction);

                            var newContractRate = new ContractRate
                            {
                                ContractId = contract.Id,
                                Date = date,
                                RateSettingId = (int)accrualSetting.RateSettingId,
                                Rate = Constants.NBRK_PENALTY_RATE,
                                CreateDate = DateTime.Now,
                                AuthorId = Constants.ADMINISTRATOR_IDENTITY,
                                ActionId = penaltyRateDecreaseAction.Id
                            };
                            List<ContractRate> contractRates = new List<ContractRate>() { newContractRate };
                            _contractRateService.DeleteAndInsert(contractRates, contract.Setting?.IsFloatingDiscrete);

                            _eventLog.Log(EventCode.ContractPenaltyRateDecrease, EventStatus.Success, EntityType.Contract, contract.Id, $"Уменьшение ставки пени для Договора {contract.Id} на {Constants.NBRK_PENALTY_RATE}", userId: authorId);
                        }
                    }

                    transaction.Commit();
                }
            }
            catch (Exception e)
            {
                _eventLog.Log(EventCode.ContractPenaltyRateDecrease, EventStatus.Failed, EntityType.Contract, contract.Id, $"Уменьшение ставки пени для Договора {contract.Id} на {Constants.NBRK_PENALTY_RATE}", userId: authorId);
                throw;
            }
        }
    }
}
