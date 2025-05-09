using Pawnshop.Data.Models.Contracts;
using System.Collections.Generic;
using System;

namespace Pawnshop.Services.PaymentSchedules.Builders
{
    public class DifferentBuilder : BaseBuilder
    {
        private DateTime _beginDate;
        private DateTime _maturityDate;
        private DateTime? _firstPaymentDate;
        private DateTime? _lastPaymentDate = default;
        private decimal _balanceCost;
        private decimal _loanCost;
        private decimal _percentPerDay;
        private int _counter = 0;
        private int _paymentsCount = 0;
        private bool _isMigrated;

        public DifferentBuilder(
            decimal loanCost,
            decimal percentPerDay,
            DateTime beginDate,
            DateTime maturityDate,
            DateTime? firstPaymentDate,
            bool isMigrated = false,
            int? paymentsCount = null)
        {
            _loanCost = loanCost;
            _percentPerDay = percentPerDay;
            _beginDate = beginDate.Date;
            _maturityDate = maturityDate.Date;
            _firstPaymentDate = firstPaymentDate?.Date;
            _paymentsCount = paymentsCount ?? this.GetPaymentsCount(_beginDate, _maturityDate, _firstPaymentDate);
            _balanceCost = _loanCost;
            _isMigrated = isMigrated;
        }

        public override List<ContractPaymentSchedule> Execute()
        {
            var schedule = new List<ContractPaymentSchedule>();
            decimal monthlyPayment = Math.Round(_loanCost / _paymentsCount, 2);

            for (; _counter < _paymentsCount; _counter++)
            {
                var item = new ContractPaymentSchedule();

                SetDateAndPeriod(item, _paymentsCount, _counter, _beginDate, _maturityDate, _lastPaymentDate, _firstPaymentDate);

                item.PercentCost = CalculatePercent(item.Period);
                item.DebtCost = monthlyPayment;
                item.DebtLeft = _balanceCost - item.DebtCost;
                item.Revision = 1;

                if (_paymentsCount == _counter + 1)
                {
                    item.DebtCost += item.DebtLeft;
                    item.DebtLeft = 0;
                }

                item.DebtCost = Math.Round(item.DebtCost, 2);
                item.DebtLeft = Math.Round(item.DebtLeft, 2);

                schedule.Add(item);

                _balanceCost -= item.DebtCost;
                _lastPaymentDate = item.Date;
            }

            return schedule;
        }

        protected override decimal CalculatePercent(int period)
        {
            var isNotDefaultDays = IsNotDefaultDays(_counter, _paymentsCount, _beginDate, _maturityDate, _firstPaymentDate, _lastPaymentDate);

            return DefaultCalculatePercent(_balanceCost, _percentPerDay, period, isNotDefaultDays, _isMigrated);
        }
    }
}
