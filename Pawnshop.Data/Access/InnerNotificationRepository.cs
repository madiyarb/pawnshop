using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using Pawnshop.Core;
using Pawnshop.Core.Impl;
using Pawnshop.Core.Queries;
using Pawnshop.Data.Models.Contracts;
using Pawnshop.Data.Models.Membership;
using Pawnshop.Data.Models.InnerNotifications;

namespace Pawnshop.Data.Access
{
    public class InnerNotificationRepository : RepositoryBase, IRepository<InnerNotification>
    {
        public InnerNotificationRepository(IUnitOfWork unitOfWork) : base(unitOfWork)
        {
        }

        public void Insert(InnerNotification entity)
        {
            using (var transaction = BeginTransaction())
            {
                entity.Id = UnitOfWork.Session.QuerySingleOrDefault<int>(@"
INSERT INTO InnerNotifications ( Message, CreatedBy, CreateDate, ReceiveBranchId, ReceiveUserId, EntityType, EntityId, DeleteDate, Status )
VALUES ( @Message, @CreatedBy, @CreateDate, @ReceiveBranchId, @ReceiveUserId, @EntityType, @EntityId, @DeleteDate, @Status )
SELECT SCOPE_IDENTITY()", entity, UnitOfWork.Transaction);

                transaction.Commit();
            }
        }
        
        public async Task InsertAsync(InnerNotification entity)
        {
            using (var transaction = BeginTransaction())
            {
                entity.Id = await UnitOfWork.Session.QuerySingleOrDefaultAsync<int>(@"
INSERT INTO InnerNotifications ( Message, CreatedBy, CreateDate, ReceiveBranchId, ReceiveUserId, EntityType, EntityId, DeleteDate, Status )
VALUES ( @Message, @CreatedBy, @CreateDate, @ReceiveBranchId, @ReceiveUserId, @EntityType, @EntityId, @DeleteDate, @Status )
SELECT SCOPE_IDENTITY()", entity, UnitOfWork.Transaction);

                transaction.Commit();
            }
        }

        public void Update(InnerNotification entity)
        {
            using (var transaction = BeginTransaction())
            {
                UnitOfWork.Session.Execute(@"
UPDATE InnerNotifications
SET Message = @Message, CreatedBy = @CreatedBy, CreateDate = @CreateDate, ReceiveBranchId = @ReceiveBranchId, ReceiveUserId = @ReceiveUserId, 
    EntityType = @EntityType, EntityId = @EntityId, DeleteDate = @DeleteDate, Status = @Status
WHERE Id = @Id", entity, UnitOfWork.Transaction);

                transaction.Commit();
            }
        }

        public void MarkAsRead(int id)
        {
            using (var transaction = BeginTransaction())
            {
                UnitOfWork.Session.Execute(@"UPDATE InnerNotifications SET Status = 10 WHERE Id = @id", new { id }, UnitOfWork.Transaction);
                transaction.Commit();
            }
        }

        public void MarkAsDone(int id)
        {
            using (var transaction = BeginTransaction())
            {
                UnitOfWork.Session.Execute(@"UPDATE InnerNotifications SET Status = 20 WHERE Id = @id", new { id }, UnitOfWork.Transaction);
                transaction.Commit();
            }
        }

        public void Delete(int id)
        {
            using (var transaction = BeginTransaction())
            {
                UnitOfWork.Session.Execute(@"UPDATE InnerNotifications SET DeleteDate = dbo.GETASTANADATE() WHERE Id = @id", new { id }, UnitOfWork.Transaction);
                transaction.Commit();
            }
        }

        public InnerNotification Get(int id)
        {
            return UnitOfWork.Session.QuerySingleOrDefault<InnerNotification>(@"
SELECT *
FROM InnerNotifications
WHERE Id = @id", new { id }, UnitOfWork.Transaction);
        }

        public InnerNotification Find(object query)
        {
            throw new NotImplementedException();
        }

        public List<InnerNotification> List(ListQuery listQuery, object query = null)
        {
            if (listQuery == null) throw new ArgumentNullException(nameof(listQuery));

            var branchId = query?.Val<int?>("BranchId");
            var userId = query?.Val<int?>("UserId");

            var pre = "n.DeleteDate IS NULL";
            if (branchId.HasValue) pre += " AND (n.ReceiveBranchId = @branchId OR n.ReceiveUserId = @userId)";
            pre += " AND Status!=20";

            var condition = listQuery.Like(pre, "n.Message");
            var order = listQuery.Order(string.Empty, new Sort
            {
                Name = "n.CreateDate",
                Direction = SortDirection.Desc
            });
            var page = listQuery.Page();

            return UnitOfWork.Session.Query<InnerNotification, User, InnerNotification>($@"
SELECT n.*,u.*
FROM InnerNotifications n with(index = idx_InnerNotifications_DeleteDate_Status)
LEFT JOIN Users u on n.CreatedBy=u.Id
WHERE {pre} {order}", (n, u) =>
                {
                    n.Author = u;
                    return n;
                },
                new
                {
                    branchId,
                    userId,
                    listQuery.Page?.Offset,
                    listQuery.Page?.Limit,
                    listQuery.Filter                    
                }).ToList();
        }

        public int Count(ListQuery listQuery, object query = null)
        {
            if (listQuery == null) throw new ArgumentNullException(nameof(listQuery));

            var branchId = query?.Val<int?>("BranchId");
            var userId = query?.Val<int?>("UserId");

            var pre = "n.DeleteDate IS NULL";
            if (branchId.HasValue) pre += " AND (n.ReceiveBranchId = @branchId OR n.ReceiveUserId = @userId)";
            pre += " AND Status!=20";

            var condition = listQuery.Like(pre);
            var page = listQuery.Page();

            return UnitOfWork.Session.ExecuteScalar<int>($@"
SELECT COUNT(*)
FROM InnerNotifications n with(index = idx_InnerNotifications_DeleteDate_Status)
WHERE {pre}",
                new
                {
                    branchId,
                    userId
                });
        }
    }
}