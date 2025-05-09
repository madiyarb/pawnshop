using System;
using System.Collections.Generic;
using System.Linq;
using Dapper;
using Pawnshop.AccountingCore.Models;
using Pawnshop.Core;
using Pawnshop.Core.Impl;
using Pawnshop.Core.Queries;
using Pawnshop.Data.Models.Dictionaries;
using Pawnshop.Data.Models.Contracts;
using Pawnshop.Data.Models.Contracts.Actions;
using Pawnshop.Data.Models.Membership;
using Type = Pawnshop.AccountingCore.Models.Type;
using System.Threading.Tasks;

namespace Pawnshop.Data.Access
{
    public class ExpenseRepository : RepositoryBase, IRepository<Expense>
    {
        public ExpenseRepository(IUnitOfWork unitOfWork) : base(unitOfWork)
        {
        }

        public void Insert(Expense entity)
        {
            using (var transaction = BeginTransaction())
            {
                entity.Id = UnitOfWork.Session.QuerySingleOrDefault<int>(@"
INSERT INTO Expenses ( Name, Cost, IsDefault, ActionType, CollateralType, CreatedBy, CreateDate, UserId, ExtraExpense, ExpenseTypeId, TypeId, NotFillUserId)
VALUES ( @Name, @Cost, @IsDefault, @ActionType, @CollateralType, @CreatedBy, @CreateDate, @UserId, @ExtraExpense, @ExpenseTypeId, @TypeId, @NotFillUserId)
SELECT SCOPE_IDENTITY()", entity, UnitOfWork.Transaction);

                transaction.Commit();
            }
        }

        public void Update(Expense entity)
        {
            using (var transaction = BeginTransaction())
            {
                UnitOfWork.Session.Execute(@"
UPDATE Expenses
SET Name = @Name, Cost = @Cost, IsDefault = @IsDefault, ActionType = @ActionType, CollateralType = @CollateralType, 
         UserId = @UserId, ExtraExpense = @ExtraExpense, ExpenseTypeId = @ExpenseTypeId, TypeId = @TypeId, NotFillUserId = @NotFillUserId
WHERE Id = @Id", entity, UnitOfWork.Transaction);

                transaction.Commit();
            }
        }

        public void Delete(int id)
        {
            using (var transaction = BeginTransaction())
            {
                UnitOfWork.Session.Execute(@"UPDATE Expenses SET DeleteDate = dbo.GETASTANADATE() WHERE Id = @id", new { id }, UnitOfWork.Transaction);

                transaction.Commit();
            }
        }

        public Expense Get(int id)
        {
            return UnitOfWork.Session.Query<Expense, User, Type, Expense>(@"
SELECT e.*, u.*, th.*
FROM Expenses e
LEFT JOIN Users u ON u.Id = e.UserId
LEFT JOIN TypesHierarchy th ON th.Id = e.TypeId
WHERE e.Id=@id",
                (e, u, th) =>
                {
                    e.User = u;
                    e.Type = th;
                    return e;
                },
                new{ id }, UnitOfWork.Transaction).FirstOrDefault();
        }

        public async Task<Expense> GetByCodeAsync(string code)
        {
            return await UnitOfWork.Session.QueryFirstOrDefaultAsync<Expense>(@"
                SELECT e.*
                FROM Expenses e
                WHERE e.Code=@code", new { code }, UnitOfWork.Transaction);
        }

        public Expense Find(object query)
        {
            throw new NotImplementedException();
        }

        public List<Expense> FindExpenses(object query)
        {
            if (query == null) throw new ArgumentNullException(nameof(query));

            var actionType = query?.Val<ContractActionType>("ActionType");
            var collateralType = query?.Val<CollateralType>("CollateralType");

            if (!collateralType.HasValue) throw new ArgumentNullException(nameof(collateralType));
            if (!actionType.HasValue) throw new ArgumentNullException(nameof(actionType));

            return UnitOfWork.Session.Query<Expense, User, Type, Expense>(@"
SELECT e.*, u.*, th.*
FROM Expenses e
LEFT JOIN Users u ON u.Id = e.UserId
LEFT JOIN TypesHierarchy th ON th.Id = e.TypeId
WHERE e.DeleteDate IS NULL
      AND (e.CollateralType = @collateralType OR e.CollateralType = 0)
      AND (ActionType = @actionType OR @actionType = 0)",
                (e, u, th) =>
                {
                    e.User = u;
                    e.Type = th;
                    return e;
                },
                new
                {
                    collateralType,
                    actionType
                }, UnitOfWork.Transaction).ToList();
        }

        public List<Expense> List(ListQuery listQuery, object query = null)
        {
            if (listQuery == null) throw new ArgumentNullException(nameof(listQuery));

            var pre = "e.DeleteDate IS NULL";
            var condition = listQuery.Like(pre, "e.Name");
            var order = listQuery.Order(string.Empty, new Sort
            {
                Name = "e.CreateDate",
                Direction = SortDirection.Desc
            });
            var page = listQuery.Page();

            return UnitOfWork.Session.Query<Expense, User, Type, Expense>($@"
SELECT e.*, u.*, th.*
FROM Expenses e
LEFT JOIN Users u ON u.Id = e.UserId
LEFT JOIN TypesHierarchy th ON th.Id = e.TypeId
{condition} {order} {page}",
                (e, u, th) =>
                {
                    e.User = u;
                    e.Type = th;
                    return e;
                },
                new
                {
                    listQuery.Page?.Offset,
                    listQuery.Page?.Limit,
                    listQuery.Filter
                }, UnitOfWork.Transaction).ToList();
        }

        public int Count(ListQuery listQuery, object query = null)
        {
            if (listQuery == null) throw new ArgumentNullException(nameof(listQuery));

            var pre = "DeleteDate IS NULL";
            var condition = listQuery.Like(pre, "Name");

            return UnitOfWork.Session.ExecuteScalar<int>($@"
SELECT COUNT(*) 
FROM Expenses
{condition}",
                new
                {
                    listQuery.Filter
                }, UnitOfWork.Transaction);
        }
    }
}