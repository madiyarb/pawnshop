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
using Pawnshop.Data.Models.Auction.HttpRequestModels;
using Pawnshop.Data.Models.CashOrders;
using Pawnshop.Data.Models.Contracts;
using Pawnshop.Data.Models.Contracts.Actions;
using Pawnshop.Data.Models.Contracts.Expenses;
using Pawnshop.Data.Models.Dictionaries;
using Pawnshop.Data.Models.Domains;
using Pawnshop.Data.Models.Membership;
using Pawnshop.Services.AccountingCore;
using Pawnshop.Services.Auction.Interfaces;
using Pawnshop.Services.Calculation;
using Pawnshop.Services.Contracts;
using Pawnshop.Services.Expenses;
using Pawnshop.Services.Models.Calculation;

namespace Pawnshop.Services.Auction
{
    public class ContractBuyOutByAuctionService : IContractBuyOutByAuctionService
    {
        private const int BalanceBoId = 177;
        private const int OffBalanceBoId = 63;

        private const string PayTypeCode = "CASH";
        private const string BuyOutReasonCode = "CAR_SOLD_THROUGH_AUCTION";
        private const string ExpenseTypeCode = "WITHOUT_REMOVING_ENCUMBRANCE";

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
        
        
        private readonly ContractRepository _contractRepository;
        private readonly IContractActionService _contractActionService;
        private readonly IAuctionRepository _auctionRepository;
        private readonly GroupRepository _groupRepository;
        private readonly ISessionContext _sessionContext;
        private readonly IBusinessOperationService _businessOperationService;
        private readonly IContractDutyService _contractDutyService;
        private readonly IContractActionBuyoutService _contractActionBuyoutService;
        private readonly IContractService _contractService;
        private readonly DomainRepository _domainRepository;
        private readonly PayTypeRepository _payTypeRepository;
        private readonly AccountRepository _accountRepository;
        private readonly ExpenseRepository _expenseRepository;
        private readonly IDeleteExpenseService _deleteExpenseService;
        private readonly ContractExpenseRepository _contractExpenseRepository;
        private readonly IAuctionContractExpenseRepository _auctionContractExpenseRepository;

        public ContractBuyOutByAuctionService(
            ContractRepository contractRepository,
            IContractActionService contractActionService,
            IAuctionRepository auctionRepository,
            GroupRepository groupRepository,
            ISessionContext sessionContext,
            IBusinessOperationService businessOperationService,
            IContractDutyService contractDutyService,
            IContractActionBuyoutService contractActionBuyoutService,
            IContractService contractService,
            DomainRepository domainRepository,
            PayTypeRepository payTypeRepository,
            AccountRepository accountRepository,
            ExpenseRepository expenseRepository,
            ContractExpenseRepository contractExpenseRepository,
            IDeleteExpenseService deleteExpenseService,
            IAuctionContractExpenseRepository auctionContractExpenseRepository)
        {
            _contractRepository = contractRepository;
            _contractActionService = contractActionService;
            _auctionRepository = auctionRepository;
            _groupRepository = groupRepository;
            _sessionContext = sessionContext;
            _businessOperationService = businessOperationService;
            _contractDutyService = contractDutyService;
            _contractActionBuyoutService = contractActionBuyoutService;
            _contractService = contractService;
            _domainRepository = domainRepository;
            _accountRepository = accountRepository;
            _expenseRepository = expenseRepository;
            _payTypeRepository = payTypeRepository;
            _contractExpenseRepository = contractExpenseRepository;
            _deleteExpenseService = deleteExpenseService;
            _auctionContractExpenseRepository = auctionContractExpenseRepository;
        }

        public async Task<ContractAction> BuyOut(ContractBuyOutByAuctionCommand command)
        {
            var contract = await _contractService.GetAsync(command.ContractId);
            if (contract == null)
                throw new PawnshopApplicationException("Договор не найден!");

            var auction = await GetAuction(contract);
            var basKenseBranch = await GetBranch(Constants.BKS);
            Domain buyOutReason = await GetBuyOutReason(command.BuyOutReasonCode);
            var payType = await GetPayType(command.PayTypeCode);
            var expenseType = await GetExpense(command.ExpenseCode);

            await CheckOperationConditions(contract, buyOutReason, payType, expenseType);

            var buyoutDate = DateTime.Now;

            var buyOutContractAction = new ContractAction
            {
                ActionType = ContractActionType.Buyout,
                ContractId = contract.Id,
                CreateDate = buyoutDate,
                Date = DateTime.Today,
                AuthorId = _sessionContext.UserId,
                PayTypeId = payType.Id,
                Discount = new ContractDutyDiscount(),
                Cost = 0,
                TotalCost = auction.WithdrawCost,
                Reason = buyOutReason.Name,
                BuyoutReasonId = buyOutReason.Id,
                BuyoutCreditLine = false,
                ExtraExpensesIds = new List<int>(),
                Rows = new ContractActionRow[] { },
            };

            using IDbTransaction transaction = _contractRepository.BeginTransaction();
            await DeleteExpensesIfExists(contract, basKenseBranch);

            _contractActionService.Save(buyOutContractAction);

            await MakePrepaymentTransactions(contract, auction, basKenseBranch, buyoutDate, buyOutContractAction);
            await MakeWithdraws(contract.Id, auction, buyOutReason.Id, buyOutContractAction);
            await MakePayOff(basKenseBranch.Id, contract, auction, buyOutContractAction);

            transaction.Commit();
            return buyOutContractAction;
        }
        
        private async Task DeleteExpensesIfExists(Contract contract, Group branch)
        {
            var unpaidExpenses = await _contractExpenseRepository.GetUnpaidExpensesAsync(contract.Id);
            if (!unpaidExpenses.IsNullOrEmpty())
            {
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
        }

        private async Task MakePrepaymentTransactions(
            Contract contract,
            CarAuction auction,
            Group remittanceBranch,
            DateTime date,
            ContractAction contractAction)
        {
            await _businessOperationService.ExecuteRegistrationAsync(
                date: date,
                businessOperationCode: Constants.AUCTION_CARTAS_PAYMENT_BOS_CODE,
                authorId: _sessionContext.UserId,
                amounts: new Dictionary<AmountType, decimal> { { AmountType.Prepayment, auction.Cost } },
                typeCode: Constants.TYPE_HIERARCHY_CONTRACTS_ALL,
                remittanceBranchId: remittanceBranch.Id,
                contractActionId: contractAction.Id,
                contract: contract,
                orderStatus: OrderStatus.Approved,
                orderUserId: Constants.AUCTION_KONTR_AGENT_USER_ID,
                note: Constants.WITHDRAW_FUNDS_TO_CARTAS_NOTE,
                clientId: contract.ClientId);
        }

        /// <summary>
        /// Списание по Аукцииону
        /// </summary>
        private async Task MakeWithdraws(int contractId, CarAuction auction, int buyOutReasonId, ContractAction contractAction)
        {
            Account provisionAccount = await _accountRepository.GetConsolidatedAccountBySettingCodeAsync(Constants.PROVISIONS);
            if (provisionAccount is null)
            {
                throw new PawnshopApplicationException("Счёт провизий не найден!");
            }
            
            var availableOperationSum = auction.WithdrawCost;

            availableOperationSum = await ProcessWithdrawsAsync(
                contractId,
                accountSettingCode: Constants.PROFIT_OFFBALANCE,
                OffBalanceBoId,
                bosId: ProfitOffBalanceLegalWriteOff,
                reason: "Списание начисленных процентов",
                availableOperationSum,
                contractAction,
                buyOutReasonId);
        
            availableOperationSum = await ProcessWithdrawsAsync(
                contractId,
                accountSettingCode: Constants.OVERDUE_PROFIT_OFFBALANCE,
                OffBalanceBoId,
                bosId: OverdueProfitOffBalanceLegalWriteOff,
                reason: "Списание просроченных процентов",
                availableOperationSum,
                contractAction,
                buyOutReasonId);
        
            availableOperationSum = await ProcessWithdrawsAsync(
                contractId,
                accountSettingCode: Constants.PENY_ACCOUNT_OFFBALANCE,
                OffBalanceBoId,
                bosId: PenyAccountOffBalanceLegalWriteOff,
                reason: "Списание пени на просроченный ОД",
                availableOperationSum,
                contractAction,
                buyOutReasonId);
        
            availableOperationSum = await ProcessWithdrawsAsync(
                contractId,
                accountSettingCode: Constants.PENY_PROFIT_OFFBALANCE,
                OffBalanceBoId,
                bosId: PenyProfitOffBalanceLegalWriteOff,
                reason: "Списание пени на просроченные проценты",
                availableOperationSum,
                contractAction,
                buyOutReasonId);
        
            availableOperationSum = await ProcessWithdrawsAsync(
                contractId,
                accountSettingCode: Constants.ACCOUNT_SETTING_OVERDUE_ACCOUNT,
                boId: BalanceBoId,
                bosId: OverdueAccountWriteOff,
                reason: "Списание просроченного основного долга",
                availableOperationSum,
                contractAction,
                debitAccountId: provisionAccount.Id);
        
            availableOperationSum = await ProcessWithdrawsAsync(
                contractId,
                accountSettingCode: Constants.ACCOUNT_SETTING_ACCOUNT,
                boId: BalanceBoId,
                bosId: AccountWriteOff,
                reason: "Списание основного долга",
                availableOperationSum,
                contractAction,
                debitAccountId: provisionAccount.Id);
        
            availableOperationSum = await ProcessWithdrawsAsync(
                contractId,
                accountSettingCode: Constants.ACCOUNT_SETTING_PROFIT,
                boId: BalanceBoId,
                bosId: ProfitWriteOff,
                reason: "Списание начисленных процентов",
                availableOperationSum,
                contractAction,
                debitAccountId: provisionAccount.Id);
        
            availableOperationSum = await ProcessWithdrawsAsync(
                contractId,
                accountSettingCode: Constants.ACCOUNT_SETTING_OVERDUE_PROFIT,
                boId: BalanceBoId,
                bosId: OverdueProfitWriteOff,
                reason: "Списание просроченных процентов",
                availableOperationSum,
                contractAction,
                debitAccountId: provisionAccount.Id);
        
            availableOperationSum = await ProcessWithdrawsAsync(
                contractId,
                accountSettingCode: Constants.ACCOUNT_SETTING_PENY_ACCOUNT,
                boId: BalanceBoId,
                bosId: PenyAccountWriteOff,
                reason: "Списание пени на просроченный ОД",
                availableOperationSum,
                contractAction);
        
            availableOperationSum = await ProcessWithdrawsAsync(
                contractId,
                accountSettingCode: Constants.ACCOUNT_SETTING_PENY_PROFIT,
                boId: BalanceBoId,
                bosId: PenyProfitWriteOff,
                reason: "Списание пени на просроченные проценты",
                availableOperationSum,
                contractAction);
        }
        
        private async Task<decimal> ProcessWithdrawsAsync(
            int contractId,
            string accountSettingCode,
            int boId,
            int bosId,
            string reason,
            decimal availableOperationSum,
            ContractAction contractAction,
            int? debitAccountId = null)
        {
            var account = await _accountRepository.GetByAccountSettingCodeAsync(accountSettingCode, contractId);
            if (account is null)
            {
                throw new PawnshopApplicationException($"Счёт {accountSettingCode} не найден!");
            }

            var withdraw = CalculateWithdrawsAmountService.CalculateSum(ref availableOperationSum, account.Balance);
            if (withdraw.ToWithdraw <= 0 || account.Balance == 0) return availableOperationSum;
           
            if (availableOperationSum < 0) availableOperationSum = Math.Abs(account.Balance);

            await _auctionRepository.WithdrawAsync(
                contractId: contractId,
                transactionDate: DateTime.Now,
                amount: withdraw.ToWithdraw,
                boId: boId,
                bosId: bosId,
                creditAccountId: account.Id,
                reason: reason,
                actionId: contractAction.Id,
                debitAccountId: debitAccountId);

            return availableOperationSum;
        }

        /// <summary>
        /// Погашение
        /// </summary>
        private async Task MakePayOff(int buyoutBranchId, Contract contract, CarAuction auction, ContractAction action)
        {
            ContractDuty contractDuty = _contractDutyService.GetContractDuty(new ContractDutyCheckModel
            {
                ContractId = contract.Id,
                ActionType = ContractActionType.Buyout,
                Cost = auction.WithdrawCost
            });

            action.Discount = contractDuty?.Discount;
            action.Rows = contractDuty.Rows.ToArray();
            action.Cost = contractDuty.Cost;
            action.TotalCost = contractDuty.Cost;
            action.ExtraExpensesCost = contractDuty.ExtraExpensesCost;
            var extraExpenses = contractDuty.ExtraContractExpenses;
            action.ExtraExpensesIds = contractDuty.ExtraContractExpenses.IsNullOrEmpty()
                ? null
                : extraExpenses.Select(e => e.Id).ToList();
            
            _contractActionService.Save(action);

            await _contractActionBuyoutService.Execute(
                action,
                _sessionContext.UserId,
                buyoutBranchId,
                forceExpensePrepaymentReturn: false,
                autoApprove: false,
                null,
                contract);

            contract.BuyoutReasonId = action.BuyoutReasonId;
            _contractService.Save(contract);
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

        private async Task<Domain> GetBuyOutReason(string code)
        {
            var domainValue = await _domainRepository.GetByCodeAsync(code);
            if (domainValue is null)
            {
                throw new PawnshopApplicationException("Причина выкупа не найдена!");
            }

            return domainValue;
        }

        private async Task CheckOperationConditions(Contract contract, Domain domain, PayType payType, Expense expense)
        {
            if (contract.ContractClass != ContractClass.Credit)
                throw new PawnshopApplicationException(
                    "Выкуп доступен только для договоров, не для траншей или кредитной линии!");

            if (contract.Status != ContractStatus.Signed)
            {
                throw new PawnshopApplicationException(
                    "Выкуп невозможен, так как данный договор не является действующим");
            }

            var incompleteExists = await _contractActionService.IncopleteActionExists(contract.Id);
            if (incompleteExists)
                throw new PawnshopApplicationException("В договоре имеется неподтвержденное действие");
            
            if (payType.OperationCode != PayTypeCode)
            {
                throw new PawnshopApplicationException("Передан неверный тип платежа!");
            }

            if (expense.Code != ExpenseTypeCode)
            {
                throw new PawnshopApplicationException("Передан неверный вид расхода!");
            }

            if (domain.Code != BuyOutReasonCode)
            {
                throw new PawnshopApplicationException("Передана неверная причина выкупа!");
            }
        }

        private async Task<CarAuction> GetAuction(Contract contract)
        {
            var auction = await _auctionRepository.GetByContractIdAsync(contract.Id);
            if (auction is null)
            {
                throw new PawnshopApplicationException("Данные по аукциону для договора не найдены!");
            }

            return auction;
        }

        private async Task<Group> GetBranch(string code)
        {
            var branch = await _groupRepository.FindAsync(new { Name = code });
            if (branch is null)
            {
                throw new PawnshopApplicationException($"Филиал {Constants.BKS} не найден!");
            }

            return branch;
        }
        
        private async Task<Expense> GetExpense(string code)
        {
            var expense = await _expenseRepository.GetByCodeAsync(code);
            if (expense is null)
            {
                throw new PawnshopApplicationException("Вид расхода не найден!");
            }

            return expense;
        }
    }
}