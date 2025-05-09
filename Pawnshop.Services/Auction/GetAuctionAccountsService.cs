using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Pawnshop.Data.Access;
using Pawnshop.Data.Models.AccountingCore;
using Pawnshop.Data.Models.Contracts;
using Pawnshop.Services.Auction.Interfaces;
using Pawnshop.Services.Contracts;

namespace Pawnshop.Services.Auction
{
    public class GetAuctionAccountsService : IGetAuctionAccountsService
    {
        private readonly IContractService _contractService;
        private readonly AccountRepository _accountRepository;

        public GetAuctionAccountsService(IContractService contractService, AccountRepository accountRepository)
        {
            _contractService = contractService;
            _accountRepository = accountRepository;
        }

        public async Task<IEnumerable<Account>> GetAccounts(Contract contract, List<string> accountsCodes)
        {
            var accounts = new List<Account>();
            var contracts = new List<Contract>();
            
            switch (contract.ContractClass)
            {
                case ContractClass.CreditLine:
                    contracts = await _contractService.GetAllTranchesAsync(contract.Id);
                    break;
                case ContractClass.Tranche:
                    contracts = await _contractService.GetAllTranchesAsync((int)contract.CreditLineId);
                    break;
                case ContractClass.Credit:
                    contracts.Add(contract);
                    break;
            }

            var contractIds = contracts.Select(t => t.Id).ToList();

            foreach (var contractId in contractIds)
            {
                var contractAccounts = await _accountRepository
                    .GetAccountsByAccountSettingsAsync(contractId, accountsCodes);

                if (contractAccounts.Any())
                {
                    accounts.AddRange(contractAccounts);
                }
            }

            return accounts;
        }
        
        public async Task<IEnumerable<Account>> GetPrePaymentAccounts(int contractId, Contract? contract = null)
        {
            const string PrePaymentAccountSettingCode = "DEPO";
            var accounts = new List<Account>();

            contract ??= await _contractService.GetOnlyContractAsync(contractId);
    
            if (contract.ContractClass == ContractClass.CreditLine || contract.ContractClass == ContractClass.Tranche)
            {
                int baseContractId = contract.ContractClass == ContractClass.CreditLine ? contract.Id : contract.CreditLineId.Value;
        
                var contractAccounts = await _accountRepository
                    .GetAccountsByAccountSettingsAsync(baseContractId, new List<string> { PrePaymentAccountSettingCode });
                if (contractAccounts.Any())
                {
                    accounts.AddRange(contractAccounts);
                }

                var tranches = await _contractService.GetAllTranchesAsync(baseContractId);
                foreach (var tranche in tranches)
                {
                    var trancheAccounts = await _accountRepository
                        .GetAccountsByAccountSettingsAsync(tranche.Id, new List<string> { PrePaymentAccountSettingCode });
                    if (trancheAccounts.Any())
                    {
                        accounts.AddRange(trancheAccounts);
                    }
                }
            }
            else
            {
                var contractAccounts = await _accountRepository
                    .GetAccountsByAccountSettingsAsync(contract.Id, new List<string> { PrePaymentAccountSettingCode });
                if (contractAccounts.Any())
                {
                    accounts.AddRange(contractAccounts);
                }
            }

            return accounts;
        }
    }
}