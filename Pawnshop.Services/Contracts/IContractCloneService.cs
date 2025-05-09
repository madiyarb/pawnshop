using Pawnshop.Data.Models.Contracts;
using Pawnshop.Data.Models.Contracts.Actions;
using System;
using System.Collections.Generic;
using System.Text;

namespace Pawnshop.Services.Contracts
{
    public interface IContractCloneService
    {
        Contract CreateContract(Contract contract, ContractAction action, int authorId, int branchId, decimal? loanCost = null,
                                bool isAddition = false, ContractRefinanceConfig refConfig = null, int? settingId = null, int? additionloanPeriod = null, int? subjectId = null, int? positionEstimatedCost = null);
    }
}
