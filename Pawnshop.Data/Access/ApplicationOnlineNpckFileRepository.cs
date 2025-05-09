using Dapper;
using Pawnshop.Core.Impl;
using Pawnshop.Core.Queries;
using Pawnshop.Core;
using Pawnshop.Data.Models.ApplicationOnlineNpck;
using System.Collections.Generic;
using System.Linq;
using System;
using Pawnshop.Data.Models.ApplicationOnlineFiles;

namespace Pawnshop.Data.Access
{
    public class ApplicationOnlineNpckFileRepository : RepositoryBase, IRepository<ApplicationOnlineNpckFile>
    {
        public ApplicationOnlineNpckFileRepository(IUnitOfWork unitOfWork) : base(unitOfWork) { }


        public int Count(ListQuery listQuery, object query = null)
        {
            throw new NotImplementedException();
        }

        public void Delete(int id)
        {
            using (var transaction = BeginTransaction())
            {
                UnitOfWork.Session.Execute(@"UPDATE ApplicationOnlineNpckFiles
   SET DeleteDate = @deleteDate
 WHERE Id = @id",
                    new { id, deleteDate = DateTime.Now }, UnitOfWork.Transaction);

                transaction.Commit();
            }
        }

        public ApplicationOnlineNpckFile Find(object query)
        {
            var fileId = query?.Val<Guid?>("ApplicationOnlineFileId");
            var applicationId = query?.Val<Guid?>("ApplicationOnlineId");

            if (!fileId.HasValue && !applicationId.HasValue)
                return null;

            var predicate = "WHERE aonf.ExpireDate > dbo.GETASTANADATE()";
            predicate += " AND aof.DeleteDate IS NULL";

            if (fileId.HasValue)
                predicate += " AND aonf.ApplicationOnlineFileId = @fileId";

            if (applicationId.HasValue)
                predicate += " AND aof.ApplicationId = @applicationId";

            return UnitOfWork.Session.QueryFirstOrDefault<ApplicationOnlineNpckFile>($@"SELECT aonf.*
  FROM ApplicationOnlineNpckFiles aonf
  JOIN ApplicationOnlineFiles aof ON aof.Id = aonf.ApplicationOnlineFileId
 {predicate}",
                new { fileId, applicationId }, UnitOfWork.Transaction);
        }

        public ApplicationOnlineNpckFile Get(int id)
        {
            return UnitOfWork.Session.QueryFirstOrDefault<ApplicationOnlineNpckFile>(@"SELECT *
  FROM ApplicationOnlineNpckFiles
 WHERE Id = @id",
                new { id }, UnitOfWork.Transaction);
        }

        public void Insert(ApplicationOnlineNpckFile entity)
        {
            using (var transaction = BeginTransaction())
            {
                entity.CreateDate = DateTime.Now;
                entity.ExpireDate = DateTime.Today.AddDays(1).AddSeconds(-1);
                entity.Id = UnitOfWork.Session.ExecuteScalar<int>(@"
INSERT INTO ApplicationOnlineNpckFiles ( CreateDate, ExpireDate, ApplicationOnlineFileId, NpckFileId, FileUrl, FutureFileStorageId )
VALUES ( @CreateDate, @ExpireDate, @ApplicationOnlineFileId, @NpckFileId, @FileUrl, @FutureFileStorageId )
SELECT SCOPE_IDENTITY()", entity, UnitOfWork.Transaction);

                transaction.Commit();
            }
        }

        public List<ApplicationOnlineNpckFile> List(ListQuery listQuery, object query = null)
        {

            var applicationId = query?.Val<Guid?>("ApplicationOnlineId");

            if (!applicationId.HasValue)
                return null;

            return UnitOfWork.Session.Query<ApplicationOnlineNpckFile, ApplicationOnlineFile, ApplicationOnlineNpckFile>(@"SELECT aonf.*,
       aof.*
  FROM ApplicationOnlineNpckFiles aonf
  JOIN ApplicationOnlineFiles aof ON aof.Id = aonf.ApplicationOnlineFileId
 WHERE aof.DeleteDate IS NULL
   AND aof.ApplicationId = @applicationId",
                (nf, af) =>
                {
                    nf.ApplicationOnlineFile = af;
                    return nf;
                },
                new { applicationId }, UnitOfWork.Transaction)
                .ToList();
        }

        public void Update(ApplicationOnlineNpckFile entity)
        {
            using (var transaction = BeginTransaction())
            {
                UnitOfWork.Session.Execute(@"UPDATE ApplicationOnlineNpckFiles
   SET FileUrl = @FileUrl
 WHERE Id = @Id",
                    entity, UnitOfWork.Transaction);

                transaction.Commit();
            }
        }
    }
}
