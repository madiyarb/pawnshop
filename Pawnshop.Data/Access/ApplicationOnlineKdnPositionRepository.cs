using Dapper;
using Pawnshop.Core.Impl;
using Pawnshop.Core.Queries;
using Pawnshop.Core;
using Pawnshop.Data.Models.ApplicationsOnline.Kdn;
using System.Collections.Generic;
using System.Linq;
using System;

namespace Pawnshop.Data.Access
{
    public class ApplicationOnlineKdnPositionRepository : RepositoryBase, IRepository<ApplicationOnlineKdnPosition>
    {
        public ApplicationOnlineKdnPositionRepository(IUnitOfWork unitOfWork) : base(unitOfWork)
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
                UnitOfWork.Session.Execute(@"UPDATE ApplicationOnlineKdnPositions
   SET DeleteDate = @deleteDate
 WHERE Id = @id",
                    new { id, deleteDate = DateTime.Now }, UnitOfWork.Transaction);

                transaction.Commit();
            }
        }

        public ApplicationOnlineKdnPosition Find(object query)
        {
            throw new NotImplementedException();
        }

        public ApplicationOnlineKdnPosition Get(int id)
        {
            throw new NotImplementedException();
        }

        public void Insert(ApplicationOnlineKdnPosition entity)
        {
            using (var transaction = UnitOfWork.BeginTransaction())
            {
                entity.Id = UnitOfWork.Session.QuerySingleOrDefault<int>(@"
INSERT INTO ApplicationOnlineKdnPositions ( CreateDate, DeleteDate, ClientId, ApplicationOnlineId, CollateralType, Name, EstimatedCost )
VALUES ( @CreateDate, @DeleteDate, @ClientId, @ApplicationOnlineId, @CollateralType, @Name, @EstimatedCost )

SELECT SCOPE_IDENTITY()",
                entity, UnitOfWork.Transaction);

                transaction.Commit();
            }
        }

        public List<ApplicationOnlineKdnPosition> List(ListQuery listQuery, object query = null)
        {
            var appOnlineId = query.Val<Guid?>("ApplicationOnlineId");

            if (!appOnlineId.HasValue)
                return new List<ApplicationOnlineKdnPosition>();

            return UnitOfWork.Session.Query<ApplicationOnlineKdnPosition>(@"SELECT *
  FROM ApplicationOnlineKdnPositions
 WHERE DeleteDate IS NULL
   AND ApplicationOnlineId = @appOnlineId",
                new { appOnlineId }, UnitOfWork.Transaction)
                .ToList();
        }

        public void Update(ApplicationOnlineKdnPosition entity)
        {
            using (var transaction = UnitOfWork.BeginTransaction())
            {
                UnitOfWork.Session.Execute(@"
UPDATE ApplicationOnlineKdnPositions
   SET EstimatedCost = @EstimatedCost
 WHERE Id = @Id",
                entity, UnitOfWork.Transaction);

                transaction.Commit();
            }
        }
    }
}
