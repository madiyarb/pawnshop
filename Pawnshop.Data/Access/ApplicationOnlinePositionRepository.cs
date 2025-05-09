using System;
using System.Linq;
using Dapper;
using Pawnshop.Core;
using Pawnshop.Core.Impl;
using Pawnshop.Data.Models.ApplicationsOnlinePosition;

namespace Pawnshop.Data.Access
{
    public sealed class ApplicationOnlinePositionRepository : RepositoryBase
    {
        public ApplicationOnlinePositionRepository(IUnitOfWork unitOfWork) : base(unitOfWork) { }

        public ApplicationOnlinePosition Get(Guid id)
        {
            return UnitOfWork.Session.Query<ApplicationOnlinePosition>(
                @"Select * from ApplicationOnlinePositions where id = @id",
                new { id }, UnitOfWork.Transaction).FirstOrDefault();
        }

        public void Insert(ApplicationOnlinePosition position)
        {
            using (var transaction = BeginTransaction())
            {
                UnitOfWork.Session.QuerySingleOrDefault(@"
                INSERT INTO ApplicationOnlinePositions (Id, CreateDate, CollateralType, LoanCost, EstimatedCost) 
                VALUES (@Id, @CreateDate, @CollateralType, @LoanCost, @EstimatedCost)
                SELECT SCOPE_IDENTITY()", position, UnitOfWork.Transaction);

                transaction.Commit();
            }
        }

        public void Update(ApplicationOnlinePosition position)
        {
            using (var transaction = BeginTransaction())
            {
                UnitOfWork.Session.QuerySingleOrDefault(@"
                UPDATE ApplicationOnlinePositions SET
                CreateDate= @CreateDate, 
                CollateralType= @CollateralType, 
                LoanCost= @LoanCost, 
                EstimatedCost= @EstimatedCost
                WHERE  Id= @id 
                SELECT SCOPE_IDENTITY()", position, UnitOfWork.Transaction);
                transaction.Commit();
            }
        }

        public ApplicationOnlinePosition GetByApplicationOnlineId(Guid appOnlineId)
        {
            return UnitOfWork.Session.QueryFirstOrDefault<ApplicationOnlinePosition>(@"SELECT aop.*
  FROM ApplicationOnlinePositions aop
  JOIN ApplicationsOnline ao ON ao.ApplicationOnlinePositionId = aop.Id
 WHERE ao.Id = @appOnlineId",
                new { appOnlineId }, UnitOfWork.Transaction);
        }
    }
}
