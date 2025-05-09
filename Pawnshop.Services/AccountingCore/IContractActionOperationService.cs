using Pawnshop.AccountingCore.Abstractions;
using Pawnshop.Data.Models.Contracts.Actions;
using Pawnshop.Data.Models.CashOrders;
using System.Threading.Tasks;

namespace Pawnshop.Services.AccountingCore
{
    public interface IContractActionOperationService
    {
        ContractAction Register(IContract contract, ContractAction action, int authorId, int? branchId = null,
            bool callActionRowBusinessOperation = true, OrderStatus? orderStatus = null, bool forceExpensePrepaymentReturn = true);
        void RevertScheduleBeforeAction(int contractId, int contractActionId, int authorId);
        Task Cancel(int contractActionId, int authorId, int branchId, bool isStorn, bool autoApprove);
        void CancelDelete(int contractActionId, int authorId, int branchId);
        void UndoCancel(int contractActionId, int authorId, int branchId);
        Task CancelActions(ContractActionAutoStorno autoStornoModel);
    }
}
