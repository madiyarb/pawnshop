using Pawnshop.AccountingCore.Models;
using Pawnshop.Core.Exceptions;
using Pawnshop.Core.Extensions;
using Pawnshop.Core;
using Pawnshop.Data.Access;
using Pawnshop.Data.Models.Contracts.Actions;
using Pawnshop.Data.Models.Contracts;
using Pawnshop.Data.Models.LoanSettings;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System;

namespace Pawnshop.Services.Contracts
{
    [Obsolete("Use PaymentScheduleService")]
    public class ContractScheduleBuilder : IContractScheduleBuilder
    {
        public readonly IContractService _contractService;
        public readonly ContractRepository _contractRepository;
        public readonly IContractRateService _contractRateService;
        public readonly IContractPaymentScheduleService _contractPaymentScheduleService;
        private readonly IContractActionService _contractActionService;
        private readonly ContractAdditionalInfoRepository _сontractAdditionalInfoRepository;

        public ContractScheduleBuilder(IContractService contractService, IContractPaymentScheduleService contractPaymentScheduleService, IContractRateService contractRateService, IContractActionService contractActionService, ContractRepository contractRepository, ContractAdditionalInfoRepository сontractAdditionalInfoRepository)
        {
            _contractRepository = contractRepository;
            _contractService = contractService;
            _contractRateService = contractRateService;
            _contractPaymentScheduleService = contractPaymentScheduleService;
            _contractActionService = contractActionService;
            _сontractAdditionalInfoRepository = сontractAdditionalInfoRepository;
        }

        public List<ContractPaymentSchedule> BuildAnnuity(DateTime beginDate, decimal loanCost, decimal loanPercentPerDay, DateTime maturityDate, DateTime? firstPaymentDate = null, int? paymentsCount = null, int? paymentDebt = null, bool isCreditLine = false, bool migratedFromOnline = false, DateTime? changeDate = null, Contract contract = null, bool isChDP = false)
        {
            var isBuyCar = contract?.IsBuyCar ?? false;
            var start = beginDate;
            ContractAdditionalInfo contractInfo = contract != null ? _сontractAdditionalInfoRepository.Get(contract.Id) : null;
            var isControlDate = contract != null && (changeDate.HasValue || (contractInfo != null && contractInfo.ChangedControlDate.HasValue));
            if (isControlDate)
            {
                beginDate = changeDate.HasValue ? changeDate.Value : contractInfo.ChangedControlDate.Value;
                beginDate = beginDate.AddMonths(-1);
                maturityDate = contract.MaturityDate;
                if (!isChDP && contract.ContractClass == ContractClass.Tranche)
                {
                    var fisrtPaymentDate = contract.PaymentSchedule.ToList().OrderBy(x => x.Date).First().Date;
                    start = fisrtPaymentDate.AddMonths(-1);
                    paymentsCount = maturityDate.MonthDifference(start);
                }
            }
            paymentsCount ??= maturityDate.MonthDifference(start);
            if (paymentsCount <= 0)
                throw new PawnshopApplicationException($@"График не может быть создан с 0 и менее месяцами");

            int counter = 0;
            int debtCounter = 0;
            decimal monthPercent = loanPercentPerDay * 30;
            decimal balanceCost = loanCost;

            decimal monthlyPayment = 0;
            if (monthPercent != 0)
            {
                //для недопущения ошибки деления на 0
                monthlyPayment = (loanCost / Convert.ToDecimal((1 - Math.Pow((double)(1 + (monthPercent / 100)), -paymentsCount.Value)) / ((double)monthPercent / 100)));
            }
            else
            {
                //для нулевой ставки по процентам (продукт Грант Оркендеу)
                monthlyPayment = loanCost / paymentsCount.Value;
            }
            List<ContractPaymentSchedule> schedule = new List<ContractPaymentSchedule>();
            var existPayments = new List<ContractPaymentSchedule>();

            if (isControlDate)
            {
                migratedFromOnline = contract.ProductType?.Code == "TSO_MIGRATION";
                if (migratedFromOnline)
                {
                    monthPercent = loanPercentPerDay * 365 / 12;
                }
                var payments = contract.PaymentSchedule.Where(x => x.Date <= DateTime.Now).ToList();
                foreach (var item in payments)
                {
                    if (item.ActionId.HasValue)
                    {
                        var act = _contractActionService.GetAsync(item.ActionId.Value).Result;
                        if (act != null)
                        {
                            if (act.ActionType != ContractActionType.PartialPayment)
                            { item.ActionType = (int)act.ActionType; }
                            else
                            {
                                if (act.ActionType == ContractActionType.PartialPayment &&
                                    Math.Round(act.Cost) == Math.Round(item.DebtCost))
                                { item.ActionType = (int)act.ActionType; }
                            }
                        }
                    }
                }
                var fisrtDate = contract.FirstPaymentDate ?? contract.PaymentSchedule.ToList().OrderBy(x => x.Date).First().Date;
                var fromfirstDate = fisrtDate.AddMonths(-1);
                paymentsCount = maturityDate.MonthDifference(fromfirstDate);

                existPayments = payments.Where(x => x.ActionId != null && x.ActionType != 40).ToList();
                if (contract.ContractClass == ContractClass.Tranche && changeDate == null)
                {
                    paymentsCount = contract.ContractDate.MonthDifferenceWithChangedDate(maturityDate);
                    var creditLineContract = _contractService.GetOnlyContract(contract.CreditLineId.Value);
                    var tranches = _contractService.GetAllSignedTranches(creditLineContract.Id).Result;
                    if (tranches.Any())
                    {
                        var firstTranche = tranches.OrderBy(x => x.ContractDate).First();
                        var firstTrancheInfo = _сontractAdditionalInfoRepository.Get(firstTranche.Id);
                        if (firstTrancheInfo != null && firstTrancheInfo.ChangedControlDate != null)
                        {
                            var nextPaymentSchedule = _contractPaymentScheduleService.GetNextPaymentSchedule(firstTranche.Id, true).Result;
                            if ((nextPaymentSchedule.Date - DateTime.Now.Date).TotalDays < 15)
                            {
                                beginDate = nextPaymentSchedule.Date;
                                paymentsCount = beginDate.MonthDifferenceWithChangedDate(maturityDate);
                            }
                            else
                            {
                                paymentsCount = beginDate.MonthDifferenceWithChangedDate(maturityDate);
                            }
                            monthlyPayment = monthPercent != 0 ? (loanCost / Convert.ToDecimal((1 - Math.Pow((double)(1 + (monthPercent / 100)), -paymentsCount.Value)) / ((double)monthPercent / 100))) :
                                loanCost / paymentsCount.Value;
                        }
                    }
                }

                if (payments.Any())
                {
                    schedule = payments;
                    var fisrtPaymentDate = contract.FirstPaymentDate ?? contract.PaymentSchedule.ToList().OrderBy(x => x.Date).First().Date;
                    var fromStart = fisrtPaymentDate.AddMonths(-1);
                    if (!isChDP)
                    {
                        paymentsCount = maturityDate.MonthDifference(fromStart);
                        paymentsCount -= existPayments.Count;// в чдп режется кол-во платежей
                        monthlyPayment = monthPercent != 0 ? (loanCost / Convert.ToDecimal((1 - Math.Pow((double)(1 + (monthPercent / 100)), -paymentsCount.Value)) / ((double)monthPercent / 100))) :
                            loanCost / paymentsCount.Value;
                    }

                    if (!isChDP)
                    {
                        if (payments.Any())
                        {
                            var prev = payments.OrderByDescending(x => x.Date).First();
                            balanceCost = prev.DebtLeft;
                            monthlyPayment = monthPercent != 0 ? (balanceCost / Convert.ToDecimal((1 - Math.Pow((double)(1 + (monthPercent / 100)), -paymentsCount.Value)) / ((double)monthPercent / 100))) :
                                loanCost / paymentsCount.Value;
                        }
                    }

                    if (firstPaymentDate.HasValue && !isChDP)
                    {
                        if (payments.Any())
                        {
                            var prev = payments.OrderByDescending(x => x.Date).First();
                            balanceCost = prev.DebtLeft;
                        }
                        monthlyPayment = monthPercent != 0 ? (balanceCost / Convert.ToDecimal((1 - Math.Pow((double)(1 + (monthPercent / 100)), -paymentsCount.Value)) / ((double)monthPercent / 100))) :
                            loanCost / paymentsCount.Value;
                    }
                }

                if (firstPaymentDate.HasValue)
                {
                    firstPaymentDate = null;
                }
            }
            DateTime? prevDate = null;

            bool notFromStart = contract == null ? false : contract.ContractDate != contract.SignDate && contract.SignDate != null && isBuyCar && contract.ContractDate < contract.SignDate;

            while (paymentsCount != counter)
            {
                var item = new ContractPaymentSchedule();
                item.Revision = 1;
                if (firstPaymentDate.HasValue)
                {
                    if (counter == 0 && (beginDate.Date.AddMonths(1).Date != firstPaymentDate.Value.Date || (
                        contract != null && contract.SignDate.HasValue && contract.SignDate.Value.AddMonths(1).Date != firstPaymentDate.Value.Date)))
                    {
                        item.Date = firstPaymentDate.Value.Date;
                        if (notFromStart)
                        {
                            item.Period = (item.Date - contract.SignDate.Value).Days;
                        }
                        else
                        {
                            item.Period = (item.Date - beginDate.Date).Days;
                        }
                    }
                    else if (counter == paymentsCount - 1 && (isCreditLine || beginDate.AddMonths(paymentsCount.Value).Date != maturityDate.Date))
                    {
                        item.Date = maturityDate.Date;
                        item.Period = (maturityDate.Date - schedule.OrderByDescending(x => x.Date).FirstOrDefault().Date).Days;
                    }
                    else
                    {
                        item.Date = firstPaymentDate.Value.Date.AddMonths(counter);
                        item.Period = 30;
                    }
                }
                else
                {
                    item.Date = beginDate.Date.AddMonths(counter + 1);
                    item.Period = 30;
                    if (notFromStart && counter == 0)
                    {
                        item.Period = (item.Date - contract.SignDate.Value).Days;
                    }
                }

                if (migratedFromOnline)
                {
                    item.PercentCost = Math.Round((monthPercent / 100) * balanceCost, 2);
                }
                else if ((counter == 0 && firstPaymentDate.HasValue && beginDate.Date.AddMonths(1).Date != firstPaymentDate.Value.Date)
                    || (counter == paymentsCount - 1 && beginDate.AddMonths(paymentsCount ?? 0).Date != maturityDate.Date))
                {
                    item.PercentCost = Math.Round(item.Period * (loanPercentPerDay * 30 * 12 / 365 / 100) * balanceCost, 2);
                }
                else
                {
                    item.PercentCost = Math.Round(item.Period * (loanPercentPerDay / 100) * balanceCost, 2);
                }

                if (paymentDebt.HasValue && debtCounter + 1 >= paymentDebt)
                {
                    item.DebtCost = loanCost / ((decimal)paymentsCount / (decimal)paymentDebt);
                    item.DebtLeft = balanceCost - item.DebtCost;
                    debtCounter = -1;
                }
                else if (paymentDebt.HasValue && debtCounter + 1 < paymentDebt)
                {
                    item.DebtCost = 0;
                    item.DebtLeft = balanceCost - item.DebtCost;
                }
                else
                {
                    item.DebtCost = monthlyPayment * (decimal)Math.Pow((double)(1 + (monthPercent / 100)), (1 - (paymentsCount.Value - counter) - 1));
                    item.DebtLeft = balanceCost - item.DebtCost;
                }

                if (counter == paymentsCount - 1)
                {
                    item.DebtCost += item.DebtLeft;
                    item.DebtLeft = 0;
                }

                item.DebtCost = Math.Round(item.DebtCost, 2);
                item.DebtLeft = Math.Round(item.DebtLeft, 2);

                if (isControlDate)
                {
                    if (counter + 1 == paymentsCount)
                    {
                        var prev = schedule.OrderByDescending(x => x.Date).First();
                        item.Date = contract.MaturityDate;
                        item.Period = (item.Date - prev.Date).Days;
                        item.PercentCost = Math.Round(item.Period * (loanPercentPerDay / 100) * balanceCost, 2);
                        if (!migratedFromOnline)
                        {
                            item.PercentCost = item.PercentCost * 360 / 365;
                        }
                    }
                    else if (counter == 0)
                    {
                        var startDate = contract.ContractDate;
                        if (notFromStart)
                        {
                            startDate = contract.SignDate.Value;
                        }
                        var date = schedule.Any() ? schedule.OrderByDescending(x => x.Date).First().Date : startDate.Date;
                        //сделал schedulle, т к в случае если было чдп, но не было платежей, не верно высчитывало
                        if (isChDP)
                        {
                            date = DateTime.Now;
                        }
                        item.Period = (item.Date - date.Date).Days;
                        item.PercentCost = Math.Round(item.Period * (loanPercentPerDay / 100) * balanceCost, 2);
                        if (!migratedFromOnline)
                        {
                            item.PercentCost = item.PercentCost * 360 / 365;
                        }
                    }
                }

                schedule.Add(item);

                balanceCost -= item.DebtCost;
                counter++;
                debtCounter++;
                prevDate = item.Date;
            }
            return schedule;
        }

        /// <summary>
        /// гибридный (смешанный) график - дискрет + аннуитет
        /// </summary>
        /// <param name="beginDate"></param>
        /// <param name="loanCost"></param>
        /// <param name="loanPercentPerDay"></param>
        /// <param name="maturityDate"></param>
        /// <param name="firstPaymentDate"></param>
        /// <param name="discretePaymentCounter"></param>
        /// <param name="annuityPaymentCounter"></param>
        /// <returns></returns>
        public List<ContractPaymentSchedule> BuildMixed(DateTime beginDate, decimal loanCost, decimal loanPercentPerDay, DateTime maturityDate, int discretePaymentCounter, int annuityPaymentCounter, DateTime? firstPaymentDate = null, int? periodType = null, DateTime? changeDate = null, Contract contract = null, bool isChDP = false)
        {
            var startingDate = beginDate;
            var schedule = new List<ContractPaymentSchedule>();
            ContractAdditionalInfo contractInfo = contract != null ? _сontractAdditionalInfoRepository.Get(contract.Id) : null;
            var isControlDate = contract != null && (changeDate.HasValue || (contractInfo != null && contractInfo.ChangedControlDate.HasValue));
            if (isControlDate)
            {
                beginDate = changeDate.HasValue ? changeDate.Value : contractInfo.ChangedControlDate.Value;
                beginDate = beginDate.AddMonths(-1);
            }
            int discreteCounter = 0;
            int annuityCounter = 0;
            decimal monthPercent = loanPercentPerDay * 30;
            decimal balanceCost = loanCost;
            var existPayments = new List<ContractPaymentSchedule>();
            var isMigrated = false;
            if (isControlDate)
            {
                isMigrated = contract.ProductType?.Code == "TSO_MIGRATION";
                var payments = contract.PaymentSchedule.Where(x => x.Date <= DateTime.Now).ToList();
                foreach (var item in payments)
                {
                    if (item.ActionId.HasValue)
                    {
                        var act = _contractActionService.GetAsync(item.ActionId.Value).Result;
                        if (act != null)
                        {
                            if (act.ActionType != ContractActionType.PartialPayment)
                            { item.ActionType = (int)act.ActionType; }
                            else
                            {
                                if (act.ActionType == ContractActionType.PartialPayment &&
                                    Math.Round(act.Cost) == Math.Round(item.DebtCost))
                                { item.ActionType = (int)act.ActionType; }
                            }
                        }
                    }
                }

                existPayments = payments.Where(x => x.ActionId != null && x.ActionType != 40).ToList();
                if (payments.Any())
                {
                    schedule = payments;
                    if (!isChDP)
                    {
                        if (existPayments.Count() > discretePaymentCounter)
                        {
                            discretePaymentCounter = 0;
                        }
                        else
                        {
                            discretePaymentCounter -= existPayments.Count();
                        }
                        if (payments.Any())
                        {
                            var prev = payments.OrderByDescending(x => x.Date).First();
                            balanceCost = prev.DebtLeft;
                        }
                    }
                }

                if (firstPaymentDate.HasValue)
                {
                    firstPaymentDate = null;
                }
            }
            //дискретная часть
            while (discreteCounter < discretePaymentCounter)
            {
                var item = new ContractPaymentSchedule();
                if (firstPaymentDate.HasValue)
                {
                    if (discreteCounter == 0)
                    {
                        item.Date = firstPaymentDate.Value;
                        item.Period = (item.Date - beginDate.Date).Days;
                    }
                    else
                    {
                        item.Date = firstPaymentDate.Value.Date.AddMonths(discreteCounter);
                        item.Period = 30;
                    }
                }
                else
                {
                    item.Date = beginDate.Date.AddMonths(discreteCounter + 1);
                    item.Period = 30;
                }
                if (discreteCounter == 0 && firstPaymentDate.HasValue && beginDate.Date != firstPaymentDate)
                {
                    item.PercentCost = Math.Round(item.Period * (loanPercentPerDay * 30 * 12 / 365 / 100) * balanceCost, 2);
                }
                else
                {
                    item.PercentCost = Math.Round(item.Period * (loanPercentPerDay / 100) * balanceCost, 2);
                }
                item.DebtCost = 0;
                item.DebtLeft = balanceCost;

                item.DebtLeft = Math.Round(item.DebtLeft, 2);

                if (isControlDate)
                {
                    if (discreteCounter == 0)
                    {

                        var startDate = contract.ContractDate;
                        var date = schedule.Any() ? schedule.OrderByDescending(x => x.Date).First().Date : startDate.Date;
                        //сделал schedulle, т к в случае если было чдп, но не было платежей, не верно высчитывало
                        if (isChDP)
                        {
                            date = DateTime.Now;
                        }

                        item.Period = (item.Date - date.Date).Days;
                        item.PercentCost = Math.Round(item.Period * (loanPercentPerDay / 100) * balanceCost, 2);
                        if (!isMigrated)
                        {
                            item.PercentCost = item.PercentCost * 360 / 365;
                        }
                    }

                }
                schedule.Add(item);
                discreteCounter++;
            }

            //аннуитетная часть
            decimal monthlyPayment = (loanCost / Convert.ToDecimal((1 - Math.Pow((double)(1 + (monthPercent / 100)), -annuityPaymentCounter)) / ((double)monthPercent / 100)));
            if (isControlDate)
            {
                var prev = schedule.OrderByDescending(x => x.Date).FirstOrDefault();
                if (prev != null)
                {
                    balanceCost = prev.DebtLeft;
                    monthlyPayment = (balanceCost / Convert.ToDecimal((1 - Math.Pow((double)(1 + (monthPercent / 100)), -annuityPaymentCounter)) / ((double)monthPercent / 100)));
                }
            }

            int paymentsCount = maturityDate.MonthDifference(startingDate) - discretePaymentCounter - existPayments.Count;
            while (annuityCounter < paymentsCount)
            {
                var item = new ContractPaymentSchedule();
                if (firstPaymentDate.HasValue)
                {
                    item.Date = schedule.OrderByDescending(x => x.Date).First().Date.AddMonths(1);
                    item.Period = 30;
                }
                else
                {
                    item.Date = beginDate.Date.AddMonths(discreteCounter + annuityCounter + 1);
                    item.Period = 30;
                }

                var percentCost = Math.Round(item.Period * (loanPercentPerDay / 100) * balanceCost, 2);
                var balance = balanceCost;
                item.PercentCost = percentCost;
                item.DebtCost = monthlyPayment - percentCost;
                item.DebtLeft = balanceCost - item.DebtCost;

                if (annuityCounter == paymentsCount - 1)
                {
                    item.DebtCost = balanceCost;
                    item.DebtLeft = 0;
                }
                balanceCost -= item.DebtCost;
                item.DebtCost = Math.Round(item.DebtCost, 2);
                item.DebtLeft = Math.Round(item.DebtLeft, 2);

                if (isControlDate)
                {
                    var prev = schedule.OrderByDescending(x => x.Date).First();
                    if (annuityCounter == paymentsCount - 1)
                    {
                        item.Date = contract.MaturityDate;
                        item.Period = (item.Date - prev.Date).Days;
                        item.PercentCost = Math.Round(item.Period * (loanPercentPerDay / 100) * balance, 2);
                        if (!isMigrated)
                        {
                            item.PercentCost = item.PercentCost * 360 / 365;
                        }
                        item.DebtLeft = 0;
                        item.DebtCost = balance;
                    }
                    else if (annuityCounter == 0 && changeDate > prev.Date)
                    {

                        var startDate = contract?.ContractDate;
                        var date = schedule.Any() ? schedule.OrderByDescending(x => x.Date).First().Date : startDate.Value.Date;
                        //сделал schedulle, т к в случае если было чдп, но не было платежей, не верно высчитывало
                        if (isChDP)
                        {
                            date = DateTime.Now;
                        }
                        item.Period = (item.Date - date.Date).Days;
                        item.PercentCost = Math.Round(item.Period * (loanPercentPerDay / 100) * balance, 2);
                        if (!isMigrated)
                        {
                            item.PercentCost = item.PercentCost * 360 / 365;
                        }
                    }
                }

                schedule.Add(item);
                annuityCounter++;
            }

            return schedule;
        }

        public List<ContractPaymentSchedule> BuildDiscrete(decimal loanCost, decimal loanPercentPerDay, DateTime maturityDate)
        {
            var result = new List<ContractPaymentSchedule>();
            result.Add(
                new ContractPaymentSchedule()
                {
                    DebtCost = loanCost,
                    DebtLeft = 0,
                    Date = maturityDate,
                    Period = 30,
                    Revision = 1,
                    PercentCost = (loanCost * ((30 * loanPercentPerDay) / 100))
                });

            return result;
        }

        /// <inheritdoc />
        public List<ContractPaymentSchedule> BuildDifferent(DateTime beginDate, decimal loanCost, decimal loanPercentPerDay, DateTime maturityDate, DateTime? firstPaymentDate = null, int? paymentsCount = null, DateTime? changeDate = null, Contract contract = null, bool isChDP = false)
        {
            var start = beginDate;
            paymentsCount ??= maturityDate.MonthDifference(beginDate);
            if (paymentsCount <= 0)
                throw new PawnshopApplicationException($@"График не может быть создан с 0 и менее месяцами");
            ContractAdditionalInfo contractInfo = contract != null ? _сontractAdditionalInfoRepository.Get(contract.Id) : null;
            var isControlDate = contract != null && (changeDate.HasValue || (contractInfo != null && contractInfo.ChangedControlDate.HasValue));
            if (isControlDate)
            {
                if (firstPaymentDate != null)
                {
                    firstPaymentDate = firstPaymentDate > beginDate ? changeDate ?? contractInfo.ChangedControlDate : beginDate;
                }

                beginDate = changeDate.HasValue ? changeDate.Value : contractInfo.ChangedControlDate.Value;
                beginDate = beginDate.AddMonths(-1);
                maturityDate = contract.MaturityDate;
            }

            var schedule = new List<ContractPaymentSchedule>();
            decimal monthlyPayment = Math.Round(loanCost / paymentsCount.Value, 2);
            decimal balanceCost = loanCost;
            int counter = 0;
            var existPayments = new List<ContractPaymentSchedule>();
            var isMigrated = false;
            if (isControlDate)
            {
                isMigrated = contract.ProductType?.Code == "TSO_MIGRATION";
                var payments = contract.PaymentSchedule.Where(x => x.Date <= DateTime.Now).ToList();
                foreach (var item in payments)
                {
                    if (item.ActionId.HasValue)
                    {
                        var act = _contractActionService.GetAsync(item.ActionId.Value).Result;
                        if (act != null)
                        {
                            if (act.ActionType != ContractActionType.PartialPayment)
                            {
                                item.ActionType = (int)act.ActionType;
                            }
                            else if (act.ActionType == ContractActionType.PartialPayment && Math.Round(act.Cost) == Math.Round(item.DebtCost))
                            {
                                item.ActionType = (int)act.ActionType;
                            }
                        }
                    }
                }
                existPayments = payments.Where(x => x.ActionId != null && x.ActionType != 40).ToList();
                schedule = payments;
                var fisrtPaymentDate = contract.FirstPaymentDate ?? contract.PaymentSchedule.ToList().OrderBy(x => x.Date).First().Date;
                var fromStart = fisrtPaymentDate.AddMonths(-1);
                if (!isChDP)
                {
                    paymentsCount = maturityDate.MonthDifference(fromStart);
                    paymentsCount -= existPayments.Count;// в чдп режется кол-во платежей
                    monthlyPayment = Math.Round(loanCost / paymentsCount.Value, 2);
                }

                if (contract.ContractClass == ContractClass.Tranche && changeDate == null)
                {
                    paymentsCount = contract.ContractDate.MonthDifferenceWithChangedDate(maturityDate);
                    var creditLineContract = _contractService.GetOnlyContract(contract.CreditLineId.Value);
                    var tranches = _contractService.GetAllSignedTranches(creditLineContract.Id).Result;
                    if (tranches.Any())
                    {
                        var firstTranche = tranches.OrderBy(x => x.ContractDate).First();
                        var firstTrancheInfo = _сontractAdditionalInfoRepository.Get(firstTranche.Id);
                        if (firstTrancheInfo != null && firstTrancheInfo.ChangedControlDate != null)
                        {
                            var nextPaymentSchedule = _contractPaymentScheduleService.GetNextPaymentSchedule(firstTranche.Id, true).Result;
                            if ((nextPaymentSchedule.Date - DateTime.Now.Date).TotalDays < 15)
                            {
                                beginDate = nextPaymentSchedule.Date.AddMonths(-1);
                                paymentsCount = beginDate.MonthDifferenceWithChangedDate(maturityDate);
                            }
                            else
                            {
                                paymentsCount = beginDate.MonthDifferenceWithChangedDate(maturityDate);
                            }
                            monthlyPayment = Math.Round(balanceCost / paymentsCount.Value, 2);
                        }
                    }
                }
                if (!isChDP)
                {
                    if (payments.Any())
                    {
                        var prev = payments.OrderByDescending(x => x.Date).First();
                        balanceCost = prev.DebtLeft;
                        monthlyPayment = Math.Round(balanceCost / paymentsCount.Value, 2);
                    }
                }
            }

            while (paymentsCount != counter)
            {
                var item = new ContractPaymentSchedule { Revision = 1 };

                SetScheduleDateAndPeriod(item, beginDate, firstPaymentDate, counter);

                if (isMigrated)
                {
                    item.PercentCost = Math.Round(balanceCost * 30 * (loanPercentPerDay * 365 / 360 / 100), 2);
                }
                else if ((counter == 0 && firstPaymentDate.HasValue && beginDate.Date.AddMonths(1) != firstPaymentDate)
                    || (counter == paymentsCount - 1 && beginDate.AddMonths(paymentsCount ?? 0).Date != maturityDate.Date))
                {
                    item.PercentCost = Math.Round(balanceCost * 30 * (loanPercentPerDay * 365 / 360 / 100), 2);
                    //item.PercentCost = Math.Round(balanceCost * item.Period * (loanPercentPerDay / 100), 2);
                }
                else
                {
                    item.PercentCost = Math.Round(balanceCost * 30 * (loanPercentPerDay / 100), 2);
                }

                item.DebtCost = monthlyPayment;
                item.DebtLeft = balanceCost - item.DebtCost;

                if (counter == paymentsCount - 1)
                {
                    item.DebtCost += item.DebtLeft;
                    item.DebtLeft = 0;
                }

                if (isControlDate)
                {
                    var startDate = contract.ContractDate;
                    if (counter == paymentsCount - 1)
                    {
                        var prev = schedule.OrderByDescending(x => x.Date).First();
                        item.Date = contract.MaturityDate;
                        item.Period = (item.Date - prev.Date).Days;
                        item.PercentCost = Math.Round(item.Period * (loanPercentPerDay / 100) * balanceCost, 2);
                        if (!isMigrated)
                        {
                            item.PercentCost = item.PercentCost * 360 / 365;
                        }
                    }
                    else if (counter == 0)
                    {
                        var date = existPayments.Any() ? schedule.OrderByDescending(x => x.Date).First().Date : startDate.Date;
                        if (isChDP)
                        {
                            date = DateTime.Now.Date;
                        }
                        item.Period = (item.Date - date).Days;
                        item.PercentCost = Math.Round(item.Period * (loanPercentPerDay / 100) * balanceCost, 2);
                        if (!isMigrated)
                        {
                            item.PercentCost = item.PercentCost * 360 / 365;
                        }
                    }
                }

                schedule.Add(item);
                balanceCost -= item.DebtCost;
                counter++;
            }

            return schedule;
        }

        public async Task BuildScheduleForNewContract(Contract contract, bool migratedFromOnline = false, bool isChDP = false, DateTime? changeDate = null, DateTime? actionDate = null)
        {
            var beginDate = contract.SignDate?.Date ?? contract.ContractDate.Date;
            var loanCost = contract.LoanCost;
            var loanPercentPerDay = contract.LoanPercent;/*contract.SettingId.HasValue ? contract.Setting.LoanPercent :*/


            if (contract.Setting != null && contract.Setting.IsFloatingDiscrete)
            {
                await BuildScheduleForNewFloatingContract(contract, isChDP, actionDate);
            }
            else
            {
                DateTime maturityDate = default;
                //если есть SettingId значит договор продуктовый
                if (contract.SettingId.HasValue)
                {
                    int paymentCount = contract.Setting.PaymentCount(contract.LoanPeriod);
                    maturityDate = beginDate.AddDays(paymentCount);
                }
                //не продуктовый договор
                else
                {
                    int loanPeriod = contract.LoanPeriod;

                    //для коротких дискретов это надо раскоментировать
                    //if (contract.PercentPaymentType == PercentPaymentType.EndPeriod
                    //    && !contract.Locked)
                    //    loanPeriod--;

                    maturityDate = beginDate.AddDays(loanPeriod);
                }

                if (contract.SettingId.HasValue && !contract.Setting.PaymentPeriod.HasValue)
                    throw new PawnshopApplicationException("Не задана настройка частоты погашения процентов в продукте, ошибка создания графика!");

                DateTime? firstPaymentDate = contract.FirstPaymentDate;

                var paymentsCount = contract.SettingId.HasValue ? contract.Setting.PaymentCount(contract.LoanPeriod) : (int)Math.Floor((decimal)contract.LoanPeriod / 30);

                Contract creditLineContract = null;

                if (contract.ContractClass == ContractClass.Tranche)
                {
                    creditLineContract = _contractService.GetOnlyContract(contract.CreditLineId.Value);
                    //beginDate = creditLineContract.SignDate?.Date ?? creditLineContract.ContractDate.Date;
                    //maturityDate = creditLineContract.MaturityDate;
                    maturityDate = contract.MaturityDate;

                    if (!isChDP)
                    {
                        paymentsCount = contract.MaturityDate.MonthDifference(beginDate);
                    }

                    var tranches = await _contractService.GetAllSignedTranches(creditLineContract.Id);

                    if (tranches.Any())
                    {
                        var firstTranche = tranches.OrderBy(x => x.ContractDate).First();
                        ContractAdditionalInfo contractInfo = _сontractAdditionalInfoRepository.Get(firstTranche.Id);

                        if (contractInfo != null && contractInfo.ChangedControlDate.HasValue && changeDate == null)
                        {
                            changeDate = contractInfo.ChangedControlDate.Value;
                        }
                    }

                }

                if (contract.PercentPaymentType == PercentPaymentType.AnnuityTwelve || contract.PercentPaymentType == PercentPaymentType.AnnuityTwentyFour || contract.PercentPaymentType == PercentPaymentType.AnnuityThirtySix)
                {
                    if (contract.PartialPaymentParentId.HasValue)
                    {
                        maturityDate = contract.MaturityDate;
                        paymentsCount = maturityDate.MonthDifference(beginDate);
                    }

                    contract.PaymentSchedule = BuildAnnuity(beginDate, loanCost, loanPercentPerDay, maturityDate,
                        firstPaymentDate, paymentsCount, contract: contract, changeDate: changeDate, isChDP: isChDP);
                }
                else if (contract.PercentPaymentType == PercentPaymentType.Product &&
                         (contract.Setting?.ScheduleType == ScheduleType.Annuity || contract.Setting?.ScheduleType == ScheduleType.Discrete))
                {
                    DateTime? controlDate = contract.Setting.PaymentPeriodType switch
                    {
                        PeriodType.Year => beginDate.AddYears(contract.Setting.PaymentPeriod ?? -1),
                        PeriodType.HalfYear => beginDate.AddMonths((contract.Setting.PaymentPeriod ?? -1) * 6),
                        PeriodType.Month => beginDate.AddMonths(contract.Setting.PaymentPeriod ?? -1),
                        PeriodType.Week => beginDate.AddDays(contract.Setting.PaymentPeriod ?? -1 * 7),
                        PeriodType.Day => beginDate.AddDays(contract.Setting.PaymentPeriod ?? -1),
                        _ => throw new ArgumentOutOfRangeException("Не найден вид периода для создания графика"),
                    };

                    var setting = contract.Setting;
                    var debtPeriod = setting.DebtPeriod * (int)setting.DebtPeriodType;
                    var paymentPeriod = setting.PaymentPeriod * (int)setting.PaymentPeriodType;
                    bool isCreditLine = false;

                    if (contract.ContractClass == ContractClass.Tranche)
                    {
                        isCreditLine = true;
                        beginDate = contract.SignDate?.Date ?? contract.ContractDate.Date;
                        var creditLineBeginDate = creditLineContract.SignDate?.Date ?? creditLineContract.ContractDate.Date;
                        if (beginDate != creditLineBeginDate)
                            firstPaymentDate = controlDate;


                        var tranches = await _contractService.GetAllSignedTranches(creditLineContract.Id);
                        var tranchesCount = tranches.Count;
                        if (contract.FirstPaymentDate.HasValue && tranchesCount == 0)
                            firstPaymentDate = contract.FirstPaymentDate;
                        if (tranchesCount > 0)
                        {
                            var firstTranche = tranches.OrderBy(x => x.ContractDate).First();
                            var nextPaymentSchedule = await _contractPaymentScheduleService.GetNextPaymentSchedule(firstTranche.Id, true);
                            firstPaymentDate = nextPaymentSchedule.Date;
                            ContractAdditionalInfo contractInfo = _сontractAdditionalInfoRepository.Get(firstTranche.Id);
                            if (contractInfo != null && contractInfo.ChangedControlDate.HasValue && changeDate == null)
                            {
                                changeDate = contractInfo.ChangedControlDate.Value;
                            }
                        }
                    }
                    if (firstPaymentDate.HasValue && firstPaymentDate.Value.Date == controlDate.Value.Date)
                    {
                        firstPaymentDate = null;
                    }
                    if (isChDP)
                    {
                        firstPaymentDate = contract.FirstPaymentDate;
                        beginDate = contract.SignDate.Value;
                        paymentsCount = contract.PaymentSchedule.Where(x => x.ActionId == null).ToList().Count;
                    }
                    if (debtPeriod != paymentPeriod)
                    {
                        if (debtPeriod % paymentPeriod != 0)
                            throw new PawnshopApplicationException("Ошибка генерации графика, периодичность погашения ОД должна быть кратной периодичности погашения процентов");
                        var paymentDebt = debtPeriod / paymentPeriod;

                        contract.PaymentSchedule = BuildAnnuity(beginDate, loanCost, loanPercentPerDay, maturityDate, firstPaymentDate, paymentsCount, paymentDebt, isCreditLine, migratedFromOnline, contract: contract, changeDate: changeDate, isChDP: isChDP);
                    }
                    else
                    {
                        contract.PaymentSchedule = BuildAnnuity(beginDate, loanCost, loanPercentPerDay, maturityDate, firstPaymentDate, paymentsCount, isCreditLine: isCreditLine, migratedFromOnline: migratedFromOnline, contract: contract, changeDate: changeDate, isChDP: isChDP);
                    }
                }
                else if (contract.PercentPaymentType == PercentPaymentType.Product
                    && contract.Setting?.ScheduleType == ScheduleType.Mixed)
                {
                    var paymentPeriodType = (int?)contract.Setting?.PaymentPeriodType ?? 30;
                    var discretePayments = contract.DebtGracePeriod / paymentPeriodType ?? 0;
                    var annuityPayments = contract.LoanPeriod / paymentPeriodType - contract.DebtGracePeriod / paymentPeriodType ?? 0;
                    int paymentCount = contract.Setting.PaymentCount(contract.LoanPeriod);
                    maturityDate = beginDate.AddDays(paymentCount * (double?)contract.Setting.PaymentPeriodType ?? 0);

                    if (contract.DebtGracePeriod is null)
                        throw new PawnshopApplicationException("Не указан льготный период для смешанного графика");

                    if (contract.LoanPeriod <= contract.DebtGracePeriod.Value)
                        throw new PawnshopApplicationException("Льготный период не должен быть больше или равен периоду договора");

                    if (discretePayments == 0 || annuityPayments == 0)
                        throw new PawnshopApplicationException("При создании смешанного графика не удалось определить сроки дискрета и/или аннуитета");

                    DateTime? controlDate = contract.Setting.PaymentPeriodType switch
                    {
                        PeriodType.Year => beginDate.AddYears(contract.Setting.PaymentPeriod ?? -1),
                        PeriodType.HalfYear => beginDate.AddMonths(
                            (contract.Setting.PaymentPeriod ?? -1) * 6),
                        PeriodType.Month => beginDate.AddMonths(
                            contract.Setting.PaymentPeriod ?? -1),
                        PeriodType.Week => beginDate.AddDays(
                            contract.Setting.PaymentPeriod ?? -1 * 7),
                        PeriodType.Day => beginDate.AddDays(contract.Setting.PaymentPeriod ?? -1),
                        _ => throw new ArgumentOutOfRangeException(
                            "Не найден вид периода для создания графика"),
                    };

                    if (firstPaymentDate.HasValue && controlDate.Value.Date == firstPaymentDate.Value.Date)
                        firstPaymentDate = null;

                    contract.PaymentSchedule = BuildMixed(beginDate, loanCost, loanPercentPerDay, maturityDate, discretePayments, annuityPayments, firstPaymentDate: firstPaymentDate, contract: contract, changeDate: changeDate);
                }
                else if (contract.PercentPaymentType == PercentPaymentType.EndPeriod)
                {
                    contract.PaymentSchedule = BuildDiscrete(loanCost, loanPercentPerDay, maturityDate);
                }
                else if (contract.Setting?.ScheduleType == ScheduleType.Differentiated)
                {
                    contract.PaymentSchedule = BuildDifferent(beginDate, loanCost, loanPercentPerDay, maturityDate, firstPaymentDate, paymentsCount, contract: contract, changeDate: changeDate, isChDP: isChDP);
                }
                else
                    throw new PawnshopApplicationException("По выбранному виду кредитования не настроена генерация графика платежей. Обратитесь к администратору.");

                contract.MaturityDate = contract.PaymentSchedule.Max(x => x.Date);
            }
        }

        public List<ContractPaymentSchedule> CalculateScheduleWithoutContract(LoanPercentSetting setting, decimal loanCost, DateTime beginDate, DateTime maturityDate, DateTime? firstPaymentDate = null)
        {
            if (setting.ScheduleType == ScheduleType.Annuity)
                return BuildAnnuity(beginDate, loanCost, setting.LoanPercent, maturityDate, firstPaymentDate);

            if (setting.ScheduleType == ScheduleType.Discrete)
            {
                var debtPeriod = setting.DebtPeriod * (int)setting.DebtPeriodType;
                var paymentPeriod = setting.PaymentPeriod * (int)setting.PaymentPeriodType;

                if (debtPeriod % paymentPeriod != 0)
                    throw new PawnshopApplicationException("Ошибка генерации графика, периодичность погашения ОД должна быть кратной периодичности погашения процентов");

                var paymentDebt = debtPeriod / paymentPeriod;

                return BuildAnnuity(beginDate, loanCost, setting.LoanPercent, maturityDate, firstPaymentDate, null, paymentDebt);
            }

            if (setting.ScheduleType == ScheduleType.Differentiated)
                return BuildDifferent(beginDate, loanCost, setting.LoanPercent, maturityDate, firstPaymentDate);

            throw new NotImplementedException();
        }

        public List<ContractPaymentSchedule> BuildFloatingDiscrete(Contract contract, DateTime maturityDate, List<decimal> rates, bool isChDP = false, DateTime? actionDate = null)
        {
            var currentDate = actionDate != null && isChDP ? actionDate.Value.Date : DateTime.Now.Date;
            var result = new List<ContractPaymentSchedule>();
            var loanCost = contract.LoanCost;
            if (currentDate > maturityDate.Date)
            {
                var nextPayments = contract.PaymentSchedule.Where(x => x.Date > currentDate && x.DeleteDate == null).ToList();
                var rateIndex = rates.Count - 1;
                var dictionary = new Dictionary<int, decimal>();
                for (var index = nextPayments.Count - 1; index >= 0; index--)
                {
                    var payment = nextPayments[index];
                    dictionary.Add(payment.Id, rates.ElementAt(rateIndex));
                    rateIndex--;
                }
                var firstPaymentId = nextPayments.First().Id;
                var last = nextPayments.Last().Id;
                foreach (var payment in nextPayments)
                {
                    var rate = payment.Id == firstPaymentId ? (dictionary[payment.Id] * 360 / 365) : dictionary[payment.Id];
                    var days = payment.Id == firstPaymentId ? (payment.Date - currentDate.Date).Days : 30;
                    var schedule = new ContractPaymentSchedule()
                    {
                        DebtCost = 0,
                        DebtLeft = loanCost,
                        Date = payment.Date,
                        Period = 30,
                        Revision = 1,
                        PercentCost = (loanCost * (days * rate) / 100)
                    };
                    if (payment.Id.Equals(last))
                    {
                        schedule.DebtCost += loanCost;
                        schedule.DebtLeft = 0;
                    }
                    result.Add(schedule);
                }
            }
            else
            {
                int counter = 0;
                var last = rates.Last();
                foreach (var rate in rates)
                {
                    var schedule = new ContractPaymentSchedule()
                    {
                        DebtCost = 0,
                        DebtLeft = loanCost,
                        Date = maturityDate.AddMonths(++counter),
                        Period = 30,
                        Revision = 1,
                        PercentCost = (loanCost * ((30 * rate) / 100))
                    };
                    if (rate.Equals(last))
                    {
                        schedule.DebtCost += loanCost;
                        schedule.DebtLeft = 0;
                    }
                    result.Add(schedule);
                }
            }
            return result;
        }

        public async Task BuildScheduleForNewFloatingContract(Contract contract, bool isChDP = false, DateTime? actionDate = null)
        {
            DateTime maturityDate = contract.ContractDate.Date;
            if (contract.SettingId.HasValue && !contract.Setting.PaymentPeriod.HasValue)
                throw new PawnshopApplicationException("Не задана настройка частоты погашения процентов в продукте, ошибка создания графика!");

            var contractRates = (await Task.Run(() =>
                _contractRateService.FindRateOnDateByFloatingContractAndRateSettingId(contract.Id))).ToList();

            var rates = contractRates.Any() ? contractRates.Select(x => x.Rate).ToList()
                : contract.Setting.LoanSettingRates
                    .Where(x => x.RateSetting.Code == Constants.ACCOUNT_SETTING_PROFIT)
                    .OrderBy(x => x.Index)
                    .Select(x => x.Rate).ToList();

            contract.PaymentSchedule = BuildFloatingDiscrete(contract, maturityDate, rates.ToList(), isChDP, actionDate);
            contract.MaturityDate = contract.PaymentSchedule.Max(x => x.Date);
        }

        /// <summary>
        /// Установка ожидаемой даты платежа и периода
        /// </summary>
        /// <param name="scheduleRow">Строка графика платежа</param>
        /// <param name="beginDate">Дата выдачи займа</param>
        /// <param name="firstPaymentDate">Дата первого платежа</param>
        /// <param name="counter">Счетчик</param>
        private void SetScheduleDateAndPeriod(ContractPaymentSchedule scheduleRow, DateTime beginDate, DateTime? firstPaymentDate, int counter)
        {
            if (!firstPaymentDate.HasValue)
            {
                scheduleRow.Date = beginDate.Date.AddMonths(counter + 1);
                scheduleRow.Period = 30;
                return;
            }

            if (counter == 0 && firstPaymentDate.HasValue && beginDate.AddMonths(1).Date != firstPaymentDate.Value.Date)
            {
                scheduleRow.Date = firstPaymentDate.Value;
                scheduleRow.Period = scheduleRow.Date.DayDifference(beginDate.Date);
                return;
            }

            scheduleRow.Date = firstPaymentDate.Value.Date.AddMonths(counter);
            scheduleRow.Period = 30;
        }

        public async Task<Contract> SaveBuilderByControlDate(ContractPaymentScheduleUpdateModel contr)
        {
            var contractId = contr.ContractId;
            Contract contract = _contractRepository.Get(contractId);
            var fPaymentDateString = contract.FirstPaymentDate?.ToString("dd.MM.yyy");
            if (contract == null)
                throw new PawnshopApplicationException($"Договор c id = {contractId} не найден");

            foreach (var item in contract.PaymentSchedule)
            {
                if (item.ActionId.HasValue)
                {
                    var act = await _contractActionService.GetAsync(item.ActionId.Value);
                    if (act != null)
                    {
                        if (act.ActionType != ContractActionType.PartialPayment)
                        {
                            item.ActionType = (int)act.ActionType;
                        }
                        else if (act.ActionType == ContractActionType.PartialPayment && Math.Round(act.Cost) == Math.Round(item.DebtCost))
                        {
                            item.ActionType = (int)act.ActionType;
                        }
                    }
                }
            }

            if (contract.PaymentSchedule.Any(x => x.ActionId == null && x.ActionType == null && x.Date == DateTime.Now.Date))
            {
                throw new PawnshopApplicationException($"У договора c id = {contract.Id} есть неоплаченный платеж");
            }
            await BuildScheduleForNewContract(contract, changeDate: contr.ChangedControlDate);

            var prevContract = _contractRepository.Get(contract.Id);

            var action = new ContractAction()
            {
                ActionType = ContractActionType.ControlDateChange,
                Date = DateTime.Now,
                Reason =
                    $"Изменение даты погашения {contract.ContractNumber} от {contract.ContractDate.ToString("dd.MM.yyyy")} с {fPaymentDateString} на {contr.ChangedControlDate?.ToString("dd.MM.yyyy")}",
                TotalCost = 0,
                Cost = 0,
                ContractId = contract.Id,
                AuthorId = 1,
                CreateDate = DateTime.Now
            };
            _contractActionService.Save(action);
            var historyId = await _contractPaymentScheduleService.InsertContractPaymentScheduleHistory(contract.Id, action.Id, (int)ContractActionStatus.Canceled);
            foreach (var item in prevContract.PaymentSchedule)
            {
                if (item.ActionId.HasValue)
                {
                    var act = await _contractActionService.GetAsync(item.ActionId.Value);
                    if (act != null)
                    {
                        if (act.ActionType != ContractActionType.PartialPayment)
                        { item.ActionType = (int)act.ActionType; }
                        else
                        {
                            if (act.ActionType == ContractActionType.PartialPayment &&
                                Math.Round(act.Cost) == Math.Round(item.DebtCost))
                            { item.ActionType = (int)act.ActionType; }
                        }
                    }
                }
                await _contractPaymentScheduleService.InsertContractPaymentScheduleHistoryItems(historyId, item);
            }

            //int authorId = _sessionContext.UserId;

            var payments = contract.PaymentSchedule.Where(x => x.ActionType != 40);
            var date = payments.First().Date;
            if (prevContract.FirstPaymentDate != date)
            {
                prevContract.FirstPaymentDate = date;
            }

            prevContract.PaymentSchedule = contract.PaymentSchedule;
            prevContract.MaturityDate = prevContract.PaymentSchedule.Max(x => x.Date);
            _contractService.CheckSchedule(prevContract, true);

            prevContract.NextPaymentDate = prevContract.PercentPaymentType == PercentPaymentType.EndPeriod
                ? prevContract.MaturityDate
                : prevContract.PaymentSchedule.Where(x => x.ActionId == null && x.Canceled == null).Min(x => x.Date);

            if (prevContract.PaymentSchedule.Where(x => prevContract.PercentPaymentType != PercentPaymentType.EndPeriod && x.Date == DateTime.Now.Date && x.ActionId == null && x.Canceled == null).Any())
            {
                throw new PawnshopApplicationException("Не возможно сделать ЧДП, т.к. по договору имеется не оплаченный платеж с сегодняшней датой оплаты. Вначале сделайте погашение имеющейся задолженности по договору через функционал оплаты(кнопка 'Оплата').");
            }

            return prevContract;
        }
    }
}
