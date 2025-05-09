using Pawnshop.AccountingCore.Models;
using Pawnshop.Core.Exceptions;
using Pawnshop.Core.Extensions;
using Pawnshop.Core;
using Pawnshop.Data.Access;
using Pawnshop.Data.Models.Contracts;
using Pawnshop.Data.Models.LoanSettings;
using Pawnshop.Services.PaymentSchedules.Builders;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System;
using Pawnshop.Services.ClientDeferments.Impl;
using Pawnshop.Services.ClientDeferments.Interfaces;

namespace Pawnshop.Services.PaymentSchedules
{
    public class PaymentScheduleService : IPaymentScheduleService
    {
        private readonly ContractRateRepository _contractRateRepository;
        private readonly ContractRepository _contractRepository;
        private readonly ContractPaymentScheduleRepository _paymentScheduleRepository;
        private readonly IClientDefermentService _clientDefermentService;

        public PaymentScheduleService(
            ContractRateRepository contractRateRepository,
            ContractRepository contractRepository,
            ContractPaymentScheduleRepository paymentScheduleRepository,
            IClientDefermentService clientDefermentService)
        {
            _contractRateRepository = contractRateRepository;
            _contractRepository = contractRepository;
            _paymentScheduleRepository = paymentScheduleRepository;
            _clientDefermentService = clientDefermentService;
        }

        public List<ContractPaymentSchedule> Build(ScheduleType scheduleType, decimal loanCost, decimal percentPerDay, DateTime beginDate, DateTime maturityDate,
            DateTime? firstPaymentDate = null, bool isPartialPayment = false, bool isMigrated = false, bool isChangeControlDate = false, int? upcomingPaymentsCount = null, bool isRestructuring = false)
        {
            var checkDatesResult = CheckDates(beginDate, maturityDate, firstPaymentDate, isPartialPayment, isChangeControlDate, isRestructuring);

            if (!checkDatesResult.IsCorrect)
                throw new PawnshopApplicationException(checkDatesResult.Message);

            if (checkDatesResult.FixMaturityDate.HasValue)
                maturityDate = checkDatesResult.FixMaturityDate.Value;

            BaseBuilder builder = scheduleType switch
            {
                ScheduleType.Annuity => new AnnuityBuilder(loanCost, percentPerDay, beginDate, maturityDate, firstPaymentDate, isMigrated, upcomingPaymentsCount),
                ScheduleType.Discrete => new DiscreteBuilder(loanCost, percentPerDay, beginDate, maturityDate, firstPaymentDate, isMigrated, upcomingPaymentsCount),
                ScheduleType.Differentiated => new DifferentBuilder(loanCost, percentPerDay, beginDate, maturityDate, firstPaymentDate, isMigrated, upcomingPaymentsCount),
                _ => null
            };

            if (builder == null)
                throw new PawnshopApplicationException($"Не удалось найти строителя графика платежей со способом погашения {scheduleType.GetDisplayName()}");

            return builder.Execute();
        }

        public void BuildForChangeControlDate(Contract contract, DateTime newControlDate)
        {
            #region fill details
            if (contract.PaymentSchedule == null || !contract.PaymentSchedule.Any())
                contract.PaymentSchedule = _paymentScheduleRepository.GetListByContractId(contract.Id);

            var lastPayment = contract.PaymentSchedule
                .OrderBy(x => x.Date)
                .LastOrDefault(x => x.ActionId.HasValue && x.ActualDate.HasValue);

            var beginDate = lastPayment?.Date ?? contract.SignDate ?? contract.ContractDate;
            var currentLoanCost = lastPayment?.DebtLeft ?? contract.LoanCost;

            var newPaymentSchedule = contract.PaymentSchedule
                .Where(x => x.ActionId.HasValue && x.ActualDate.HasValue)
                .ToList();
            #endregion

            #region special old annuity
            if (IsSpecialOldAnnuity(contract.PercentPaymentType))
            {
                newPaymentSchedule.AddRange(Build(ScheduleType.Annuity, currentLoanCost, contract.LoanPercent, beginDate, contract.MaturityDate, newControlDate, isChangeControlDate: true));
                contract.PaymentSchedule = newPaymentSchedule;
                return;
            }
            #endregion

            #region default
            if (IsDefaultScheduleType(contract.PercentPaymentType, contract.Setting?.ScheduleType))
            {
                if (contract.ContractClass == ContractClass.CreditLine)
                    return;

                var isMigrated = contract.ProductType?.Code == "TSO_MIGRATION";

                newPaymentSchedule.AddRange(Build(contract.Setting.ScheduleType.Value, currentLoanCost, contract.LoanPercent, beginDate, contract.MaturityDate, newControlDate, isMigrated: isMigrated, isChangeControlDate: true));
                contract.PaymentSchedule = newPaymentSchedule;
                return;
            }
            #endregion

            throw new PawnshopApplicationException("По выбранному виду кредитования не настроена генерация графика платежей. Обратитесь к администратору.");
        }

        public void BuildForPartialPayment(Contract contract, DateTime startDate, DateTime firstPaymentDate, decimal leftLoanCost,
            decimal amortizedBalanceOfDefferedPercent = 0, decimal amortizedBalanceOfOverduePercent = 0, decimal amortizedPenaltyOfOverdueDebt = 0, decimal amortizedPenaltyOfOverduePercent = 0)
        {
            #region special old annuity
            if (IsSpecialOldAnnuity(contract.PercentPaymentType))
            {
                var upcomingPaymentsCount = contract.PaymentSchedule?.Count(x => !x.ActionId.HasValue && !x.ActualDate.HasValue);

                if (contract.IsContractRestructured)
                {
                    contract.PaymentSchedule.Clear();
                    contract.PaymentSchedule.AddRange(BuildAfterDefermentSchedulePeriod(
                        upcomingPaymentsCount.Value,
                        Build(
                            ScheduleType.Annuity,
                            leftLoanCost,
                            contract.LoanPercent,
                            startDate,
                            contract.MaturityDate,
                            firstPaymentDate,
                            true,
                            upcomingPaymentsCount: upcomingPaymentsCount,
                            isRestructuring: true),
                        amortizedBalanceOfDefferedPercent,
                        amortizedBalanceOfOverduePercent,
                        amortizedPenaltyOfOverdueDebt,
                        amortizedPenaltyOfOverduePercent));
                }
                else
                {
                    contract.PaymentSchedule = Build(
                        ScheduleType.Annuity,
                        leftLoanCost,
                        contract.LoanPercent,
                        startDate,
                        contract.MaturityDate,
                        firstPaymentDate,
                        true,
                        upcomingPaymentsCount: upcomingPaymentsCount);
                }

                return;
            }
            #endregion

            #region floating discrete
            if (contract.Setting != null && contract.Setting.IsFloatingDiscrete)
            {
                if (contract.SettingId.HasValue && !contract.Setting.PaymentPeriod.HasValue)
                    throw new PawnshopApplicationException("Не задана настройка частоты погашения процентов в продукте, ошибка создания графика!");

                if (contract.PaymentSchedule == null || !contract.PaymentSchedule.Any())
                    contract.PaymentSchedule = _paymentScheduleRepository.GetListByContractId(contract.Id);

                var paidPaymentsCount = contract.PaymentSchedule.Count(x => x.ActionId.HasValue && x.ActualDate.HasValue);

                var rates = GetFloatingRates(contract).Skip(paidPaymentsCount);

                var builder = new FloatingDiscreteBuilder(leftLoanCost, rates, startDate, contract.MaturityDate, firstPaymentDate);

                contract.PaymentSchedule = builder.Execute();
                return;
            }
            #endregion

            #region short discrete
            if (contract.PercentPaymentType == PercentPaymentType.EndPeriod)
            {
                var builder = new ShortDiscreteBuilder(leftLoanCost, contract.LoanPercent, firstPaymentDate);

                contract.PaymentSchedule = builder.Execute();
                contract.MaturityDate = contract.PaymentSchedule.Max(x => x.Date);
                return;
            }
            #endregion

            #region mixed
            if (contract.PercentPaymentType == PercentPaymentType.Product && contract.Setting?.ScheduleType == ScheduleType.Mixed)
                return; // TODO: create builder for mixed
            #endregion

            #region default
            if (IsDefaultScheduleType(contract.PercentPaymentType, contract.Setting?.ScheduleType))
            {
                if (contract.ContractClass == ContractClass.CreditLine)
                    return;

                var isMigrated = contract.ProductType?.Code == "TSO_MIGRATION";
                var upcomingPaymentsCount = contract.PaymentSchedule?.Count(x => !x.ActionId.HasValue && !x.ActualDate.HasValue);

                if (contract.IsContractRestructured)
                {
                    contract.PaymentSchedule.Clear();
                    contract.PaymentSchedule.AddRange(BuildAfterDefermentSchedulePeriod(
                        upcomingPaymentsCount.Value,
                        Build(
                            contract.Setting.ScheduleType.Value,
                            leftLoanCost,
                            contract.LoanPercent,
                            startDate,
                            contract.MaturityDate,
                            firstPaymentDate,
                            true,
                            isMigrated,
                            upcomingPaymentsCount: upcomingPaymentsCount,
                            isRestructuring: true),
                        amortizedBalanceOfDefferedPercent,
                        amortizedBalanceOfOverduePercent,
                        amortizedPenaltyOfOverdueDebt,
                        amortizedPenaltyOfOverduePercent));
                }
                else
                {
                    contract.PaymentSchedule = Build(
                        contract.Setting.ScheduleType.Value,
                        leftLoanCost,
                        contract.LoanPercent,
                        startDate,
                        contract.MaturityDate,
                        firstPaymentDate,
                        true,
                        isMigrated,
                        upcomingPaymentsCount: upcomingPaymentsCount);
                }

                    return;
            }
            #endregion

            throw new PawnshopApplicationException("По выбранному виду кредитования не настроена генерация графика платежей. Обратитесь к администратору.");
        }

        public void BuildWithContract(Contract contract)
        {
            #region special old annuity
            if (IsSpecialOldAnnuity(contract.PercentPaymentType))
            {
                contract.PaymentSchedule = Build(ScheduleType.Annuity, contract.LoanCost, contract.LoanPercent, contract.SignDate?.Date ?? contract.ContractDate.Date, contract.MaturityDate, contract.FirstPaymentDate);
                return;
            }
            #endregion

            #region floating discrete
            if (contract.Setting != null && contract.Setting.IsFloatingDiscrete)
            {
                if (contract.SettingId.HasValue && !contract.Setting.PaymentPeriod.HasValue)
                    throw new PawnshopApplicationException("Не задана настройка частоты погашения процентов в продукте, ошибка создания графика!");

                var beginDate = contract.ContractDate.Date;
                var rates = GetFloatingRates(contract);

                var builder = new FloatingDiscreteBuilder(contract.LoanCost, rates, beginDate, beginDate.AddMonths(rates.Count()));

                contract.PaymentSchedule = builder.Execute();
                contract.MaturityDate = contract.PaymentSchedule.Max(x => x.Date);

                return;
            }
            #endregion

            #region short discrete
            if (contract.PercentPaymentType == PercentPaymentType.EndPeriod)
            {
                var builder = new ShortDiscreteBuilder(contract.LoanCost, contract.LoanPercent, contract.MaturityDate);

                contract.PaymentSchedule = builder.Execute();
                return;
            }
            #endregion

            #region mixed
            if (contract.PercentPaymentType == PercentPaymentType.Product && contract.Setting?.ScheduleType == ScheduleType.Mixed)
                return; // TODO: create builder for mixed
            #endregion

            #region default
            if (IsDefaultScheduleType(contract.PercentPaymentType, contract.Setting?.ScheduleType))
            {
                if (contract.ContractClass == ContractClass.CreditLine)
                    return;

                var isMigrated = contract.ProductType?.Code == "TSO_MIGRATION";
                var firstPaymentDate = contract.FirstPaymentDate;

                if (contract.ContractClass == ContractClass.Tranche)
                {
                    var creditLineNextPaymentDate = GetNextPaymentDateByCreditLineId(contract.CreditLineId.Value);

                    if (creditLineNextPaymentDate.HasValue)
                    {
                        firstPaymentDate = creditLineNextPaymentDate.Value;

                        if (contract.Status == ContractStatus.Draft)
                        {
                            contract.FirstPaymentDate = firstPaymentDate;
                        }
                    }
                }

                contract.PaymentSchedule = Build(contract.Setting.ScheduleType.Value, contract.LoanCost, contract.LoanPercent, contract.SignDate?.Date ?? contract.ContractDate.Date, contract.MaturityDate, firstPaymentDate, isMigrated: isMigrated);
                contract.MaturityDate = contract.PaymentSchedule.Max(x => x.Date);

                return;
            }
            #endregion

            throw new PawnshopApplicationException("По выбранному виду кредитования не настроена генерация графика платежей. Обратитесь к администратору.");
        }

        public void CheckPayDay(int day)
        {
            var excludePayDays = new List<int> { 25, 26, 27, 28, 29, 30, 31 };

            if (excludePayDays.Contains(day))
                throw new PawnshopApplicationException($"День платежа не может быть с 25 по 31 включительно.");
        }

        public async Task CheckPayDayFromContract(Contract contract)
        {
            if (contract.ContractClass == ContractClass.CreditLine || !contract.FirstPaymentDate.HasValue)
                return;

            if (contract.ContractClass == ContractClass.Credit)
            {
                CheckPayDay(contract.FirstPaymentDate.Value.Day);
            }

            if (contract.ContractClass == ContractClass.Tranche)
            {
                var activeTrancheCount = await _contractRepository.GetActiveTranchesCount(contract.Id);

                if (activeTrancheCount == 0)
                    CheckPayDay(contract.FirstPaymentDate.Value.Day);
            }
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

        public bool IsDefaultScheduleType(PercentPaymentType percentPaymentType, ScheduleType? scheduleType)
        {
            if (percentPaymentType != PercentPaymentType.Product)
                return false;

            return scheduleType switch
            {
                ScheduleType.Annuity => true,
                ScheduleType.Discrete => true,
                ScheduleType.Differentiated => true,
                _ => false
            };
        }

        public List<RestructuredContractPaymentSchedule> BuildAfterDefermentSchedulePeriod(
            int restructuredMonthCount,
            List<ContractPaymentSchedule> schedulePartAfterDeferment,
            decimal amortizedBalanceOfDefferedPercent,
            decimal amortizedBalanceOfOverduePercent,
            decimal amortizedPenaltyOfOverdueDebt,
            decimal amortizedPenaltyOfOverduePercent)
        {
            var restructuredSchedule = new List<RestructuredContractPaymentSchedule>();

            var paymentBalanceOfDefferedPercent = amortizedBalanceOfDefferedPercent / restructuredMonthCount;
            var paymentBalanceOfOverduePercent = amortizedBalanceOfOverduePercent / restructuredMonthCount;
            var paymentPenaltyOfOverdueDebt = amortizedPenaltyOfOverdueDebt / restructuredMonthCount;
            var paymentPenaltyOfOverduePercent = amortizedPenaltyOfOverduePercent / restructuredMonthCount;

            decimal? fractionalDefferedPercent = 0.0m;
            decimal? fractionalOverduePercent = 0.0m;
            decimal? fractionalPenaltyOfOverdueDebt = 0.0m;
            decimal? fractionalPenaltyOfOverduePercent = 0.0m;

            schedulePartAfterDeferment.ForEach(s =>
            {
                var currentPaymentDefferedPercent = paymentBalanceOfDefferedPercent + fractionalDefferedPercent;
                var currentPaymentOverduePercent = paymentBalanceOfOverduePercent + fractionalOverduePercent;
                var currentPaymentPenaltyOfOverdueDebt = paymentPenaltyOfOverdueDebt + fractionalPenaltyOfOverdueDebt;
                var currentPaymentPenaltyOfOverduePercent = paymentPenaltyOfOverduePercent + fractionalPenaltyOfOverduePercent;

                var roundedPaymentDefferedPercent = Math.Floor(currentPaymentDefferedPercent.Value * 100) / 100;
                var roundedPaymentOverduePercent = Math.Floor(currentPaymentOverduePercent.Value * 100) / 100;
                var roundedPaymentPenaltyOfOverdueDebt = Math.Floor(currentPaymentPenaltyOfOverdueDebt.Value * 100) / 100;
                var roundedPaymentPenaltyOfOverduePercent = Math.Floor(currentPaymentPenaltyOfOverduePercent.Value * 100) / 100;

                fractionalDefferedPercent = currentPaymentDefferedPercent - roundedPaymentDefferedPercent;
                fractionalOverduePercent = currentPaymentOverduePercent - roundedPaymentOverduePercent;
                fractionalPenaltyOfOverdueDebt = currentPaymentPenaltyOfOverdueDebt - roundedPaymentPenaltyOfOverdueDebt;
                fractionalPenaltyOfOverduePercent = currentPaymentPenaltyOfOverduePercent - roundedPaymentPenaltyOfOverduePercent;

                amortizedBalanceOfDefferedPercent -= roundedPaymentDefferedPercent;
                amortizedBalanceOfOverduePercent -= roundedPaymentOverduePercent;
                amortizedPenaltyOfOverdueDebt -= roundedPaymentPenaltyOfOverdueDebt;
                amortizedPenaltyOfOverduePercent -= roundedPaymentPenaltyOfOverduePercent;

                if (amortizedBalanceOfDefferedPercent > 0 && amortizedBalanceOfDefferedPercent < 0.1m)
                {
                    roundedPaymentDefferedPercent += amortizedBalanceOfDefferedPercent;
                    amortizedBalanceOfDefferedPercent = 0;
                }
                if (amortizedBalanceOfOverduePercent > 0 && amortizedBalanceOfOverduePercent < 0.1m)
                {
                    roundedPaymentOverduePercent += amortizedBalanceOfOverduePercent;
                    amortizedBalanceOfOverduePercent = 0;
                }
                if (amortizedPenaltyOfOverdueDebt > 0 && amortizedPenaltyOfOverdueDebt < 0.1m)
                {
                    roundedPaymentPenaltyOfOverdueDebt += amortizedPenaltyOfOverdueDebt;
                    amortizedPenaltyOfOverdueDebt = 0;
                }
                if (amortizedPenaltyOfOverduePercent > 0 && amortizedPenaltyOfOverduePercent < 0.1m)
                {
                    roundedPaymentPenaltyOfOverduePercent += amortizedPenaltyOfOverduePercent;
                    amortizedPenaltyOfOverduePercent = 0;
                }

                restructuredSchedule.Add(new RestructuredContractPaymentSchedule(0, 0, s.Date, s.ActualDate, s.DebtLeft, s.DebtCost, s.PercentCost, s.PenaltyCost, s.CreateDate, null, null, s.Canceled, s.Prolongated,
                    s.Status, s.Period, s.Revision, s.ActionType, s.NextWorkingDate,
                    roundedPaymentDefferedPercent, amortizedBalanceOfDefferedPercent, roundedPaymentOverduePercent, amortizedBalanceOfOverduePercent,
                    roundedPaymentPenaltyOfOverdueDebt, amortizedPenaltyOfOverdueDebt, roundedPaymentPenaltyOfOverduePercent, amortizedPenaltyOfOverduePercent));
            });

            return restructuredSchedule;
        }

        private CheckDatesResult CheckDates(DateTime beginDate, DateTime maturityDate, DateTime? firstPaymentDate, bool isPartialPayment, bool isChangeControlDate, bool isRestructuring)
        {
            if (beginDate.AddMonths(1) > maturityDate)
                return new CheckDatesResult { Message = "Срок займа должен быть не менее 1 месяца." };

            if (isPartialPayment || isChangeControlDate)
                return new CheckDatesResult { IsCorrect = true };

            if (!firstPaymentDate.HasValue)
                return new CheckDatesResult { IsCorrect = true };

            if (firstPaymentDate.Value > maturityDate)
                return new CheckDatesResult { Message = "Дата первого платежа не может быть больше даты выкупа." };

            var diffDays = Math.Abs((firstPaymentDate.Value - beginDate).Days);

            if ((diffDays < Constants.PAYMENT_RANGE_MIN_DAYS || diffDays > Constants.PAYMENT_RANGE_MAX_DAYS) && !isRestructuring)
                return new CheckDatesResult { Message = $"Период первого платежа от даты выдачи не может быть менее 15 дней и более 45 дней, получено значение {diffDays}." };

            var monthDiff = maturityDate.MonthDifferenceWithDays(firstPaymentDate.Value);

            return new CheckDatesResult { IsCorrect = true, FixMaturityDate = firstPaymentDate.Value.AddMonths(monthDiff) };
        }

        private IEnumerable<decimal> GetFloatingRates(Contract contract)
        {
            var contractRates = _contractRateRepository.FindRateOnDateByFloatingContractAndRateSettingId(contract.Id).ToList();

            return contractRates.Any() ? contractRates.Select(x => x.Rate)
                : contract.Setting.LoanSettingRates
                    .Where(x => x.RateSetting.Code == Constants.ACCOUNT_SETTING_PROFIT)
                    .OrderBy(x => x.Index)
                    .Select(x => x.Rate);
        }

        private bool IsSpecialOldAnnuity(PercentPaymentType percentPaymentType) =>
            percentPaymentType switch
            {
                PercentPaymentType.AnnuityTwelve => true,
                PercentPaymentType.AnnuityTwentyFour => true,
                PercentPaymentType.AnnuityThirtySix => true,
                _ => false
            };

        protected class CheckDatesResult
        {
            public DateTime? FixMaturityDate { get; set; }
            public bool IsCorrect { get; set; }
            public string Message { get; set; }
        }
    }
}
