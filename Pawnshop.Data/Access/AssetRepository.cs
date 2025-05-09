using System;
using System.Collections.Generic;
using System.Linq;
using Dapper;
using Pawnshop.Core;
using Pawnshop.Core.Impl;
using Pawnshop.Core.Queries;
using Pawnshop.Data.Models.Dictionaries;
using Pawnshop.Data.Models.Membership;

namespace Pawnshop.Data.Access
{
    public class AssetRepository : RepositoryBase, IRepository<Asset>
    {
        public AssetRepository(IUnitOfWork unitOfWork) : base(unitOfWork)
        {
        }

        public void Insert(Asset entity)
        {
            using (var transaction = UnitOfWork.BeginTransaction())
            {
                entity.Id = UnitOfWork.Session.QuerySingleOrDefault<int>(@"
INSERT INTO Assets ( Number, Name, ManagerId, RegisterDate, Cost, DisposalDate, Note, BranchId, UserId, CreateDate, DeleteDate )
VALUES ( @Number, @Name, @ManagerId, @RegisterDate, @Cost, @DisposalDate, @Note, @BranchId, @UserId, @CreateDate, @DeleteDate )
SELECT SCOPE_IDENTITY()", entity, UnitOfWork.Transaction);
                transaction.Commit();
            }
        }

        public void Update(Asset entity)
        {
            using (var transaction = UnitOfWork.BeginTransaction())
            {
                UnitOfWork.Session.Execute(@"
UPDATE Assets
SET Number = @Number, Name = @Name, ManagerId = @ManagerId, RegisterDate = @RegisterDate, Cost = @Cost, DisposalDate = @DisposalDate, 
    Note = @Note, BranchId = @BranchId, UserId = @UserId, CreateDate = @CreateDate, DeleteDate = @DeleteDate
WHERE Id = @Id", entity, UnitOfWork.Transaction);
                transaction.Commit();
            }
        }

        public void Delete(int id)
        {
            using (var transaction = UnitOfWork.BeginTransaction())
            {
                UnitOfWork.Session.Execute(@"UPDATE Assets SET DeleteDate = dbo.GETASTANADATE() WHERE Id = @id", new { id }, UnitOfWork.Transaction);
                transaction.Commit();
            }
        }

        public Asset Get(int id)
        {
            return UnitOfWork.Session.Query<Asset, User, Group, User, Asset>(@"
SELECT TOP 1 a.*, m.*, b.*, u.*
FROM Assets a
JOIN Users m ON a.ManagerId = m.Id
JOIN Groups b ON a.BranchId = b.Id
JOIN Users u ON a.UserId = u.Id
WHERE a.Id = @id", (a, m, b, u) => {
                a.Manager = m;
                a.Branch = b;
                a.User = u;
                return a; 
            }, new { id }).FirstOrDefault();
        }

        public Asset Find(object query)
        {
            throw new NotImplementedException();
        }

        public List<Asset> List(ListQuery listQuery, object query = null)
        {
            if (listQuery == null) throw new ArgumentNullException(nameof(listQuery));

            var branchId = query?.Val<int?>("BranchId");
            var disposal = query?.Val<bool?>("Disposal");

            if (!branchId.HasValue) throw new ArgumentNullException(nameof(branchId));

            var pre = "a.DeleteDate IS NULL";
            pre += branchId.HasValue ? " AND a.BranchId = @branchId" : string.Empty;
            pre += disposal.HasValue && disposal.Value ? string.Empty : " AND a.DisposalDate IS NULL";

            var condition = listQuery.Like(pre, "a.Number", "a.Name");
            var order = listQuery.Order(string.Empty, new Sort
            {
                Name = "a.Name",
                Direction = SortDirection.Asc
            });
            var page = listQuery.Page();

            return UnitOfWork.Session.Query<Asset, User, Group, User, Asset>($@"
SELECT a.*, m.*, b.*, u.*
FROM Assets a
JOIN Users m ON a.ManagerId = m.Id
JOIN Groups b ON a.BranchId = b.Id
JOIN Users u ON a.UserId = u.Id
{condition} {order} {page}", (a, m, b, u) => {
                a.Manager = m;
                a.Branch = b;
                a.User = u;
                return a;
            }, new { 
                branchId,
                listQuery.Page?.Offset,
                listQuery.Page?.Limit,
                listQuery.Filter
            }).ToList();
        }

        public int Count(ListQuery listQuery, object query = null)
        {
            if (listQuery == null) throw new ArgumentNullException(nameof(listQuery));

            var branchId = query?.Val<int?>("BranchId");
            var disposal = query?.Val<bool?>("Disposal");

            if (!branchId.HasValue) throw new ArgumentNullException(nameof(branchId));

            var pre = "a.DeleteDate IS NULL";
            pre += branchId.HasValue ? " AND a.BranchId = @branchId" : string.Empty;
            pre += disposal.HasValue && disposal.Value ? " AND a.DisposalDate IS NOT NULL" : string.Empty;

            var condition = listQuery.Like(pre, "a.Number", "a.Name");

            return UnitOfWork.Session.ExecuteScalar<int>($@"
SELECT a.*, m.*, b.*, u.*
FROM Assets a
JOIN Users m ON a.ManagerId = m.Id
JOIN Groups b ON a.BranchId = b.Id
JOIN Users u ON a.UserId = u.Id
{condition}", new
            {
                branchId,
                listQuery.Filter
            });
        }
    }
}
