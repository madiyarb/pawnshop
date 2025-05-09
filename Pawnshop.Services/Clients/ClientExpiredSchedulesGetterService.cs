using System;
using System.Collections.Generic;
using System.Linq;
using Pawnshop.AccountingCore.Models;
using Pawnshop.Data.Access;
using Serilog;

namespace Pawnshop.Services.Clients
{
    public sealed class ClientExpiredSchedulesGetterService : IClientExpiredSchedulesGetterService
    {
        private readonly ContractPaymentScheduleRepository _contractPaymentScheduleRepository;
        private readonly ContractRepository _contractRepository;
        private readonly ILogger _logger;
        public ClientExpiredSchedulesGetterService(
            ContractPaymentScheduleRepository contractPaymentScheduleRepository,
            ContractRepository contractRepository, 
            ILogger logger)
        {
            _contractPaymentScheduleRepository = contractPaymentScheduleRepository;
            _contractRepository = contractRepository;
            _logger = logger;
        }

        public int? Calculate(int clientId)
        {
            var contracts =
                _contractRepository.GetContractsByClientIdAndContractClases(clientId, new List<int>{ 1, 3 })
                    .Where(contract => (int)contract.Status >= 30);
                //Contracts & tranches
            int maxExpirePeriod = 0;
            foreach (var contract in contracts)
            {
                if (contract.IsOffBalance)
                {
                    try
                    {
                        var schedules = _contractPaymentScheduleRepository
                            .GetListByContractId(contract.Id);
                        var maxDelay = schedules
                            .Where(cps => cps.ActualDate != null && cps.Date != null)
                            .Max(cps => cps.ActualDate - cps.Date)
                            .Value.Days;
                        if (maxDelay > maxExpirePeriod)
                            maxExpirePeriod = maxDelay;

                        var lastPaymentShedule = schedules
                            .Where(cps => cps.ActualDate == null)
                            .OrderByDescending(cps => cps.Date)
                            .FirstOrDefault();
                        maxDelay = (DateTime.Now - lastPaymentShedule.Date).Days;
                        if (maxDelay > maxExpirePeriod)
                            maxExpirePeriod = maxDelay;
                    }
                    catch (NullReferenceException exception)
                    {
                        _logger.Error(exception, exception.Message);
                    }
                    catch (InvalidOperationException exception)
                    {
                        _logger.Error(exception, exception.Message);
                    }
                }
                else
                {
                    try
                    {
                        var schedules = _contractPaymentScheduleRepository
                            .GetListByContractId(contract.Id);
                        if (schedules != null && schedules.Count > 0)
                        {
                            var maxDelay = schedules
                                .Where(cps => cps.ActualDate != null && cps.Date != null)
                                .Max(cps => cps.ActualDate - cps.Date)
                                .Value.Days;
                            if (maxDelay > maxExpirePeriod)
                                maxExpirePeriod = maxDelay;
                        }
                    }
                    catch (NullReferenceException exception)
                    {
                        _logger.Error(exception, exception.Message);
                    }
                    catch (InvalidOperationException exception)
                    {
                        _logger.Error(exception, exception.Message);
                    }
                }
            }

            return maxExpirePeriod;
        }

        public List<ExpiredPaymentSchedule> GetAllExpiredPaymentSchedules(int clientId)
        {
            List<ExpiredPaymentSchedule> expiredSchedules = new List<ExpiredPaymentSchedule>();
            var contracts =
                _contractRepository.GetContractsByClientIdAndContractClases(clientId, new List<int>{ 1, 3 })
                    .Where(contract => (int)contract.Status >= 30); ;//Contracts & tranches
            foreach (var contract in contracts)
            {
                var schedules = _contractPaymentScheduleRepository
                    .GetListByContractId(contract.Id);
                expiredSchedules.AddRange(schedules
                    .Where(cps => cps.ActualDate != null && (cps.ActualDate - cps.Date).Value.Days > 0)
                    .Select(schedule => new ExpiredPaymentSchedule
                    {
                        Amount = schedule.DebtCost + schedule.PenaltyCost ?? 0 + schedule.PercentCost,
                        ContractNumber = contract.ContractNumber,
                        ExpiredDays = (schedule.ActualDate.Value - schedule.Date).Days,
                        MainDebt = schedule.DebtCost,
                        PaymentDate = schedule.Date,
                        RealPaymentDate = schedule.ActualDate,
                        Percent = schedule.PercentCost,
                        Penalty = schedule.PenaltyCost,
                        IsPayed = true
                    }));

                expiredSchedules.AddRange(schedules
                    .OrderBy(cps => cps.Date)
                    .Where(cps => cps.ActualDate == null && (DateTime.Now - cps.Date).Days > 0 
                                                         && (contract.BuyoutDate == null || cps.ActualDate < contract.BuyoutDate))
                    .Select(cps => 
                        new ExpiredPaymentSchedule
                        {
                            Amount = cps.DebtCost + cps.PenaltyCost ?? 0 + cps.PercentCost,
                            ContractNumber = contract.ContractNumber,
                            ExpiredDays = (DateTime.Now - cps.Date).Days,
                            RealPaymentDate = cps.ActualDate,
                            MainDebt = cps.DebtCost,
                            PaymentDate = cps.Date,
                            Percent = cps.PercentCost,
                            Penalty = cps.PenaltyCost,
                            IsPayed = false
                        }));
            }
            return expiredSchedules;
        }

        public bool SomeCurrentContractsOnExpiredNow(int clientId)
        {
            List<ExpiredPaymentSchedule> expiredSchedules = new List<ExpiredPaymentSchedule>();
            return _contractRepository.GetContractsByClientIdAndContractClases(clientId, new List<int> { 1, 3 })
                    .Any(contract => contract.IsOffBalance && contract.Status != ContractStatus.BoughtOut);
        }
    }
}
