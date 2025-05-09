using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Dapper;
using Pawnshop.Core;
using Pawnshop.Data.Models.Crm;
using Pawnshop.Core.Impl;
using Pawnshop.Core.Queries;
using Pawnshop.Data.Models._1c;

namespace Pawnshop.Data.Access
{
    public class PayOperationQueryRepository : RepositoryBase, IRepository<PayOperationQuery>
    {
        public PayOperationQueryRepository(IUnitOfWork unitOfWork) : base(unitOfWork)
        {
        }

        public int Count(ListQuery listQuery, object query = null)
        {
            throw new NotImplementedException();
        }

        public void Delete(int id)
        {
            throw new NotImplementedException();
        }

        public PayOperationQuery Find(object query)
        {
            throw new NotImplementedException();
        }
        public List<PayOperationQuery> Find()
        {
            return UnitOfWork.Session.Query<PayOperationQuery>($@"
SELECT *
FROM PayOperationQueryQueue WHERE QueryDate IS NULL", UnitOfWork.Transaction).ToList();
        }

        public PayOperationQuery Get(int id)
        {
            return UnitOfWork.Session.Query<PayOperationQuery>($@"
SELECT *
FROM PayOperationQueryQueue WHERE Id = @id", new {id}, UnitOfWork.Transaction).FirstOrDefault();
        }

        public void Insert(PayOperationQuery entity)
        {
            using (var transaction = BeginTransaction())
            {
                entity.Id = UnitOfWork.Session.Execute(@"
INSERT INTO PayOperationQueryQueue ( OperationId, QueryType, Status, AuthorId, CreateDate) VALUES ( @OperationId, @QueryType, @Status, @AuthorId, @CreateDate )
SELECT SCOPE_IDENTITY()", entity, UnitOfWork.Transaction);

                transaction.Commit();
            }
        }

        public List<PayOperationQuery> List(ListQuery listQuery, object query = null)
        {
            return UnitOfWork.Session.Query<PayOperationQuery>($@"
SELECT *
FROM PayOperationQueryQueue WHERE QueryDate IS NULL", UnitOfWork.Transaction).ToList();
        }

        public void Update(PayOperationQuery entity)
        {
            using (var transaction = BeginTransaction())
            {
                UnitOfWork.Session.Execute(@"
UPDATE PayOperationQueryQueue
SET QueryDate = @QueryDate, Status = @Status
WHERE Id = @Id", entity, UnitOfWork.Transaction);

                transaction.Commit();
            }
        }
    }
}
