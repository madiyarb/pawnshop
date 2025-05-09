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
    public class AccountPlanRepository : RepositoryBase, IRepository<AccountPlan>
    {
        public AccountPlanRepository(IUnitOfWork unitOfWork) : base(unitOfWork)
        {
        }

        public void Insert(AccountPlan entity)
        {
            using var transaction = BeginTransaction();

            entity.Id = UnitOfWork.Session.QuerySingleOrDefault<int>(@"
INSERT INTO AccountPlans ( Code, Name, NameAlt, OrganizationId, IsActive, IsBalance, CreateDate, AuthorId )
VALUES ( @Code, @Name, @NameAlt, @OrganizationId, @IsActive, @IsBalance, @CreateDate, @AuthorId  )
SELECT SCOPE_IDENTITY()", entity, UnitOfWork.Transaction);

            transaction.Commit();
        }

        public void Update(AccountPlan entity)
        {
            using var transaction = BeginTransaction();

            UnitOfWork.Session.Execute(@"
UPDATE AccountPlans
SET Code = @Code, Name = @Name, NameAlt = @NameAlt, OrganizationId = @OrganizationId, IsActive = @IsActive, IsBalance = @IsBalance
WHERE Id = @Id", entity, UnitOfWork.Transaction);

            transaction.Commit();
        }

        public void Delete(int id)
        {
            using var transaction = BeginTransaction();

            UnitOfWork.Session.Execute(@"UPDATE AccountPlans SET DeleteDate = dbo.GETASTANADATE() WHERE Id = @id", new { id }, UnitOfWork.Transaction);

            transaction.Commit();
        }

        public AccountPlan Get(int id)
        {
            return UnitOfWork.Session.QuerySingleOrDefault<AccountPlan>(@"
SELECT *
FROM AccountPlans
WHERE Id = @id", new { id }, UnitOfWork.Transaction);
        }

        public async Task<AccountPlan> GetAsync(int id)
        {
            return await UnitOfWork.Session.QueryFirstOrDefaultAsync<AccountPlan>(@"
SELECT *
FROM AccountPlans
WHERE Id = @id", new { id }, UnitOfWork.Transaction);
        }

        public AccountPlan Find(object query)
        {
            return UnitOfWork.Session.QuerySingleOrDefault<AccountPlan>(@"
SELECT TOP 1 *
FROM AccountPlans", UnitOfWork.Transaction);
        }

        public List<AccountPlan> List(ListQuery listQuery, object query = null)
        {
            if (listQuery == null) throw new ArgumentNullException(nameof(listQuery));

            var condition = listQuery.Like(string.Empty, "Code");
            var order = listQuery.Order(string.Empty, new Sort
            {
                Name = "Code",
                Direction = SortDirection.Asc
            });
            var page = listQuery.Page();

            return UnitOfWork.Session.Query<AccountPlan>($@"
SELECT *
FROM AccountPlans
{condition} {order} {page}", new
            {
                listQuery.Page?.Offset,
                listQuery.Page?.Limit,
                listQuery.Filter
            }, UnitOfWork.Transaction).AsList();
        }

        public int Count(ListQuery listQuery, object query = null)
        {
            if (listQuery == null) throw new ArgumentNullException(nameof(listQuery));

            var condition = listQuery.Like(string.Empty, "Code");

            return UnitOfWork.Session.ExecuteScalar<int>($@"
SELECT COUNT(*)
FROM AccountPlans
{condition}", new
            {
                listQuery.Filter
            }, UnitOfWork.Transaction);
        }

        public int RelationCount(int id)
        {
            return UnitOfWork.Session.ExecuteScalar<int>(@"
SELECT SUM(AccountPlansCounts.AccountPlansCount)
FROM (
    SELECT 1 as AccountPlansCount --WHERE @id = @id
) AccountPlansCounts", new { id });
        }
    }
}