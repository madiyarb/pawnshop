using Pawnshop.Data.Models.Contracts;
using Pawnshop.Data.Models.Contracts.Actions;
using Pawnshop.Data.Models.Contracts.Kdn;
using Pawnshop.Data.Models.Membership;
using Pawnshop.Services.Models.Contracts.Kdn;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Pawnshop.Services.Contracts
{
    public interface IContractKdnService
    {
        bool IsKdnRequired(Contract contract);
        List<ContractKdnEstimatedIncomeModel> GetContractKdnEstimatedIncomeModels(Contract contract, User author);
        void ContractCheckKdn(Contract contract, ContractKdnModel contractKdnModel, User author, int? parentContractId = null, bool isAddition = false);
        ContractKdnModel FillKdnModel(Contract contract, User author, bool isAddition = false, ContractAction? childAction = null);
        bool PassContractToNextStep(ContractKdnModel contractKdnModel);
        void SaveClientExpense(ContractKdnModel contractKdnModel);
        ContractKdnModel CheckKdn4AdditionWithCreateChild(ContractAction action, int branchId, User author, decimal? surchargeAmount, decimal additionCost, int? settingId = null, int? additionalLoanPeriod = null, int? subjectId = null, int? positionEstimatedCost = null);
        ContractKdnCalculationLog GetContractKdnCalculationLog4AdditionDate(int contractId, DateTime additionDate);

        Task<KdnCalculationLog> GetKDNMessage(int contractId);

        Task<bool> IsKDNPassed(int contractId, bool isAddition);
    }
}
