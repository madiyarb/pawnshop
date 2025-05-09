using Pawnshop.Data.Models.Contracts.Actions;
using System;
using System.Collections.Generic;
using System.Text;

namespace Pawnshop.Services.AccountingCore
{
    public interface IContractActionOperationPermisisonService
    {
        bool CanPayExtraExpenses(ContractActionType contractActionType);
        bool CanPayEncumbrance(ContractActionType contractActionType);
        bool CanMakePayOperations(ContractAction contractAction);
        bool CanPayCertainExtraExpenses(ContractActionType contractActionType);
        bool DoNotValidateActionsRowsIntegrity(ContractActionType contractActionType);
        bool DoNotValidateActionsRowsAndDiscounts(ContractActionType contractActionType);
        bool RestrictedExtraExpensesForAddition(string typeHierarchyCode);
    }
}
