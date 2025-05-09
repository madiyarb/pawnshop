using Pawnshop.AccountingCore.Models;
using Pawnshop.Data.Models.Contracts;
using Pawnshop.Data.Models.Contracts.Actions;
using Pawnshop.Data.Models.Contracts.Penalty;
using System;
using System.Collections.Generic;
using System.Text;

namespace Pawnshop.Services.PenaltyLimit
{
    public interface IPenaltyRateService
    {
        void DecreaseRates(Contract contract, DateTime date, int authorId, List<AccrualBase> accrualSettings);
        void IncreaseRates(Contract contract, DateTime date, int authorId, ContractAction? parentAction = null);
        void IncreaseOrDecreaseRateManualy(Contract contract, DateTime date, int authorId, bool increase = true);
    }
}
