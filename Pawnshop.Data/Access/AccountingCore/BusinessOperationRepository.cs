using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dapper;
using Pawnshop.Core;
using Pawnshop.Core.Impl;
using Pawnshop.Core.Queries;
using Pawnshop.Data.Models.AccountingCore;
using Type = Pawnshop.AccountingCore.Models.Type;

namespace Pawnshop.Data.Access.AccountingCore
{
    public class BusinessOperationRepository : RepositoryBase, IRepository<BusinessOperation>
    {
        public BusinessOperationRepository(IUnitOfWork unitOfWork) : base(unitOfWork)
        {
        }

        public void Insert(BusinessOperation entity)
        {
            using var transaction = BeginTransaction();

            entity.Id = UnitOfWork.Session.QuerySingleOrDefault<int>(@"
INSERT INTO BusinessOperations ( Code, Name, NameAlt, TypeId, OrganizationId, BranchId, CreateDate, AuthorId, IsManual, OrdersCreateStatus, HasExpenseArticleType )
VALUES ( @Code, @Name, @NameAlt, @TypeId, @OrganizationId, @BranchId, @CreateDate, @AuthorId, @IsManual, @OrdersCreateStatus, @HasExpenseArticleType  )
SELECT SCOPE_IDENTITY()", entity, UnitOfWork.Transaction);

            transaction.Commit();
        }

        public void Update(BusinessOperation entity)
        {
            using var transaction = BeginTransaction();

            UnitOfWork.Session.Execute(@"
UPDATE BusinessOperations
SET Code = @Code, Name = @Name, NameAlt = @NameAlt, TypeId = @TypeId, OrganizationId = @OrganizationId, BranchId = @BranchId, IsManual = @IsManual, 
    OrdersCreateStatus = @OrdersCreateStatus, HasExpenseArticleType = @HasExpenseArticleType
WHERE Id = @Id", entity, UnitOfWork.Transaction);

            transaction.Commit();
        }

        public void Delete(int id)
        {
            using var transaction = BeginTransaction();

            UnitOfWork.Session.Execute(@"UPDATE BusinessOperations SET DeleteDate = dbo.GETASTANADATE() WHERE Id = @id", new {id}, UnitOfWork.Transaction);

            transaction.Commit();
        }

        public BusinessOperation Get(int id)
        {
            return UnitOfWork.Session.QuerySingleOrDefault<BusinessOperation>(@"
SELECT *
FROM BusinessOperations
WHERE Id = @id", new {id}, UnitOfWork.Transaction);
        }

        public async Task<BusinessOperation> GetAsync(int id)
        {
            return await UnitOfWork.Session.QueryFirstOrDefaultAsync<BusinessOperation>(@"
SELECT *
FROM BusinessOperations
WHERE Id = @id", new { id }, UnitOfWork.Transaction);
        }

        public BusinessOperation Find(object query)
        {
            return UnitOfWork.Session.QuerySingleOrDefault<BusinessOperation>(@"
SELECT TOP 1 *
FROM BusinessOperations", UnitOfWork.Transaction);
        }
        
        public List<BusinessOperation> List(ListQuery listQuery, object query = null)
        {
            if (listQuery == null) throw new ArgumentNullException(nameof(listQuery));
            var typeId = query?.Val<int?>("TypeId");
            var code = query?.Val<string>("Code");
            var organizationId = query?.Val<int?>("OrganizationId");
            var branchId = query?.Val<int?>("BranchId");
            var accountId = query?.Val<int?>("AccountId");
            var isManual = query?.Val<bool?>("IsManual");

            var pre = "bo.DeleteDate IS NULL";
            var from = "FROM BusinessOperations bo WITH (NOLOCK)";

            pre += typeId.HasValue ? " AND bo.TypeId = @typeId" : string.Empty;
            pre += string.IsNullOrWhiteSpace(code) ? string.Empty : " AND bo.Code = @code";
            pre += organizationId.HasValue ? " AND (bo.OrganizationId IS NULL OR bo.OrganizationId = @organizationId)" : string.Empty;
            pre += branchId.HasValue ? " AND (bo.BranchId IS NULL OR bo.BranchId = @branchId)" : string.Empty;
            pre += isManual.HasValue ? " AND bo.IsManual = @isManual" : string.Empty;

            if (accountId.HasValue)
            {
                from += " JOIN BusinessOperationSettings bos ON bos.BusinessOperationId = bo.Id JOIN AccountRecords ar ON ar.BusinessOperationSettingId = bos.Id AND ar.AccountId = @accountId";
            }

            var condition = listQuery.Like(pre, "Code", "Name", "NameAlt");
            var order = listQuery.Order(string.Empty, new Sort
            {
                Name = "Name",
                Direction = SortDirection.Asc
            });
            var page = listQuery.Page();

            return UnitOfWork.Session.Query<BusinessOperation>($@"
SELECT DISTINCT bo.*
{from}
{condition} {order} {page}", new
            {
                listQuery.Page?.Offset,
                listQuery.Page?.Limit,
                listQuery.Filter,
                typeId,
                code,
                organizationId,
                branchId,
                accountId,
                isManual
            }, UnitOfWork.Transaction).AsList();
        }

        public async Task<List<BusinessOperation>> ListAsync(BusinessOperationQueryFilter listQuery)
        {
            if (listQuery == null) throw new ArgumentNullException(nameof(listQuery));

            var typeId = listQuery.TypeId;
            var code = listQuery.Code;
            var organizationId = listQuery.OrganizationId;
            var branchId = listQuery.BranchId;
            var accountId = listQuery.AccountId;
            var isManual = listQuery.IsManual;

            var whereConditions = new List<string> { "bo.DeleteDate IS NULL" };
            var fromClause = new StringBuilder("FROM BusinessOperations bo WITH (NOLOCK)");

            if (typeId.HasValue)
                whereConditions.Add("bo.TypeId = @typeId");

            if (!string.IsNullOrWhiteSpace(code))
                whereConditions.Add("bo.Code = @code");

            if (organizationId.HasValue)
                whereConditions.Add("(bo.OrganizationId IS NULL OR bo.OrganizationId = @organizationId)");

            if (branchId.HasValue)
                whereConditions.Add("(bo.BranchId IS NULL OR bo.BranchId = @branchId)");

            if (isManual.HasValue)
                whereConditions.Add("bo.IsManual = @isManual");

            if (accountId.HasValue)
            {
                fromClause.Append(" JOIN BusinessOperationSettings bos ON bos.BusinessOperationId = bo.Id ");
                fromClause.Append(
                    " JOIN AccountRecords ar ON ar.BusinessOperationSettingId = bos.Id AND ar.AccountId = @accountId");
            }

            var whereClause = string.Join(" AND ", whereConditions);

            var query = $@"
                SELECT DISTINCT bo.* 
                {fromClause}
                WHERE {whereClause}";

            var result = await UnitOfWork.Session.QueryAsync<BusinessOperation>(
                query,
                new
                {
                    typeId,
                    code,
                    organizationId,
                    branchId,
                    accountId,
                    isManual
                },
                UnitOfWork.Transaction
            );

            return result.AsList();
        }

        public int Count(ListQuery listQuery, object query = null)
        {
            if (listQuery == null) throw new ArgumentNullException(nameof(listQuery));
            var typeId = query?.Val<int?>("TypeId");
            var code = query?.Val<string>("Code");
            var organizationId = query?.Val<int?>("OrganizationId");
            var branchId = query?.Val<int?>("BranchId");
            var accountId = query?.Val<int?>("AccountId");
            var isManual = query?.Val<bool?>("IsManual");

            var pre = "bo.DeleteDate IS NULL";
            var from = "FROM BusinessOperations bo";

            pre += typeId.HasValue ? " AND bo.TypeId = @typeId" : string.Empty;
            pre += string.IsNullOrWhiteSpace(code) ? string.Empty : " AND bo.Code = @code";
            pre += organizationId.HasValue ? " AND (bo.OrganizationId IS NULL OR bo.OrganizationId = @organizationId)" : string.Empty;
            pre += branchId.HasValue ? " AND (bo.BranchId IS NULL OR bo.BranchId = @branchId)" : string.Empty;
            pre += isManual.HasValue ? " AND bo.IsManual = @isManual" : string.Empty;

            if (accountId.HasValue)
            {
                from += " JOIN BusinessOperationSettings bos ON bos.BusinessOperationId = bo.Id JOIN AccountRecords ar ON ar.BusinessOperationSettingId = bos.Id AND ar.AccountId = @accountId";
            }

            var condition = listQuery.Like(pre, "Code", "Name", "NameAlt");

            return UnitOfWork.Session.ExecuteScalar<int>($@"
SELECT COUNT(DISTINCT bo.Id)
{from}
{condition}", new
            {
                listQuery.Filter,
                typeId,
                code,
                organizationId,
                branchId,
                accountId,
                isManual
            }, UnitOfWork.Transaction);
        }

        public int RelationCount(int id)
        {
            return UnitOfWork.Session.ExecuteScalar<int>(@"
SELECT SUM(BusinessOperationCounts.BusinessOperationCount)
FROM (
    SELECT COUNT(*) as BusinessOperationCount FROM CashOrders WHERE BusinessOperationId = @id
    UNION ALL
    SELECT COUNT(*) as BusinessOperationCount FROM AccountRecords WHERE BusinessOperationId = @id
) BusinessOperationCounts", new { id });
        }
    }
}