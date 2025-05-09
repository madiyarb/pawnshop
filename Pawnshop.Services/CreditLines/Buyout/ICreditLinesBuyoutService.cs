using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Pawnshop.Data.Models.Contracts.Actions;

namespace Pawnshop.Services.CreditLines.Buyout
{
    public interface ICreditLinesBuyoutService
    {
        public Task<CreditLineAccountBalancesDistributionForBuyOut> GetCreditLineAccountBalancesDistributionForBuyBack(
            int creditLineId, List<int> buyBackedContracts, int? expenseId = null, DateTime? date = null);

        public Task<int?> TransferPrepaymentAndBuyBack(int creditLineId, int authorId, int payTypeId, int branchId,
            List<int> buyBackedContracts, int buyoutReasonId,
            bool buyoutCreditLine = false, DateTime? date = null, string note = null, bool autoApprove = false, int? expenseId = null);

        public Task<int?> TransferPrepaymentAndBuyOutOnline(int creditLineId, int authorId, int branchId,
            List<int> buyBackedContracts);

        Task<List<ContractAction>> BuyOut(int contractId, int creditLineId, int payTypeId,
            ContractAction parentAction, int authorId, int branchId, int? buyoutReasonId, bool buyoutCreditLine,
            bool autoApprove = false, bool forceBuyOutCreditLine = false, int? expenseId = null, DateTime? date = null);
    }
}
