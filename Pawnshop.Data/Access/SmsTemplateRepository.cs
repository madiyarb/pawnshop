using Dapper;
using Pawnshop.Core.Impl;
using Pawnshop.Core.Queries;
using Pawnshop.Core;
using Pawnshop.Data.Models.Dictionaries;
using Pawnshop.Data.Models.Sms;
using System.Collections.Generic;
using System.Linq;
using System;

namespace Pawnshop.Data.Access
{
    public class SmsTemplateRepository : RepositoryBase, IRepository<SmsTemplate>
    {
        public SmsTemplateRepository(IUnitOfWork unitOfWork) : base(unitOfWork)
        {
        }

        public int Count(ListQuery listQuery, object query = null)
        {
            var entityType = query.Val<string>("EntityType");
            var manualSendRoleId = query.Val<int?>("ManualSendRoleId");

            var pre = "WHERE st.DeleteDate IS NULL";

            if (!string.IsNullOrEmpty(entityType))
                pre += " AND smt.EntityType = @entityType";

            if (manualSendRoleId.HasValue)
                pre += " AND (st.ManualSendRoleId = @manualSendRoleId OR st.ManualSendRoleId IS NULL)";

            return UnitOfWork.Session.ExecuteScalar<int>($@"SELECT COUNT(st.Id)
  FROM SmsTemplates st
  LEFT JOIN SmsMessageTypes smt ON smt.Id = st.SmsMessageTypeId
  {pre}",
                new { entityType, manualSendRoleId }, UnitOfWork.Transaction);
        }

        public void Delete(int id)
        {
            using (var transaction = BeginTransaction())
            {
                UnitOfWork.Session.Execute(@"UPDATE SmsTemplates
   SET DeleteDate = @deleteDate
 WHERE Id = @id",
                    new { id, deleteDate = DateTime.Now }, UnitOfWork.Transaction);

                transaction.Commit();
            }
        }

        public SmsTemplate Find(object query)
        {
            throw new NotImplementedException();
        }

        public SmsTemplate Get(int id)
        {
            return UnitOfWork.Session.Query<SmsTemplate, SmsMessageType, Role, SmsTemplate>($@"SELECT st.*,
       smt.*,
       r.*
  FROM SmsTemplates st
  LEFT JOIN SmsMessageTypes smt ON smt.Id = st.SmsMessageTypeId
  LEFT JOIN Roles r ON r.Id = st.ManualSendRoleId
 WHERE st.Id = @id",
                (st, smt, r) =>
                {
                    st.SmsMessageType = smt;
                    st.Role = r;
                    return st;
                },
                new { id }, UnitOfWork.Transaction)
                .FirstOrDefault();
        }

        public void Insert(SmsTemplate entity)
        {
            using (var transaction = BeginTransaction())
            {
                entity.Id = UnitOfWork.Session.QuerySingleOrDefault<int>(@"INSERT INTO SmsTemplates ( CreateDate, SmsMessageTypeId, Title, ManualSendRoleId, MessageTemplate )
VALUES ( @CreateDate, @SmsMessageTypeId, @Title, @ManualSendRoleId, @MessageTemplate )

SELECT SCOPE_IDENTITY()",
                    entity, UnitOfWork.Transaction);

                transaction.Commit();
            }
        }

        public List<SmsTemplate> List(ListQuery listQuery, object query = null)
        {
            var offset = query.Val<int?>("Offset");
            var limit = query.Val<int?>("Limit");
            var entityType = query.Val<string>("EntityType");
            var manualSendRoleId = query.Val<int?>("ManualSendRoleId");

            var pre = "WHERE st.DeleteDate IS NULL";

            if (!string.IsNullOrEmpty(entityType))
                pre += " AND smt.EntityType = @entityType";

            if (manualSendRoleId.HasValue)
                pre += " AND (st.ManualSendRoleId = @manualSendRoleId OR st.ManualSendRoleId IS NULL)";

            var page = string.Empty;

            if (offset.HasValue)
            {
                page = "OFFSET (@offset) ROWS FETCH NEXT @limit ROWS ONLY";

                if (!limit.HasValue)
                    limit = 20;
            }

            return UnitOfWork.Session.Query<SmsTemplate, SmsMessageType, Role, SmsTemplate>($@"SELECT st.*,
       smt.*,
       r.*
  FROM SmsTemplates st
  LEFT JOIN SmsMessageTypes smt ON smt.Id = st.SmsMessageTypeId
  LEFT JOIN Roles r ON r.Id = st.ManualSendRoleId
 {pre}
{page}",
                (st, smt, r) =>
                {
                    st.SmsMessageType = smt;
                    st.Role = r;
                    return st;
                },
                new { entityType, manualSendRoleId }, UnitOfWork.Transaction)
                .ToList();
        }

        public void Update(SmsTemplate entity)
        {
            using (var transaction = BeginTransaction())
            {
                UnitOfWork.Session.Execute(@"UPDATE SmsTemplates
   SET SmsMessageTypeId = @SmsMessageTypeId,
       Title = @Title,
       ManualSendRoleId = @ManualSendRoleId,
       MessageTemplate = @MessageTemplate,
       UpdateDate = @UpdateDate
 WHERE Id = @Id",
                    entity, UnitOfWork.Transaction);

                transaction.Commit();
            }
        }
    }
}
