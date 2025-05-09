using Dapper;
using Pawnshop.Core.Impl;
using Pawnshop.Core.Queries;
using Pawnshop.Core;
using Pawnshop.Data.Models.InteractionResults;
using Pawnshop.Data.Models.Membership;
using System.Collections.Generic;
using System.Linq;
using System;

namespace Pawnshop.Data.Access
{
    public class InteractionResultsRepository : RepositoryBase, IRepository<InteractionResult>
    {
        public InteractionResultsRepository(IUnitOfWork unitOfWork) : base(unitOfWork)
        {
        }

        public int Count(ListQuery listQuery, object query = null)
        {
            return UnitOfWork.Session.ExecuteScalar<int>($@"SELECT COUNT(Id) FROM InteractionResults WHERE DeleteDate IS NULL", null, UnitOfWork.Transaction);
        }

        public void Delete(int id)
        {
            using (var transaction = BeginTransaction())
            {
                UnitOfWork.Session.Execute(@"UPDATE InteractionResults
   SET DeleteDate = @deleteDate
 WHERE Id = @id",
                    new { id, deleteDate = DateTime.Now }, UnitOfWork.Transaction);

                transaction.Commit();
            }
        }

        public InteractionResult Find(object query)
        {
            throw new NotImplementedException();
        }

        public InteractionResult Get(int id)
        {
            return UnitOfWork.Session.Query<InteractionResult, User, User, InteractionResult>(@"SELECT TOP 1 ir.*,
       u.*,
       u2.*
  FROM InteractionResults ir
  LEFT JOIN Users u ON u.Id = ir.AuthorId
  LEFT JOIN Users u2 ON u2.Id = ir.UpdateAuthorId
 WHERE ir.DeleteDate IS NULL
   AND ir.Id = @id",
                (ir, u, u2) =>
                {
                    ir.Author = u;
                    ir.UpdateAuthor = u2;
                    return ir;
                },
                new { id }, UnitOfWork.Transaction)
                .FirstOrDefault();
        }

        public void Insert(InteractionResult entity)
        {
            using (var transaction = BeginTransaction())
            {
                entity.CreateDate = DateTime.Now;
                entity.Id = UnitOfWork.Session.QuerySingleOrDefault<int>(@"INSERT INTO InteractionResults ( CreateDate, AuthorId, Title )
 VALUES ( @CreateDate, @AuthorId, @Title )

SELECT SCOPE_IDENTITY()",
                    entity, UnitOfWork.Transaction);

                transaction.Commit();
            }
        }

        public List<InteractionResult> List(ListQuery listQuery, object query = null)
        {
            return UnitOfWork.Session.Query<InteractionResult>(@"SELECT * FROM InteractionResults WHERE DeleteDate IS NULL", null, UnitOfWork.Transaction).ToList();
        }

        public void Update(InteractionResult entity)
        {
            using (var transaction = BeginTransaction())
            {
                UnitOfWork.Session.Execute(@"UPDATE InteractionResults
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
