using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Pawnshop.AccountingCore.Abstractions;
using Pawnshop.AccountingCore.Models;
using Pawnshop.Services.Models.Filters;

namespace Pawnshop.Services.AccountingCore
{
    public interface IAccountService : IDictionaryWithSearchService<Account, AccountFilter>
    {
        List<Account> OpenForContract(IContract contract);
        Account OpenForContract(IContract contract, AccountSetting accountSetting);
        IEnumerable<(DateTime, decimal)> CalculateForPenaltyAccrual(int accountId, DateTime accrualDate);
        IEnumerable<(DateTime, decimal)> CalculateForPenaltyAccrualForRestructured(int accountId, DateTime accrualDate);
        decimal GetAccountBalance(int accountId, DateTime date, bool isAccountCurrency = true, bool isOutgoingBalance = true);
        Task<Account> GetByAccountSettingIdAsync(int accountSettingId);
        Task<Account> GetByAccountSettingId(int contractId, int accountSettingId);
        void CheckAccountPlan(IContract contract);
        Account GetByAccountSettingCode(int? contractId, string accountSettingCode);
        List<Account> GetAccountsForContractByAccrualTypeInterestAccrualOnOverdueDebt(int contractId);
        IEnumerable<(DateTime, decimal)> CalculateForInterestAccrualOnOverdueDebt(int accountId, DateTime accrualDate);
        Task CloseAccount(int id);
        Task UndoCloseAccount(int id);
        Task DeleteAccount(int id);
    }
}
