using Dapper;
using Pawnshop.Core.Impl;
using Pawnshop.Core.Queries;
using Pawnshop.Core;
using Pawnshop.Data.Models.CallPurpose;
using Pawnshop.Data.Models.Membership;
using System.Collections.Generic;
using System.Linq;
using System;

namespace Pawnshop.Data.Access
{
    public class CallPurposesRepository : RepositoryBase, IRepository<CallPurpose>
    {
        public CallPurposesRepository(IUnitOfWork unitOfWork) : base(unitOfWork)
        {
        }

        public int Count(ListQuery listQuery, object query = null)
        {
            return UnitOfWork.Session.ExecuteScalar<int>($@"SELECT COUNT(Id) FROM CallPurposes WHERE DeleteDate IS NULL", null, UnitOfWork.Transaction);
        }

        public void Delete(int id)
        {
            using (var transaction = BeginTransaction())
            {
                UnitOfWork.Session.Execute(@"UPDATE CallPurposes
   SET DeleteDate = @deleteDate
 WHERE Id = @id",
                    new { id, deleteDate = DateTime.Now }, UnitOfWork.Transaction);

                transaction.Commit();
            }
        }

        public CallPurpose Find(object query)
        {
            throw new NotImplementedException();
        }

        public CallPurpose Get(int id)
        {
            return UnitOfWork.Session.Query<CallPurpose, User, User, CallPurpose>(@"SELECT TOP 1 cp.*,
       u.*,
       u2.*
  FROM CallPurposes cp
  LEFT JOIN Users u ON u.Id = cp.AuthorId
  LEFT JOIN Users u2 ON u2.Id = cp.UpdateAuthorId
 WHERE cp.DeleteDate IS NULL
   AND cp.Id = @id",
                (cp, u, u2) =>
                {
                    cp.Author = u;
                    cp.UpdateAuthor = u2;
                    return cp;
                },
                new { id }, UnitOfWork.Transaction)
                .FirstOrDefault();
        }

        public void Insert(CallPurpose entity)
        {
            using (var transaction = BeginTransaction())
            {
                entity.CreateDate = DateTime.Now;
                entity.Id = UnitOfWork.Session.QuerySingleOrDefault<int>(@"INSERT INTO CallPurposes ( CreateDate, AuthorId, Title )
 VALUES ( @CreateDate, @AuthorId, @Title )

SELECT SCOPE_IDENTITY()",
                    entity, UnitOfWork.Transaction);

                transaction.Commit();
            }
        }

        public List<CallPurpose> List(ListQuery listQuery, object query = null)
        {
            return UnitOfWork.Session.Query<CallPurpose>(@"SELECT * FROM CallPurposes WHERE DeleteDate IS NULL", null, UnitOfWork.Transaction).ToList();
        }

        public void Update(CallPurpose entity)
        {
            using (var transaction = BeginTransaction())
            {
                UnitOfWork.Session.Execute(@"UPDATE CallPurposes
   SET Title = @Title,
       UpdateDate = @UpdateDate,
       UpdateAuthorId = @UpdateAuthorId
 WHERE Id = @Id",
                    entity, UnitOfWork.Transaction);

                transaction.Commit();
            }
        }
    }
}
