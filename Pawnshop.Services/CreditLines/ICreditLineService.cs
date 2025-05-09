using Pawnshop.Data.Models.Contracts.Actions;
using Pawnshop.Data.Models.Contracts;
using Pawnshop.Data.Models.CreditLines;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;

namespace Pawnshop.Services.CreditLines
{
    public interface ICreditLineService
    {
        public Task<CheckForOpenTrancheModel> CheckForOpenTranche(int creditLineId, Contract creditLine);

        public Task<decimal> GetAmountForCurrentlyPayment(int creditLineId, DateTime? accrualDate = null);

        public Task<CreditLineBalance> GetCurrentlyDebtForCreditLine(int creditLineId, List<int> selectedContracts = null, DateTime? date = null);

        public RefillableAccountsInfo GetExpensesPaymentSum(decimal summaryExpenseAmount, decimal amount, int creditLineId);

        public int GetRemainingPaymentsCount(int creditLineId, Contract creditLine);

        public ContractAction MovePrepayment(int creditLineId, int contractId, decimal value, int authorId,
                    int branchId, string note = null, ContractAction action = null, bool autoApprove = false, DateTime? date = null);

        public ContractAction MovePrepaymentFromTrancheToCreditLine(int creditLineId, int contractId, decimal value,
            int authorId, int branchId, string note = null,
            ContractAction action = null, bool autoApprove = false, DateTime? date = null);

        public ContractAction PayExtraExpenses(int creditLineId, decimal amount, DateTime date, int payTypeId,
            int authorId, int branchId, bool autoApprove);

        public Task<bool> UnconfirmedActionExists(List<int> contractIds);

        public Task<decimal> GetLimitForInsuranceByContractId(int contractId);

        public Task<decimal> GetLimitForInsuranceByPosition(decimal estimatedCost);

        public Task<decimal> GetLimitPersentForInsurance(decimal estimatedCost);

        Task<bool> IncompleteActionExistsAsync(List<int> contractIds);

        Task<bool> HasExpenses(List<int> contractIds);
    }
}
