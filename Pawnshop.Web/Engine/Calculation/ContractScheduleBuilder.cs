using Pawnshop.Core.Exceptions;
using Pawnshop.Data.Models.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using Pawnshop.AccountingCore.Abstractions;
using Pawnshop.AccountingCore.Models;
using Pawnshop.Core.Queries;
using Pawnshop.Data.Access;
using Pawnshop.Data.Models.LoanSettings;
using Pawnshop.Services.Contracts;

namespace Pawnshop.Web.Engine.Calculation
{
    /*public class ContractScheduleBuilder //: ICalculation
    {
        public List<ContractPaymentSchedule> BuildAnnuity(DateTime beginDate, decimal loanCost, decimal loanPercentPerDay, DateTime maturityDate, DateTime? firstPaymentDate = null, int? paymentsCount = null, int? paymentDebt = null)
        {
            paymentsCount ??= maturityDate.MonthDifference(beginDate);
            if (paymentsCount <= 0) throw new PawnshopApplicationException($@"График не может быть создан с 0 и менее месяцами");
            int counter = 0;
            int debtCounter = 0;
            decimal monthPercent = loanPercentPerDay * 30;
            decimal balanceCost = loanCost;
            List<ContractPaymentSchedule> schedule = new List<ContractPaymentSchedule>();


            decimal monthlyPayment = (loanCost / Convert.ToDecimal((1 - Math.Pow((double)(1 + (monthPercent / 100)), -paymentsCount.Value)) / ((double)monthPercent / 100)));
            while (paymentsCount != counter)
            {
                var item = new ContractPaymentSchedule();
                item.Revision = 1;
                if (firstPaymentDate.HasValue)
                {
                    if (counter == 0)
                    {
                        item.Date = firstPaymentDate.Value.Date;
                        item.Period = (item.Date - beginDate.Date).Days;
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
                }


                item.PercentCost = Math.Round(item.Period * (loanPercentPerDay / 100) * balanceCost, 2);
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

                schedule.Add(item);

                balanceCost -= item.DebtCost;
                counter++;
                debtCounter++;
            }
            return schedule;
        }

        public List<ContractPaymentSchedule> BuildDiscrete(decimal loanCost, decimal loanPercentPerDay,
            DateTime maturityDate)
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

        public void BuildScheduleForNewContract(Contract contract)
        {
            var beginDate = contract.SignDate?.Date ?? contract.ContractDate.Date;
            var loanCost = contract.LoanCost;
            var loanPercentPerDay = contract.SettingId.HasValue ? contract.Setting.LoanPercent : contract.LoanPercent;

            DateTime maturityDate = default;
            if (contract.SettingId.HasValue)
            {
                int paymentCount = contract.Setting.PaymentCount(contract.LoanPeriod);
                maturityDate = beginDate.AddDays(paymentCount);
            }
            else
            {
                int loanPeriod = contract.LoanPeriod;
                if (contract.PercentPaymentType == PercentPaymentType.EndPeriod 
                    && !contract.Locked)
                    loanPeriod--;

                maturityDate = beginDate.AddDays(loanPeriod);
            }

            if (contract.SettingId.HasValue && !contract.Setting.PaymentPeriod.HasValue) throw new PawnshopApplicationException("Не задана настройка частоты погашения процентов в продукте, ошибка создания графика!");
            DateTime? firstPaymentDate = contract.FirstPaymentDate;

            var paymentsCount = contract.SettingId.HasValue ? contract.Setting.PaymentCount(contract.LoanPeriod) : (int)Math.Floor((decimal)contract.LoanPeriod/30);

            if (contract.PercentPaymentType == PercentPaymentType.AnnuityTwelve || contract.PercentPaymentType == PercentPaymentType.AnnuityTwentyFour || contract.PercentPaymentType == PercentPaymentType.AnnuityThirtySix)
            {
                if (contract.PartialPaymentParentId.HasValue)
                {
                    maturityDate = contract.MaturityDate;
                    paymentsCount = maturityDate.MonthDifference(beginDate);
                }

                contract.PaymentSchedule = BuildAnnuity(beginDate, loanCost, loanPercentPerDay, maturityDate,
                    firstPaymentDate, paymentsCount);
            }
            else if (contract.PercentPaymentType == PercentPaymentType.Product &&
                     (contract.Setting?.ScheduleType == ScheduleType.Annuity || contract.Setting?.ScheduleType == ScheduleType.Discrete))
            {
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
                if (contract.FirstPaymentDate.HasValue)
                {
                    firstPaymentDate = contract.FirstPaymentDate.Value;
                }

                if (firstPaymentDate.HasValue && firstPaymentDate.Value.Date == controlDate.Value.Date)
                {
                    firstPaymentDate = null;
                }

                var setting = contract.Setting;
                var debtPeriod = setting.DebtPeriod * (int)setting.DebtPeriodType;
                var paymentPeriod = setting.PaymentPeriod * (int)setting.PaymentPeriodType;

                if (debtPeriod != paymentPeriod)
                {
                    if (contract.PartialPaymentParentId.HasValue)
                    {
                        maturityDate = contract.MaturityDate;
                        paymentsCount = maturityDate.MonthDifference(beginDate);
                    }
                    
                    if (debtPeriod % paymentPeriod != 0) throw new PawnshopApplicationException("Ошибка генерации графика, периодичность погашения ОД должна быть кратной периодичности погашения процентов");
                    var paymentDebt = debtPeriod / paymentPeriod;

                    contract.PaymentSchedule = BuildAnnuity(beginDate, loanCost, loanPercentPerDay, maturityDate, firstPaymentDate, paymentsCount, paymentDebt);
                }
                else
                {
                    if (contract.PartialPaymentParentId.HasValue)
                    {
                        maturityDate = contract.MaturityDate;
                        paymentsCount = maturityDate.MonthDifference(beginDate);
                    }

                    contract.PaymentSchedule = BuildAnnuity(beginDate, loanCost, loanPercentPerDay, maturityDate, firstPaymentDate, paymentsCount);
                }       
            }
            else if (contract.PercentPaymentType == PercentPaymentType.EndPeriod)
            {
                contract.PaymentSchedule = BuildDiscrete(loanCost, loanPercentPerDay, maturityDate);
            }
            else throw new PawnshopApplicationException("По выбранному виду кредитования не настроена генерация графика платежей. Обратитесь к администратору.");

            contract.MaturityDate = contract.PaymentSchedule.Max(x=>x.Date);
        }
    }*/
}
