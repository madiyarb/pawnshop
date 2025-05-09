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
    public class ExpenseGroupRepository : RepositoryBase, IRepository<ExpenseGroup>
    {
        public ExpenseGroupRepository(IUnitOfWork unitOfWork) : base(unitOfWork)
        {
        }

        public void Insert(ExpenseGroup entity)
        {
            using (var transaction = BeginTransaction())
            {
                entity.Id = UnitOfWork.Session.QuerySingleOrDefault<int>(@"
INSERT INTO ExpenseGroups ( Name, OrderBy )
VALUES ( @Name, @OrderBy )
SELECT SCOPE_IDENTITY()", entity, UnitOfWork.Transaction);

                transaction.Commit();
            }
        }

        public void Update(ExpenseGroup entity)
        {
            using (var transaction = BeginTransaction())
            {
                UnitOfWork.Session.Execute(@"
UPDATE ExpenseGroups
SET Name = @Name, OrderBy = @OrderBy
WHERE Id = @Id", entity, UnitOfWork.Transaction);

                transaction.Commit();
            }
        }

        public void Delete(int id)
        {
            using (var transaction = BeginTransaction())
            {
                UnitOfWork.Session.Execute(@"UPDATE ExpenseGroups SET DeleteDate = dbo.GETASTANADATE() WHERE Id = @id", new { id }, UnitOfWork.Transaction);

                transaction.Commit();
            }
        }

        public ExpenseGroup Get(int id)
        {
            return UnitOfWork.Session.QuerySingleOrDefault<ExpenseGroup>(@"
SELECT *
FROM ExpenseGroups
WHERE Id = @id", new { id = id });
        }

        public ExpenseGroup Find(object query)
        {
            throw new System.NotImplementedException();
        }

        public List<ExpenseGroup> List(ListQuery listQuery, object query = null)
        {
            if (listQuery == null) throw new ArgumentNullException(nameof(listQuery));

            var pre = "DeleteDate IS NULL";
            var condition = listQuery.Like(pre, "Name");
            var order = listQuery.Order(string.Empty, new Sort
            {
                Name = "OrderBy",
                Direction = SortDirection.Asc
            });
            var page = listQuery.Page();

            return UnitOfWork.Session.Query<ExpenseGroup>($@"
SELECT *
FROM ExpenseGroups
{condition} {order} {page}", new
            {
                listQuery.Page?.Offset,
                listQuery.Page?.Limit,
                listQuery.Filter
            }).ToList();
        }

        public int Count(ListQuery listQuery, object query = null)
        {
            if (listQuery == null) throw new ArgumentNullException(nameof(listQuery));

            var pre = "DeleteDate IS NULL";
            var condition = listQuery.Like(pre, "Name");

            return UnitOfWork.Session.ExecuteScalar<int>($@"
SELECT COUNT(*)
FROM ExpenseGroups
{condition}", new
            {
                listQuery.Filter
            });
        }
    }
}