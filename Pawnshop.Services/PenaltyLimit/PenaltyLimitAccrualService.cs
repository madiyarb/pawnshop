using System;
using System.Collections.Generic;
using Pawnshop.AccountingCore.Models;
using Pawnshop.Core;
using Pawnshop.Core.Exceptions;
using Pawnshop.Data.Models.Audit;
using Pawnshop.Data.Models.Contracts;
using Pawnshop.Data.Models.Contracts.Actions;
using Pawnshop.Data.Models.Membership;
using Pawnshop.Services.AccountingCore;
using Pawnshop.Services.ClientDeferments.Impl;
using Pawnshop.Services.ClientDeferments.Interfaces;
using Pawnshop.Services.Contracts;
using Pawnshop.Services.Models.Contracts;
using Pawnshop.Services.Models.Filters;

namespace Pawnshop.Services.PenaltyLimit
{
    public class PenaltyLimitAccrualService : IPenaltyLimitAccrualService
    {
        private readonly IContractService _contractService;
        private readonly IContractActionOperationService _contractActionOperationService;
        private readonly IContractActionService _contractActionService;
        private readonly IBusinessOperationService _businessOperationService;
        private readonly IEventLog _eventLog;
        private readonly IDictionaryWithSearchService<Group, BranchFilter> _branchService;
        private readonly IClientDefermentService _clientDefermentService;

        public PenaltyLimitAccrualService(
            IContractService contractService,
            IContractActionOperationService contractActionOperationService,
            IContractActionService contractActionService,
            IBusinessOperationService businessOperationService,
            IEventLog eventLog,
            IDictionaryWithSearchService<Group, BranchFilter> branchService,
            IClientDefermentService clientDefermentService)
        {
            _contractService = contractService;
            _contractActionOperationService = contractActionOperationService;
            _contractActionService = contractActionService;
            _businessOperationService = businessOperationService;
            _eventLog = eventLog;
            _branchService = branchService;
            _clientDefermentService = clientDefermentService;
        }

        public void Execute(Contract contract, DateTime date, int authorId)
        {
            if (contract is null)
                throw new PawnshopApplicationException($"Ожидалось что {nameof(contract)} будет не null");

            var defermentInformation = _clientDefermentService.GetDefermentInformation(contract.Id, date);
            if (defermentInformation != null &&
                ((defermentInformation.IsInDefermentPeriod && defermentInformation.Status == Data.Models.Restructuring.RestructuringStatusEnum.Restructured) ||
                defermentInformation.Status == Data.Models.Restructuring.RestructuringStatusEnum.Frozen))
            {
                throw new PawnshopApplicationException($"Договор в статусе {Data.Models.Restructuring.RestructuringStatusEnum.Frozen} или находится в отсроченном периоде, действие недоступно");
            }

            var penaltyLimitBalance = _contractService.GetPenaltyLimitBalance(contract.Id, date);

            decimal amount = Math.Round(contract.LoanCost * 0.1M - penaltyLimitBalance, 2);
            
            if (amount > 0 )
                PenaltyLimitAccrual(contract, date, authorId, amount);
        }

        public void Execute(Contract contract, Contract parentContract, DateTime date, int authorId)
        {
            if (contract is null)
                throw new PawnshopApplicationException($"Ожидалось что {nameof(contract)} будет не null");

            var defermentInformation = _clientDefermentService.GetDefermentInformation(contract.Id, date);
            if (defermentInformation != null &&
                ((defermentInformation.IsInDefermentPeriod && defermentInformation.Status == Data.Models.Restructuring.RestructuringStatusEnum.Restructured) ||
                defermentInformation.Status == Data.Models.Restructuring.RestructuringStatusEnum.Frozen))
            {
                throw new PawnshopApplicationException($"Договор в статусе {Data.Models.Restructuring.RestructuringStatusEnum.Frozen} или находится в отсроченном периоде");
            }

            var penaltyLimitBalance = _contractService.GetPenaltyLimitBalance(contract.Id, date);

            decimal amount = Math.Round(parentContract.LoanCost * 0.1M - penaltyLimitBalance, 2);
            
            if (amount > 0)
                PenaltyLimitAccrual(contract, date, authorId, amount);
        }

        private void PenaltyLimitAccrual(Contract contract, DateTime date, int authorId, decimal amount)
        {
            var amountDict = new Dictionary<AmountType, decimal>()
            {
                {AmountType.PenaltyLimit, amount}
            };

            var penaltyLimitAccrualAction = new ContractAction
            {
                ActionType = ContractActionType.PenaltyLimitAccrual,
                AuthorId = authorId,
                ContractId = contract.Id,
                TotalCost = amount,
                Cost = amount,
                Reason = $"Начисление лимита пени по договору {contract.ContractNumber} от {date:dd.MM.yyyy}",
                Date = date,
                CreateDate = DateTime.Now
            };

            var branch = _branchService.GetAsync(contract.BranchId).Result;

            try
            {
                using (var transaction = _contractActionService.BeginContractActionTransaction())
                {
                    _contractActionService.Save(penaltyLimitAccrualAction);
                    _businessOperationService.Register(contract, date, Constants.BO_PENALTY_LIMIT_ACCRUAL, branch, authorId, amountDict, action: penaltyLimitAccrualAction);
                    _contractActionOperationService.Register(contract, penaltyLimitAccrualAction, authorId, branch.Id, false);

                    transaction.Commit();
                }

                _eventLog.Log(EventCode.ContractPenaltyLimitAccrual, EventStatus.Success, EntityType.Contract, contract.Id, $"Начисление лимита пени, сумма = {amount}", userId: authorId);
            }
            catch (Exception e)
            {
                _eventLog.Log(EventCode.ContractPenaltyLimitAccrual, EventStatus.Failed, EntityType.Contract, contract.Id, $"Начисление лимита пени, сумма = {amount}", e.StackTrace, userId: authorId);
                throw;
            }
        }

        public void ManualPenaltyLimitAccrual(Contract contract, DateTime date, int authorId)
        {
            if (contract.Status != ContractStatus.Signed && contract.Status != ContractStatus.SoldOut)
                throw new PawnshopApplicationException("Договор должен быть подписан или отправлен на реализацию");

            if (!contract.UsePenaltyLimit)
                throw new PawnshopApplicationException($"Для договора {contract.ContractNumber} нельзя начислить лимит пени");

            if (date.Date < contract.ContractDate)
                throw new PawnshopApplicationException($"Дата начисления должна быть после даты подписания договора");

            if (date.Date < Constants.PENY_LIMIT_DATE)
                throw new PawnshopApplicationException($"Дата начисления должна быть после {Constants.PENY_LIMIT_DATE:dd.MM.yyyy}");

            if (contract.ParentId.HasValue)
            {
                var parentContract = _contractService.Get(contract.ParentId.Value);

                if (date.Day != parentContract.ContractDate.Day || date.Month != parentContract.ContractDate.Month)
                    throw new PawnshopApplicationException($"Дата начисления должна быть в день и месяц создания родительского договора");

                Execute(contract, parentContract, date, authorId);
            }
            else
            {
                if (date.Day != contract.ContractDate.Day || date.Month != contract.ContractDate.Month)
                    throw new PawnshopApplicationException($"Дата начисления должна быть в день и месяц создания договора");

                Execute(contract, date, authorId);
            }
        }
    }
}