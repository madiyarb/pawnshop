using System;
using System.Collections.Generic;
using System.Linq;
using Dapper;
using Pawnshop.Core;
using Pawnshop.Core.Impl;
using Pawnshop.Core.Queries;
using Pawnshop.Data.Models.Files;
using Pawnshop.Data.Models.Membership;
using Pawnshop.Data.Models.PayOperations;

namespace Pawnshop.Data.Access
{
    public class PayOperationActionRepository : RepositoryBase, IRepository<PayOperationAction>
    {
        public PayOperationActionRepository(IUnitOfWork unitOfWork) : base(unitOfWork)
        {
        }

        public void Insert(PayOperationAction entity)
        {
            using (var transaction = BeginTransaction())
            {
                entity.Id = UnitOfWork.Session.QuerySingleOrDefault<int>(@"
INSERT INTO PayOperationActions ( ActionType, Date, OperationId, Note, AuthorId, CreateDate )
VALUES ( @ActionType, @Date, @OperationId, @Note,  @AuthorId, @CreateDate )
SELECT SCOPE_IDENTITY()", entity, UnitOfWork.Transaction);

                transaction.Commit();
            }
        }

        public void Update(PayOperationAction entity)
        {
            using (var transaction = BeginTransaction())
            {
                UnitOfWork.Session.Execute(@"
UPDATE PayOperationActions
SET Note = @Note, Date = @Date
WHERE Id = @Id", entity, UnitOfWork.Transaction);

                transaction.Commit();
            }
        }

        public void Delete(int id)
        {
            throw new NotImplementedException();
        }

        public PayOperationAction Get(int id)
        {
            var result = UnitOfWork.Session.Query<PayOperationAction, User, PayOperationAction>(@"
SELECT pa.*, u.*
FROM PayOperationActions pa
LEFT JOIN Users u ON u.Id = pa.AuthorId
WHERE pa.Id = @id", (po, u) => {
                po.Author = u;
                return po;
            }, new { id }, UnitOfWork.Transaction).FirstOrDefault();

            return result;
        }

        public PayOperationAction Find(object query)
        {
            throw new NotImplementedException();
        }

        public List<PayOperationAction> List(ListQuery listQuery, object query = null)
        {
            throw new NotImplementedException();
        }

        public int Count(ListQuery listQuery, object query = null)
        {
            throw new NotImplementedException();
        }
    }
}