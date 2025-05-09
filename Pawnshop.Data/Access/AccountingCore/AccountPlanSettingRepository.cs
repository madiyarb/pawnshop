using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using Pawnshop.Core;
using Pawnshop.Core.Impl;
using Pawnshop.Core.Queries;
using Pawnshop.Data.Models.AccountingCore;

namespace Pawnshop.Data.Access.AccountingCore
{
    public class AccountPlanSettingRepository : RepositoryBase, IRepository<AccountPlanSetting>
    {
        private readonly AccountRepository _accountRepository;
        public AccountPlanSettingRepository(IUnitOfWork unitOfWork, AccountRepository accountRepository) : base(unitOfWork)
        {
            _accountRepository = accountRepository;
        }

        public void Insert(AccountPlanSetting entity)
        {
            using var transaction = BeginTransaction();

            entity.Id = UnitOfWork.Session.QuerySingleOrDefault<int>(@"
INSERT INTO AccountPlanSettings ( AccountSettingId, AccountPlanId, ContractTypeId, PeriodTypeId, AccountId, OrganizationId, BranchId, CreateDate, AuthorId )
VALUES ( @AccountSettingId, @AccountPlanId, @ContractTypeId, @PeriodTypeId, @AccountId, @OrganizationId, @BranchId, @CreateDate, @AuthorId  )
SELECT SCOPE_IDENTITY()", entity, UnitOfWork.Transaction);

            transaction.Commit();
        }

        public void Update(AccountPlanSetting entity)
        {
            using var transaction = BeginTransaction();

            UnitOfWork.Session.Execute(@"
UPDATE AccountPlanSettings
SET AccountSettingId = @AccountSettingId, AccountPlanId = @AccountPlanId, ContractTypeId = @ContractTypeId, PeriodTypeId = @PeriodTypeId,
AccountId = @AccountId, OrganizationId = @OrganizationId, BranchId = @BranchId
WHERE Id = @Id", entity, UnitOfWork.Transaction);

            transaction.Commit();
        }

        public void Delete(int id)
        {
            using var transaction = BeginTransaction();

            UnitOfWork.Session.Execute(@"UPDATE AccountPlanSettings SET DeleteDate = dbo.GETASTANADATE() WHERE Id = @id", new {id}, UnitOfWork.Transaction);

            transaction.Commit();
        }

        public AccountPlanSetting Get(int id)
        {
            return UnitOfWork.Session.Query<AccountPlanSetting, Account, AccountPlanSetting>(@"
SELECT aps.*, a.*
FROM AccountPlanSettings aps
LEFT JOIN Accounts a ON set.AccountId = a.Id
WHERE aps.Id = @id", (set, a) =>
            {
                set.Account = a;
                return set;
            }, new { id }).FirstOrDefault();
        }
        public async Task<AccountPlanSetting> GetAsync(int id)
        {
            var set= await UnitOfWork.Session.QueryFirstOrDefaultAsync<AccountPlanSetting>(@"
SELECT aps.*
FROM AccountPlanSettings aps
WHERE aps.Id = @id", new { id });


            return set;
        }

        public AccountPlanSetting Find(object query)
        {
            return UnitOfWork.Session.QuerySingleOrDefault<AccountPlanSetting>(@"
SELECT TOP 1 *
FROM AccountPlanSettings");
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="listQuery"></param>
        /// <param name="query">
        /// Возможные значения(фильтры):
        /// AccountPlanId, AccountSettingId, ContractTypeId, PeriodTypeId, AccountId, OrganizationId, BranchId
        /// </param>
        /// <returns></returns>
        public List<AccountPlanSetting> List(ListQuery listQuery, object query = null)
        {
            if (listQuery == null) throw new ArgumentNullException(nameof(listQuery));
            var planId = query?.Val<int?>("AccountPlanId");
            var accountSettingId = query?.Val<int?>("AccountSettingId");
            var contractTypeId = query?.Val<int?>("ContractTypeId");
            var periodTypeId = query?.Val<int?>("PeriodTypeId");
            var accountId = query?.Val<int?>("AccountId");
            var organizationId = query?.Val<int?>("OrganizationId");
            var branchId = query?.Val<int?>("BranchId");

            var pre = "aps.DeleteDate IS NULL";
            pre += planId.HasValue ? " AND (aps.AccountPlanId IS NULL OR aps.AccountPlanId = @planId)" : string.Empty;
            pre += accountSettingId.HasValue ? " AND aps.AccountSettingId = @accountSettingId" : string.Empty;
            pre += contractTypeId.HasValue ? " AND aps.ContractTypeId = @contractTypeId" : string.Empty;
            pre += periodTypeId.HasValue ? " AND aps.PeriodTypeId = @periodTypeId" : string.Empty;
            pre += accountId.HasValue ? " AND (aps.AccountId IS NULL OR aps.AccountId = @accountId)" : string.Empty;
            pre += organizationId.HasValue ? " AND (aps.OrganizationId IS NULL OR aps.OrganizationId = @organizationId)" : string.Empty;
            pre += branchId.HasValue ? " AND (aps.BranchId IS NULL OR aps.BranchId = @branchId)" : string.Empty;

            var condition = listQuery.Like(pre, "a.Code");

            var order = listQuery.Order(string.Empty, new Sort
            {
                Name = "aps.Id",
                Direction = SortDirection.Desc
            });
            var page = listQuery.Page();

            return UnitOfWork.Session.Query<AccountPlanSetting, Account, AccountPlanSetting>($@"
SELECT aps.*, a.*
FROM AccountPlanSettings aps WITH (NOLOCK)
LEFT JOIN Accounts a ON aps.AccountId = a.Id
{condition} {order} {page}", (aps, a) =>
            {
                aps.Account = a;
                return aps;
            }, new
            {
                listQuery.Page?.Offset,
                listQuery.Page?.Limit,
                listQuery.Filter,
                planId,
                accountSettingId,
                contractTypeId,
                periodTypeId,
                accountId,
                organizationId,
                branchId
            }, UnitOfWork.Transaction).AsList();
        }

        public int Count(ListQuery listQuery, object query = null)
        {
            if (listQuery == null) throw new ArgumentNullException(nameof(listQuery));
            var planId = query?.Val<int?>("AccountPlanId");
            var accountSettingId = query?.Val<int?>("AccountSettingId");
            var contractTypeId = query?.Val<int?>("ContractTypeId");
            var periodTypeId = query?.Val<int?>("PeriodTypeId");
            var accountId = query?.Val<int?>("AccountId");
            var organizationId = query?.Val<int?>("OrganizationId");
            var branchId = query?.Val<int?>("BranchId");

            var pre = "aps.DeleteDate IS NULL";
            pre += planId.HasValue ? " AND (aps.AccountPlanId IS NULL OR aps.AccountPlanId = @planId)" : string.Empty;
            pre += accountSettingId.HasValue ? " AND (aps.AccountSettingId IS NULL OR aps.AccountSettingId = @accountSettingId)" : string.Empty;
            pre += contractTypeId.HasValue ? " AND (aps.ContractTypeId IS NULL OR aps.ContractTypeId = @contractTypeId)" : string.Empty;
            pre += periodTypeId.HasValue ? " AND (aps.PeriodTypeId IS NULL OR aps.PeriodTypeId = @periodTypeId)" : string.Empty;
            pre += accountId.HasValue ? " AND (aps.AccountId IS NULL OR aps.AccountId = @accountId)" : string.Empty;
            pre += organizationId.HasValue ? " AND (aps.OrganizationId IS NULL OR aps.OrganizationId = @organizationId)" : string.Empty;
            pre += branchId.HasValue ? " AND (aps.BranchId IS NULL OR aps.BranchId = @branchId)" : string.Empty;

            var condition = listQuery.Like(pre, "a.Code");

            return UnitOfWork.Session.ExecuteScalar<int>($@"
SELECT COUNT(*)
FROM AccountPlanSettings aps
LEFT JOIN Accounts a ON aps.AccountId = a.Id
{condition}", new
            {
                listQuery.Filter,
                planId,
                accountSettingId,
                contractTypeId,
                periodTypeId,
                accountId,
                organizationId,
                branchId
            }, UnitOfWork.Transaction);
        }

        public int RelationCount(int id)
        {
            return UnitOfWork.Session.ExecuteScalar<int>(@"
SELECT SUM(AccountPlanSettingCounts.AccountPlanSettingCount)
FROM (
    SELECT 1 as AccountPlanSettingCount
) AccountPlanSettingCounts", new { id });
        }
    }
}