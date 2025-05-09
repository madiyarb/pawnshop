using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Pawnshop.AccountingCore.Models;
using Pawnshop.Core.Exceptions;
using Pawnshop.Data.Access;
using Pawnshop.Data.Access.LegalCollection;
using Pawnshop.Data.Models.Contracts;
using Pawnshop.Data.Models.Contracts.Inscriptions;
using Pawnshop.Data.Models.LegalCollection.Action;
using Pawnshop.Data.Models.LegalCollection.Dtos;
using Pawnshop.Services.AccountingCore;
using Pawnshop.Services.Contracts;
using Pawnshop.Services.LegalCollection.HttpServices.Interfaces;
using Pawnshop.Services.LegalCollection.Inerfaces;
using Account = Pawnshop.Data.Models.AccountingCore.Account;

namespace Pawnshop.Services.LegalCollection
{
    public class LegalCollectionActionOptionsService : ILegalCollectionActionOptionsService
    {
        private readonly ILegalCaseContractsStatusRepository _legalCaseContractsRepository;
        private readonly ILegalCollectionRepository _legalCollectionsRepository;
        private readonly ILegalCaseHttpService _legalCaseHttpService;
        private readonly IContractService _contractService;
        private readonly IInscriptionService _inscriptionService;
        private readonly AccountRepository _accounts;
        private readonly AccountService _accountService;
        private readonly ILogger<LegalCollectionActionOptionsService> _logger;

        public LegalCollectionActionOptionsService(
            ILegalCaseContractsStatusRepository legalCaseContractsRepository,
            ILegalCollectionRepository legalCollectionsRepository,
            ILegalCaseHttpService legalCaseHttpService,
            IContractService contractService,
            ILogger<LegalCollectionActionOptionsService> logger,
            IInscriptionService inscriptionService,
            AccountRepository accounts,
            AccountService accountService
            )
        {
            _legalCaseContractsRepository = legalCaseContractsRepository;
            _legalCollectionsRepository = legalCollectionsRepository;
            _legalCaseHttpService = legalCaseHttpService;
            _contractService = contractService;
            _inscriptionService = inscriptionService;
            _accounts = accounts;
            _accountService = accountService;
            _logger = logger;
        }

        public async Task<ActionOptionsLegalCaseViewModel> GetActionOptionsAsync(LegalCaseActionOptionsQuery request)
        {
            _logger.LogInformation("Начало операции получения Action-options для объекта LegalCase с Id: {ID}", request.LegalCaseId);
            var contract = await _legalCaseContractsRepository.GetContractByLegalCaseIdAsync(request.LegalCaseId);

            if (contract is null)
            {
                throw new PawnshopApplicationException($"Контракт по делу с id: {request.LegalCaseId} не найден");
            }

            try
            {
                var response = await _legalCaseHttpService.GetLegalCaseActionOptions(request.LegalCaseId);
            
                var actionOptionsViewModel = await GetContractAmounts(contract);

                actionOptionsViewModel.CaseCourt = response.CaseCourt;
                actionOptionsViewModel.CaseCourtId = response.CaseCourtId;
                actionOptionsViewModel.Actions = response.Actions;
                actionOptionsViewModel.Courses = response.Courses;
                actionOptionsViewModel.Courts = response.Courts;

                /// если действие "остановить начисление", и еще нет исп. надписи -> true
                if (response.Actions.Any(a => a.ActionCode == "STOP_ACCRUAL") && !contract.InscriptionId.HasValue)
                {
                    actionOptionsViewModel.CanPassStateFeeAmount = true;
                }

                /// действия при которых будет возобновление начислений, но гос. пошлину передавать нельзя
                var resumeAccrualsActionCodes = new List<string>
                {
                    "CONFIRM_WITHDRAW_CLAIM", "CONFIRM_WITHDRAW_INSCRIPTION"
                };

                if (response.Actions.Any(action => resumeAccrualsActionCodes.Contains(action.ActionCode)))
                {
                    actionOptionsViewModel.CanPassStateFeeAmount = false;
                }
            
                if (contract.InscriptionId != null)
                {
                    var dutyInscriptionRow = await GetDutyInscriptionRow(contract.Id, contract);
                    if (dutyInscriptionRow is null)
                    {
                        actionOptionsViewModel.CanPassStateFeeAmount = true;
                    }
                }
                _logger.LogInformation("Action-options для объекта LegalCase с Id: {ID} успешно получены", request.LegalCaseId);
                return actionOptionsViewModel;
            }
            catch (Exception e)
            {
                _logger.LogInformation(
                    "При получении Action-options для LegalCase с Id: {ID} произошла ошибка: {Error}",
                    request.LegalCaseId, e.Message);
                throw;
            }
        }
        
        private async Task<InscriptionRow> GetDutyInscriptionRow(int contractId, Contract? contract = null)
        {
            contract ??= await _contractService.GetOnlyContractAsync(contractId);
            var inscription = await _inscriptionService.GetAsync(contract.InscriptionId);

            return inscription?.Rows?.FirstOrDefault(r => r.PaymentType == AmountType.Duty);
        }
        
        private async Task<ActionOptionsLegalCaseViewModel> GetContractAmounts(Contract contract)
        {
            _logger.LogInformation("Получение сумм по счетам договора с Id: {ContractId}", contract?.Id);
            /// Начисленные проценты на внебалансе
            const string PROFIT_OFFBALANCE = "PROFIT_OFFBALANCE";
            
            /// Просроченные проценты на внебалансе
            const string OVERDUE_PROFIT_OFFBALANCE = "OVERDUE_PROFIT_OFFBALANCE";
            
            /// Пеня на просроченный основной долг на внебалансе
            const string PENY_ACCOUNT_OFFBALANCE = "PENY_ACCOUNT_OFFBALANCE";
            
            /// Пеня на просроченные проценты на внебалансе
            const string PENY_PROFIT_OFFBALANCE = "PENY_PROFIT_OFFBALANCE";
            
            var contractBalance = await _accounts.GetBalanceByContractIdAsync(contract.Id);

            var pennyAccountId = await _legalCollectionsRepository.GetPennyAccountIdByContractIdAsync(contract.Id);
            var profitAccountId = await _legalCollectionsRepository.GetPennyProfitIdByContractIdAsync(contract.Id);

            var accountPenny = await _accountService.GetAccountBalanceAsync(pennyAccountId, DateTime.Now);
            var profitPenny = await _accountService.GetAccountBalanceAsync(profitAccountId, DateTime.Now);
            
            List<Account> accounts = await _legalCollectionsRepository.GetAccountsByAccountSettingsAsync(contract.Id);

            decimal? debt = null;
            
            if (contract.InscriptionId != null)
            {
                var dutyInscriptionRow = await GetDutyInscriptionRow(contract.Id, contract);
                if (dutyInscriptionRow != null)
                {
                    debt = dutyInscriptionRow.Cost;
                }
            }
            
            var totalSum =
                (contractBalance.ProfitAmount != null ? contractBalance.ProfitAmount : 0) +
                (contractBalance.OverdueProfitAmount != null ? contractBalance.OverdueProfitAmount : 0) +
                (contractBalance.AccountAmount != null ? contractBalance.AccountAmount : 0) +
                (contractBalance.OverdueAccountAmount != null ? contractBalance.OverdueAccountAmount : 0) +
                (accountPenny != null ? accountPenny : 0) +
                (profitPenny != null ? profitPenny : 0) +
                (debt ?? 0);
            
            var contractAmounts = new List<ContractAmountsInfoDto>
            {
                new ContractAmountsInfoDto
                {
                    Name = "Начисленные проценты на внебалансе",
                    Cost = Math.Abs(accounts.FirstOrDefault(a => a.Code == PROFIT_OFFBALANCE).Balance)
                },
                new ContractAmountsInfoDto
                {
                    Name = "Просроченные проценты на внебалансе",
                    Cost =  Math.Abs(accounts.FirstOrDefault(a => a.Code == OVERDUE_PROFIT_OFFBALANCE).Balance)
                },
                new ContractAmountsInfoDto
                {
                    Name = "Пеня на просроченный основной долг на внебалансе",
                    Cost = Math.Abs(accounts.FirstOrDefault(a => a.Code == PENY_ACCOUNT_OFFBALANCE).Balance)
                },
                new ContractAmountsInfoDto
                {
                    Name = "Пеня на просроченные проценты на внебалансе",
                    Cost = Math.Abs(accounts.FirstOrDefault(a => a.Code == PENY_PROFIT_OFFBALANCE).Balance)
                },
                new ContractAmountsInfoDto
                {
                    Name = "Начисленное вознаграждение",
                    Cost = contractBalance.ProfitAmount
                },
                new ContractAmountsInfoDto
                {
                    Name = "Просроченное вознаграждение",
                    Cost = contractBalance.OverdueProfitAmount
                },
                new ContractAmountsInfoDto
                {
                    Name = "Основной долг",
                    Cost = contractBalance.AccountAmount
                },
                new ContractAmountsInfoDto
                {
                    Name = "Штраф/пеня на основной долг",
                    Cost = accountPenny
                },
                new ContractAmountsInfoDto
                {
                    Name = "Штраф/пеня на вознаграждение/проценты",
                    Cost = profitPenny
                },
                new ContractAmountsInfoDto
                {
                    Name = "Просроченный основной долг",
                    Cost = contractBalance.OverdueAccountAmount
                },
                new ContractAmountsInfoDto
                {
                    Name = "Гос. пошлина",
                    Cost = debt ?? 0,
                    StringCost = GetCostInStringFormat(debt ?? 0)
                }
            };
            
            _logger.LogInformation("Все суммы по счетам договора с Id: {ContractId} получены", contract?.Id);

            return new ActionOptionsLegalCaseViewModel
            {
                Amounts = contractAmounts,
                TotalCost = totalSum,
                StateFeeAmount = debt ?? 0
            };
        }
        
        private string GetCostInStringFormat(decimal cost)
        {
            CultureInfo culture = new CultureInfo("ru-RU");
            return cost.ToString(cost % 1 == 0 ? "N0" : "N2", culture);
        }
    }
}