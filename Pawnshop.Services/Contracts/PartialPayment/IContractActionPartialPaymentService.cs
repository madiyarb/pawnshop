using Pawnshop.Data.Models.Contracts.Actions;
using Pawnshop.Data.Models.Contracts;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Pawnshop.Services.Contracts.PartialPayment
{
    public interface IContractActionPartialPaymentService
    {
        Task<ContractAction> Exec(ContractAction action, int authorId, int branchId, bool unsecuredContractSignNotallowed, bool forceExpensePrepaymentReturn);
        Task<List<ContractPartialPayment>> GetContractPartialPayments(int ContractId);
        Task<ContractPartialPayment> GetContractParentPayments(int ContractId);
    }
}
