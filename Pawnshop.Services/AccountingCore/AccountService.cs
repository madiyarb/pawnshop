using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Pawnshop.AccountingCore.Abstractions;
using Pawnshop.AccountingCore.Models;
using Pawnshop.Core.Exceptions;
using Pawnshop.Core.Queries;
using Pawnshop.Data.Access;
using Pawnshop.Data.Models.Membership;
using Pawnshop.Services.Models.Filters;
using Pawnshop.Services.Models.List;

namespace Pawnshop.Services.AccountingCore
{
    public class AccountService : IAccountService
    {
        private readonly AccountRepository _repository;
        private readonly IAccountBuilderService _accountBuilder;
        private readonly IAccountPlanSettingService _accountPlanSettingService;
        private readonly IDictionaryService<AccountPlan> _accountPlanService;
        private readonly IDictionaryWithSearchService<AccountSetting, AccountSettingFilter> _accountSettingService;
        private readonly IDictionaryWithSearchService<Group, BranchFilter> _branchService;

        public AccountService(AccountRepository repository,
            IAccountBuilderService accountBuilder,
            IAccountPlanSettingService accountPlanSettingService,
            IDictionaryService<AccountPlan> accountPlanService,
            IDictionaryWithSearchService<AccountSetting, AccountSettingFilter> accountSettingService,
            IDictionaryWithSearchService<Group, BranchFilter> branchService)
        {
            _repository = repository;
            _accountBuilder = accountBuilder;
            _accountPlanService = accountPlanService;
            _accountSettingService = accountSettingService;
            _accountPlanSettingService = accountPlanSettingService;
            _branchService = branchService;
        }

        public Account Save(Account model)
        {
            var m = new Data.Models.AccountingCore.Account(model);
            if (model.Id > 0)
            {
                _repository.Update(m);
            }
            else
            {
                _repository.Insert(m);
            }

            model.Id = m.Id;
            return m;
        }

        public void Delete(int id)
        {
            _repository.Delete(id);
        }

        public async Task<Account> GetAsync(int id)
        {
            return await _repository.GetAsync(id);
        }

        public ListModel<Account> List(ListQuery listQuery)
        {
            return new ListModel<Account>
            {
                List = _repository.List(listQuery).AsEnumerable<Account>().ToList(),
                Count = _repository.Count(listQuery)
            };
        }

        public ListModel<Account> List(ListQueryModel<AccountFilter> listQuery)
        {
            return new ListModel<Account>
            {
                List = _repository.List(listQuery, listQuery.Model).AsEnumerable<Account>().ToList(),
                Count = _repository.Count(listQuery, listQuery.Model)
            };
        }

        public Account GetByAccountSettingCode(int? contractId, string accountSettingCode)
        {
            var accountListQueryModel = new ListQueryModel<AccountFilter>
            {
                Page = null,
                Model = new AccountFilter
                {
                    ContractId = contractId.HasValue ? contractId : null,
                    IsOpen = true,
                    SettingCodes = new List<string> { accountSettingCode }.ToArray()
                }
            };
            ListModel<Account> accountListModel = List(accountListQueryModel);
            if (accountListModel == null)
                throw new PawnshopApplicationException($"Ожидалось что {nameof(accountListModel)} не будет null");

            if (accountListModel.List == null)
                throw new PawnshopApplicationException($"Ожидалось что {nameof(accountListModel)}.{nameof(accountListModel.List)} не будет null");

            List<Account> accounts = accountListModel.List;
            Account account = accounts.Find(x => x.AccountSetting.Code == accountSettingCode && !x.DeleteDate.HasValue && !x.CloseDate.HasValue);
            if (account == null)
                throw new PawnshopApplicationException($"Ожидалось что {nameof(account)} не будет null");
            if (account.AccountSetting == null)
                throw new PawnshopApplicationException($"Ожидалось что {nameof(account.AccountSetting)} не будет null");
            return account;
        }
        
        public async Task<Account> GetByAccountSettingId(int contractId, int accountSettingId)
        {
            return await _repository.GetByAccountSettingIdAsync(contractId, accountSettingId);
        }

        /// <summary>
        /// Открывает все необходимые счета по договору, если они не открыты
        /// </summary>
        /// <param name="contract">Договор</param>
        /// <returns></returns>
        public List<Account> OpenForContract(IContract contract)
        {
            var accounts = new List<Account>();

            //ищем все из AccountSettings с TypeId = type.Id и IsConsolidate == 0
            var accountSettings = _accountSettingService.List(new ListQueryModel<AccountSettingFilter>() { Page = null, Model = new AccountSettingFilter() { TypeId = contract.ContractTypeId, IsConsolidated = false, WithAllParents = true } });

            var exists = List(new ListQueryModel<AccountFilter>() { Page = null, Model = new AccountFilter() { ContractId = contract.Id, IsOpen = true } });

            //проверяем - существуют ли уже счета на договоре
            if (exists.List.Any())
            {
                accountSettings.List = accountSettings.List.Where(x => exists.List.All(acc => acc.AccountSettingId != x.Id)).ToList();
            }

            var branch = _branchService.GetAsync(contract.BranchId).Result;

            //проходим по всем settings
            foreach (var accountSetting in accountSettings.List)
            {
                //ищем AccountPlanSetting для каждого setting
                var planSetting = _accountPlanSettingService.Find(branch.OrganizationId, accountSetting.Id,
                    contract.BranchId, contract.ContractTypeId, contract.PeriodTypeId);

                if (planSetting != null && planSetting.AccountPlanId.HasValue)
                {
                    //Поиск плана счета
                    var plan = _accountPlanService.GetAsync(planSetting.AccountPlanId.Value).Result;
                    //Открытие счета
                    var account = _accountBuilder.OpenForContract(contract, plan, accountSetting);
                    //Сохранение счета
                    account = Save(account);
                    accounts.Add(account);
                }
                else
                    throw new PawnshopApplicationException(
                        $"Для счета {accountSetting.Name}(Code = {accountSetting.Code}, Id = {accountSetting.Id}) не найден план счетов");
            }

            return accounts;
        }

        public Account OpenForContract(IContract contract, AccountSetting accountSetting)
        {
            var branch = _branchService.GetAsync(contract.BranchId).Result;

            //ищем AccountPlanSetting для каждого setting
            var planSetting = _accountPlanSettingService.Find(branch.OrganizationId, accountSetting.Id,
                contract.BranchId, contract.ContractTypeId, contract.PeriodTypeId);

            if (planSetting != null && planSetting.AccountPlanId.HasValue)
            {
                //Поиск плана счета
                AccountPlan plan = _accountPlanService.GetAsync(planSetting.AccountPlanId.Value).Result;
                //Открытие счета
                Account account = _accountBuilder.OpenForContract(contract, plan, accountSetting);
                //Сохранение счета
                return Save(account);

            }

            throw new PawnshopApplicationException($"Для счета {accountSetting.Name}(Code = {accountSetting.Code}, Id = {accountSetting.Id}) не найден план счетов");
        }

        public IEnumerable<(DateTime, decimal)> CalculateForPenaltyAccrual(int accountId, DateTime accrualDate) => _repository.GetBalanceForPenaltyAccrual(accountId, accrualDate);

        public IEnumerable<(DateTime, decimal)> CalculateForPenaltyAccrualForRestructured(int accountId, DateTime accrualDate) => _repository.GetBalanceForPenaltyAccrualForRestructured(accountId, accrualDate);

        public IEnumerable<(DateTime, decimal)> CalculateForInterestAccrualOnOverdueDebt(int accountId, DateTime accrualDate) => _repository.GetBalanceForInterestAccrualOnOverdueDebt(accountId, accrualDate);

        public decimal GetAccountBalance(int accountId, DateTime date, bool isAccountCurrency = true, bool isOutgoingBalance = true) => _repository.GetAccountBalance(accountId, date, isAccountCurrency, isOutgoingBalance);
        
        public async Task<Account> GetByAccountSettingIdAsync(int accountSettingId) => await _repository.GetConsolidatedAccountBySettingIdAsync(accountSettingId);
        
        public async Task<decimal> GetAccountBalanceAsync(int accountId, DateTime date, bool isAccountCurrency = true, bool isOutgoingBalance = true) => await _repository.GetAccountBalanceAsync(accountId, date, isAccountCurrency, isOutgoingBalance);

        public void CheckAccountPlan(IContract contract)
        {
            var accounts = List(new ListQueryModel<AccountFilter>() { Page = null, Model = new AccountFilter() { ContractId = contract.Id } });

            var branch = _branchService.GetAsync(contract.BranchId).Result;

            //проходим по всем accounts
            foreach (var account in accounts.List)
            {
                //ищем AccountPlanSetting для каждого setting
                var planSetting = _accountPlanSettingService.Find(branch.OrganizationId, account.AccountSettingId,
                    contract.BranchId, contract.ContractTypeId, contract.PeriodTypeId);

                if (planSetting != null && planSetting.AccountPlanId.HasValue)
                {
                    //Поиск плана счета
                    var plan = _accountPlanService.GetAsync(planSetting.AccountPlanId.Value).Result;
                    //Проверка на разницу
                    if (plan.Id != account.AccountPlanId)
                    {
                        account.Code = plan.Code;
                        account.AccountPlanId = plan.Id;
                    }

                    //Сохранение счета
                    Save(account);
                }
                else
                    throw new PawnshopApplicationException(
                        $"Для счета {account.AccountSetting.Name}(Code = {account.AccountSetting.Code}, Id = {account.AccountSetting.Id}) не найден план счетов");
            }
        }

        public List<Account> GetAccountsForContractByAccrualTypeInterestAccrualOnOverdueDebt(int contractId) =>
            _repository.GetAccountsForContractByAccrualType(AccrualType.InterestAccrualOnOverdueDebt, contractId);

        public async Task CloseAccount(int id)
        {
            var account = await _repository.GetAsync(id);
            account.CloseDate = DateTime.Now;
            await _repository.UpdateAsync(account);
        }

        public async Task UndoCloseAccount(int id)
        {
            var account = await _repository.GetAsync(id);
            if (account.CloseDate.HasValue)
                account.CloseDate = null;

            await _repository.UpdateAsync(account);
        }

        public async Task DeleteAccount(int id)
        {
            var account = await _repository.GetAsync(id);
            if(account != null)
                await _repository.DeleteAsync(id);
        }
    }
}
