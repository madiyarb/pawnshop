using Account = Pawnshop.Data.Models.AccountingCore.Account;
using AccountSetting = Pawnshop.AccountingCore.Models.AccountSetting;
using Dapper;
using Pawnshop.AccountingCore.Models;
using Pawnshop.Core.Impl;
using Pawnshop.Core.Queries;
using Pawnshop.Core;
using Pawnshop.Data.Models.AccountRecords;
using Pawnshop.Data.Models.Contracts;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System;

namespace Pawnshop.Data.Access
{
    public class AccountRepository : RepositoryBase, IRepository<Account>
    {
        public AccountRepository(IUnitOfWork unitOfWork) : base(unitOfWork)
        {
        }

        public void Insert(Account entity)
        {
            using (var transaction = BeginTransaction())
            {
                entity.Id = UnitOfWork.Session.QuerySingleOrDefault<int>(@"
INSERT INTO Accounts ( Code, Name, NameAlt, ContractId, ClientId, AccountNumber, OpenDate, CloseDate, AccountPlanId, AccountSettingId, Balance, BalanceNC, LastMoveDate, BranchId, CurrencyId, CreateDate, AuthorId, IsOutmoded, RedBalanceAllowed )
VALUES ( @Code, @Name, @NameAlt, @ContractId, @ClientId, @AccountNumber, @OpenDate, @CloseDate, @AccountPlanId, @AccountSettingId, @Balance, @BalanceNC, @LastMoveDate, @BranchId, @CurrencyId, @CreateDate, @AuthorId, @IsOutmoded, @RedBalanceAllowed )
SELECT SCOPE_IDENTITY()", entity, UnitOfWork.Transaction);

                transaction.Commit();
            }
        }

        public void Update(Account entity)
        {
            using (var transaction = BeginTransaction())
            {
                UnitOfWork.Session.Execute(@"
UPDATE Accounts
SET Code = @Code, Name = @Name, NameAlt = @NameAlt, ContractId = @ContractId, ClientId = @ClientId, AccountNumber = @AccountNumber, CloseDate = @CloseDate, Balance = @Balance, BalanceNC = @BalanceNC, LastMoveDate = @LastMoveDate, IsOutmoded = @IsOutmoded, RedBalanceAllowed = @RedBalanceAllowed, AccountPlanId = @AccountPlanId
WHERE Id = @Id", entity, UnitOfWork.Transaction, 240);

                transaction.Commit();
            }
        }

        public async Task UpdateAsync(Account entity)
        {
            using (var transaction = BeginTransaction())
            {
                await UnitOfWork.Session.ExecuteAsync(@"
UPDATE Accounts
SET Code = @Code, Name = @Name, NameAlt = @NameAlt, ContractId = @ContractId, ClientId = @ClientId, AccountNumber = @AccountNumber, CloseDate = @CloseDate, Balance = @Balance, BalanceNC = @BalanceNC, LastMoveDate = @LastMoveDate, IsOutmoded = @IsOutmoded, RedBalanceAllowed = @RedBalanceAllowed, AccountPlanId = @AccountPlanId
WHERE Id = @Id", entity, UnitOfWork.Transaction, 240);

                transaction.Commit();
            }
        }

        public void Delete(int id)
        {
            using (var transaction = BeginTransaction())
            {
                UnitOfWork.Session.Execute(@"UPDATE Accounts SET DeleteDate = dbo.GETASTANADATE() WHERE Id = @id", new { id }, UnitOfWork.Transaction);
                transaction.Commit();
            }
        }

        public async Task DeleteAsync(int id)
        {
            using (var transaction = BeginTransaction())
            {
                await UnitOfWork.Session.ExecuteAsync(@"UPDATE Accounts SET DeleteDate = dbo.GETASTANADATE() WHERE Id = @id", new { id }, UnitOfWork.Transaction);
                transaction.Commit();
            }
        }

        public Account Get(int id)
        {
            return UnitOfWork.Session.QuerySingleOrDefault<Account>(@"
SELECT *
FROM Accounts
WHERE Id = @id", new { id }, UnitOfWork.Transaction);
        }

        public async Task<Account> GetAsync(int id)
        {
            return await UnitOfWork.Session.QueryFirstOrDefaultAsync<Account>(@"
SELECT *
FROM Accounts
WHERE Id = @id", new { id }, UnitOfWork.Transaction);
        }
        
        public async Task<Account> GetConsolidatedAccountBySettingIdAsync(int accountSettingId)
        {
            var parameters = new { AccountSettingId = accountSettingId };
            var sqlQuery = @"
                SELECT A.*
                FROM Accounts A
                         LEFT JOIN AccountPlans AP ON AP.Code = A.Code
                         LEFT JOIN dbo.AccountPlanSettings APS on A.Id = APS.AccountId
                         LEFT join dbo.AccountSettings ACS on ACS.Id = APS.AccountSettingId
                WHERE ACS.DeleteDate IS NULL
                  AND ACS.Id =  @AccountSettingId";
            
            var result = await UnitOfWork.Session
                .QueryFirstOrDefaultAsync<Account>(sqlQuery, parameters, UnitOfWork.Transaction);
            return result;
        }
        
        public async Task<Account> GetByAccountSettingIdAsync(int contractId, int accountSettingId)
        {
            var parameters = new { ContractId = contractId, AccountSettingId = accountSettingId };
            var sqlQuery = @"
                SELECT *
                FROM Accounts
                WHERE DeleteDate IS NULL
                  AND AccountSettingId = @AccountSettingId
                  AND ContractId = @ContractId";
            
            return await UnitOfWork.Session
                .QueryFirstOrDefaultAsync<Account>(sqlQuery, parameters, UnitOfWork.Transaction);
        }
        
        /// <summary>
        /// поиск счёта по коду настройки. Лучше использовать для поиска консолидированного счёта
        /// </summary>
        /// <param name="accountCode">AccountSettings.Code</param>
        public async Task<Account> GetConsolidatedAccountBySettingCodeAsync(string accountCode)
        {
            var parameters = new {Code = accountCode };
            
            var sqlQuery = @"
                SELECT top 1 A.*
                FROM Accounts A
                    JOIN dbo.AccountSettings ACS on A.AccountSettingId = ACS.Id
                WHERE A.DeleteDate IS NULL
                  AND ACS.Code = @Code";

            return await UnitOfWork.Session
                .QueryFirstOrDefaultAsync<Account>(sqlQuery, parameters, UnitOfWork.Transaction);
        }
        
        public async Task<Account> GetByAccountSettingCodeAsync(string accountCode, int contractId)
        {
            var parameters = new {Code = accountCode, ContractId = contractId };
            
            var sqlQuery = @"
                SELECT top 1 A.*
                FROM Accounts A
                    JOIN dbo.AccountSettings ACS on A.AccountSettingId = ACS.Id
                WHERE A.DeleteDate IS NULL
                  AND ACS.Code = @Code
                  AND A.ContractId = @ContractId";

            return await UnitOfWork.Session
                .QueryFirstOrDefaultAsync<Account>(sqlQuery, parameters, UnitOfWork.Transaction);
        }

        public Account Find(object query)
        {
            throw new System.NotImplementedException();
        }

        public List<Account> List(ListQuery listQuery, object query = null)
        {
            if (listQuery == null) throw new ArgumentNullException(nameof(listQuery));
            var contractId = query?.Val<int?>("ContractId");
            var isOutmoded = query?.Val<bool?>("IsOutmoded");
            var accountSettingId = query?.Val<int?>("AccountSettingId");
            var isOpen = query?.Val<bool?>("IsOpen");
            var isConsolidated = query?.Val<bool?>("IsConsolidated");
            var accountPlanId = query?.Val<int?>("AccountPlanId");
            var branchId = query?.Val<int?>("BranchId");
            var codes = query?.Val<string[]>("SettingCodes");

            var pre = "acc.DeleteDate IS NULL";
            pre += contractId.HasValue ? " AND acc.ContractId = @contractId" : string.Empty;
            pre += isOutmoded.HasValue ? " AND acc.IsOutmoded = @isOutmoded" : string.Empty;
            pre += accountSettingId.HasValue ? " AND acc.AccountSettingId = @accountSettingId" : string.Empty;
            pre += isConsolidated.HasValue ? " AND accset.IsConsolidated= @isConsolidated" : string.Empty;
            pre += accountPlanId.HasValue ? " AND acc.AccountPlanId = @accountPlanId" : string.Empty;
            pre += branchId.HasValue ? " AND acc.BranchId = @branchId" : string.Empty;
            pre += codes != null && codes.Any() ? " AND accset.code IN @codes" : string.Empty;

            if (isOpen.HasValue)
            {
                pre += isOpen.Value ? " AND acc.CloseDate IS NULL" : " AND acc.CloseDate IS NOT NULL";
            }

            var condition = listQuery.Like(pre, "acc.Code", "acc.Name", "acc.NameAlt");
            var order = listQuery.Order(string.Empty, new Sort
            {
                Name = "acc.AccountSettingId",
                Direction = SortDirection.Asc
            });
            var page = listQuery.Page();

            return UnitOfWork.Session.Query<Account, AccountSetting, Account>($@"
SELECT acc.*, accset.*
FROM Accounts acc
LEFT JOIN AccountSettings accset ON acc.AccountSettingId = accset.Id
{condition} {order} {page}", (acc, accset) =>
            {
                acc.AccountSetting = accset;
                return acc;
            }, new
            {
                listQuery.Page?.Offset,
                listQuery.Page?.Limit,
                listQuery.Filter,
                contractId,
                isOutmoded,
                accountSettingId,
                isConsolidated,
                accountPlanId,
                codes,
                branchId
            }, UnitOfWork.Transaction).ToList();
        }

        public async Task<IEnumerable<Account>> GetMultipleBySettingCodeAsync(IEnumerable<int> contractIds, string accountSettingCode)
        {
            var parameters = new { AccountSettingCode = accountSettingCode, ContractIds = contractIds };
            var sqlQuery = @"
                SELECT A.*
                FROM Accounts A
                    JOIN dbo.AccountSettings ACS on A.AccountSettingId = ACS.Id
                WHERE A.DeleteDate IS NULL
                  AND ACS.Code = @AccountSettingCode
                  AND A.ContractId IN @ContractIds";

            return await UnitOfWork.Session
                .QueryAsync<Account>(sqlQuery, parameters, UnitOfWork.Transaction);
        }

        public async Task<List<Account>> GetAccountsByAccountSettingsAsync(int contractId, List<string> accountCodes)
        {
            var codesString = string.Join(",", accountCodes.Select(code => $"'{code}'"));
            var parameters = new { ContractId = contractId };

            var sqlQuery = $@"
                SELECT  *
                FROM Accounts A
                    JOIN dbo.AccountSettings ACS on A.AccountSettingId = ACS.Id
                WHERE ACS.Code IN ({codesString})
                  AND ContractId = @ContractId";

            var result = await UnitOfWork.Session
                .QueryAsync<Account>(sqlQuery, parameters, UnitOfWork.Transaction);

            return result?.ToList();
        }

        public int Count(ListQuery listQuery, object query = null)
        {
            if (listQuery == null) throw new ArgumentNullException(nameof(listQuery));
            var contractId = query?.Val<int?>("ContractId");
            var isOutmoded = query?.Val<bool?>("IsOutmoded");
            var accountSettingId = query?.Val<int?>("AccountSettingId");
            var isOpen = query?.Val<bool?>("IsOpen");
            var isConsolidated = query?.Val<bool?>("IsConsolidated");
            var accountPlanId = query?.Val<int?>("AccountPlanId");
            var codes = query?.Val<string[]>("SettingCodes");

            var pre = "acc.DeleteDate IS NULL";
            pre += contractId.HasValue ? " AND acc.ContractId = @contractId" : string.Empty;
            pre += isOutmoded.HasValue ? " AND acc.IsOutmoded = @isOutmoded" : string.Empty;
            pre += accountSettingId.HasValue ? " AND acc.AccountSettingId = @accountSettingId" : string.Empty;
            pre += isConsolidated.HasValue ? " AND accset.IsConsolidated= @isConsolidated" : string.Empty;
            pre += accountPlanId.HasValue ? " AND acc.AccountPlanId = @accountPlanId" : string.Empty;
            pre += codes != null && codes.Any() ? " AND accset.code IN @codes" : string.Empty;

            if (isOpen.HasValue)
            {
                pre += isOpen.Value ? " AND acc.CloseDate IS NULL" : " AND acc.CloseDate IS NOT NULL";
            }

            var condition = listQuery.Like(pre, "acc.Code", "acc.Name", "acc.NameAlt");

            return UnitOfWork.Session.ExecuteScalar<int>($@"
SELECT COUNT(*)
FROM Accounts acc
LEFT JOIN AccountSettings accset ON acc.AccountSettingId = accset.Id
{condition}", new
            {
                listQuery.Filter,
                contractId,
                isOutmoded,
                accountSettingId,
                isConsolidated,
                accountPlanId,
                codes
            }, UnitOfWork.Transaction);
        }

        public int RelationCount(int accountId)
        {
            return UnitOfWork.Session.ExecuteScalar<int>(@"
SELECT SUM(AccountCounts.AccountCount)
FROM (
    SELECT COUNT(*) as AccountCount
    FROM ContractActionRows
    WHERE DebitAccountId = @accountId OR CreditAccountId = @accountId
    UNION ALL
    SELECT COUNT(*) as AccountCount
    FROM CashOrders
    WHERE DebitAccountId = @accountId OR CreditAccountId = @accountId
) AccountCounts", new { accountId });
        }

        public string GetNextNumber()
        {
            //TODO: переделать на генерацию из бд? или оставить также?
            return Guid.NewGuid().ToString().Substring(0, 20);
        }

        public IEnumerable<(DateTime, decimal)> GetBalanceForPenaltyAccrual(int accountId, DateTime accrualDate)
        {
            return UnitOfWork.Session.Query<(DateTime, decimal)>(@"SELECT * FROM dbo.GetRecordsForPenaltyAccrual(@accountId,@accrualDate)",
                new
                {
                    accountId,
                    accrualDate
                }, UnitOfWork.Transaction);
        }

        public IEnumerable<(DateTime, decimal)> GetBalanceForPenaltyAccrualForRestructured(int accountId, DateTime accrualDate)
        {
            return UnitOfWork.Session.Query<(DateTime, decimal)>(@"SELECT * FROM dbo.GetRecordsForPenaltyAccrualForRestructured(@accountId,@accrualDate)",
                new
                {
                    accountId,
                    accrualDate
                }, UnitOfWork.Transaction);
        }

        public IEnumerable<(DateTime, decimal)> GetBalanceForInterestAccrualOnOverdueDebt(int accountId, DateTime accrualDate)
        {
            return UnitOfWork.Session.Query<(DateTime, decimal)>(@"SELECT * FROM dbo.GetRecordsForInterestAccrualOnOverdueDebt(@accountId,@accrualDate)",
                new
                {
                    accountId,
                    accrualDate
                }, UnitOfWork.Transaction);
        }

        public decimal GetAccountBalance(int accountId, DateTime date, bool isAccountCurrency = true, bool isOutgoingBalance = true)
        {
            return UnitOfWork.Session.ExecuteScalar<decimal>(@"SELECT ABS(dbo.GetAccountBalanceById(@accountId, @date, @isAccountCurrency, @isOutgoingBalance))",
                new
                {
                    accountId,
                    date,
                    isAccountCurrency,
                    isOutgoingBalance
                }, UnitOfWork.Transaction);
        }

        public async Task<decimal> GetAccountBalanceAsync(int accountId, DateTime date, bool isAccountCurrency = true, bool isOutgoingBalance = true)
        {
            return await UnitOfWork.Session.ExecuteScalarAsync<decimal>(
                @"SELECT ABS(dbo.GetAccountBalanceById(@accountId, @date, @isAccountCurrency, @isOutgoingBalance))",
                new
                {
                    accountId,
                    date,
                    isAccountCurrency,
                    isOutgoingBalance
                }, UnitOfWork.Transaction);
        }

        public List<Pawnshop.AccountingCore.Models.Account> GetAccountsForContractByAccrualType(AccrualType accrualType, int contractId)
        {
            return UnitOfWork.Session.Query<Pawnshop.AccountingCore.Models.Account>(@"
                    SELECT acc.* FROM Accounts acc
                        JOIN AccrualBases ab on ab.BaseSettingId = acc.AccountSettingId
                            WHERE ab.AccrualType = @accrualType
                                AND acc.DeleteDate IS NULL
                                    AND ab.IsActive = 1
                                    AND acc.ContractId = @contractId", new { accrualType, contractId }, UnitOfWork.Transaction).ToList();
        }

        public Account GetBranchMainAccount(int branchId)
        {
            return UnitOfWork.Session.QuerySingleOrDefault<Account>($@"SELECT acc.* 
 FROM AccountPlanSettings aps JOIN AccountSettings ac ON aps.AccountSettingId = ac.Id
JOIN Accounts acc ON aps.AccountId = acc.Id
 WHERE ac.Code = 'CASH'
   AND Aps.BranchId = @branchId",
                new { branchId }, UnitOfWork.Transaction);
        }

        public Account GetBranchDamuAccount(int branchId)
        {
            return UnitOfWork.Session.QuerySingleOrDefault<Account>($@"SELECT acc.* 
 FROM AccountPlanSettings aps JOIN AccountSettings ac ON aps.AccountSettingId = ac.Id
JOIN Accounts acc ON aps.AccountId = acc.Id
 WHERE ac.Code = 'CASH_DAMU'
   AND Aps.BranchId = @branchId",
                new { branchId }, UnitOfWork.Transaction);
        }

        public async Task<ContractBalance> GetBalanceByContractIdAsync(int contractId)
        {
            return await UnitOfWork.Session.QueryFirstOrDefaultAsync<ContractBalance>(@"WITH contractBalance AS (
  SELECT ContractId
         ,ISNULL([DEPO], 0) AS DEPO
         ,ISNULL([ACCOUNT], 0) AS ACCOUNT
         ,ISNULL([PROFIT], 0) AS PROFIT
         ,ISNULL([OVERDUE_ACCOUNT], 0) AS OVERDUE_ACCOUNT
         ,ISNULL([OVERDUE_PROFIT], 0) AS OVERDUE_PROFIT
         ,ISNULL([PENY_PROFIT], 0) AS PENY_PROFIT
         ,ISNULL([PENY_ACCOUNT], 0) AS PENY_ACCOUNT
         ,ISNULL([EXPENSE], 0) AS EXPENSE
    FROM (SELECT a.ContractId
                 ,ISNULL(IIF(ap.IsActive = 1, Balance * -1, a.Balance), 0) AS Balance
                 ,acs.Code
            FROM Accounts a
            JOIN AccountSettings acs ON acs.Id = a.AccountSettingId
            JOIN AccountPlans ap ON ap.Id = a.AccountPlanId
           WHERE a.DeleteDate IS NULL
             AND acs.Code IN ('DEPO', 'ACCOUNT', 'PROFIT', 'OVERDUE_ACCOUNT', 'OVERDUE_PROFIT', 'PENY_PROFIT', 'PENY_ACCOUNT', 'EXPENSE')
             AND a.ContractId = @contractId
         ) AS cb
   PIVOT (
     MAX(cb.Balance)
     FOR cb.Code IN ([DEPO], [ACCOUNT], [PROFIT], [OVERDUE_ACCOUNT], [OVERDUE_PROFIT], [PENY_PROFIT], [PENY_ACCOUNT], [EXPENSE])
   ) AS pivotTable
),
wrapperBalance AS (
SELECT cb.ContractId
       ,IIF(sch.DebtLeft IS NULL, 0, cb.ACCOUNT - ISNULL(sch.DebtLeft, 0)) AS RepaymentAccountAmount
       ,IIF(sch.DebtLeft IS NULL, 0, cb.PROFIT) AS RepaymentProfitAmount
       ,cb.ACCOUNT AS AccountAmount
       ,cb.PROFIT AS ProfitAmount
       ,cb.OVERDUE_ACCOUNT AS OverdueAccountAmount
       ,cb.OVERDUE_PROFIT AS OverdueProfitAmount
       ,cb.PENY_ACCOUNT + cb.PENY_PROFIT AS PenyAmount
       ,cb.EXPENSE AS Expense
       ,cb.DEPO AS PrepaymentBalance
       ,cb.OVERDUE_ACCOUNT + cb.OVERDUE_PROFIT + cb.PENY_ACCOUNT + cb.PENY_PROFIT + cb.EXPENSE AS RepaymentAmount
       ,cb.ACCOUNT + cb.PROFIT + cb.OVERDUE_ACCOUNT + cb.OVERDUE_PROFIT + cb.PENY_ACCOUNT + cb.PENY_PROFIT + cb.EXPENSE AS RedemptionAmount
  FROM contractBalance cb
 OUTER APPLY (SELECT cps.DebtLeft
                FROM ContractPaymentSchedule cps
               WHERE ContractId = cb.ContractId
                 AND Date = cast((select dbo.GETASTANADATE()) as date)
                 AND ActualDate IS NULL
                 AND DeleteDate IS NULL
       ) sch
)
SELECT ContractId
       ,AccountAmount
       ,ProfitAmount
       ,OverdueAccountAmount
       ,OverdueProfitAmount
       ,PenyAmount
       ,AccountAmount + OverdueAccountAmount AS TotalAcountAmount
       ,ProfitAmount + OverdueProfitAmount AS TotalProfitAmount
       ,Expense
       ,PrepaymentBalance
       ,RepaymentAmount + RepaymentAccountAmount + RepaymentProfitAmount AS CurrentDebt
       ,IIF((RepaymentAmount - PrepaymentBalance + RepaymentAccountAmount + RepaymentProfitAmount) < 0, 0, RepaymentAmount - PrepaymentBalance + RepaymentAccountAmount + RepaymentProfitAmount) AS TotalRepaymentAmount
       ,IIF(RedemptionAmount - PrepaymentBalance < 0, 0, RedemptionAmount - PrepaymentBalance) AS TotalRedemptionAmount
  FROM wrapperBalance",
                new { contractId }, UnitOfWork.Transaction);
        }

        public async Task<IEnumerable<MovementsOfDepoAccount>> GetMovementsOfDepoAccountAsync(int contractId)
        {
            var accountId = await UnitOfWork.Session.QueryFirstOrDefaultAsync<int?>(@"SELECT ac.Id
  FROM Accounts ac
  JOIN AccountSettings acs ON acs.Id = ac.AccountSettingId
 WHERE ac.DeleteDate IS NULL
   AND ac.ContractId = @contractId
   AND acs.Code = 'DEPO'",
                new { contractId }, UnitOfWork.Transaction);

            if (!accountId.HasValue)
                return null;

            return await UnitOfWork.Session.QueryAsync<MovementsOfDepoAccount>(@"WITH accountRecordsResult AS (
  SELECT Date
         ,ISNULL([ACCOUNT], 0) AS ACCOUNT
         ,ISNULL([OVERDUE_ACCOUNT], 0) AS OVERDUE_ACCOUNT
         ,ISNULL([PROFIT], 0) AS PROFIT
         ,ISNULL([OVERDUE_PROFIT], 0) AS OVERDUE_PROFIT
         ,ISNULL([PENY_REVENUE], 0) AS PENY_REVENUE
    FROM (SELECT acs.Code,
                 acr.Amount,
                 CAST(acr.Date AS date) AS Date
            FROM AccountRecords acr
            JOIN BusinessOperationSettings bos ON bos.Id = acr.BusinessOperationSettingId
            JOIN BusinessOperations bo ON bo.Id = bos.BusinessOperationId
            JOIN Accounts ac ON ac.Id = acr.CorrAccountId
            JOIN AccountSettings acs ON acs.Id = ac.AccountSettingId
           WHERE bo.Code = 'PAYMENT'
             AND acr.AccountId = @accountId
             AND acr.DeleteDate IS NULL
             AND acs.Code IN ('ACCOUNT', 'OVERDUE_ACCOUNT', 'PROFIT', 'OVERDUE_PROFIT', 'PENY_REVENUE')
         ) AS ar
   PIVOT (
     MAX(ar.Amount)
     FOR ar.Code IN ([ACCOUNT], [OVERDUE_ACCOUNT], [PROFIT], [OVERDUE_PROFIT], [PENY_REVENUE])
    ) AS pivotTable
)
SELECT Date,
       SUM(ACCOUNT + OVERDUE_ACCOUNT) AS TotalAcountAmount,
       SUM(PROFIT + OVERDUE_PROFIT) AS TotalProfitAmount,
       SUM(PENY_REVENUE) AS PenyAmount
  FROM accountRecordsResult
 GROUP BY Date",
                new { accountId }, UnitOfWork.Transaction);
        }

        public async Task<decimal> GetCoborrowerContractsAccountBalance(int clientId)
        {
            return await UnitOfWork.Session.QuerySingleOrDefaultAsync<decimal>(
                   @"              
                    WITH contractBalance AS (
                      SELECT ContractId
                             ,ISNULL([ACCOUNT], 0) AS ACCOUNT
                             ,ISNULL([OVERDUE_ACCOUNT], 0) AS OVERDUE_ACCOUNT
                        FROM (SELECT a.ContractId
                                     ,ISNULL(IIF(ap.IsActive = 1, Balance * -1, a.Balance), 0) AS Balance
                                     ,acs.Code
                                FROM Accounts a
                                JOIN AccountSettings acs ON acs.Id = a.AccountSettingId
                                JOIN AccountPlans ap ON ap.Id = a.AccountPlanId
                               WHERE a.DeleteDate IS NULL
                                 AND acs.Code IN ('ACCOUNT','OVERDUE_ACCOUNT')
                             ) AS cb
                       PIVOT (
                         MAX(cb.Balance)
                         FOR cb.Code IN ([ACCOUNT],[OVERDUE_ACCOUNT])
                       ) AS pivotTable
                    )

                      SELECT
				        SUM(IIF(cb.OVERDUE_ACCOUNT IS NULL,0,cb.OVERDUE_ACCOUNT)) +
				        SUM(IIF(cb.ACCOUNT IS NULL,0,cb.ACCOUNT))
                      FROM ContractLoanSubjects cls
			          LEFT JOIN Contracts c on cls.ContractId = c.Id
			          LEFT JOIN contractBalance cb on cb.ContractId = cls.ContractId
                      LEFT JOIN LoanSubjects ls on cls.SubjectId = ls.Id
	                    WHERE 
					        ls.Code = 'COBORROWER' AND
					        c.Status in (30,50) AND
					        cls.DeleteDate IS NULL
					        and cls.ClientId = @clientId
		             GROUP BY cls.ClientId", new
                   {
                       clientId,
                   }, UnitOfWork.Transaction);
        }

        public async Task<decimal> GetClientActiveContractsAccountBalance(int clientId)
        {
            return await UnitOfWork.Session.QuerySingleOrDefaultAsync<decimal>(
                @"
                WITH contractBalance AS (
                  SELECT ContractId
                         ,ISNULL([ACCOUNT], 0) AS ACCOUNT
                         ,ISNULL([OVERDUE_ACCOUNT], 0) AS OVERDUE_ACCOUNT
                    FROM (SELECT a.ContractId
                                 ,ISNULL(IIF(ap.IsActive = 1, Balance * -1, a.Balance), 0) AS Balance
                                 ,acs.Code
                            FROM Accounts a
                            JOIN AccountSettings acs ON acs.Id = a.AccountSettingId
                            JOIN AccountPlans ap ON ap.Id = a.AccountPlanId
                           WHERE a.DeleteDate IS NULL
                             AND acs.Code IN ('ACCOUNT','OVERDUE_ACCOUNT')
                         ) AS cb
                   PIVOT (
                     MAX(cb.Balance)
                     FOR cb.Code IN ([ACCOUNT],[OVERDUE_ACCOUNT])
                   ) AS pivotTable
                )

			     select 
				    SUM(IIF(cb.OVERDUE_ACCOUNT IS NULL,0,cb.OVERDUE_ACCOUNT)) +
				    SUM(IIF(cb.ACCOUNT IS NULL,0,cb.ACCOUNT))
			     from Clients clt
			     LEFT JOIN Contracts c on clt.Id = c.ClientId
			     LEFT JOIN contractBalance cb on cb.ContractId = c.Id
			      WHERE 
					    c.Status in (30,50) AND c.DeleteDate is null
					    and clt.Id = @ClientId
			    group by clt.Id
            ", new
                {
                    clientId,
                }, UnitOfWork.Transaction);
        }

        public IList<ContractBalance> GetWithRestructedBalances(IList<int> contractIds)
        {
            if (!contractIds.Any())
                return new List<ContractBalance>();

            return UnitOfWork.Session.Query<ContractBalance>(@"DECLARE @date DATE = dbo.GETASTANADATE();

WITH contractBalance AS (
  SELECT ContractId
         ,ISNULL([DEPO], 0) AS DEPO
		 ,ISNULL([DEFERMENT_PROFIT], 0) AS DEFERMENT_PROFIT
		 ,ISNULL([AMORTIZED_PROFIT], 0) AS AMORTIZED_PROFIT
		 ,ISNULL([AMORTIZED_PENY_ACCOUNT], 0) AS AMORTIZED_PENY_ACCOUNT
		 ,ISNULL([AMORTIZED_PENY_PROFIT], 0) AS AMORTIZED_PENY_PROFIT
         ,ISNULL([ACCOUNT], 0) AS ACCOUNT
         ,ISNULL([PROFIT], 0) AS PROFIT
         ,ISNULL([OVERDUE_ACCOUNT], 0) AS OVERDUE_ACCOUNT
         ,ISNULL([OVERDUE_PROFIT], 0) AS OVERDUE_PROFIT
         ,ISNULL([PENY_PROFIT], 0) AS PENY_PROFIT
         ,ISNULL([PENY_ACCOUNT], 0) AS PENY_ACCOUNT
         ,ISNULL([EXPENSE], 0) AS EXPENSE
    FROM (SELECT a.ContractId
                 ,ISNULL(IIF(ap.IsActive = 1, Balance * -1, a.Balance), 0) AS Balance
                 ,acs.Code
            FROM Accounts a
            JOIN AccountSettings acs ON acs.Id = a.AccountSettingId
            JOIN AccountPlans ap ON ap.Id = a.AccountPlanId
           WHERE a.DeleteDate IS NULL
             AND acs.Code IN ('DEPO', 'DEFERMENT_PROFIT', 'AMORTIZED_PROFIT', 'AMORTIZED_PENY_ACCOUNT', 'AMORTIZED_PENY_PROFIT', 'ACCOUNT', 'PROFIT', 'OVERDUE_ACCOUNT', 'OVERDUE_PROFIT', 'PENY_PROFIT', 'PENY_ACCOUNT', 'EXPENSE')
             AND a.ContractId IN @contractIds
         ) AS cb
   PIVOT (
     MAX(cb.Balance)
     FOR cb.Code IN ([DEPO], [DEFERMENT_PROFIT], [AMORTIZED_PROFIT], [AMORTIZED_PENY_ACCOUNT], [AMORTIZED_PENY_PROFIT], [ACCOUNT], [PROFIT], [OVERDUE_ACCOUNT], [OVERDUE_PROFIT], [PENY_PROFIT], [PENY_ACCOUNT], [EXPENSE])
   ) AS pivotTable
),
wrapperBalance AS (
SELECT cb.ContractId
	   ,cb.ACCOUNT AS AccountAmount
	   ,cb.DEFERMENT_PROFIT as DefermentProfit
	   ,cb.AMORTIZED_PROFIT as AmortizedProfit
	   ,cb.AMORTIZED_PENY_ACCOUNT as AmortizedPenyAccount
	   ,cb.AMORTIZED_PENY_PROFIT as AmortizedPenyProfit
       ,IIF(sch.DebtLeft IS NULL, 0, cb.ACCOUNT - ISNULL(sch.DebtLeft, 0)) AS RepaymentAccountAmount
       ,IIF(sch.DebtLeft IS NULL, 0, cb.PROFIT - ISNULL(accrued_profit.amount, 0)) AS RepaymentProfitAmount
       ,cb.PROFIT AS ProfitAmount
       ,cb.OVERDUE_ACCOUNT AS OverdueAccountAmount
       ,cb.OVERDUE_PROFIT AS OverdueProfitAmount
       ,cb.PENY_ACCOUNT + cb.PENY_PROFIT  AS PenyAmount
	   ,cb.DEFERMENT_PROFIT + cb.AMORTIZED_PROFIT + cb.AMORTIZED_PENY_ACCOUNT + cb.AMORTIZED_PENY_PROFIT AS AmortizedAmount
       ,cb.EXPENSE AS Expense
       ,cb.DEPO AS PrepaymentBalance
       ,cb.OVERDUE_ACCOUNT + cb.OVERDUE_PROFIT + cb.PENY_ACCOUNT + cb.PENY_PROFIT + cb.EXPENSE AS RepaymentAmount
       ,cb.ACCOUNT + cb.PROFIT + cb.OVERDUE_ACCOUNT + cb.OVERDUE_PROFIT + cb.PENY_ACCOUNT + cb.PENY_PROFIT + cb.EXPENSE AS RedemptionAmount
  FROM contractBalance cb
 OUTER APPLY (SELECT cps.DebtLeft, cps.Date
                FROM ContractPaymentSchedule cps
               WHERE ContractId = cb.ContractId
                 AND @date BETWEEN Date AND ISNULL(cps.NextWorkingDate, Date)
                 AND ActualDate IS NULL
                 AND DeleteDate IS NULL
       ) sch
OUTER APPLY (SELECT TOP 1 ROUND(cps.PercentCost * DATEDIFF(DAY, sch.Date, @date) / IIF(cps.Period = 0, 30, cps.Period), 2) AS amount
                FROM ContractPaymentSchedule cps
               WHERE cps.ContractId = cb.ContractId
                 AND cps.DeleteDate IS NULL
                 AND cps.Date > sch.Date
                 AND EXISTS (SELECT * FROM dbo.CashOrders co JOIN dbo.BusinessOperationSettings bos ON co.BusinessOperationSettingId = bos.Id
                              WHERE co.ContractId = cb.ContractId
                                AND co.DeleteDate IS NULL
                                AND co.OrderDate >= DATEADD(DAY, 1, sch.Date)
                                AND co.OrderDate < DATEADD(DAY, 1, @date)
                                AND co.OrderType = 30
                                AND co.ApproveStatus = 10
                                AND bos.Code = 'INTEREST_ACCRUAL')
               ORDER BY cps.Date) accrued_profit
)
SELECT ContractId
       ,AccountAmount
	   ,DefermentProfit
	   ,AmortizedProfit
	   ,AmortizedPenyAccount
	   ,AmortizedPenyProfit
       ,ProfitAmount
       ,OverdueAccountAmount
       ,OverdueProfitAmount
       ,PenyAmount
       ,AccountAmount + OverdueAccountAmount + DefermentProfit AS TotalAcountAmount
       ,ProfitAmount + OverdueProfitAmount + AmortizedProfit AS TotalProfitAmount
       ,Expense
       ,PrepaymentBalance
       ,RepaymentAmount + RepaymentAccountAmount + RepaymentProfitAmount AS CurrentDebt
       ,IIF((RepaymentAmount - PrepaymentBalance + RepaymentAccountAmount + RepaymentProfitAmount) < 0, 0, RepaymentAmount - PrepaymentBalance + RepaymentAccountAmount + RepaymentProfitAmount) AS TotalRepaymentAmount
       ,IIF(RedemptionAmount - PrepaymentBalance + AmortizedAmount < 0, 0, RedemptionAmount - PrepaymentBalance + AmortizedAmount ) AS TotalRedemptionAmount
  FROM wrapperBalance",
            new { contractIds }, UnitOfWork.Transaction).ToList();
        }
    }
}