using System;
using System.Collections.Generic;
using System.Linq;
using Dapper;
using Pawnshop.AccountingCore.Models;
using Pawnshop.Core;
using Pawnshop.Core.Impl;
using Pawnshop.Core.Queries;
using Pawnshop.Data.Models.Clients;
using Pawnshop.Data.Models.Contracts;
using Pawnshop.Data.Models.Notifications;

namespace Pawnshop.Data.Access
{
    public class NotificationReceiverRepository : RepositoryBase, IRepository<NotificationReceiver>
    {
        public NotificationReceiverRepository(IUnitOfWork unitOfWork) : base(unitOfWork)
        {
        }

        public void Insert(NotificationReceiver entity)
        {
            if(entity == null)
                throw new ArgumentNullException(nameof(entity));

            using (var transaction = UnitOfWork.BeginTransaction())
            {
                entity.Id = UnitOfWork.Session.QuerySingleOrDefault<int>(@"
INSERT INTO NotificationReceivers ( NotificationId, ClientId, Status, CreateDate, TryCount, ContractId)
VALUES ( @NotificationId, @ClientId, @Status, @CreateDate, @TryCount, @ContractId )
SELECT SCOPE_IDENTITY()", entity, UnitOfWork.Transaction);

                transaction.Commit();
            }
        }

        public void Update(NotificationReceiver entity)
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));

            using (var transaction = UnitOfWork.BeginTransaction())
            {
                UnitOfWork.Session.Execute(@"
                UPDATE NotificationReceivers
                SET 
                    NotificationId = @NotificationId, 
                    ClientId = @ClientId, 
                    Status = @Status, 
                    CreateDate = @CreateDate, 
                    TryCount = @TryCount,
                    Address = @Address,
                    DeliveredAt = @DeliveredAt,
                    SentAt = @SentAt,
                    ContractId = @ContractId,
                    MessageId = @MessageId
                WHERE Id = @Id", entity, UnitOfWork.Transaction);
                transaction.Commit();
            }
        }

        public void UpdateStatus(int id, NotificationStatus status)
        {
            using (var transaction = UnitOfWork.BeginTransaction())
            {
                UnitOfWork.Session.Execute(@"
                UPDATE NotificationReceivers
                SET 
                    Status = @Status
                WHERE Id = @Id", new { id, status }, UnitOfWork.Transaction);
                transaction.Commit();
            }
        }

        public void ForSend(int notificationId)
        {
            using (var transaction = UnitOfWork.BeginTransaction())
            {
                UnitOfWork.Session.Execute(@"
UPDATE NotificationReceivers
SET Status = 10
WHERE NotificationId = @notificationId
    AND Status = 0", new { notificationId }, UnitOfWork.Transaction);

                transaction.Commit();
            }
        }

        public void Delete(int id)
        {
            using (var transaction = UnitOfWork.BeginTransaction())
            {
                UnitOfWork.Session.Execute("DELETE FROM NotificationReceivers WHERE Id = @id", new { id }, UnitOfWork.Transaction);
                transaction.Commit();
            }
        }

        public NotificationReceiver Get(int id)
        {
            return UnitOfWork.Session.Query<NotificationReceiver, Notification, Client, NotificationReceiver>(@"
SELECT nr.*, n.*, c.*
FROM NotificationReceivers nr
JOIN Notifications n ON nr.NotificationId = n.Id
JOIN Clients c ON nr.ClientId = c.Id
WHERE nr.Id = @id",
                (nr, n, c) =>
                {
                    nr.Notification = n;
                    nr.Client = c;
                    return nr;
                }, new { id }, UnitOfWork.Transaction).FirstOrDefault();
        }

        public NotificationReceiver Find(object query = null)
        {
            var condition = "WHERE nr.Status = 10 AND nr.TryCount < 5 AND n.IsNonSchedule = 0";
            var order = "ORDER BY nr.CreateDate";
            return UnitOfWork.Session.Query<NotificationReceiver, Notification, Client, NotificationReceiver>($@"
SELECT TOP 1 nr.*, n.*, c.*
FROM NotificationReceivers nr
JOIN Notifications n ON nr.NotificationId = n.Id
JOIN Clients c ON nr.ClientId = c.Id
{condition} {order}",
                (nr, n, c) =>
                {
                    nr.Notification = n;
                    nr.Client = c;

                    return nr;
                }, new { }, UnitOfWork.Transaction).FirstOrDefault();
        }

        public List<NotificationReceiver> Find()
        {
            var condition = "WHERE nr.Status = 10 AND nr.TryCount < 5";
            var order = "ORDER BY nr.CreateDate";

            return UnitOfWork.Session.Query<NotificationReceiver, Notification, Client, NotificationReceiver>($@"
SELECT TOP 1 nr.*, n.*, c.*
FROM NotificationReceivers nr
JOIN Notifications n ON nr.NotificationId = n.Id
JOIN Clients c ON nr.ClientId = c.Id
{condition} {order}",
                (nr, n, c) =>
                {
                    nr.Notification = n;
                    nr.Client = c;
                    return nr;
                }).ToList();
        }

        public List<NotificationReceiver> List(ListQuery listQuery, object query = null)
        {
            if (listQuery == null) throw new ArgumentNullException(nameof(listQuery));

            var notificationId = query?.Val<int>("NotificationId");

            var pre = "nr.NotificationId = @notificationId";
            var condition = listQuery.Like(pre, "cl.FullName");
            var order = listQuery.Order(string.Empty, new Sort
            {
                Name = "cl.FullName",
                Direction = SortDirection.Asc
            });
            var page = listQuery.Page();

            return UnitOfWork.Session.Query<NotificationReceiver, Notification, Client, Contract, NotificationReceiver>($@"
SELECT nr.*, n.*, cl.*, c.*
FROM NotificationReceivers nr
JOIN Notifications n ON nr.NotificationId = n.Id
JOIN Clients cl ON nr.ClientId = cl.Id
LEFT JOIN Contracts c on c.Id = nr.ContractId
{condition} {order} {page}", (nr, n, cl, cntc) =>
            {
                nr.Contract = cntc;
                nr.Notification = n;
                nr.Client = cl;
                return nr;
            }, new
            {
                notificationId,
                listQuery.Page?.Offset,
                listQuery.Page?.Limit,
                listQuery.Filter
            }, UnitOfWork.Transaction).ToList();
        }

        public int Count(ListQuery listQuery, object query = null)
        {
            if (listQuery == null) throw new ArgumentNullException(nameof(listQuery));

            var notificationId = query?.Val<int>("NotificationId");

            var pre = "nr.NotificationId = @notificationId";
            var condition = listQuery.Like(pre, "cl.FullName");

            return UnitOfWork.Session.ExecuteScalar<int>($@"
SELECT COUNT(*)
FROM NotificationReceivers nr
JOIN Notifications n ON nr.NotificationId = n.Id
JOIN Clients cl ON nr.ClientId = cl.Id
{condition}", new
            {
                notificationId,
                listQuery.Filter
            }, UnitOfWork.Transaction);
        }

        public void Select(ListQuery listQuery, object query = null)
        {
            if (listQuery == null) throw new ArgumentNullException(nameof(listQuery));

            var notificationId = query?.Val<int>("NotificationId");
            var branchId = query?.Val<int>("BranchId");
            var collateralType = query?.Val<CollateralType?>("CollateralType");
            var cardType = query?.Val<CardType?>("CardType");

            if (!notificationId.HasValue) throw new ArgumentNullException(nameof(notificationId));
            if (!branchId.HasValue) throw new ArgumentNullException(nameof(branchId));

            using (var transaction = UnitOfWork.BeginTransaction())
            {
                var condition = @"
WHERE cl.DeleteDate IS NULL
    AND c.BranchId = @branchId
    AND cl.Id NOT IN (
        SELECT nr.ClientId
        FROM NotificationReceivers nr
        WHERE nr.NotificationId = @notificationId
    )";

                if (collateralType.HasValue) condition += " AND c.CollateralType = @collateralType";
                if (cardType.HasValue) condition += " AND cl.CardType = @cardType";

                UnitOfWork.Session.Execute($@"
INSERT INTO NotificationReceivers ( NotificationId, ClientId, Status, CreateDate, TryCount, ContractId )
SELECT DISTINCT @notificationId, cl.Id, 0, dbo.GETASTANADATE(), 0, c.Id
FROM Contracts c
JOIN Clients cl ON c.ClientId = cl.Id
{condition}", new { notificationId, branchId, collateralType, cardType }, UnitOfWork.Transaction);

                transaction.Commit();
            }
        }

        public List<NotificationReceiver> GetForUpdateReport()
        {
            return UnitOfWork.Session.Query<NotificationReceiver>($@"
SELECT nr.*
FROM NotificationReceivers nr
WHERE nr.SentAt > DATEADD (DAY, -2, dbo.GetAstanaDate()) 
AND nr.[Status] = 20
AND nr.MessageId IS NOT NULL
order by nr.Id desc", UnitOfWork.Transaction).ToList();
        }

    }
}