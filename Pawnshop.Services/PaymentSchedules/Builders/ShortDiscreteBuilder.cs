using Pawnshop.Data.Models.Contracts;
using System.Collections.Generic;
using System;

namespace Pawnshop.Services.PaymentSchedules.Builders
{
    public class ShortDiscreteBuilder : BaseBuilder
    {
        private DateTime _maturityDate;
        private decimal _loanCost;
        private decimal _percentPerDay;

        public ShortDiscreteBuilder(decimal loanCost, decimal percentPerDay, DateTime maturityDate)
        {
            _loanCost = loanCost;
            _percentPerDay = percentPerDay;
            _maturityDate = maturityDate.Date;
        }

        public override List<ContractPaymentSchedule> Execute()
        {
            var schedule = new List<ContractPaymentSchedule>
            {
                new ContractPaymentSchedule
                {
                    DebtCost = _loanCost,
                    DebtLeft = 0,
                    Date = _maturityDate,
                    Period = 30,
                    Revision = 1,
                    PercentCost = CalculatePercent(30),
                }
            };

            return schedule;
        }

        protected override decimal CalculatePercent(int period)
        {
            return _loanCost * ((period * _percentPerDay) / 100);
        }
    }
}
