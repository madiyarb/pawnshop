using Pawnshop.Data.Models.Contracts;
using System.Collections.Generic;
using System;

namespace Pawnshop.Services.PaymentSchedules.Builders
{
    public class DiscreteBuilder : BaseBuilder
    {
        private DateTime _beginDate;
        private DateTime _maturityDate;
        private DateTime? _firstPaymentDate;
        private DateTime? _lastPaymentDate = default;
        private decimal _loanCost;
        private decimal _percentPerDay;
        private int _counter = 0;
        private int _paymentsCount = 0;
        private bool _isMigrated;

        public DiscreteBuilder(
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
            _isMigrated = isMigrated;
        }

        public override List<ContractPaymentSchedule> Execute()
        {
            var schedule = new List<ContractPaymentSchedule>();

            for (; _counter < _paymentsCount; _counter++)
            {
                var item = new ContractPaymentSchedule();

                SetDateAndPeriod(item, _paymentsCount, _counter, _beginDate, _maturityDate, _lastPaymentDate, _firstPaymentDate);

                item.PercentCost = CalculatePercent(item.Period);
                item.DebtCost = 0;
                item.DebtLeft = _loanCost;
                item.Revision = 1;

                if (_paymentsCount == _counter + 1)
                {
                    item.DebtCost = _loanCost;
                    item.DebtLeft = 0;
                }

                schedule.Add(item);

                _lastPaymentDate = item.Date;
            }

            return schedule;
        }

        protected override decimal CalculatePercent(int period)
        {
            var isNotDefaultDays = IsNotDefaultDays(_counter, _paymentsCount, _beginDate, _maturityDate, _firstPaymentDate, _lastPaymentDate);

            return DefaultCalculatePercent(_loanCost, _percentPerDay, period, isNotDefaultDays, _isMigrated);
        }
    }
}
