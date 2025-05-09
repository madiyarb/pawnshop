using System;
using System.Collections.Generic;
using System.Linq;
using Dapper;
using Pawnshop.Core;
using Pawnshop.Core.Impl;
using Pawnshop.Core.Queries;
using Pawnshop.Data.Models.Dictionaries;

namespace Pawnshop.Data.Access
{
    public class ExpenseTypeRepository : RepositoryBase, IRepository<ExpenseType>
    {
        public ExpenseTypeRepository(IUnitOfWork unitOfWork) : base(unitOfWork)
        {
        }

        public void Insert(ExpenseType entity)
        {
            using (var transaction = BeginTransaction())
            {
                entity.Id = UnitOfWork.Session.QuerySingleOrDefault<int>(@"
INSERT INTO ExpenseTypes ( Name, ExpenseGroupId, AccountId, OrderBy )
VALUES ( @Name, @ExpenseGroupId, @AccountId, @OrderBy )
SELECT SCOPE_IDENTITY()", entity, UnitOfWork.Transaction);

                transaction.Commit();
            }
        }

        public void Update(ExpenseType entity)
        {
            using (var transaction = BeginTransaction())
            {
                UnitOfWork.Session.Execute(@"
UPDATE ExpenseTypes
SET Name = @Name, ExpenseGroupId = @ExpenseGroupId, AccountId = @AccountId, OrderBy = @OrderBy
WHERE Id = @Id", entity, UnitOfWork.Transaction);

                transaction.Commit();
            }
        }

        public void Delete(int id)
        {
            using (var transaction = BeginTransaction())
            {
                UnitOfWork.Session.Execute(@"UPDATE ExpenseTypes SET DeleteDate = dbo.GETASTANADATE() WHERE Id = @id", new { id }, UnitOfWork.Transaction);

                transaction.Commit();
            }
        }

        public ExpenseType Get(int id)
        {
            return UnitOfWork.Session.Query<ExpenseType, ExpenseGroup, ExpenseType>(@"
SELECT et.*, eg.*
FROM ExpenseTypes et
JOIN ExpenseGroups eg ON et.ExpenseGroupId = eg.Id
WHERE et.Id = @id", (et, eg) => { et.ExpenseGroup = eg; return et; }, new { id = id }).FirstOrDefault();
        }

        public ExpenseType Find(object query)
        {
            throw new System.NotImplementedException();
        }

        public List<ExpenseType> List(ListQuery listQuery, object query = null)
        {
            if (listQuery == null) throw new ArgumentNullException(nameof(listQuery));

            var pre = "et.DeleteDate IS NULL AND eg.DeleteDate IS NULL";
            var expenseGroupId = query?.Val<int?>("ExpenseGroupId");
            if (expenseGroupId.HasValue) pre += " AND et.ExpenseGroupId = @expenseGroupId";

            var condition = listQuery.Like(pre, "et.Name");
            var order = listQuery.Order("eg.OrderBy", new Sort
            {
                Name = "et.OrderBy",
                Direction = SortDirection.Asc
            });
            var page = listQuery.Page();

            return UnitOfWork.Session.Query<ExpenseType, ExpenseGroup, ExpenseType>($@"
SELECT et.*, eg.*
FROM ExpenseTypes et
JOIN ExpenseGroups eg ON et.ExpenseGroupId = eg.Id
{condition} {order} {page}",
            (et, eg) =>
            {
                et.ExpenseGroup = eg;
                return et;
            },
            new
            {
                expenseGroupId,
                listQuery.Page?.Offset,
                listQuery.Page?.Limit,
                listQuery.Filter
            }).ToList();
        }

        public int Count(ListQuery listQuery, object query = null)
        {
            if (listQuery == null) throw new ArgumentNullException(nameof(listQuery));

            var pre = "et.DeleteDate IS NULL AND eg.DeleteDate IS NULL";
            var expenseGroupId = query?.Val<int?>("ExpenseGroupId");
            if (expenseGroupId.HasValue) pre += " AND et.ExpenseGroupId = @expenseGroupId";

            var condition = listQuery.Like(pre, "et.Name");

            return UnitOfWork.Session.ExecuteScalar<int>($@"
SELECT COUNT(*)
FROM ExpenseTypes et
JOIN ExpenseGroups eg ON et.ExpenseGroupId = eg.Id
{condition}", new
            {
                expenseGroupId,
                listQuery.Filter
            });
        }
    }
}