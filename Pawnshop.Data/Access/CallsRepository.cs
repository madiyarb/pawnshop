using Dapper;
using Pawnshop.Core;
using Pawnshop.Core.Impl;
using Pawnshop.Data.Models.Calls;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Pawnshop.Data.Access
{
    public class CallsRepository : RepositoryBase
    {
        public CallsRepository(IUnitOfWork unitOfWork) : base(unitOfWork)
        {
        }

        public void Delete(int id)
        {
            using (var transaction = BeginTransaction())
            {
                UnitOfWork.Session.Execute(@"UPDATE Calls
   SET DeleteDate = @deleteDate
 WHERE Id = @id",
                    new { id, deleteDate = DateTime.Now }, UnitOfWork.Transaction);

                transaction.Commit();
            }
        }

        public Call Get(int id)
        {
            return UnitOfWork.Session.QueryFirstOrDefault<Call>(@"SELECT *
  FROM Calls
 WHERE DeleteDate IS NULL
   AND Id = @id",
                new { id }, UnitOfWork.Transaction);
        }

        public Call Insert(Call entity, Guid? applicationOnlineId = null)
        {
            using (var transaction = BeginTransaction())
            {
                entity.CreateDate = DateTime.Now;
                entity.Id = UnitOfWork.Session.QuerySingleOrDefault<int>(@"INSERT INTO Calls ( CallPbxId, CreateDate, PhoneNumber, ClientId, Status, Duration, Direction, Language, RecordFile, UserInternalPhone, UserId )
 VALUES ( @CallPbxId, @CreateDate, @PhoneNumber, @ClientId, @Status, @Duration, @Direction, @Language, @RecordFile, @UserInternalPhone, @UserId )

 SELECT SCOPE_IDENTITY()",
                    entity, UnitOfWork.Transaction);

                if (applicationOnlineId != null)
                    UnitOfWork.Session.Execute(@"INSERT INTO ApplicationOnlineCalls ( ApplicationOnlineId, CallId )
 VALUES ( @applicationOnlineId, @callId )",
                        new { applicationOnlineId, callId = entity.Id }, UnitOfWork.Transaction);

                transaction.Commit();

                return entity;
            }
        }

        public void Update(Call entity)
        {
            using (var transaction = BeginTransaction())
            {
                entity.UpdateDate = DateTime.Now;

                UnitOfWork.Session.Execute(@"UPDATE Calls
   SET UpdateDate = @UpdateDate,
       ClientId = @ClientId,
       Status = @Status,
       Duration = @Duration,
       Direction = @Direction,
       Language = @Language,
       RecordFile = @RecordFile,
       UserInternalPhone = @UserInternalPhone,
       UserId = @UserId
 WHERE Id = @id",
                    entity, UnitOfWork.Transaction);

                transaction.Commit();
            }
        }

        public List<Call> List(object query)
        {
            if (query == null)
                return null;

            var clientId = query.Val<string>("ClientId");
            var applicationOnlineId = query.Val<string>("ApplicationOnlineId");

            var predicate = string.Empty;

            if (!string.IsNullOrEmpty(clientId))
                predicate += "\r\n   AND c.ClientId = @clientId";

            if (!string.IsNullOrEmpty(applicationOnlineId))
                predicate += "\r\n   AND aoc.ApplicationOnlineId = @applicationOnlineId";

            if (string.IsNullOrEmpty(predicate))
                return null;

            return UnitOfWork.Session.Query<Call>($@"SELECT *
  FROM Calls c
  LEFT JOIN ApplicationOnlineCalls aoc ON aoc.CallId = c.Id
 WHERE c.DeleteDate IS NULL
 {predicate}",
                new { clientId, applicationOnlineId }, UnitOfWork.Transaction)
                .ToList();
        }

        public Call GetByCallPbxId(string callPbxId)
        {
            return UnitOfWork.Session.QueryFirstOrDefault<Call>(@"SELECT * FROM Calls WHERE CallPbxId = @callPbxId",
                new { callPbxId }, UnitOfWork.Transaction);
        }
    }
}
