using Pawnshop.Data.Models.Contracts.Actions;
using System;
using System.Threading.Tasks;

namespace Pawnshop.Services.CreditLines.Payment
{
    public interface ICreditLinePaymentService
    {
        public Task<int?> TransferPrepaymentAndPayment(int creditLineId, int authorId, int payTypeId, int branchId,
            DateTime? date = null, string note = null, decimal amount = 0, bool autoApprove = false);

        public Task<CreditLineAccountBalancesDistribution> GetCreditLineAccountBalancesDistribution(
            int creditLineId, decimal amount = 0, DateTime? date = null);

        public Task<ContractAction> PaymentOnContract(int contractId, decimal value, int payTypeId,
            ContractAction parentAction, int authorId, int branchId, bool autoApprove = false, DateTime? date = null);
    }
}
