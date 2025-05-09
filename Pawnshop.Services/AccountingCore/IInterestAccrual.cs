using System;
using System.Collections.Generic;
using Pawnshop.AccountingCore.Models;
using Pawnshop.Data.Models.Contracts;
using Pawnshop.Data.Models.Contracts.Actions;

namespace Pawnshop.AccountingCore.Abstractions
{
    public interface IInterestAccrual
    {
        decimal OnControlDate(decimal scheduledPercentCost, decimal accrualProfit);
        void OnControlDate(IContract contract, int authorId, DateTime? accrualDate = null);
        decimal OnAnyDate(decimal scheduledPercentCost, decimal accrualProfit, decimal calculatedPercentCost);
        ContractAction OnAnyDateAccrual(Contract contract, int authorId, DateTime? accrualDate = null,
            bool isFloatingDiscrete = false, IEnumerable<ContractRate> contractRates = null, decimal totalSum = 0);
        void OnAnyDateOnOverdueDebt(IContract contract, int authorId, DateTime accrualDate);
        void ManualInterestAccrualOnOverdueDebt(IContract contract, int authorId, DateTime accrualDate);
    }
}
