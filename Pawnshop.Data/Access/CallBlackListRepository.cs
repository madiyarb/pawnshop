using Dapper;
using Pawnshop.Core.Impl;
using Pawnshop.Core.Queries;
using Pawnshop.Core;
using Pawnshop.Data.Models.Calls;
using Pawnshop.Data.Models.Membership;
using System.Collections.Generic;
using System.Linq;
using System;

namespace Pawnshop.Data.Access
{
    public class CallBlackListRepository : RepositoryBase, IRepository<CallBlackList>
    {
        public CallBlackListRepository(IUnitOfWork unitOfWork) : base(unitOfWork)
        {
        }

        public int Count(ListQuery listQuery, object query = null)
        {
            throw new NotImplementedException();
        }

        public void Delete(int id)
        {
            using (var transaction = UnitOfWork.BeginTransaction())
            {
                UnitOfWork.Session.Execute(@"UPDATE CallBlackList
   SET DeleteDate = @deleteDate
 WHERE Id = @id",
                    new { id, deleteDate = DateTime.Now }, UnitOfWork.Transaction);

                transaction.Commit();
            }
        }

        public CallBlackList Find(object query)
        {
            throw new NotImplementedException();
        }

        public CallBlackList Get(int id)
        {
            return UnitOfWork.Session.Query<CallBlackList, User, CallBlackList>(@"SELECT cbl.*,
       u.*
  FROM CallBlackList cbl
  LEFT JOIN Users u ON u.Id = cbl.AuthorId
 WHERE cbl.DeleteDate IS NULL
   AND cbl.Id = @id",
                (cbl, u) =>
                {
                    if (u != null)
                        cbl.Author = u;

                    return cbl;
                },
                new { id }, UnitOfWork.Transaction)
                .FirstOrDefault();
        }

        public void Insert(CallBlackList entity)
        {
            using (var transaction = UnitOfWork.BeginTransaction())
            {
                entity.CreateDate = DateTime.Now;
                entity.Id = UnitOfWork.Session.QuerySingleOrDefault<int>(@"INSERT INTO CallBlackList ( PhoneNumber, Reason, CreateDate, ExpireDate, AuthorId )
 VALUES ( @PhoneNumber, @Reason, @CreateDate, @ExpireDate, @AuthorId )

SELECT SCOPE_IDENTITY()",
                    entity, UnitOfWork.Transaction);

                if (entity.AuthorId.HasValue)
                    entity.Author = UnitOfWork.Session.QueryFirstOrDefault<User>(@"SELECT * FROM Users WHERE Id = @id",
                        new { id = entity.AuthorId.Value }, UnitOfWork.Transaction);

                transaction.Commit();
            }
        }

        public List<CallBlackList> List(ListQuery listQuery, object query = null)
        {
            if (query == null)
                return null;

            var phoneNumber = query.Val<string>("PhoneNumber");

            if (string.IsNullOrEmpty(phoneNumber))
                return null;

            return UnitOfWork.Session.Query<CallBlackList, User, CallBlackList>(@"SELECT cbl.*,
       u.*
  FROM CallBlackList cbl
  LEFT JOIN Users u ON u.Id = cbl.AuthorId
 WHERE cbl.DeleteDate IS NULL
   AND cbl.ExpireDate >= @currentDate
   AND cbl.PhoneNumber = @phoneNumber",
                (cbl, u) =>
                {
                    if (u != null)
                        cbl.Author = u;

                    return cbl;
                },
                new { currentDate = DateTime.Now, phoneNumber }, UnitOfWork.Transaction)
                .ToList();
        }

        public void Update(CallBlackList entity)
        {
            using (var transaction = UnitOfWork.BeginTransaction())
            {
                entity.UpdateDate = DateTime.Now;
                UnitOfWork.Session.Execute(@"UPDATE CallBlackList
   SET UpdateDate = @UpdateDate,
       ExpireDate = @ExpireDate,
       Reason = @Reason,
       AuthorId = @AuthorId
 WHERE Id = @Id",
                    entity, UnitOfWork.Transaction);

                transaction.Commit();
            }
        }
    }
}
