using System;
using System.Collections.Generic;
using System.Linq;
using Dapper;
using Pawnshop.Core;
using Pawnshop.Core.Impl;
using Pawnshop.Core.Queries;
using Pawnshop.Data.Models.Clients;
using Pawnshop.Data.Models.Investments;
using Pawnshop.Data.Models.Membership;

namespace Pawnshop.Data.Access
{
    public class InvestmentRepository : RepositoryBase, IRepository<Investment>
    {
        public InvestmentRepository(IUnitOfWork unitOfWork) : base(unitOfWork)
        {
        }

        public void Insert(Investment entity)
        {
            using (var transaction = UnitOfWork.BeginTransaction())
            {
                entity.Id = UnitOfWork.Session.QuerySingleOrDefault<int>(@"
INSERT INTO Investments ( ClientId, InvestmentCost, InvestmentPercent, InvestmentBeginDate, InvestmentPeriod, InvestmentEndDate,
    ActualCost, ActualPeriod, ActualEndDate, RepaymentDay, RepaymentType, RepaymentPartCost, OrderId, Status, CreateDate,
    BranchId, AuthorId, OwnerId, DeleteDate )
VALUES ( @ClientId, @InvestmentCost, @InvestmentPercent, @InvestmentBeginDate, @InvestmentPeriod, @InvestmentEndDate,
    @ActualCost, @ActualPeriod, @ActualEndDate, @RepaymentDay, @RepaymentType, @RepaymentPartCost, @OrderId, @Status, @CreateDate,
    @BranchId, @AuthorId, @OwnerId, @DeleteDate )
SELECT SCOPE_IDENTITY()", entity, UnitOfWork.Transaction);

                transaction.Commit();
            }
        }

        public void Update(Investment entity)
        {
            using (var transaction = UnitOfWork.BeginTransaction())
            {
                UnitOfWork.Session.Execute(@"
UPDATE Investments
SET ClientId = @ClientId, InvestmentCost = @InvestmentCost, InvestmentPercent = @InvestmentPercent, InvestmentBeginDate = @InvestmentBeginDate, 
    InvestmentPeriod = @InvestmentPeriod, InvestmentEndDate = @InvestmentEndDate, ActualCost = @ActualCost, ActualPeriod = @ActualPeriod, 
    ActualEndDate = @ActualEndDate, RepaymentDay = @RepaymentDay, RepaymentType = @RepaymentType, RepaymentPartCost = @RepaymentPartCost, 
    OrderId = @OrderId, Status = @Status, CreateDate = @CreateDate, BranchId = @BranchId, AuthorId = @AuthorId, OwnerId = @OwnerId, DeleteDate = @DeleteDate
WHERE Id = @Id", entity, UnitOfWork.Transaction);

                transaction.Commit();
            }
        }

        public void Delete(int id)
        {
            using (var transaction = UnitOfWork.BeginTransaction())
            {
                UnitOfWork.Session.Execute(@"UPDATE Investments SET DeleteDate = dbo.GETASTANADATE() WHERE Id = @id", new { id }, UnitOfWork.Transaction);
                transaction.Commit();
            }
        }

        public Investment Get(int id)
        {
            return UnitOfWork.Session.Query<Investment, Client, Group, User, Investment>(@"
SELECT i.*, c.*, g.*, u.*
FROM Investments i
JOIN Clients c ON i.ClientId = c.Id
JOIN Groups g ON i.BranchId = g.Id
JOIN Users u ON i.AuthorId = u.Id
WHERE i.Id = @id", (i, c, g, u) =>
            {
                i.Client = c;
                i.Branch = g;
                i.Author = u;
                return i;
            }, new { id }).FirstOrDefault();
        }

        public Investment Find(object query)
        {
            throw new NotImplementedException();
        }

        public List<Investment> List(ListQuery listQuery, object query = null)
        {
            if (listQuery == null) throw new ArgumentNullException(nameof(listQuery));

            var beginDate = query?.Val<DateTime?>("BeginDate");
            var endDate = query?.Val<DateTime?>("EndDate");
            var ownerId = query?.Val<int>("OwnerId");

            var pre = "i.DeleteDate IS NULL";
            pre += beginDate.HasValue ? " AND DATEFROMPARTS(YEAR(dbo.GETASTANADATE()), MONTH(dbo.GETASTANADATE()), i.RepaymentDay) >= @beginDate" : string.Empty;
            pre += endDate.HasValue ? " AND DATEFROMPARTS(YEAR(dbo.GETASTANADATE()), MONTH(dbo.GETASTANADATE()), i.RepaymentDay) <= @endDate" : string.Empty;
            pre += " AND mr.LeftMemberId = @ownerId";

            var condition = listQuery.Like(pre, "c.FullName");
            var order = listQuery.Order(string.Empty, new Sort
            {
                Name = "c.FullName",
                Direction = SortDirection.Desc
            });
            var page = listQuery.Page();

            return UnitOfWork.Session.Query<Investment, Client, Group, User, Investment>($@"
SELECT i.*, c.*, g.*, u.*
FROM Investments i
JOIN MemberRelations mr ON mr.RightMemberId = i.OwnerId
JOIN Clients c ON i.ClientId = c.Id
JOIN Groups g ON i.BranchId = g.Id
JOIN Users u ON i.AuthorId = u.Id
{condition} {order} {page}", (i, c, g, u) =>
            {
                i.Client = c;
                i.Branch = g;
                i.Author = u;
                return i;
            }, new 
            {
                beginDate,
                endDate,
                ownerId,
                listQuery.Page?.Offset,
                listQuery.Page?.Limit,
                listQuery.Filter
            }).ToList();
        }

        public int Count(ListQuery listQuery, object query = null)
        {
            if (listQuery == null) throw new ArgumentNullException(nameof(listQuery));

            var beginDate = query?.Val<DateTime?>("BeginDate");
            var endDate = query?.Val<DateTime?>("EndDate");
            var ownerId = query?.Val<int>("OwnerId");

            var pre = "i.DeleteDate IS NULL";
            pre += beginDate.HasValue ? " AND DATEFROMPARTS(YEAR(dbo.GETASTANADATE()), MONTH(dbo.GETASTANADATE()), i.RepaymentDay) >= @beginDate" : string.Empty;
            pre += endDate.HasValue ? " AND DATEFROMPARTS(YEAR(dbo.GETASTANADATE()), MONTH(dbo.GETASTANADATE()), i.RepaymentDay) <= @endDate" : string.Empty;
            pre += " AND mr.LeftMemberId = @ownerId";

            var condition = listQuery.Like(pre, "c.FullName");

            return UnitOfWork.Session.ExecuteScalar<int>($@"
SELECT COUNT(*)
FROM Investments i
JOIN MemberRelations mr ON mr.RightMemberId = i.OwnerId
JOIN Clients c ON i.ClientId = c.Id
JOIN Groups g ON i.BranchId = g.Id
JOIN Users u ON i.AuthorId = u.Id
{condition}", new {
                beginDate,
                endDate,
                ownerId,
                listQuery.Filter
            });
        }
    }
}
