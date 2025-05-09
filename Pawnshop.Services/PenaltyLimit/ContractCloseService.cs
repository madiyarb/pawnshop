using System;
using System.Collections.Generic;
using Pawnshop.AccountingCore.Models;
using Pawnshop.Core;
using Pawnshop.Core.Exceptions;
using Pawnshop.Data.Models.CashOrders;
using Pawnshop.Data.Models.Contracts;
using Pawnshop.Data.Models.Contracts.Actions;
using Pawnshop.Data.Models.Membership;
using Pawnshop.Services.AccountingCore;
using Pawnshop.Services.Contracts;
using Pawnshop.Services.Models.Filters;

namespace Pawnshop.Services.PenaltyLimit
{
    public class ContractCloseService : IContractCloseService
    {
        private readonly IContractService _contractService;
        private readonly IContractActionOperationService _contractActionOperationService;
        private readonly IBusinessOperationService _businessOperationService;
        private readonly IContractActionService _contractActionService;
        private readonly IDictionaryWithSearchService<Group, BranchFilter> _branchService;

        public ContractCloseService(
            IContractService contractService, 
            IContractActionOperationService contractActionOperationService, 
            IBusinessOperationService businessOperationService, 
            IContractActionService contractActionService, 
            IDictionaryWithSearchService<Group, BranchFilter> branchService)
        {
            _contractService = contractService;
            _contractActionOperationService = contractActionOperationService;
            _businessOperationService = businessOperationService;
            _contractActionService = contractActionService;
            _branchService = branchService;
        }

        public ContractAction Exec(Contract contract, DateTime date, int authorId, ContractAction childAction = null, OrderStatus? orderStatus = null)
        {
            if (contract is null)
                throw new PawnshopApplicationException("Договор не найден");

            var penaltyLimitBalance = _contractService.GetPenaltyLimitBalance(contract.Id, date);

            ContractAction newAction = null;

            if (penaltyLimitBalance > 0)
                newAction = PenaltyLimitWriteOffSwapChildAndParentAction(contract, date, authorId, penaltyLimitBalance, childAction, orderStatus);

            return newAction;
        }


        public ContractAction CloseContractByCreditLine(Contract contract, DateTime date, int authorId, ContractAction childAction = null, OrderStatus? orderStatus = null)
        {
            if (contract is null)
                throw new PawnshopApplicationException("Договор не найден");

            var penaltyLimitBalance = _contractService.GetPenaltyLimitBalance(contract.Id, date);

            ContractAction newAction = null;

            if (penaltyLimitBalance > 0)
                newAction = PenaltyLimitWriteOffSwapChildAndParentAction(contract, date, authorId, penaltyLimitBalance, childAction, orderStatus);

            return newAction;
        }

        private ContractAction PenaltyLimitWriteOffSwapChildAndParentAction(Contract contract, DateTime date, int authorId, decimal penaltyLimitBalance, ContractAction childAction = null, OrderStatus? orderStatus = null)
        {
            var newAction = new ContractAction()
            {
                ActionType = ContractActionType.PenaltyLimitWriteOff,
                AuthorId = authorId,
                ContractId = contract.Id,
                TotalCost = penaltyLimitBalance,
                Cost = penaltyLimitBalance,
                Reason = $"Списание лимита пени при закрытии договора",
                Date = date,
                CreateDate = DateTime.Now,
                Note = childAction?.Note,
                ParentActionId = childAction?.Id
            };

            var amountDict = new Dictionary<AmountType, decimal>()
            {
                {
                    AmountType.PenaltyLimit, penaltyLimitBalance
                }
            };

            var branch = _branchService.GetAsync(contract.BranchId).Result;

            _contractActionService.Save(newAction);
            _businessOperationService.Register(contract, date, Constants.BO_PENALTY_LIMIT_WRITEOFF, branch, authorId, amountDict, action: newAction, orderStatus: orderStatus);
            _contractActionOperationService.Register(contract, newAction, authorId, branch.Id, false);

            childAction.ChildActionId = newAction.Id;
            _contractActionService.Save(childAction);

            return newAction;
        }


        private ContractAction PenaltyLimitWriteOff(Contract contract, DateTime date, int authorId, decimal penaltyLimitBalance, ContractAction childAction = null, OrderStatus? orderStatus = null)
        {
            var newAction = new ContractAction()
            {
                ActionType = ContractActionType.PenaltyLimitWriteOff,
                AuthorId = authorId,
                ContractId = contract.Id,
                TotalCost = penaltyLimitBalance,
                Cost = penaltyLimitBalance,
                Reason = $"Списание лимита пени при закрытии договора",
                Date = date,
                CreateDate = DateTime.Now,
                Note = childAction?.Note,
                ChildActionId = childAction?.Id
            };

            var amountDict = new Dictionary<AmountType, decimal>()
            {
                {
                    AmountType.PenaltyLimit, penaltyLimitBalance
                }
            };

            var branch = _branchService.GetAsync(contract.BranchId).Result;

            _contractActionService.Save(newAction);
            _businessOperationService.Register(contract, date, Constants.BO_PENALTY_LIMIT_WRITEOFF, branch, authorId, amountDict, action: newAction, orderStatus: orderStatus);
            _contractActionOperationService.Register(contract, newAction, authorId, branch.Id, false);

            childAction.ParentActionId = newAction.Id;
            _contractActionService.Save(childAction);

            return newAction;
        }
    }
}