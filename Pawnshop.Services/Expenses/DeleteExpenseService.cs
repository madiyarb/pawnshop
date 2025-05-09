using System;
using System.Data;
using System.Threading.Tasks;
using Pawnshop.Core.Exceptions;
using Pawnshop.Data.Access.AccountingCore;
using Pawnshop.Services.AccountingCore;
using Pawnshop.Services.Contracts;

namespace Pawnshop.Services.Expenses
{
    public class DeleteExpenseService : IDeleteExpenseService
    {
        private readonly AccountRecordRepository _accountRecordRepository;
        private readonly Lazy<IContractExpenseOperationService> _contractExpenseOperationService;
        private readonly IAccountRecordService _accountRecordService;

        public DeleteExpenseService(
            AccountRecordRepository accountRecordRepository,
            Lazy<IContractExpenseOperationService> contractExpenseOperationService,
            IAccountRecordService accountRecordService)
        {
            _accountRecordRepository = accountRecordRepository;
            _contractExpenseOperationService = contractExpenseOperationService;
            _accountRecordService = accountRecordService;
        }

        /// <summary>
        /// Удаление расхода(ContractExpenses) договора. Логика вынесена из ContractActionController.ExpenseDelete
        /// </summary>
        /// <param name="contractExpenseId">Идентификатор расхода</param>
        /// <param name="branchId">Идентификатор филиала</param>
        public async Task DeleteExpenseWithRecalculation(int contractExpenseId, int branchId)
        {
            using IDbTransaction transaction = _accountRecordRepository.BeginTransaction();

            var recalculateBalanceAccountDict =
                _contractExpenseOperationService.Value.DeleteWithOrders(contractExpenseId, 1, branchId);

            if (recalculateBalanceAccountDict == null)
            {
                throw new PawnshopApplicationException(
                    $"Ожидалось что {nameof(_contractExpenseOperationService)}." +
                    $"{nameof(_contractExpenseOperationService.Value.DeleteWithOrders)} не будет null");
            }

            foreach (var (accountId, (accountRecordId, date)) in recalculateBalanceAccountDict)
            {
                DateTime? recalculateDate = null;

                var accountRecordBeforeDate =
                    _accountRecordRepository.GetLastRecordByAccountIdAndEndDate(accountId, accountRecordId, date);

                if (accountRecordBeforeDate != null)
                {
                    recalculateDate = accountRecordBeforeDate.Date;
                }

                _accountRecordService.RecalculateBalanceOnAccount(accountId, beginDate: recalculateDate);
            }

            transaction.Commit();
        }
    }
}