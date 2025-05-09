using Dapper;
using Pawnshop.Core.Impl;
using Pawnshop.Core.Queries;
using Pawnshop.Core;
using Pawnshop.Data.Models.Calls;
using Pawnshop.Data.Models.Clients;
using Pawnshop.Data.Models.Interaction;
using Pawnshop.Data.Models.Membership;
using Pawnshop.Data.Models.Notifications;
using System.Collections.Generic;
using System.Linq;
using System;
using Pawnshop.Core.Extensions;

namespace Pawnshop.Data.Access
{
    public class InteractionsRepository : RepositoryBase, IRepository<Interaction>
    {
        public InteractionsRepository(IUnitOfWork unitOfWork) : base(unitOfWork)
        {
        }

        public int Count(ListQuery listQuery, object query = null)
        {
            var clientId = query?.Val<int?>("ClientId");
            var interactionType = query?.Val<InteractionType?>("InteractionType");
            var externalPhone = query?.Val<string?>("ExternalPhone");
            var authorId = query?.Val<int?>("AuthorId");

            var pre = "WHERE i.DeleteDate IS NULL";

            if (clientId.HasValue)
                pre += " AND i.ClientId = @clientId";

            if (interactionType.HasValue)
                pre += " AND i.InteractionType = @interactionType";

            if (!string.IsNullOrEmpty(externalPhone))
                pre += " AND i.ExternalPhone = @externalPhone";

            if (authorId.HasValue)
                pre += " AND i.AuthorId = @authorId";

            return UnitOfWork.Session.ExecuteScalar<int>($@"SELECT COUNT(i.Id)
  FROM Interactions i
 {pre}",
                new { clientId, interactionType, externalPhone, authorId }, UnitOfWork.Transaction);
        }

        public void Delete(int id)
        {
            using (var transaction = BeginTransaction())
            {
                UnitOfWork.Session.Execute(@"UPDATE Interactions
   SET DeleteDate = @deleteDate
 WHERE Id = @id",
                    new { id, deleteDate = DateTime.Now }, UnitOfWork.Transaction);

                transaction.Commit();
            }
        }

        public Interaction Find(object query)
        {
            throw new NotImplementedException();
        }

        public Interaction Get(int id)
        {
            return UnitOfWork.Session.Query<Interaction, User, User, Client, Call, Notification, Interaction>($@"SELECT DISTINCT i.*,
       u.*,
       u2.*,
       c.*,
       cc.*,
       n.*
  FROM Interactions i
  JOIN Users u ON u.Id = i.AuthorId
  LEFT JOIN Users u2 ON u2.Id = i.UpdateAuthorId
  LEFT JOIN Clients c ON c.Id = i.ClientId
  LEFT JOIN Calls cc ON cc.Id = i.CallId
  LEFT JOIN Notifications n ON n.Id = i.SmsNotificationId
 WHERE i.Id = @id",
                (i, user, user2, client, call, notif) =>
                {
                    i.Author = user;
                    i.UpdateAuthor = user2;
                    i.Client = client;
                    i.Call = call;
                    i.SmsNotification = notif;

                    return i;
                },
                new { id }, UnitOfWork.Transaction)
                .FirstOrDefault();
        }

        public void Insert(Interaction entity)
        {
            using (var transaction = BeginTransaction())
            {
                entity.CreateDate = DateTime.Now;
                entity.Id = UnitOfWork.Session.QuerySingleOrDefault<int>(@"INSERT INTO Interactions ( CreateDate, AuthorId, InteractionType, ClientId, ApplicationOnlineId, InternalPhone, ExternalPhone, Result, CallPurposeId, AttractionChannelId, CallId, SmsNotificationId, CarYear, Firstname, Surname, Patronymic, PreferredLanguage )
VALUES ( @CreateDate, @AuthorId, @InteractionType, @ClientId, @ApplicationOnlineId, @InternalPhone, @ExternalPhone, @Result, @CallPurposeId, @AttractionChannelId, @CallId, @SmsNotificationId, @CarYear, @Firstname, @Surname, @Patronymic, @PreferredLanguage )

 SELECT SCOPE_IDENTITY()",
                    entity, UnitOfWork.Transaction);

                transaction.Commit();
            }
        }

        public List<Interaction> List(ListQuery listQuery, object query = null)
        {
            var clientId = query?.Val<int?>("ClientId");
            var interactionType = query?.Val<InteractionType?>("InteractionType");
            var externalPhone = query?.Val<string?>("ExternalPhone");
            var authorId = query?.Val<int?>("AuthorId");

            var pre = "WHERE i.DeleteDate IS NULL";

            if (clientId.HasValue)
                pre += " AND i.ClientId = @clientId";

            if (interactionType.HasValue)
                pre += " AND i.InteractionType = @interactionType";

            if (!string.IsNullOrEmpty(externalPhone))
                pre += " AND i.ExternalPhone = @externalPhone";

            if (authorId.HasValue)
                pre += " AND i.AuthorId = @authorId";

            var order = listQuery.Order(string.Empty, new Sort
            {
                Name = "i.Id",
                Direction = SortDirection.Desc
            });

            var page = listQuery.Page();

            var interactionList = UnitOfWork.Session.Query<Interaction, User, User, Client, Notification, Interaction>($@"SELECT DISTINCT i.*,
       u.*,
       u2.*,
       c.*,
       n.*
  FROM Interactions i
  JOIN Users u ON u.Id = i.AuthorId
  LEFT JOIN Users u2 ON u2.Id = i.UpdateAuthorId
  LEFT JOIN Clients c ON c.Id = i.ClientId
  LEFT JOIN Notifications n ON n.Id = i.SmsNotificationId
 {pre} {order} {page}",
                (i, user, user2, client, notif) =>
                {
                    i.Author = user;
                    i.UpdateAuthor = user2;
                    i.Client = client;
                    i.SmsNotification = notif;

                    return i;
                },
                new { clientId, interactionType, externalPhone, authorId, listQuery.Page.Limit, listQuery.Page.Offset }, UnitOfWork.Transaction)
                .ToList();

            var callInteractions = interactionList.Where(x => x.CallId.HasValue);

            if (!callInteractions.Any())
                return interactionList;

            var calls = UnitOfWork.Session.Query<Call, User, Call>(@"SELECT c.*,
       u.*
  FROM Calls c
  LEFT JOIN Users u ON u.Id = c.UserId
 WHERE c.Id IN @callIds",
                       (call, user) =>
                       {
                           call.User = user;

                           return call;
                       },
                       new { callIds = callInteractions.Select(x => x.CallId) }, UnitOfWork.Transaction);

            interactionList.ForEach(x =>
            {
                if (x.CallId.HasValue)
                {
                    x.Call = calls?.FirstOrDefault(c => c.Id == x.CallId);
                }
            });

            return interactionList;
        }

        public void Update(Interaction entity)
        {
            using (var transaction = BeginTransaction())
            {
                UnitOfWork.Session.Execute(@"UPDATE Interactions
   SET UpdateDate = @UpdateDate,
       UpdateAuthorId = @UpdateAuthorId,
       Result = @Result,
       CallPurposeId = @CallPurposeId,
       AttractionChannelId = @AttractionChannelId,
       CarYear = @CarYear,
       Firstname = @Firstname,
       Surname = @Surname,
       Patronymic = @Patronymic,
       PreferredLanguage = @PreferredLanguage,
       CallId = @CallId
 WHERE Id = @id",
                    entity, UnitOfWork.Transaction);

                transaction.Commit();
            }
        }

        public Interaction FindForLink(int? authorId)
        {
            if (!authorId.HasValue)
                return null;

            return UnitOfWork.Session.QueryFirstOrDefault<Interaction>(@"SELECT *
  FROM Interactions
 WHERE AuthorId = @authorId
   AND InteractionType = @type
   AND CallId IS NULL
   AND CreateDate > DATEADD(MINUTE, -15, dbo.GETASTANADATE())",
                new { authorId, type = InteractionType.CALL_OUTGOING }, UnitOfWork.Transaction);
        }
    }
}
