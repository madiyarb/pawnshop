using Pawnshop.Core;
using Pawnshop.Data.Models.Contracts;
using System.Collections.Generic;
using System;

namespace Pawnshop.Services.PaymentSchedules.Builders
{
    public abstract class BaseBuilder
    {
        public abstract List<ContractPaymentSchedule> Execute();
        protected abstract decimal CalculatePercent(int period);


        protected decimal DefaultCalculatePercent(decimal loanCost, decimal percentPerDay, int period, bool isNotDefaultDays, bool isMigrated = false)
        {
            if (!isMigrated && isNotDefaultDays)
                return Math.Round(loanCost * period * (percentPerDay * 360 / 365 / 100), 2);
            else if (isMigrated && !isNotDefaultDays)
                return Math.Round(loanCost * period * (percentPerDay * 365 / 360 / 100), 2);
            else
                return Math.Round(loanCost * period * (percentPerDay / 100), 2);
        }

        protected int GetPaymentsCount(DateTime beginDate, DateTime maturityDate, DateTime? firstPaymentDate = null)
        {
            var period = 0;
            bool run = true;

            while (run)
            {
                int diffDays;

                if (period == 0)
                    diffDays = Math.Abs((beginDate - maturityDate).Days);
                else if (firstPaymentDate.HasValue)
                    diffDays = Math.Abs((firstPaymentDate.Value.AddMonths(period - 1) - maturityDate).Days);
                else
                    diffDays = Math.Abs((beginDate.AddMonths(period) - maturityDate).Days);

                if (diffDays >= Constants.PAYMENT_RANGE_MIN_DAYS && diffDays <= Constants.PAYMENT_RANGE_MAX_DAYS)
                    run = false;

                period++;
            }

            return period;
        }

        protected bool IsFirstPaymentDateCorrect(DateTime beginDate, DateTime? firstPaymentDate)
        {
            if (!firstPaymentDate.HasValue)
                return true;

            if (firstPaymentDate.Value == beginDate.AddMonths(1))
                return true;

            return false;
        }

        protected bool IsLastPaymentDateCorrect(int paymentsCount, DateTime beginDate, DateTime maturityDate, DateTime? lastPaymentDate)
        {
            var planedMaturityDate = beginDate.AddMonths(paymentsCount);

            if (!lastPaymentDate.HasValue && planedMaturityDate == maturityDate)
                return true;

            if (lastPaymentDate.HasValue && lastPaymentDate.Value.AddMonths(1) == maturityDate)
                return true;

            return false;
        }

        protected bool IsNotDefaultDays(int counter, int paymentsCount, DateTime beginDate, DateTime maturityDate, DateTime? firstPaymentDate, DateTime? lastPaymentDate)
        {
            if (counter == 0 && firstPaymentDate.HasValue && !IsFirstPaymentDateCorrect(beginDate, firstPaymentDate))
                return true;

            if (paymentsCount == counter + 1 && !IsLastPaymentDateCorrect(paymentsCount, beginDate, maturityDate, lastPaymentDate))
                return true;

            return false;
        }

        protected void SetDateAndPeriod(ContractPaymentSchedule item, int period, int index, DateTime beginDate, DateTime maturityDate, DateTime? lastPaymentDate = null, DateTime? firstPaymentDate = null)
        {
            if (index == 0)
            {
                item.Date = firstPaymentDate ?? beginDate.AddMonths(1);
                item.Period = IsFirstPaymentDateCorrect(beginDate, firstPaymentDate) ? 30 : (firstPaymentDate.Value - beginDate).Days;

                return;
            }

            if (period == index + 1)
            {
                item.Date = maturityDate;

                if (IsLastPaymentDateCorrect(period, beginDate, maturityDate, lastPaymentDate))
                    item.Period = 30;
                else
                    item.Period = Math.Abs((item.Date - lastPaymentDate.Value).Days);

                return;
            }

            if (firstPaymentDate.HasValue)
            {
                item.Date = firstPaymentDate.Value.AddMonths(index);
                item.Period = 30;

                return;
            }

            item.Date = beginDate.AddMonths(index + 1);
            item.Period = 30;

            return;
        }
    }
}
