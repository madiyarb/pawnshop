using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Pawnshop.Core.Queries;
using Pawnshop.Data.Access;
using Pawnshop.Data.Models.CashOrders;
using Pawnshop.Data.Models.Contracts.Actions;
using Pawnshop.Services.Models.Filters;
using Pawnshop.Services.Models.List;

namespace Pawnshop.Services.Contracts
{
    public class ContractActionService : IContractActionService
    {
        private readonly ContractActionRepository _repository;
        private readonly ContractActionRowRepository _actionRowRepository;

        public ContractActionService(ContractActionRepository repository, ContractActionRowRepository actionRowRepository)
        {
            _repository = repository;
            _actionRowRepository = actionRowRepository;
        }

        public IDbTransaction BeginContractActionTransaction()
        {
            return _repository.BeginTransaction();
        }

        public ContractAction Save(ContractAction model)
        {
            using (var transaction = BeginContractActionTransaction())
            {
                if (model.Id > 0)
                    _repository.Update(model);
                else
                    _repository.Insert(model);

                transaction.Commit();
            }

            return model;
        }

        public void Delete(int id) => _repository.Delete(id);

        public Task<ContractAction> GetAsync(int id) => Task.Run(() => _repository.Get(id));

        public ListModel<ContractAction> List(ListQuery listQuery)
        {
            return new ListModel<ContractAction>
            {
                Count = _repository.Count(listQuery),
                List = _repository.List(listQuery)
            };
        }

        public ListModel<ContractAction> List(ListQueryModel<ContractActionFilter> listQuery)
        {
            return new ListModel<ContractAction>
            {
                Count = _repository.Count(listQuery, listQuery.Model),
                List = _repository.List(listQuery, listQuery.Model)
            };
        }

        public Task<ContractAction> FindByProcessingAsync(long processingId, ProcessingType processingType) => Task.Run(() => _repository.FindByProcessing(processingId, processingType));

        public ContractActionRow SaveRow(ContractActionRow model)
        {
            if (model.Id > 0)
                _actionRowRepository.Update(model);
            else
                _actionRowRepository.Insert(model);

            return model;
        }

        public DateTime FindLastDateInterestAccrualOnOverdue(DateTime nextPaymentDate, DateTime accrualDate, int contractId) =>
            _repository.FindLastDateInterestAccrualOnOverdue(nextPaymentDate, accrualDate, contractId);

        public async Task<List<ContractAction>> GetContractActionsByContractId(int contractId)
        {
            return await _repository.GetByContractId(contractId);
        }

        public async Task<IEnumerable<ContractAction>> GetByContractIdAndDates(int contractId, DateTime startDate, DateTime endDate)
        {
            return await _repository.GetByContractIdAndDates(contractId, startDate, endDate);
        }

        public async Task<List<int>> GetRelatedContractActionsByOrder(CashOrder order)
        {
            var list = new List<int>();
            if (order.ContractActionId.HasValue)
            {
                list.Add(order.ContractActionId.Value);
                parents = new List<int>();
                list.AddRange(await GetParentContractAction(await GetAsync(order.ContractActionId.Value)));

                children = new List<int>();
                list.AddRange(await GetChildContractAction(await GetAsync(order.ContractActionId.Value)));
            }
            return list.Distinct().ToList();
        }
        private List<int> parents;
        private List<int> children;

        public async Task<List<int>> GetRelatedContractActionsByActionId(int actionId)
        {
            var list = new List<int>();

            list.Add(actionId);
            parents = new List<int>();
            list.AddRange(await GetParentContractAction(await GetAsync(actionId)));

            children = new List<int>();
            list.AddRange(await GetChildContractAction(await GetAsync(actionId)));
            return list.Distinct().ToList();
        }

        private async Task<List<int>> GetParentContractAction(ContractAction contractAction)
        {
            if (contractAction.ParentActionId.HasValue)
            {
                parents.Add(contractAction.ParentActionId.Value);
                await GetParentContractAction(await GetAsync(contractAction.ParentActionId.Value));
            }
            var bychilds = await GetByChildActionId(contractAction.Id);
            var byparents = await GetByParentActionId(contractAction.Id);
            parents.AddRange(byparents);
            parents.AddRange(bychilds);
            return parents;
        }

        private async Task<List<int>> GetChildContractAction(ContractAction contractAction)
        {
            if (contractAction.ChildActionId.HasValue)
            {
                children.Add(contractAction.ChildActionId.Value);
                await GetChildContractAction(await GetAsync(contractAction.ChildActionId.Value));
            }
            var bychilds = await GetByChildActionId(contractAction.Id);
            var byparents = await GetByParentActionId(contractAction.Id);
            children.AddRange(bychilds);
            children.AddRange(byparents);
            return children;
        }

        private async Task<List<int>> GetByParentActionId(int contractActionId)
        {
            var parents = await _repository.GetByParentId(contractActionId);
            return parents.Select(x => x.Id).ToList();
        }

        private async Task<List<int>> GetByChildActionId(int contractActionId)
        {
            var childs = await _repository.GetByChildId(contractActionId);
            return childs.Select(x => x.Id).ToList();
        }

        public async Task<bool> IncopleteActionExists(int contractId)
        {
            var incompleteActions = await _repository.GetIncompleteActions(contractId);
            return incompleteActions.Count > 0;
        }

        public async Task<List<ContractAction>> GetAllAwaitingForApproveActions()
        {
            return await _repository.GetAllAwaitingForApproveActions();
        }

        public async Task<List<ContractAction>> GetAllAwaitingForCancelActions()
        {
            return await _repository.GetAllAwaitingForCancelActions();
        }

        public async Task<ContractAction> GetSignAction(List<int> relatedActions)
        {
            return await _repository.GetSignAction(relatedActions);
        }
    }
}
