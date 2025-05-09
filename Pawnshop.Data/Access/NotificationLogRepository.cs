using System;
using System.Collections.Generic;
using System.Linq;
using Dapper;
using Pawnshop.Core;
using Pawnshop.Core.Impl;
using Pawnshop.Core.Queries;
using Pawnshop.Data.Models.Notifications;

namespace Pawnshop.Data.Access
{
    public class NotificationLogRepository : RepositoryBase, IRepository<NotificationLog>
    {
        public NotificationLogRepository(IUnitOfWork unitOfWork) : base(unitOfWork)
        {
        }

        public void Insert(NotificationLog entity)
        {
            using (var transaction = UnitOfWork.BeginTransaction())
            {
                entity.Id = UnitOfWork.Session.QuerySingleOrDefault<int>(@"
INSERT INTO NotificationLogs ( NotificationReceiverId, CreateDate, StatusMessage )
VALUES ( @NotificationReceiverId, @CreateDate, @StatusMessage )
SELECT SCOPE_IDENTITY()", entity, UnitOfWork.Transaction);

                transaction.Commit();
            }
        }

        public void Update(NotificationLog entity)
        {
            throw new NotImplementedException();
        }

        public void Delete(int id)
        {
            throw new NotImplementedException();
        }

        public NotificationLog Get(int id)
        {
            throw new NotImplementedException();
        }

        public NotificationLog Find(object query)
        {
            throw new NotImplementedException();
        }

        public List<NotificationLog> List(ListQuery listQuery, object query = null)
        {
            if (listQuery == null) throw new ArgumentNullException(nameof(listQuery));

            var notificationReceiverId = query?.Val<int>("NotificationReceiverId");
            var condition = "WHERE NotificationReceiverId = @notificationReceiverId";
            var order = listQuery.Order(string.Empty, new Sort
            {
                Name = "CreateDate",
                Direction = SortDirection.Asc
            });

            return UnitOfWork.Session.Query<NotificationLog>($@"
SELECT *
FROM NotificationLogs
{condition} {order}", new { notificationReceiverId }).ToList();

        }

        public int Count(ListQuery listQuery, object query = null)
        {
            throw new NotImplementedException();
        }
    }
}