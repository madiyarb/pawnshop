using System;
using System.Collections.Generic;
using System.Text;
using Pawnshop.Data.Models.Contracts.Actions;
using Pawnshop.Data.Models.CashOrders;

namespace Pawnshop.Services.Contracts
{
    public interface IContractPaymentService : IService
    {
        List<ContractActionRow> Preview(decimal cost, int contractId);
        void Payment(ContractAction action, int branchId, int authorId, bool forceExpensePrepaymentReturn, bool autoApprove);
        ContractAction PaymentWithReturnContractAction(ContractAction action, int branchId, int authorId, bool forceExpensePrepaymentReturn, bool autoApprove);
        public List<CashOrder> GetPayedInterest(int contractId, DateTime endDate, DateTime? beginDate = null);
        void ExecuteOnApprove(ContractAction action, int branchId, int authorId, bool forceExpensePrepaymentReturn);
    }
}
