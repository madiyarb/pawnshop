using Pawnshop.AccountingCore.Abstractions;
using Pawnshop.AccountingCore.Models;
using Pawnshop.Core;
using Pawnshop.Data.Models.Audit;
using Pawnshop.Data.Models.Contracts;
using Pawnshop.Data.Models.Contracts.Inscriptions;
using Pawnshop.Services.Contracts;
using Pawnshop.Services.Models.Filters;
using Pawnshop.Services.Models.List;
using Pawnshop.Services.PenaltyLimit;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Pawnshop.Services.AccountingCore
{
    public class ProcessingService : IProcessingService
    {
        private readonly ITakeAwayToDelay _takeAwayToDelayService;
        private readonly IPenaltyRateService _penaltyRateService;
        private IDictionaryWithSearchService<AccrualBase, AccrualBaseFilter> _accrualBaseService;
        private readonly IPenaltyLimitAccrualService _penaltyLimitAccrualService;
        private readonly IEventLog _eventLog;
        private readonly IPenaltyAccrual _penaltyAccrualService;
        private readonly IInterestAccrual _interestAccrualService;
        private readonly IContractService _contractService;

        public ProcessingService(
            ITakeAwayToDelay takeAwayToDelayService,
            IPenaltyRateService penaltyRateService,
            IDictionaryWithSearchService<AccrualBase, AccrualBaseFilter> accrualBaseService,
            IPenaltyLimitAccrualService penaltyLimitAccrualService,
            IPenaltyAccrual penaltyAccrualService,
            IInterestAccrual interestAccrualService,
            IContractService contractService,
            IEventLog eventLog
            )
        {
            _takeAwayToDelayService = takeAwayToDelayService;
            _penaltyRateService = penaltyRateService;
            _accrualBaseService = accrualBaseService;
            _penaltyLimitAccrualService = penaltyLimitAccrualService;
            _penaltyAccrualService = penaltyAccrualService;
            _interestAccrualService = interestAccrualService;
            _contractService = contractService;
            _eventLog = eventLog;
        }

        private bool isDateToTakeAway(ContractPaymentSchedule overdueSchedule, DateTime accrualDate)
        {
            if (overdueSchedule == null)
            {
                return false;
            }

            if (overdueSchedule.Date == accrualDate)
            {
                return true;
            }

            return overdueSchedule.NextWorkingDate.HasValue && overdueSchedule.NextWorkingDate == accrualDate;
        }
        private void TakeAwayToDelay(ref Contract contract, DateTime accrualDate)
        {
            try
            {
                ContractPaymentSchedule overdueSchedule = contract.PaymentSchedule.Where(x => x.Date <= accrualDate.Date && x.ActualDate == null).OrderByDescending(x => x.Date).FirstOrDefault();

                if (isDateToTakeAway(overdueSchedule, accrualDate))
                {
                    _takeAwayToDelayService.TakeAwayToDelay(contract, overdueSchedule.Date, accrualDate.Date, Constants.ADMINISTRATOR_IDENTITY);

                    if (contract.PercentPaymentType == PercentPaymentType.EndPeriod)
                    {
                        contract.PaymentSchedule = _contractService.GetOnlyPaymentSchedule(contract.Id);
                    }
                }
            }
            catch (Exception ex)
            {
                _eventLog.Log(EventCode.OnlinePaymentError, EventStatus.Failed, EntityType.Contract, contract.Id, $"Ошибка при выносе на просрочку {ex.Message}");
            }
        }

        private void DecreaseRates(ref Contract contract, DateTime accrualDate)
        {
            try
            {
                double dateDiff = (accrualDate.Date - contract.NextPaymentDate.Value.Date).TotalDays;
                if (dateDiff == Constants.NBRK_PENALTY_DECREASE_PERIOD_FROM)
                {
                    List<AccrualBase> accrualSettings = _accrualBaseService.List(new ListQueryModel<AccrualBaseFilter> { Page = null, Model = new AccrualBaseFilter { AccrualType = AccrualType.PenaltyAccrual, IsActive = true } }).List;
                    _penaltyRateService.DecreaseRates(contract, accrualDate, Constants.ADMINISTRATOR_IDENTITY, accrualSettings);
                }
            }
            catch (Exception ex)
            {
                _eventLog.Log(EventCode.OnlinePaymentError, EventStatus.Failed, EntityType.Contract, contract.Id, $"Ошибка при снижении ставки пени {ex.Message}");
            }
        }

        private void PenaltyAccrual(ref Contract contract, DateTime accrualDate)
        {
            try
            {
                _penaltyAccrualService.Execute(contract, accrualDate, Constants.ADMINISTRATOR_IDENTITY);
            }
            catch (Exception ex)
            {
                _eventLog.Log(EventCode.OnlinePaymentError, EventStatus.Failed, EntityType.Contract, contract.Id, $"Ошибка при начислении пени {ex.Message}");
            }
        }

        private void InterestAccrual(ref Contract contract, DateTime accrualDate)
        {
            try
            {
                ContractPaymentSchedule nextSchedule = contract.PaymentSchedule.Where(x => x.Date >= accrualDate.Date && x.ActualDate == null).FirstOrDefault();
                if (nextSchedule != null)
                {
                    _interestAccrualService.OnAnyDateAccrual(contract, Constants.ADMINISTRATOR_IDENTITY, accrualDate);
                }
            }
            catch (Exception ex)
            {
                _eventLog.Log(EventCode.OnlinePaymentError, EventStatus.Failed, EntityType.Contract, contract.Id, $"Ошибка при начислении процентов {ex.Message}");
            }
        }

        private void InterestAccrualOnOverdueDebt(ref Contract contract, DateTime accrualDate)
        {
            try
            {
                _interestAccrualService.OnAnyDateOnOverdueDebt(contract, Constants.ADMINISTRATOR_IDENTITY, accrualDate);
            }
            catch (Exception ex)
            {
                _eventLog.Log(EventCode.OnlinePaymentError, EventStatus.Failed, EntityType.Contract, contract.Id, $"Ошибка при начислении процентов на просроченный ОД {ex.Message}");
            }
        }

        private void PenaltyLimitAcctrual(ref Contract contract, DateTime accrualDate)
        {
            try
            {
                if (contract.ParentId.HasValue)
                {
                    Contract parentContract = _contractService.Get(contract.ParentId.Value);

                    if (accrualDate.Day == parentContract.ContractDate.Day && accrualDate.Month == parentContract.ContractDate.Month)
                        _penaltyLimitAccrualService.Execute(contract, parentContract, accrualDate, Constants.ADMINISTRATOR_IDENTITY);
                }
                else
                {
                    if (accrualDate.Day == contract.ContractDate.Day && accrualDate.Month == contract.ContractDate.Month)
                        _penaltyLimitAccrualService.Execute(contract, accrualDate, Constants.ADMINISTRATOR_IDENTITY);
                }
            }
            catch (Exception ex)
            {
                _eventLog.Log(EventCode.OnlinePaymentError, EventStatus.Failed, EntityType.Contract, contract.Id, $"Ошибка при начислении лимита пени {ex.Message}");
            }
        }

        public void InitAccruals(Contract contract, DateTime accrualDate)
        {
            // вынос
            if (contract.PaymentSchedule != null && contract.PaymentSchedule.Any())
            {
                TakeAwayToDelay(ref contract, accrualDate.AddDays(-1));
            }

            // Изменение ставки пени
            if (contract.NextPaymentDate.HasValue && contract.UsePenaltyLimit)
            {
                DecreaseRates(ref contract, accrualDate);
            }

            bool hasInscription = contract.InscriptionId.HasValue && contract.Inscription != null && contract.Inscription.Status != InscriptionStatus.Denied;

            // Начисление лимита пени
            if (contract.UsePenaltyLimit && !hasInscription)
            {
                PenaltyLimitAcctrual(ref contract, accrualDate);
            }

            // Начисление пени
            if (contract.NextPaymentDate.HasValue && contract.NextPaymentDate < accrualDate && !hasInscription)
            {
                PenaltyAccrual(ref contract, accrualDate);
            }

            // Начисление процентов
            if (contract.PaymentSchedule != null && contract.PaymentSchedule.Any())
            {
                InterestAccrual(ref contract, accrualDate);
            }

            // Начисление процентов на просроченный ОД
            if (contract.PercentPaymentType == PercentPaymentType.EndPeriod && contract.NextPaymentDate.HasValue && contract.NextPaymentDate.Value.Date < accrualDate.Date)
            {
                InterestAccrualOnOverdueDebt(ref contract, accrualDate);
            }
        }

        // проверить время запроса
        // если время с 22,30 до 10,00 заполнить дату начислений
        public DateTime? CalcAccrualDate(DateTime date)
        {

            DateTime? accrualDate = null;
            TimeSpan dayTime;
            if (date == default)
                dayTime = DateTime.Now.TimeOfDay;
            else
                dayTime = date.TimeOfDay;

            if (dayTime >= Constants.STOP_ONLINE_PAYMENTS)
                accrualDate = date == default ? DateTime.Now.Date.AddDays(1) : date.Date.AddDays(1);
            else if (dayTime < Constants.START_ONLINE_PAYMENTS)
                accrualDate = date == default ? DateTime.Now.Date : date.Date;

            return accrualDate;
        }
    }
}
