using System.Collections.Generic;
using System.Threading.Tasks;
using Dapper;
using Pawnshop.Core;
using Pawnshop.Core.Exceptions;
using Pawnshop.Core.Impl;
using Pawnshop.Core.Queries;
using Pawnshop.Data.Models.Notifications;
using Pawnshop.Data.Models.Notifications.NotificationTemplates;

namespace Pawnshop.Data.Access
{
    public class NotificationTemplateRepository : RepositoryBase, IRepository<NotificationTemplate>
    {
        public NotificationTemplateRepository(IUnitOfWork unitOfWork) : base(unitOfWork)
        {
        }

        public void Insert(NotificationTemplate entity)
        {
            using (var transaction = BeginTransaction())
            {
                entity.Id = UnitOfWork.Session.QuerySingleOrDefault<int>(@"
                    INSERT INTO NotificationTemplates ( Subject, Message, MessageType, NotificationPaymentType, CreateDate, DeleteDate, IsActive, Note )
                        VALUES ( @Subject, @Message, @MessageType, @NotificationPaymentType, @CreateDate, @DeleteDate, @IsActive, @Note )
                    SELECT SCOPE_IDENTITY()", entity, UnitOfWork.Transaction);

                transaction.Commit();
            }
        }

        public void Update(NotificationTemplate entity)
        {
            throw new System.NotImplementedException();
        }

        public void Delete(int id)
        {
            using (var transaction = BeginTransaction())
            {
                UnitOfWork.Session.Execute(@"UPDATE NotificationTemplates SET DeleteDate = dbo.GETASTANADATE() WHERE Id = @id", new { id }, UnitOfWork.Transaction);
                transaction.Commit();
            }
        }

        public NotificationTemplate Get(int id)
        {
            throw new System.NotImplementedException();
        }

        public NotificationTemplate Find(object query)
        {
            throw new System.NotImplementedException();
        }

        public List<NotificationTemplate> List(ListQuery listQuery, object query = null)
        {
            throw new System.NotImplementedException();
        }

        public int Count(ListQuery listQuery, object query = null)
        {
            throw new System.NotImplementedException();
        }

        public NotificationTemplate Select(MessageType MessageType, NotificationPaymentType NotificationPaymentType)
        {
            return UnitOfWork.Session.QuerySingleOrDefault<NotificationTemplate>(@"
                SELECT *
                FROM NotificationTemplates
                WHERE   IsActive = 1 
                    AND NotificationPaymentType = @NotificationPaymentType
                    AND MessageType = @MessageType
                    AND DeleteDate IS NULL", new { MessageType, NotificationPaymentType }, UnitOfWork.Transaction);
        }

        public NotificationTemplate GetByCode(string messageCode)
        {
            return UnitOfWork.Session.QueryFirstOrDefault<NotificationTemplate>(@"
                SELECT *
                FROM NotificationTemplates
                WHERE   IsActive = 1 
                    AND MessageCode = @messageCode
                    AND DeleteDate IS NULL", new { messageCode }, UnitOfWork.Transaction);
        }

        public async Task<NotificationTemplate> GetByCodeAsync(string messageCode)
        {
            return await UnitOfWork.Session.QueryFirstOrDefaultAsync<NotificationTemplate>(@"
                SELECT *
                FROM NotificationTemplates
                WHERE   IsActive = 1 
                    AND MessageCode = @messageCode
                    AND DeleteDate IS NULL", new { messageCode }, UnitOfWork.Transaction);
        }
    }
}
