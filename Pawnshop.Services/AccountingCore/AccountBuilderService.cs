using System;
using System.Collections.Generic;
using System.Text;
using Pawnshop.AccountingCore.Abstractions;
using Pawnshop.AccountingCore.Models;
using Pawnshop.Core;
using Pawnshop.Data.Access;
using Pawnshop.Data.Models.Contracts;
using Pawnshop.Data.Models.Dictionaries;

namespace Pawnshop.Services.AccountingCore
{
    public class AccountBuilderService : IAccountBuilderService
    {
        private readonly AccountRepository _accountRepository;
        private readonly CurrencyRepository _currencyRepository;

        public AccountBuilderService(AccountRepository accountRepository, CurrencyRepository currencyRepository)
        {
            _accountRepository = accountRepository;
            _currencyRepository = currencyRepository;
        }

        public Account OpenForContract(IContract contract, AccountPlan plan, AccountSetting setting)
        {
            return new Account(contract.AuthorId)
            {
                AccountPlanId = plan.Id,
                AccountSettingId = setting.Id,
                Code = plan.Code,
                Name = setting.Name ?? plan.Name,
                NameAlt = setting.NameAlt ?? plan.NameAlt,
                ClientId = contract.ClientId,
                ContractId = contract.Id,
                AccountNumber = _accountRepository.GetNextNumber(),
                BranchId = contract.BranchId,
                CurrencyId = _currencyRepository.Find(new { IsDefault = true }).Id,
                OpenDate = contract.CreatedToday ? DateTime.Now : contract.ContractDate,
                LastMoveDate = contract.CreatedToday ? DateTime.Now : contract.ContractDate
            };
        }
    }
}
