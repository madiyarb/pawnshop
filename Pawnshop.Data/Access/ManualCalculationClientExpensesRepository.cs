using System;
using System.Collections.Generic;
using System.Linq;
using Dapper;
using Pawnshop.Core;
using Pawnshop.Core.Impl;
using Pawnshop.Core.Queries;
using Pawnshop.Data.Models.Clients;
using Pawnshop.Data.Models.Dictionaries;
using Pawnshop.Data.Models.Membership;

namespace Pawnshop.Data.Access
{
    public class ManualCalculationClientExpensesRepository : RepositoryBase, IRepository<ManualCalculationClientExpense>
    {
        private readonly ISessionContext _sessionContext;

        public ManualCalculationClientExpensesRepository(IUnitOfWork unitOfWork, ISessionContext sessionContext) : base(unitOfWork)
        {
            _sessionContext = sessionContext;
        }

        public void Insert(ManualCalculationClientExpense entity)
        {
            using (var transaction = BeginTransaction())
            {
                entity.CreateDate = DateTime.Now;
                entity.AuthorId = _sessionContext.UserId;

                entity.Id = UnitOfWork.Session.QuerySingleOrDefault<int>(@"
                    INSERT INTO ManualCalculationClientExpenses (Debt, ClientId, AuthorId, Date, CreateDate)
                    VALUES (@Debt, @ClientId, @AuthorId, @Date, @CreateDate)
                    SELECT SCOPE_IDENTITY()",
                    entity, UnitOfWork.Transaction);
                transaction.Commit();
            }
        }

        public void Update(ManualCalculationClientExpense entity)
        {
            using (var transaction = BeginTransaction())
            {
                UnitOfWork.Session.Execute(@"
                    UPDATE ManualCalculationClientExpenses
                    SET Debt = @Debt, ClientId = @ClientId, AuthorId = @AuthorId, Date = @Date, CreateDate = @CreateDate  
                    WHERE Id = @Id",
                    entity, UnitOfWork.Transaction);
                transaction.Commit();
            }
        }

        public void Delete(int id)
        {
            using (var transaction = BeginTransaction())
            {
                UnitOfWork.Session.Execute("DELETE FROM ManualCalculationClientExpenses WHERE Id = @id", new { id }, UnitOfWork.Transaction);
                transaction.Commit();
            }
        }

        public ManualCalculationClientExpense Get(int id)
        {
            return UnitOfWork.Session.Query<ManualCalculationClientExpense, User, Client, ManualCalculationClientExpense>(@"
                SELECT e.*, u.*, c.*
                FROM ManualCalculationClientExpenses e
                LEFT JOIN Users u ON u.Id = e.AuthorId
                LEFT JOIN Clients c ON c.Id = e.ClientId
                WHERE e.Id = @id",
                (e, u, c) =>
                {
                    e.Author = u;
                    e.Client = c;
                    return e;
                }, new { id },
                UnitOfWork.Transaction).FirstOrDefault();
        }

        public ManualCalculationClientExpense GetByClientIdAndDate(int clientId, DateTime date)
        {
            return UnitOfWork.Session.Query<ManualCalculationClientExpense>(@"
                SELECT e.*
                FROM ManualCalculationClientExpenses e
                WHERE e.ClientId = @clientId AND CONVERT(Date, e.Date)=CONVERT(Date, @date)",
                new { clientId, date },
                UnitOfWork.Transaction).FirstOrDefault();
        }

        public ManualCalculationClientExpense Find(object query)
        {
            throw new NotImplementedException();
        }

        public List<ManualCalculationClientExpense> List(ListQuery listQuery, object query = null)
        {
            if (listQuery == null) throw new ArgumentNullException(nameof(listQuery));

            var date = query?.Val<DateTime?>("Date");

            var pre = "e.DeleteDate IS NULL";
            pre += date.HasValue ? " AND e.Date >= @date AND e.Date < DATEADD(DAY, 1, @date)" : string.Empty;

            var condition = listQuery.Like(pre, "u.FullName", "c.FullName", "c.IdentityNumber");
            var order = listQuery.Order(string.Empty, new Sort
            {
                Name = "e.Date",
                Direction = SortDirection.Desc
            });
            var page = listQuery.Page();

            return UnitOfWork.Session.Query<ManualCalculationClientExpense, User, Client, ManualCalculationClientExpense>($@"
                SELECT e.*, u.*, c.*
                FROM ManualCalculationClientExpenses e
                LEFT JOIN Users u ON u.Id = e.AuthorId
                LEFT JOIN Clients c ON c.Id = e.ClientId
                {condition} {order} {page}",
                (e, u, c) =>
                {
                    e.Author = u;
                    e.Client = c;
                    return e;
                },
                new
                {
                    date,
                    listQuery.Page?.Offset,
                    listQuery.Page?.Limit,
                    listQuery.Filter
                }).ToList();
        }

        public int Count(ListQuery listQuery, object query = null)
        {
            if (listQuery == null) throw new ArgumentNullException(nameof(listQuery));

            var date = query?.Val<DateTime?>("Date");

            var pre = "e.DeleteDate IS NULL";
            pre += date.HasValue ? " AND e.Date >= @date AND e.Date < DATEADD(DAY, 1, @date)" : string.Empty;

            var condition = listQuery.Like(pre, "u.FullName", "c.FullName", "c.IdentityNumber");

            return UnitOfWork.Session.ExecuteScalar<int>($@"
                SELECT COUNT(*)
                FROM ManualCalculationClientExpenses e
                LEFT JOIN Users u ON u.Id = e.AuthorId
                LEFT JOIN Clients c ON c.Id = e.ClientId
                {condition}",
                new
                {
                    date,
                    listQuery.Filter
                });
        }
    }
}