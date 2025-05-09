using Pawnshop.Data.Models.CashOrders;
using Pawnshop.Data.Models.Contracts.Expenses;
using Pawnshop.Data.Models.Dictionaries;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Pawnshop.Services.Contracts
{
    public interface IContractExpenseOperationService
    {
        Task RegisterAsync(ContractExpense contractExpense, int authorId, int branchId, int? actionId = null, bool forcePrepaymentReturn = true, OrderStatus orderStatus = OrderStatus.WaitingForApprove);
        Task PayExtraExpensesAsync(int contractExpenseId, int authorId, int branchId, int? actionId = null, bool forcePrepaymentReturn = true, int? prepaymentContractId = null);
        Task CancelAsync(int expenseId, int authorId, int branchId, int? actionId = null);
        Task<IDictionary<int, (int, DateTime)>> CancelExpensesByActionIdAsync(int actionId, int authorId, int branchId, bool isStorn);
        IDictionary<int, (int, DateTime)> DeleteWithOrders(int id, int authorId, int branchId, int? contractActionId = null);
        List<ContractExpense> GetNeededExpensesForPrepayment(int contractId, int branchId, IEnumerable<int> extraExpensesIds = null);
        List<ContractExpense> GetIncomingExtraExpensesByContractId(int contractId);

        Expense GetExpenseType(int expenseId);
    }
}
