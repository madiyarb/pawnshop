using System;
using System.Collections.Generic;
using System.Linq;
using Dapper;
using Pawnshop.Core;
using Pawnshop.Core.Impl;
using Pawnshop.Core.Queries;
using Pawnshop.Data.Models.Investments;
using Pawnshop.Data.Models.Membership;

namespace Pawnshop.Data.Access
{
    public class InvestmentActionRepository : RepositoryBase, IRepository<InvestmentAction>
    {
        public InvestmentActionRepository(IUnitOfWork unitOfWork) : base(unitOfWork)
        {
        }

        public void Insert(InvestmentAction entity)
        {
            using (var transaction = UnitOfWork.BeginTransaction())
            {
                entity.Id = UnitOfWork.Session.QuerySingleOrDefault<int>(@"
INSERT INTO InvestmentActions ( InvestmentId, ActionDate, ActionType, ActionCost, OrderId, CreateDate, UserId, DeleteDate )
VALUES ( @InvestmentId, @ActionDate, @ActionType, @ActionCost, @OrderId, @CreateDate, @UserId, @DeleteDate )
SELECT SCOPE_IDENTITY()", entity, UnitOfWork.Transaction);

                transaction.Commit();
            }
        }

        public void Update(InvestmentAction entity)
        {
            throw new NotImplementedException();
        }

        public void Delete(int id)
        {
            using (var transaction = UnitOfWork.BeginTransaction())
            {
                UnitOfWork.Session.Execute(@"UPDATE InvestmentActions SET DeleteDate = dbo.GETASTANADATE() WHERE Id = @id", new { id }, UnitOfWork.Transaction);
                transaction.Commit();
            }
        }

        public InvestmentAction Get(int id)
        {
            return UnitOfWork.Session.Query<InvestmentAction, User, InvestmentAction>(@"
SELECT ia.*, u.*
FROM InvestmentActions ia
JOIN Users u ON ia.UserId = u.Id
WHERE ia.Id = @id", (ia, u) => {
                ia.User = u;
                return ia;
            }, new { id }).FirstOrDefault();
        }

        public InvestmentAction Find(object query)
        {
            throw new NotImplementedException();
        }

        public List<InvestmentAction> List(ListQuery listQuery, object query = null)
        {
            throw new NotImplementedException();
        }

        public int Count(ListQuery listQuery, object query = null)
        {
            throw new NotImplementedException();
        }
    }
}
