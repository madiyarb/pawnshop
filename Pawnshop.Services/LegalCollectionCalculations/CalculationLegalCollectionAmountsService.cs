using Pawnshop.Data.Access.LegalCollection;
using Pawnshop.Data.Access;
using Pawnshop.Data.Models.Contracts;
using Pawnshop.Services.Contracts;
using Pawnshop.Data.Models.Contracts.LegalCollectionCalculations;
using Pawnshop.Data.Models.AccountingCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Pawnshop.Data.Models.Contracts.Expenses;
using Pawnshop.Services.LegalCollectionCalculations;
using Pawnshop.Core;
using Pawnshop.Services.LegalCollection.Inerfaces;

namespace Pawnshop.Services.LegalCollectionCalculation
{
    public class CalculationLegalCollectionAmountsService : ICalculationLegalCollectionAmountsService
    {
        private readonly ILegalCollectionRepository _legalCollectionsRepository;
        private readonly IContractService _contractService;
        private readonly IContractPaymentScheduleService _contractPaymentScheduleService;
        private readonly ILegalCaseContractsStatusRepository _legalCaseContractsStatusRepository;
        private readonly AccountRepository _accountRepository;
        private readonly ContractExpenseRepository _contractExpenseRepository;
        private readonly ILegalCollectionDetailsService _legalCollectionDetailsService;
        private readonly ISessionContext _sessionContext;

        public CalculationLegalCollectionAmountsService(
            ILegalCollectionRepository legalCollectionsRepository,
            IContractService contractService,
            IContractPaymentScheduleService contractPaymentScheduleService,
            ILegalCaseContractsStatusRepository legalCaseContractsStatusRepository,
            AccountRepository accountRepository,
            ContractExpenseRepository contractExpenseRepository,
            ILegalCollectionDetailsService legalCollectionDetailsService,
            ISessionContext sessionContext
        )
        {
            _legalCollectionsRepository = legalCollectionsRepository;
            _contractService = contractService;
            _contractPaymentScheduleService = contractPaymentScheduleService;
            _legalCaseContractsStatusRepository = legalCaseContractsStatusRepository;
            _accountRepository = accountRepository;
            _contractExpenseRepository = contractExpenseRepository;
            _legalCollectionDetailsService = legalCollectionDetailsService;
            _sessionContext = sessionContext;
        }

        private async Task<int?> GetLegalCaseIdByContractIdAsync(int contractId)
        {
            var legalCaseStatus = await _legalCaseContractsStatusRepository.GetActiveLegalCaseByContractIdAsync(contractId);
            return legalCaseStatus?.LegalCaseId;
        }

        private async Task<IEnumerable<ContractExpense>> GetUnpaidExpensesAsync(int contractId)
        {
            var expenses = await _contractExpenseRepository.GetContractExpenseAsync(contractId);
            return expenses.Where(e => !e.IsPayed);
        }

        public async Task<decimal> CalculateTotalUnpaidExpenses(int contractId, ContractClass contractClass)
        {
            decimal totalUnpaid = 0;
            if (contractClass == ContractClass.Tranche)
            {
                var creditLineId = await _contractService.GetCreditLineId(contractId);
                var unpaidExpenses = await GetUnpaidExpensesAsync(creditLineId);
                totalUnpaid = unpaidExpenses.Sum(e => e.TotalCost);
            }
            else if (contractClass == ContractClass.Credit || contractClass == ContractClass.CreditLine)
            {
                var unpaidExpenses = await GetUnpaidExpensesAsync(contractId);
                totalUnpaid = unpaidExpenses.Sum(e => e.TotalCost);
            }
            return totalUnpaid;
        }


        private async Task<List<Account>> GetAllAccounts(Contract contract)
        {
            switch (contract.ContractClass)
            {
                case ContractClass.CreditLine:
                    var contractIds = (await _contractService.GetAllTranchesAsync((int)contract.Id)).Select(c => c.Id)
                            .Distinct().ToList();
                    contractIds.Add(contract.Id);

                    return await GetContractIdsAccount(contractIds);
                case ContractClass.Tranche:
                    return await GetTrancheAccount(contract.Id);
                case ContractClass.Credit:
                    return await GetTrancheAccount(contract.Id);
            }

            throw new NotImplementedException();
        }

        private async Task<List<Account>> GetContractIdsAccount(List<int> contractIds)
        {
            var accounts = new List<Account>();

            foreach (var id in contractIds)
            {
                var contractAccounts = await GetTrancheAccount(id);

                if (contractAccounts != null && contractAccounts.Any())
                {
                    accounts.AddRange(contractAccounts);
                }
            }

            return accounts;
        }

        private async Task<List<Account>> GetTrancheAccount(int contractId)
        {
            return await _accountRepository.GetAccountsByAccountSettingsAsync(contractId, new List<string>
            {                                                                                                       
                Constants.ACCOUNT_SETTING_DEPO,
                Constants.ACCOUNT_SETTING_ACCOUNT,
                Constants.ACCOUNT_SETTING_PROFIT,
                Constants.ACCOUNT_SETTING_OVERDUE_ACCOUNT,
                Constants.ACCOUNT_SETTING_OVERDUE_PROFIT,
                Constants.ACCOUNT_SETTING_PENY_ACCOUNT,
                Constants.ACCOUNT_SETTING_PENY_PROFIT,
                Constants.ACCOUNT_SETTING_PROFIT_OFFBALANCE,
                Constants.ACCOUNT_SETTING_OVERDUE_PROFIT_OFFBALANCE,
                Constants.ACCOUNT_SETTING_PENY_ACCOUNT_OFFBALANCE,
                Constants.ACCOUNT_SETTING_PENY_PROFIT_OFFBALANCE
            });
        }

        public async Task<LegalAmountsViewModel> CalculateLegalAmounts(Contract contract)
        {

            if (contract.ContractClass == ContractClass.CreditLine)
            {
                var tranches = await _contractService.GetAllTranchesAsync(contract.Id);
                if (tranches.All(t => !t.IsOffBalance))
                {
                    return null;
                }
            }

            var accounts = await GetAllAccounts(contract);
            var unpaidExpenses = await CalculateTotalUnpaidExpenses(contract.Id, contract.ContractClass);
            var paymentSchedule = _contractPaymentScheduleService.GetListByContractId(contract.Id);

            if (contract.ContractClass == ContractClass.CreditLine)
            {
                var tranches = await _contractService.GetAllTranchesAsync(contract.Id);
                foreach (var tranche in tranches)
                {
                    var paymentScheduleForTranche = _contractPaymentScheduleService.GetListByContractId(tranche.Id);
                    paymentSchedule.AddRange(paymentScheduleForTranche);
                }
            }
            else
            {
                paymentSchedule = _contractPaymentScheduleService.GetListByContractId(contract.Id);
            }
            var paymentScheduleItems = paymentSchedule.Where(x => x.Date.Date == DateTime.Now.Date);
            var isOnControlDate = paymentScheduleItems.Count() > 0;

            var redemptionAmount = GetAccountsSum(accounts.Where(a =>
                a.Code == Constants.ACCOUNT_SETTING_ACCOUNT ||
                a.Code == Constants.ACCOUNT_SETTING_PROFIT ||
                a.Code == Constants.ACCOUNT_SETTING_OVERDUE_ACCOUNT ||
                a.Code == Constants.ACCOUNT_SETTING_OVERDUE_PROFIT ||
                a.Code == Constants.ACCOUNT_SETTING_PENY_ACCOUNT ||
                a.Code == Constants.ACCOUNT_SETTING_PENY_PROFIT));

            var totalDebtAmount = GetAccountsSum(accounts.Where(a =>
                (a.Code == Constants.ACCOUNT_SETTING_PROFIT && isOnControlDate) ||
                a.Code == Constants.ACCOUNT_SETTING_OVERDUE_ACCOUNT ||
                a.Code == Constants.ACCOUNT_SETTING_OVERDUE_PROFIT ||
                a.Code == Constants.ACCOUNT_SETTING_PENY_ACCOUNT ||
                a.Code == Constants.ACCOUNT_SETTING_PENY_PROFIT ||
                (a.Code == Constants.ACCOUNT_SETTING_PROFIT_OFFBALANCE && isOnControlDate) ||
                a.Code == Constants.ACCOUNT_SETTING_OVERDUE_PROFIT_OFFBALANCE ||
                a.Code == Constants.ACCOUNT_SETTING_PENY_ACCOUNT_OFFBALANCE ||
                a.Code == Constants.ACCOUNT_SETTING_PENY_PROFIT_OFFBALANCE));

            totalDebtAmount += unpaidExpenses;

            if (isOnControlDate)
                totalDebtAmount += paymentScheduleItems.Sum(x => x.DebtCost);

            var depositAmount = GetAccountsSum(accounts.Where(a => a.Code == Constants.ACCOUNT_SETTING_DEPO));

            decimal totalDebtSumWithUnpaid = totalDebtAmount + unpaidExpenses;
            decimal redemptionAmountMinusDepo = (redemptionAmount + unpaidExpenses) - depositAmount;

            redemptionAmountMinusDepo = Math.Max(redemptionAmountMinusDepo, 0);
            decimal amountPayMinusDepo = Math.Max(totalDebtAmount - depositAmount, 0);

            var hasOffBalanceTranche = false;
            if (contract.ContractClass == ContractClass.CreditLine)
            {
                var tranches = await _contractService.GetAllTranchesAsync(contract.Id);
                hasOffBalanceTranche = tranches.Any(t => t.IsOffBalance);
            }

            if (contract.ContractClass != ContractClass.CreditLine || (contract.IsOffBalance && hasOffBalanceTranche))
            {
                var legalCaseStatus = await _legalCaseContractsStatusRepository.GetActiveLegalCaseByContractIdAsync(contract.Id);
                if (legalCaseStatus?.LegalCaseId != null && legalCaseStatus?.LegalCaseId != 0)
                {
                    var legalStageDetails = await _legalCollectionDetailsService.GetDetailsAsync(new Data.Models.LegalCollection.Details.LegalCaseDetailsQuery()
                    { LegalCaseId = (int)legalCaseStatus?.LegalCaseId });
                    var legalStage = legalStageDetails.Find(x => x.Course != null);

                    if (contract.IsOffBalance == true)
                    {
                        if (legalStage != null && legalStage?.Stage.StageCode != Constants.LEGAL_STAGE_SENT_PUBLIC_EXECUTOR &&
                            legalStage?.Stage.StageCode != Constants.LEGAL_STAGE_EXECUTION_WRIT_RECEIVED)
                        {
                            return new LegalAmountsViewModel
                            {
                                DepositAmount = depositAmount,
                                TotalDebtAmount = totalDebtAmount,
                                AmountPayMinusDepo = amountPayMinusDepo,
                                RedemptionAmount = redemptionAmount + unpaidExpenses,
                                RedemptionAmountMinusDepo = redemptionAmountMinusDepo,
                            };
                        }
                        else if (legalStage != null && legalStage?.Stage.StageCode == Constants.LEGAL_STAGE_SENT_PUBLIC_EXECUTOR ||
                                 legalStage?.Stage.StageCode == Constants.LEGAL_STAGE_EXECUTION_WRIT_RECEIVED)
                        {
                            bool hasPermissionToViewAmounts = _sessionContext.Permissions.Any(p => p == Permissions.LegalCollectionAmountsView);

                            if (hasPermissionToViewAmounts)
                            {
                                return new LegalAmountsViewModel
                                {
                                    DepositAmount = depositAmount,
                                    TotalDebtAmount = totalDebtAmount,
                                    AmountPayMinusDepo = amountPayMinusDepo,
                                    RedemptionAmount = redemptionAmount + unpaidExpenses,
                                    RedemptionAmountMinusDepo = redemptionAmountMinusDepo,
                                };
                            }
                            else
                            {
                                return new LegalAmountsViewModel
                                {
                                    DepositAmount = depositAmount,
                                    TotalDebtAmount = 0,
                                    AmountPayMinusDepo = 0,
                                    RedemptionAmount = redemptionAmount + unpaidExpenses,
                                    RedemptionAmountMinusDepo = redemptionAmountMinusDepo,
                                    DisplayConditionForZeroSum = true
                                };                                                 
                            }
                        }
                    }
                }
            }

            return new LegalAmountsViewModel
            {   
                DepositAmount = depositAmount,
                AmountPayMinusDepo = amountPayMinusDepo,
                TotalDebtAmount = totalDebtAmount,
                RedemptionAmount = redemptionAmount + unpaidExpenses,
                RedemptionAmountMinusDepo = redemptionAmountMinusDepo,
            };
        }

        private async Task<decimal> GetPrePayment(Contract contract)
        {
            var accounts = new List<Account>();

            if (contract.ContractClass == ContractClass.CreditLine)
            {
                var creditLineContract = await _contractService.GetOnlyContractAsync(contract.Id);
                if (creditLineContract != null)
                {
                    var creditLineAccounts = await _accountRepository
                        .GetAccountsByAccountSettingsAsync(creditLineContract.Id, new List<string> { Constants.ACCOUNT_SETTING_DEPO });
                    if (creditLineAccounts.Any())
                    {
                        accounts.AddRange(creditLineAccounts);
                    }
                }

                var tranches = await _contractService.GetAllTranchesAsync(contract.Id);
                if (tranches.Any())
                {
                    foreach (var tranche in tranches)
                    {
                        var trancheAccounts = await _accountRepository
                            .GetAccountsByAccountSettingsAsync(tranche.Id, new List<string> { Constants.ACCOUNT_SETTING_DEPO });

                        if (trancheAccounts.Any())
                        {
                            accounts.AddRange(trancheAccounts);
                        }
                    }
                }
            }
            else if (contract.ContractClass == ContractClass.Credit || contract.ContractClass == ContractClass.Tranche)
            {
                var contractAccounts = await _accountRepository
                              .GetAccountsByAccountSettingsAsync(contract.Id, new List<string> { Constants.ACCOUNT_SETTING_DEPO });

                if (contractAccounts.Any())
                {
                    accounts.AddRange(contractAccounts);
                }
            }

            return GetAccountsSum(accounts);
        }

        private static decimal GetAccountsSum(IEnumerable<Account> accounts)
        {
            return accounts.Sum(a => Math.Abs(a.Balance));
        }
    }
}