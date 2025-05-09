using Pawnshop.Core.Impl;
using Pawnshop.Core;
using Pawnshop.Data.Models.OnlineApplications;
using System;
using System.Collections.Generic;
using Pawnshop.Core.Queries;
using Dapper;
using System.Linq;

namespace Pawnshop.Data.Access
{
    public class OnlineApplicationPositionRepository : RepositoryBase, IRepository<OnlineApplicationPosition>
    {
        public OnlineApplicationPositionRepository(IUnitOfWork unitOfWork) : base(unitOfWork)
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

        public OnlineApplicationPosition Find(object query)
        {
            throw new NotImplementedException();
        }

        public OnlineApplicationPosition Get(int id)
        {
            return UnitOfWork.Session.Query<OnlineApplicationPosition, OnlineApplicationCar, OnlineApplicationPosition>(@"SELECT oap.*, oac.*
  FROM OnlineApplicationPositions oap
  LEFT JOIN OnlineApplicationCars oac ON oac.Id = oap.Id
 WHERE oap.Id = @id",
                (oap, oac) =>
                {
                    oap.Car = oac;
                    return oap;
                },
                new { id }, UnitOfWork.Transaction).FirstOrDefault();
        }

        public void Insert(OnlineApplicationPosition entity)
        {
            using (var transaction = BeginTransaction())
            {
                InternalInsert(entity);
                transaction.Commit();
            }
        }

        public void InternalInsert(OnlineApplicationPosition entity)
        {
            entity.CreateDate = DateTime.Now;
            entity.Id = UnitOfWork.Session.QuerySingleOrDefault<int>(@"
INSERT INTO OnlineApplicationPositions ( CreateDate, CollateralType, LoanCost, EstimatedCost )
VALUES ( @CreateDate, @CollateralType, @LoanCost, @EstimatedCost )

SELECT SCOPE_IDENTITY()",
                entity, UnitOfWork.Transaction);
        }

        public List<OnlineApplicationPosition> List(ListQuery listQuery, object query = null)
        {
            throw new NotImplementedException();
        }

        public void Update(OnlineApplicationPosition entity)
        {
            using (var transaction = BeginTransaction())
            {
                InternalUpdate(entity);
                transaction.Commit();
            }
        }

        public void InternalUpdate(OnlineApplicationPosition entity)
        {
            UnitOfWork.Session.Execute(@"
UPDATE OnlineApplicationPositions
   SET LoanCost = @LoanCost, EstimatedCost = @EstimatedCost
 WHERE Id = @Id",
                entity, UnitOfWork.Transaction);
        }
    }
}
