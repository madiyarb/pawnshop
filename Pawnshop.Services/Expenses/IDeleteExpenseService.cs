using System.Threading.Tasks;

namespace Pawnshop.Services.Expenses
{
    public interface IDeleteExpenseService
    {
        /// <summary>
        /// Удаление расхода(ContractExpenses) договора. Логика вынесена из ContractActionController.ExpenseDelete
        /// </summary>
        /// <param name="contractExpenseId">Идентификатор расхода</param>
        /// <param name="branchId">Идентификатор филиала</param>
        Task DeleteExpenseWithRecalculation(int contractExpenseId, int branchId);
    }
}