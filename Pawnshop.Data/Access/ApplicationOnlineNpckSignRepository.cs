using Dapper;
using Pawnshop.Core.Impl;
using Pawnshop.Core.Queries;
using Pawnshop.Core;
using Pawnshop.Data.Models.ApplicationOnlineNpck;
using System.Collections.Generic;
using System;

namespace Pawnshop.Data.Access
{
    public class ApplicationOnlineNpckSignRepository : RepositoryBase, IRepository<ApplicationOnlineNpckSign>
    {
        private readonly int _maxUrlLiveMinutes = 15;

        public ApplicationOnlineNpckSignRepository(IUnitOfWork unitOfWork) : base(unitOfWork) { }


        public int Count(ListQuery listQuery, object query = null)
        {
            throw new NotImplementedException();
        }

        public void Delete(int id)
        {
            throw new NotImplementedException();
        }

        public ApplicationOnlineNpckSign Find(object query)
        {
            var appId = query?.Val<Guid?>("ApplicationOnlineId");
            var checkExpireDate = query?.Val<bool?>("CheckExpireDate");

            if (appId == null)
                return null;

            var predicate = "WHERE ApplicationOnlineId = @appId";

            if (checkExpireDate.HasValue)
                predicate += "  AND ExpireDate > dbo.GETASTANADATE()";

            return UnitOfWork.Session.QueryFirstOrDefault<ApplicationOnlineNpckSign>($@"SELECT *
  FROM ApplicationOnlineNpckSign
 {predicate}",
                new { appId }, UnitOfWork.Transaction);
        }

        public ApplicationOnlineNpckSign Get(int id)
        {
            return UnitOfWork.Session.QueryFirstOrDefault<ApplicationOnlineNpckSign>(@"SELECT *
  FROM ApplicationOnlineNpckSign
 WHERE Id = @id",
                new { id }, UnitOfWork.Transaction);
        }

        public void Insert(ApplicationOnlineNpckSign entity)
        {
            using (var transaction = BeginTransaction())
            {
                entity.CreateDate = DateTime.Now;
                entity.ExpireDate = entity.CreateDate.AddMinutes(_maxUrlLiveMinutes);
                entity.Id = UnitOfWork.Session.ExecuteScalar<int>(@"
INSERT INTO ApplicationOnlineNpckSign ( CreateDate, ExpireDate, ApplicationOnlineId, SignUrl, IsSigned, Code )
VALUES ( @CreateDate, @ExpireDate, @ApplicationOnlineId, @SignUrl, @IsSigned, @Code )
SELECT SCOPE_IDENTITY()", entity, UnitOfWork.Transaction);

                transaction.Commit();
            }
        }

        public List<ApplicationOnlineNpckSign> List(ListQuery listQuery, object query = null)
        {
            throw new NotImplementedException();
        }

        public void Update(ApplicationOnlineNpckSign entity)
        {
            using (var transaction = BeginTransaction())
            {
                UnitOfWork.Session.Execute(@"UPDATE ApplicationOnlineNpckSign
   SET IsSigned = @IsSigned,
       Code = @Code
 WHERE Id = @Id",
                    entity, UnitOfWork.Transaction);

                transaction.Commit();
            }
        }
    }
}
