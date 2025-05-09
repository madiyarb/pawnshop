using Pawnshop.Data.Models.Contracts.Actions;
using System.Threading.Tasks;
using System;

namespace Pawnshop.Services.CreditLines.PartialPayment
{
    public interface ICreditLinePartialPaymentService
    {
        public Task<CreditLineAccountBalancesDistributionForPartialPayment> GetCreditLineAccountBalancesDistribution(
            int creditLineId, int partialPaymentContractId, decimal amount, DateTime? date = null);

        public Task<int?> PartialPaymentAndTransfer(int creditLineId, int authorId, int partialPaymentContractId, int payTypeId,
            int branchId, decimal amount, DateTime? date = null, bool unsecuredContractSignNotallowed = false, bool changeCategory = false);

        public Task<ContractAction> PartialPayment(int contractId, int payTypeId, int authorId, int branchId,
            decimal cost, decimal totalCost, bool unsecuredContractSignNotAllowed, DateTime? date = null);
        public Task<int?> PartialPaymentAndTransferByOnline(int creditLineId, int authorId, int partialPaymentContractId, int payTypeId,
            int branchId, decimal amount, DateTime? date = null, bool unsecuredContractSignNotallowed = false, bool changeCategory = false);
    }
}
