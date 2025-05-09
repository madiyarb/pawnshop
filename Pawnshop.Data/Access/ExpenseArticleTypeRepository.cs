using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using Pawnshop.Core;
using Pawnshop.Core.Impl;
using Pawnshop.Core.Queries;
using Pawnshop.Data.Models.CashOrders;

namespace Pawnshop.Data.Access.AccountingCore
{
    public class ExpenseArticleTypeRepository : RepositoryBase, IRepository<ExpenseArticleType>
    {
        public ExpenseArticleTypeRepository(IUnitOfWork unitOfWork) : base(unitOfWork)
        {
        }

        public void Insert(ExpenseArticleType entity)
        {
            using var transaction = BeginTransaction();

            entity.Id = UnitOfWork.Session.QuerySingleOrDefault<int>(@"
INSERT INTO ExpenseArticleTypes ( Code, Name, NameAlt, CreateDate, AuthorId )
VALUES ( @Code, @Name, @NameAlt, @CreateDate, @AuthorId)
SELECT SCOPE_IDENTITY()", entity, UnitOfWork.Transaction);

            transaction.Commit();
        }

        public void Update(ExpenseArticleType entity)
        {
            using var transaction = BeginTransaction();

            UnitOfWork.Session.Execute(@"
UPDATE ExpenseArticleTypes
SET Code = @Code, Name = @Name, NameAlt = @NameAlt
WHERE Id = @Id", entity, UnitOfWork.Transaction);

            transaction.Commit();
        }

        public void Delete(int id)
        {
            using var transaction = BeginTransaction();

            UnitOfWork.Session.Execute(@"UPDATE ExpenseArticleTypes SET DeleteDate = dbo.GETASTANADATE() WHERE Id = @id", new {id}, UnitOfWork.Transaction);

            transaction.Commit();
        }

        public ExpenseArticleType Get(int id)
        {
            return UnitOfWork.Session.QuerySingleOrDefault<ExpenseArticleType>(@"
SELECT *
FROM ExpenseArticleTypes
WHERE Id = @id", new {id}, UnitOfWork.Transaction);
        }

        public async Task<ExpenseArticleType> GetAsync(int id)
        {
            return await UnitOfWork.Session.QueryFirstOrDefaultAsync<ExpenseArticleType>(@"
SELECT *
FROM ExpenseArticleTypes
WHERE Id = @id", new { id }, UnitOfWork.Transaction);
        }

        public ExpenseArticleType Find(object query)
        {
            return UnitOfWork.Session.QuerySingleOrDefault<ExpenseArticleType>(@"
SELECT TOP 1 *
FROM ExpenseArticleTypes", UnitOfWork.Transaction);
        }

        public List<ExpenseArticleType> List(ListQuery listQuery, object query = null)
        {
            if (listQuery == null) throw new ArgumentNullException(nameof(listQuery));
            var code = query?.Val<string>("Code");
   
            var pre = "eat.DeleteDate IS NULL";
            var from = "FROM ExpenseArticleTypes eat WITH (NOLOCK)";

            pre += string.IsNullOrWhiteSpace(code) ? string.Empty : " AND eat.Code = @code";
            
            var condition = listQuery.Like(pre, "Code", "Name", "NameAlt");
            var order = listQuery.Order(string.Empty, new Sort
            {
                Name = "Code",
                Direction = SortDirection.Asc
            });
            var page = listQuery.Page();

            return UnitOfWork.Session.Query<ExpenseArticleType>($@"
SELECT DISTINCT eat.*
{from}
{condition} {order} {page}", new
            {
                listQuery.Page?.Offset,
                listQuery.Page?.Limit,
                listQuery.Filter,
                code
            }, UnitOfWork.Transaction).AsList();
        }

        public int Count(ListQuery listQuery, object query = null)
        {
            if (listQuery == null) throw new ArgumentNullException(nameof(listQuery));
            var code = query?.Val<string>("Code");

            var pre = "eat.DeleteDate IS NULL";
            var from = "FROM ExpenseArticleTypes eat";

            pre += string.IsNullOrWhiteSpace(code) ? string.Empty : " AND eat.Code = @code";
          
            var condition = listQuery.Like(pre, "Code", "Name", "NameAlt");

            return UnitOfWork.Session.ExecuteScalar<int>($@"
SELECT COUNT(DISTINCT eat.Id)
{from}
{condition}", new
            {
                listQuery.Filter,
                code,
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