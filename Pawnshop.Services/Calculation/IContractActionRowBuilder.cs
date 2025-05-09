using Pawnshop.Data.Models.Contracts;
using Pawnshop.Data.Models.Contracts.Actions;
using Pawnshop.Services.Models.Calculation;
using Pawnshop.AccountingCore.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace Pawnshop.Services.Calculation
{
    public interface IContractActionRowBuilder : IContractAmount
    {
        List<ContractActionRow> Build(Contract contract, ContractDutyCheckModel model, int? branchId = null, decimal refinance = 0);
        void InitAccounts(Contract contract);
        Dictionary<AmountType, decimal> GetDistinctRowAmounts(List<ContractActionRow> rows, HashSet<AmountType> loanAmountTypes);
    }
}
