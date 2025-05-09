using System;
using System.Collections.Generic;
using Dapper;
using Pawnshop.Core;
using Pawnshop.Core.Impl;
using Pawnshop.Core.Queries;
using Pawnshop.Data.Models.Insurances;

namespace Pawnshop.Data.Access
{
    public class InsuranceActionRepository : RepositoryBase, IRepository<InsuranceAction>
    {
        public InsuranceActionRepository(IUnitOfWork unitOfWork) : base(unitOfWork)
        {
        }

        public void Insert(InsuranceAction entity)
        {
            using (var transaction = UnitOfWork.BeginTransaction())
            {
                entity.Id = UnitOfWork.Session.QuerySingleOrDefault<int>(@"
INSERT INTO InsuranceActions ( InsuranceId, ActionType, ActionDate, OrderId, AuthorId, DeleteDate )
VALUES ( @InsuranceId, @ActionType, @ActionDate, @OrderId, @AuthorId, @DeleteDate )
SELECT SCOPE_IDENTITY()", entity, UnitOfWork.Transaction);
                
                transaction.Commit();
            }
        }

        public void Update(InsuranceAction entity)
        {
            throw new NotImplementedException();
        }

        public void Delete(int id)
        {
            using (var transaction = UnitOfWork.BeginTransaction())
            {
                UnitOfWork.Session.Execute(@"UPDATE InsuranceActions SET DeleteDate = dbo.GETASTANADATE() WHERE Id = @id", new { id }, UnitOfWork.Transaction);
                transaction.Commit();
            }
        }

        public InsuranceAction Get(int id)
        {
            throw new NotImplementedException();
        }

        public InsuranceAction Find(object query)
        {
            throw new NotImplementedException();
        }

        public List<InsuranceAction> List(ListQuery listQuery, object query = null)
        {
            throw new NotImplementedException();
        }

        public int Count(ListQuery listQuery, object query = null)
        {
            throw new NotImplementedException();
        }
    }
}
