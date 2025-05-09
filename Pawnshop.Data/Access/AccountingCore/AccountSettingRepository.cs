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
    public class AccountSettingRepository : RepositoryBase, IRepository<AccountSetting>
    {
        public AccountSettingRepository(IUnitOfWork unitOfWork) : base(unitOfWork)
        {
        }

        public void Insert(AccountSetting entity)
        {
            using var transaction = BeginTransaction();

            entity.Id = UnitOfWork.Session.QuerySingleOrDefault<int>(@"
                INSERT INTO AccountSettings 
                    (Code, Name, NameAlt, TypeId, IsConsolidated, CreateDate, AuthorId, DefaultAmountType, SearchBranchBySessionContext)
                VALUES 
                    ( @Code, @Name, @NameAlt, @TypeId, @IsConsolidated, @CreateDate, @AuthorId, @DefaultAmountType, @SearchBranchBySessionContext)
                SELECT SCOPE_IDENTITY()", entity, UnitOfWork.Transaction);

            transaction.Commit();
        }

        public void Update(AccountSetting entity)
        {
            using var transaction = BeginTransaction();

            UnitOfWork.Session.Execute(@"
                UPDATE AccountSettings
                SET 
                    Code = @Code, 
                    Name = @Name, 
                    NameAlt = @NameAlt, 
                    TypeId = @TypeId, 
                    IsConsolidated = @IsConsolidated, 
                    DefaultAmountType = @DefaultAmountType, 
                    SearchBranchBySessionContext = @SearchBranchBySessionContext
                WHERE Id = @Id", entity, UnitOfWork.Transaction);

            transaction.Commit();
        }

        public void Delete(int id)
        {
            using var transaction = BeginTransaction();

            UnitOfWork.Session.Execute(@"UPDATE AccountSettings SET DeleteDate = dbo.GETASTANADATE() WHERE Id = @id", new {id}, UnitOfWork.Transaction);

            transaction.Commit();
        }

        public AccountSetting Get(int id)
        {
            return UnitOfWork.Session.QuerySingleOrDefault<AccountSetting>(@"
SELECT *
FROM AccountSettings
WHERE Id = @id", new { id }, UnitOfWork.Transaction);
        }

        public async Task<AccountSetting> GetAsync(int id)
        {
            return await UnitOfWork.Session.QueryFirstOrDefaultAsync<AccountSetting>(@"
SELECT *
FROM AccountSettings
WHERE Id = @id", new { id }, UnitOfWork.Transaction);
        }

        public AccountSetting Find(object query)
        {
            return UnitOfWork.Session.QuerySingleOrDefault<AccountSetting>(@"
SELECT TOP 1 *
FROM AccountSettings");
        }

        public List<AccountSetting> List(ListQuery listQuery, object query = null)
        {
            if (listQuery == null) throw new ArgumentNullException(nameof(listQuery));
            var typeId = query?.Val<int?>("TypeId");
            var code = query?.Val<string>("Code");
            var isConsolidated = query?.Val<bool?>("IsConsolidated");
            var withAllParents = query?.Val<bool?>("WithAllParents");

            var pre = "accset.DeleteDate IS NULL";
            pre += isConsolidated.HasValue ? " AND accset.IsConsolidated = @isConsolidated" : string.Empty;
            pre += !string.IsNullOrEmpty(code) ? " AND accset.code = @code" : string.Empty;

            if (!(withAllParents.HasValue && withAllParents.Value && typeId.HasValue))
            {
                pre += typeId.HasValue ? " AND accset.TypeId = @typeId" : string.Empty;
            }

            var condition = listQuery.Like(pre, "accset.Code");

            var order = listQuery.Order(string.Empty, new Sort
            {
                Name = "accset.Code",
                Direction = SortDirection.Asc
            });
            var page = listQuery.Page();

            return UnitOfWork.Session.Query<AccountSetting>($@"
SELECT accset.*
FROM AccountSettings accset WITH (NOLOCK)
{((withAllParents.HasValue && withAllParents.Value && typeId.HasValue) ? "JOIN (SELECT * FROM dbo.ListTypeHierarchy(@typeId)) AS t ON t.Id = accset.TypeId" : string.Empty)}
{condition} {order} {page}", new
            {
                listQuery.Page?.Offset,
                listQuery.Page?.Limit,
                listQuery.Filter,
                typeId,
                isConsolidated,
                code
            }, UnitOfWork.Transaction).AsList();
        }

        public int Count(ListQuery listQuery, object query = null)
        {
            if (listQuery == null) throw new ArgumentNullException(nameof(listQuery));
            var typeId = query?.Val<int?>("TypeId");
            var code = query?.Val<string>("Code");
            var isConsolidated = query?.Val<bool?>("IsConsolidated");
            var withAllParents = query?.Val<bool?>("WithAllParents");

            var pre = "accset.DeleteDate IS NULL";
            pre += isConsolidated.HasValue ? " AND accset.IsConsolidated = @isConsolidated" : string.Empty;
            pre += !string.IsNullOrEmpty(code) ? " AND accset.code = @code" : string.Empty;

            if (!(withAllParents.HasValue && withAllParents.Value && typeId.HasValue))
            {
                pre += typeId.HasValue ? " AND accset.TypeId = @typeId" : string.Empty;
            }

            var condition = listQuery.Like(pre, "accset.Code");

            return UnitOfWork.Session.ExecuteScalar<int>($@"
SELECT COUNT(*)
FROM AccountSettings accset
{((withAllParents.HasValue && withAllParents.Value && typeId.HasValue) ? "JOIN (SELECT * FROM dbo.ListTypeHierarchy(@typeId)) AS t ON t.Id = accset.TypeId" : string.Empty)}
{condition}", new
            {
                listQuery.Filter,
                typeId,
                isConsolidated,
                code
            }, UnitOfWork.Transaction);
        }

        public int RelationCount(int id)
        {
            return UnitOfWork.Session.ExecuteScalar<int>(@"
SELECT SUM(AccountSettingCounts.AccountSettingCount)
FROM (
    SELECT COUNT(*) as AccountSettingCount FROM CashOrders WHERE AccountSettingId = @id
    UNION ALL
    SELECT COUNT(*) as AccountSettingCount FROM AccountRecords WHERE AccountSettingId = @id
) AccountSettingCounts", new { id });
        }
    }
}