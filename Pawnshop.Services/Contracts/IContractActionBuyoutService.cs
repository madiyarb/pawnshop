using Pawnshop.Data.Models.Contracts.Actions;
using Pawnshop.Data.Models.Contracts.Expenses;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Pawnshop.Data.Models.Contracts;

namespace Pawnshop.Services.Contracts
{
    public interface IContractActionBuyoutService
    {
        Task Execute(ContractAction contractAction, int authorId, int branchId, bool forceExpensePrepaymentReturn, bool autoApprove, ContractAction childAction);

        Task Execute(
            ContractAction contractAction,
            int authorId,
            int branchId,
            bool forceExpensePrepaymentReturn,
            bool autoApprove,
            ContractAction childAction,
            Contract? contract);

        Task<List<ContractAction>> ExecuteWithReturnContractAction(ContractAction action, int authorId, int branchId,
            bool forceExpensePrepaymentReturn,
            bool autoApporve, ContractAction prepaymentAction = null, bool forceBuyoutCreditLine = false, ContractExpense expense = null);
        Task ExecuteOnApprove(ContractAction action, int authorId, int branchId, ContractAction childAction);

        Task ExecuteOnApprove(
            ContractAction action,
            int authorId,
            int branchId,
            ContractAction childAction = null,
            Contract contract = null);
    }
}
