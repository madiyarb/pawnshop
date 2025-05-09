using Pawnshop.Data.Models.CashOrders;
using Pawnshop.Data.Models.Contracts.Actions;

namespace Pawnshop.Web.Engine.Services.Interfaces
{
    public interface IContractActionSignService
    {
        void Exec(ContractAction action,
            int authorId, int branchId,
            bool unsecuredContractSignNotAllowed,
            bool ignoreVerification = false,
            bool ignoreCheckQuestionnaireFilledStatus = false,
            int? parentContractId = null,
            OrderStatus? orderStatus = null,
            int? cashIssueBranchId = null);
    }
}
