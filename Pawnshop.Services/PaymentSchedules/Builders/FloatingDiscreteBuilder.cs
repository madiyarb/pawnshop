using Pawnshop.Data.Models.Contracts;
using System.Collections.Generic;
using System.Linq;
using System;

namespace Pawnshop.Services.PaymentSchedules.Builders
{
    public class FloatingDiscreteBuilder : BaseBuilder
    {
        private DateTime _beginDate;
        private DateTime _maturityDate;
        private DateTime? _firstPaymentDate;
        private DateTime? _lastPaymentDate = default;
        private decimal _loanCost;
        private List<decimal> _rates;
        private decimal _percentPerDay;
        private int _counter = 0;
        private int _paymentsCount = 0;

        public FloatingDiscreteBuilder(
            decimal loanCost,
            IEnumerable<decimal> rates,
            DateTime beginDate,
            DateTime maturityDate,
            DateTime? firstPaymentDate = null)
        {
            _loanCost = loanCost;
            _rates = rates.ToList();
            _beginDate = beginDate;
            _maturityDate = maturityDate;
            _firstPaymentDate = firstPaymentDate;
            _paymentsCount = _rates.Count;
        }

        public override List<ContractPaymentSchedule> Execute()
        {
            var schedule = new List<ContractPaymentSchedule>();

            for (; _counter < _paymentsCount; _counter++)
            {
                var item = new ContractPaymentSchedule();
                _percentPerDay = _rates[_counter];

                SetDateAndPeriod(item, _paymentsCount, _counter, _beginDate, _maturityDate, _lastPaymentDate, _firstPaymentDate);

                item.PercentCost = CalculatePercent(item.Period);
                item.DebtCost = 0;
                item.DebtLeft = _loanCost;
                item.Revision = 1;

                if (_paymentsCount == _counter + 1)
                {
                    item.DebtCost += item.DebtLeft;
                    item.DebtLeft = 0;
                }

                item.DebtCost = Math.Round(item.DebtCost, 2);
                item.DebtLeft = Math.Round(item.DebtLeft, 2);

                schedule.Add(item);

                _lastPaymentDate = item.Date;
            }

            return schedule;
        }

        protected override decimal CalculatePercent(int period)
        {
            var isNotDefaultDays = IsNotDefaultDays(_counter, _paymentsCount, _beginDate, _maturityDate, _firstPaymentDate, _lastPaymentDate);

            return DefaultCalculatePercent(_loanCost, _percentPerDay, period, isNotDefaultDays);
        }
    }
}
