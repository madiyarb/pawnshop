using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using Pawnshop.Data.Models.CashOrders;
using Pawnshop.Data.Models.Contracts.Actions;
using Pawnshop.Services.Models.Filters;

namespace Pawnshop.Services.Contracts
{
    public interface IContractActionService : IDictionaryWithSearchService<ContractAction, ContractActionFilter>
    {
        IDbTransaction BeginContractActionTransaction();
        Task<ContractAction> FindByProcessingAsync(long processingId, ProcessingType processingType);
        DateTime FindLastDateInterestAccrualOnOverdue(DateTime nextPaymentDate, DateTime accrualDate, int contractId);
        Task<List<int>> GetRelatedContractActionsByOrder(CashOrder order);
        Task<List<int>> GetRelatedContractActionsByActionId(int actionId);
        Task<List<ContractAction>> GetContractActionsByContractId(int contractId);
        Task<IEnumerable<ContractAction>> GetByContractIdAndDates(int contractId, DateTime startDate, DateTime endDate);
        Task<bool> IncopleteActionExists(int contractId);
        Task<List<ContractAction>> GetAllAwaitingForApproveActions();
        Task<List<ContractAction>> GetAllAwaitingForCancelActions();
        Task<ContractAction> GetSignAction(List<int> relatedActions);
    }
}
