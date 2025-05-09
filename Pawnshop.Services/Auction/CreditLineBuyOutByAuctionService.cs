using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Pawnshop.AccountingCore.Models;
using Pawnshop.Core;
using Pawnshop.Core.Exceptions;
using Pawnshop.Core.Extensions;
using Pawnshop.Data.Access;
using Pawnshop.Data.Access.Auction.Interfaces;
using Pawnshop.Data.Access.Interfaces;
using Pawnshop.Data.Models;
using Pawnshop.Data.Models.Auction;
using Pawnshop.Data.Models.CashOrders;
using Pawnshop.Data.Models.Collection;
using Pawnshop.Data.Models.Contracts;
using Pawnshop.Data.Models.Contracts.Actions;
using Pawnshop.Data.Models.Contracts.Expenses;
using Pawnshop.Data.Models.Dictionaries;
using Pawnshop.Data.Models.Membership;
using Pawnshop.Services.AccountingCore;
using Pawnshop.Services.Auction.Interfaces;
using Pawnshop.Services.Calculation;
using Pawnshop.Services.Collection;
using Pawnshop.Services.Contracts;
using Pawnshop.Services.CreditLines;
using Pawnshop.Services.CreditLines.Buyout;
using Pawnshop.Services.Expenses;

namespace Pawnshop.Services.Auction
{
    public class CreditLineBuyOutByAuctionService : ICreditLineBuyOutByAuctionService
    {
        private readonly IAuctionRepository _auctionRepository;
        private readonly IBusinessOperationService _businessOperationService;
        private readonly IContractActionService _contractActionService;
        private readonly ISessionContext _sessionContext;
        private readonly GroupRepository _groupRepository;
        private readonly IContractService _contractService;
        private readonly ICreditLineService _creditLineService;
        private readonly ICreditLinesBuyoutService _creditLinesBuyoutService;
        private readonly AccountRepository _accountRepository;
        private readonly PayTypeRepository _payTypeRepository;
        private readonly ICollectionService _collectionService;
        private readonly ContractExpenseRepository _contractExpenseRepository;
        private readonly CashOrderRepository _cashOrderRepository;
        private readonly IAuctionPaymentRepository _auctionPaymentRepository;
        private readonly IContractActionRowBuilder _contractActionRowBuilder;
        private readonly IContractActionPrepaymentService _contractActionPrepaymentService;
        private readonly IDeleteExpenseService _deleteExpenseService;
        private readonly IAuctionContractExpenseRepository _auctionContractExpenseRepository;

        private const int BalanceBoId = 177;
        private const int OffBalanceBoId = 63;
        private const int ProfitOffBalanceLegalWriteOff = 249;
        private const int OverdueProfitOffBalanceLegalWriteOff = 248;
        private const int PenyAccountOffBalanceLegalWriteOff = 252;
        private const int PenyProfitOffBalanceLegalWriteOff = 253;
        private const int OverdueAccountWriteOff = 484;
        private const int AccountWriteOff = 485;
        private const int ProfitWriteOff = 483;
        private const int OverdueProfitWriteOff = 482;
        private const int PenyAccountWriteOff = 486;
        private const int PenyProfitWriteOff = 487;
        private ContractAction _parentAction;
        private readonly List<int> _contractActionIds = new List<int>();

        public CreditLineBuyOutByAuctionService(
            IAuctionRepository auctionRepository,
            IBusinessOperationService businessOperationService,
            IContractActionService contractActionService,
            ISessionContext sessionContext,
            GroupRepository groupRepository,
            IContractService contractService,
            ICreditLineService creditLineService,
            ICreditLinesBuyoutService creditLinesBuyoutService,
            AccountRepository accountRepository,
            PayTypeRepository payTypeRepository,
            ICollectionService collectionService,
            ContractExpenseRepository contractExpenseRepository,
            CashOrderRepository cashOrderRepository,
            IAuctionPaymentRepository auctionPaymentRepository,
            IContractActionRowBuilder contractActionRowBuilder,
            IContractActionPrepaymentService contractActionPrepaymentService,
            IDeleteExpenseService deleteExpenseService,
            IAuctionContractExpenseRepository auctionContractExpenseRepository)
        {
            _auctionRepository = auctionRepository;
            _businessOperationService = businessOperationService;
            _contractActionService = contractActionService;
            _sessionContext = sessionContext;
            _groupRepository = groupRepository;
            _contractService = contractService;
            _creditLineService = creditLineService;
            _creditLinesBuyoutService = creditLinesBuyoutService;
            _accountRepository = accountRepository;
            _payTypeRepository = payTypeRepository;
            _collectionService = collectionService;
            _contractExpenseRepository = contractExpenseRepository;
            _cashOrderRepository = cashOrderRepository;
            _auctionPaymentRepository = auctionPaymentRepository;
            _contractActionRowBuilder = contractActionRowBuilder;
            _contractActionPrepaymentService = contractActionPrepaymentService;
            _deleteExpenseService = deleteExpenseService;
            _auctionContractExpenseRepository = auctionContractExpenseRepository;
        }

        public async Task BuyoutByAuctionAsync(int creditLineId)
        {
            var contract = await _contractService.GetAsync(creditLineId);
            if (contract == null)
            {
                throw new PawnshopApplicationException("Договор не найден");
            }

            await CheckOperationConditions(contract);
            var auction = await GetAuction(creditLineId);
            var basKenseBranch = await GetBranch(Constants.BKS);
            var tranches = await GetTranches(creditLineId);
            var payType = await GetPayType("CASH");

            var buyOutDate = DateTime.Now;

            using IDbTransaction transaction = _groupRepository.BeginTransaction();
            
            await DeleteExpensesIfExists(contract, basKenseBranch);
            await MakePrepaymentTransactions(contract, tranches, auction.Cost, basKenseBranch, payType, buyOutDate);
            await MakeWithdraws(auction.WithdrawCost, tranches, payType, buyOutDate);

            for (var index = 0; index < tranches.Count; index++)
            {
                var tranche = tranches[index];
                bool singleOrLastContract = tranches.Count == 1 || index == tranches.Count - 1;

                await PayOffTranche(
                    creditLineId,
                    buyoutBranchId: basKenseBranch.Id,
                    tranche,
                    buyOutDate,
                    payType,
                    forceExpensePrepaymentReturn: singleOrLastContract);
            }

            await CreateAuctionPayments(auction);

            transaction.Commit();
        }

        private async Task CheckOperationConditions(Contract contract)
        {
            if (contract.ContractClass != ContractClass.CreditLine)
            {
                throw new PawnshopApplicationException("Можно выкупить только кредитную линию!");
            }

            var incompleteExists = await _contractActionService.IncopleteActionExists(contract.Id);
            if (incompleteExists)
            {
                throw new PawnshopApplicationException("В договоре имеется неподтвержденное действие");
            }
        }

        private async Task<Group> GetBranch(string name)
        {
            Group branch = await _groupRepository.FindAsync(new { Name = name });
            if (branch is null)
            {
                throw new PawnshopApplicationException($"Филиал {Constants.BKS} не найден!");
            }

            return branch;
        }

        private async Task<PayType> GetPayType(string payTypeOperationCode)
        {
            var payType = await _payTypeRepository.GetByOperationCode(payTypeOperationCode);
            if (payType is null)
            {
                throw new PawnshopApplicationException($"\"PayType\" с кодом {payTypeOperationCode} не найден!");
            }

            return payType;
        }

        private async Task<CarAuction> GetAuction(int creditLineId)
        {
            var auction = await _auctionRepository.GetByContractIdAsync(creditLineId);
            if (auction is null)
            {
                throw new PawnshopApplicationException("Данные по аукциону для договора не найдены!");
            }

            return auction;
        }

        private async Task<List<Contract>> GetTranches(int creditLineId)
        {
            var tranches = await _contractService.GetAllTranchesAsync(creditLineId);
            if (tranches.IsNullOrEmpty())
            {
                throw new PawnshopApplicationException("Траншы КЛ не найдены");
            }

            return tranches.Where(t => t.Status == ContractStatus.Signed).ToList();
        }

        private async Task MakePrepaymentTransactions(Contract contract, List<Contract> tranches, decimal amount,
            Group remittanceBranch, PayType payType, DateTime date)
        {
            var prepaymentAction = new ContractAction
            {
                ActionType = ContractActionType.Prepayment,
                AuthorId = _sessionContext.UserId,
                ContractId = contract.Id,
                Cost = amount,
                TotalCost = amount,
                Rows = new ContractActionRow[] { },
                Reason = "Выкуп по аукциону",
                Date = DateTime.Today,
                CreateDate = date,
                PayTypeId = payType.Id,
                Data = new ContractActionData(),
                EmployeeId = _sessionContext.UserId,
                CategoryChanged = false,
                BuyoutCreditLine = false
            };

            _contractActionService.Save(prepaymentAction);

            var ordersWithRecords = await _businessOperationService.ExecuteRegistrationAsync(
                date: prepaymentAction.Date,
                businessOperationCode: Constants.AUCTION_CARTAS_PAYMENT_BOS_CODE,
                authorId: prepaymentAction.AuthorId,
                amounts: new Dictionary<AmountType, decimal> { { AmountType.Prepayment, amount } },
                typeCode: Constants.TYPE_HIERARCHY_CONTRACTS_ALL,
                remittanceBranchId: remittanceBranch.Id,
                contractActionId: prepaymentAction.Id,
                contract: contract,
                orderStatus: OrderStatus.Approved,
                orderUserId: Constants.AUCTION_KONTR_AGENT_USER_ID,
                note: Constants.WITHDRAW_FUNDS_TO_CARTAS_NOTE,
                clientId: contract.ClientId);

            _parentAction = prepaymentAction;
            _contractActionIds.Add(prepaymentAction.Id);
            
            await TransferDepoFromTrancheToCreditLine(contract.Id, tranches, remittanceBranch, date);
        }
        
        private async Task TransferDepoFromTrancheToCreditLine(int creditLineId, List<Contract> tranches, Group transferGroup,
            DateTime transferDate)
        {
            foreach (var tranche in tranches)
            {
                var depoAccount = await _accountRepository.GetByAccountSettingCodeAsync(Constants.ACCOUNT_SETTING_DEPO, tranche.Id);

                if (depoAccount == null || depoAccount.Balance <= 0) continue;
                var prepaymentModel = new MovePrepayment
                {
                    SourceContractId = tranche.Id,
                    Date = transferDate,
                    Amount = depoAccount.Balance,
                    RecipientContractId = creditLineId,
                    Note = "Перемещение дс со счёта транша на счёт кредитной линии для выкупа транша КЛ по аукциону"
                };
                
                var transferAction = _contractActionPrepaymentService
                    .MovePrepayment(prepaymentModel, _sessionContext.UserId, transferGroup, _parentAction);

                UpdateActionParentChild(transferAction);
                _contractActionIds.Add(transferAction.Id);
            }
        }

        private async Task PayExpenses(int creditLineId, int branchId, decimal amount, DateTime date, PayType payType, bool autoApprove)
        {
            if (amount <= 0) return;

            var expenseAction = _creditLineService.PayExtraExpenses(
                creditLineId,
                amount,
                date.Date,
                payType.Id,
                authorId: _sessionContext.UserId,
                branchId,
                autoApprove);

            if (expenseAction != null)
            {
                UpdateActionParentChild(expenseAction);
            }
        }

        private async Task<decimal> ProcessWithdrawsAsync(
            IEnumerable<int> trancheIds,
            string accountSettingCode,
            int boId,
            int bosId,
            string reason,
            decimal availableOperationSum,
            PayType payType,
            DateTime date,
            int? debitAccountId = null)
        {
            var accounts = await _accountRepository
                .GetMultipleBySettingCodeAsync(trancheIds, accountSettingCode);

            foreach (var account in accounts)
            {
                var withdraw = CalculateWithdrawsAmountService.CalculateSum(ref availableOperationSum, account.Balance);
                if (withdraw.ToWithdraw <= 0 || account.Balance == 0) continue;

                if (availableOperationSum < 0)
                {
                    availableOperationSum = Math.Abs(account.Balance);
                }
                
                var withdrawAction = new ContractAction
                {
                    ActionType = ContractActionType.WithdrawByAuction,
                    AuthorId = _sessionContext.UserId,
                    ContractId = (int)account.ContractId,
                    Cost = withdraw.ToWithdraw,
                    TotalCost = withdraw.ToWithdraw,
                    Rows = new ContractActionRow[] {},
                    Reason = reason,
                    Date = DateTime.Today,
                    CreateDate = date,
                    PayTypeId = payType.Id,
                    Data = new ContractActionData(),
                    EmployeeId = _sessionContext.UserId,
                    CategoryChanged = false,
                    BuyoutCreditLine = false,
                    BuyoutReasonId = 115
                };

                _contractActionService.Save(withdrawAction);

                var cashOrderId = await _auctionRepository.WithdrawAsync(
                    contractId: (int)account.ContractId,
                    transactionDate: DateTime.Now,
                    amount: withdraw.ToWithdraw,
                    boId: boId,
                    bosId: bosId,
                    creditAccountId: account.Id,
                    reason: reason,
                    actionId: withdrawAction.Id,
                    debitAccountId: debitAccountId);

                UpdateActionParentChild(withdrawAction);
                _contractActionIds.Add(withdrawAction.Id);
            }

            return availableOperationSum;
        }

        private async Task MakeWithdraws(decimal buyoutSum, List<Contract> tranches, PayType payType, DateTime date)
        {
            Account provisionAccount = await _accountRepository.GetConsolidatedAccountBySettingCodeAsync(Constants.PROVISIONS);
            if (provisionAccount is null)
            {
                throw new PawnshopApplicationException("Счёт провизий не найден!");
            }
            
            if (buyoutSum <= 0) return;
            
            var trancheIds = tranches.Select(t => t.Id).ToList();

            var availableOperationSum = buyoutSum;
            
            availableOperationSum = await ProcessWithdrawsAsync(
                trancheIds,
                accountSettingCode: Constants.ACCOUNT_SETTING_OVERDUE_ACCOUNT,
                boId: BalanceBoId,
                bosId: OverdueAccountWriteOff,
                reason: "Списание просроченного основного долга",
                availableOperationSum,
                payType,
                date,
                debitAccountId: provisionAccount.Id);
            
            availableOperationSum = await ProcessWithdrawsAsync(
                trancheIds,
                accountSettingCode: Constants.ACCOUNT_SETTING_ACCOUNT,
                boId: BalanceBoId,
                bosId: AccountWriteOff,
                reason: "Списание основного долга",
                availableOperationSum,
                payType,
                date,
                debitAccountId: provisionAccount.Id);
            
            availableOperationSum = await ProcessWithdrawsAsync(
                trancheIds,
                accountSettingCode: Constants.OVERDUE_PROFIT_OFFBALANCE,
                boId: BalanceBoId,
                bosId: OverdueProfitOffBalanceLegalWriteOff,
                reason: "Списание просроченных процентов",
                availableOperationSum,
                payType,
                date);

            availableOperationSum = await ProcessWithdrawsAsync(
                trancheIds,
                accountSettingCode: Constants.PROFIT_OFFBALANCE,
                boId: BalanceBoId,
                bosId: ProfitOffBalanceLegalWriteOff,
                reason: "Списание начисленных процентов",
                availableOperationSum,
                payType,
                date);

            availableOperationSum = await ProcessWithdrawsAsync(
                trancheIds,
                accountSettingCode: Constants.PENY_ACCOUNT_OFFBALANCE,
                boId: BalanceBoId,
                bosId: PenyAccountOffBalanceLegalWriteOff,
                reason: "Списание пени на просроченный ОД",
                availableOperationSum,
                payType,
                date);

            availableOperationSum = await ProcessWithdrawsAsync(
                trancheIds,
                accountSettingCode: Constants.PENY_PROFIT_OFFBALANCE,
                boId: BalanceBoId,
                bosId: PenyProfitOffBalanceLegalWriteOff,
                reason: "Списание пени на просроченные проценты",
                availableOperationSum,
                payType,
                date);

            availableOperationSum = await ProcessWithdrawsAsync(
                trancheIds,
                accountSettingCode: Constants.ACCOUNT_SETTING_PROFIT,
                boId: OffBalanceBoId,
                bosId: ProfitWriteOff,
                reason: "Списание начисленных процентов",
                availableOperationSum,
                payType,
                date,
                debitAccountId: provisionAccount.Id);

            availableOperationSum = await ProcessWithdrawsAsync(
                trancheIds,
                accountSettingCode: Constants.ACCOUNT_SETTING_OVERDUE_PROFIT,
                boId: OffBalanceBoId,
                bosId: OverdueProfitWriteOff,
                reason: "Списание просроченных процентов",
                availableOperationSum,
                payType,
                date,
                debitAccountId: provisionAccount.Id);

            availableOperationSum = await ProcessWithdrawsAsync(
                trancheIds,
                accountSettingCode: Constants.ACCOUNT_SETTING_PENY_ACCOUNT,
                boId: OffBalanceBoId,
                bosId: PenyAccountWriteOff,
                reason: "Списание пени на просроченный ОД",
                availableOperationSum,
                payType,
                date);

            availableOperationSum = await ProcessWithdrawsAsync(
                trancheIds,
                accountSettingCode: Constants.ACCOUNT_SETTING_PENY_PROFIT,
                boId: OffBalanceBoId,
                bosId: PenyProfitWriteOff,
                reason: "Списание пени на просроченные проценты",
                availableOperationSum,
                payType,
                date);
        }
        
        private async Task DeleteExpensesIfExists(Contract contract, Group branch)
        {
            var unpaidExpenses = await _contractExpenseRepository.GetUnpaidExpensesAsync(contract.Id);
            if (unpaidExpenses.IsNullOrEmpty()) return;
            foreach (var expense in unpaidExpenses)
            {
                await _deleteExpenseService.DeleteExpenseWithRecalculation(expense.Id, branch.Id);
                await _auctionContractExpenseRepository.InsertAsync(new AuctionContractExpense
                {
                    ContractId = contract.Id,
                    ContractExpenseId = expense.Id,
                    AuthorName = _sessionContext.UserName
                });
            }
        }
        
        private async Task PayOffTranche(int creditLineId, int buyoutBranchId, Contract tranche, DateTime buyOutDate,
            PayType payType, bool forceExpensePrepaymentReturn = false)
        {
            var buyOutSum = await CalculateTrancheBuyOutSum(tranche, buyOutDate);
            if (buyOutSum > 0)
            {
                var movePrepaymentAction = _creditLineService.MovePrepayment(
                    creditLineId: creditLineId,
                    contractId: tranche.Id,
                    value: Math.Abs(buyOutSum),
                    authorId: _sessionContext.UserId,
                    action: _parentAction,
                    autoApprove: true,
                    branchId: buyoutBranchId,
                    date: buyOutDate);

                UpdateActionParentChild(movePrepaymentAction);
                _contractActionIds.Add(movePrepaymentAction.Id);
            }

            var buyoutActions = await _creditLinesBuyoutService.BuyOut(
                contractId: tranche.Id,
                creditLineId: creditLineId,
                payTypeId: payType.Id,
                parentAction: _parentAction,
                authorId: _sessionContext.UserId,
                branchId: buyoutBranchId,
                buyoutReasonId: 115,
                autoApprove: false,
                buyoutCreditLine: true,
                forceBuyOutCreditLine: forceExpensePrepaymentReturn,
                date: buyOutDate);

            foreach (var action in buyoutActions.Where(action => action != null).OrderBy(action => action.Id))
            {
                UpdateActionParentChild(action);
                CloseCollection(tranche.Id, action.Id);
                _contractActionIds.Add(action.Id);
            }
        }
        
        private async Task<decimal> CalculateTrancheBuyOutSum(Contract tranche, DateTime buyOutDate)
        {
            _contractActionRowBuilder.Init(tranche, buyOutDate, ContractActionType.Buyout);
            var buyOutSum = _contractActionRowBuilder.BuyoutAmount;
            var depoAccount = await _accountRepository.GetByAccountSettingCodeAsync(Constants.ACCOUNT_SETTING_DEPO, tranche.Id);

            if (depoAccount is null)
            {
                throw new PawnshopApplicationException($"Счёт аванса для договора: {tranche.ContractNumber} не найден");
            }

            if (buyOutSum > 0 && depoAccount.Balance > 0)
            {
                buyOutSum -= depoAccount.Balance;
            }

            return buyOutSum;
        }

        private void UpdateActionParentChild(ContractAction action)
        {
            _parentAction.ChildActionId = action.Id;
            _contractActionService.Save(_parentAction);

            action.ParentActionId = _parentAction.Id;
            _contractActionService.Save(action);

            _parentAction = action;
        }

        private void CloseCollection(int trancheId, int contractActionId)
        {
            _collectionService.CloseContractCollection(new CollectionClose
            {
                ContractId = trancheId,
                ActionId = contractActionId
            });
        }

        private async Task CreateAuctionPayments(CarAuction auction)
        {
            if (_contractActionIds.IsNullOrEmpty()) return;

            var auctionPayments = new List<AuctionPayment>();
            var operationCashOrders = await _cashOrderRepository.GetMultipleByActionIds(_contractActionIds);

            if (operationCashOrders != null && operationCashOrders.Any())
            {
                auctionPayments.AddRange(operationCashOrders.Select(order => new AuctionPayment
                {
                    RequestId = auction.OrderRequestId,
                    AuthorId = _sessionContext.UserId,
                    CashOrderId = order.Id,
                    CreateDate = DateTime.Now
                }));
            }

            if (auctionPayments.Any())
            {
                await _auctionPaymentRepository.InsertMultipleAsync(auctionPayments);
            }
        }
    }
}