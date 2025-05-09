using Dapper;
using Pawnshop.Core.Impl;
using Pawnshop.Core.Queries;
using Pawnshop.Core;
using Pawnshop.Data.Models.ApplicationOnlineNpck;
using System.Collections.Generic;
using System;

namespace Pawnshop.Data.Access
{
    public class ApplicationOnlineNpckSignFileRepository : RepositoryBase, IRepository<ApplicationOnlineNpckSignFile>
    {
        public ApplicationOnlineNpckSignFileRepository(IUnitOfWork unitOfWork) : base(unitOfWork) { }


        public int Count(ListQuery listQuery, object query = null)
        {
            throw new NotImplementedException();
        }

        public void Delete(int id)
        {
            throw new NotImplementedException();
        }

        public ApplicationOnlineNpckSignFile Find(object query)
        {
            throw new NotImplementedException();
        }

        public ApplicationOnlineNpckSignFile Get(int id)
        {
            throw new NotImplementedException();
        }

        public void Insert(ApplicationOnlineNpckSignFile entity)
        {
            using (var transaction = BeginTransaction())
            {
                entity.CreateDate = DateTime.Now;
                entity.Id = UnitOfWork.Session.ExecuteScalar<int>(@"
INSERT INTO ApplicationOnlineNpckSignFiles ( CreateDate, ApplicationOnlineFileId, EsignFileId )
VALUES ( @CreateDate, @ApplicationOnlineFileId, @EsignFileId )
SELECT SCOPE_IDENTITY()", entity, UnitOfWork.Transaction);

                transaction.Commit();
            }
        }

        public List<ApplicationOnlineNpckSignFile> List(ListQuery listQuery, object query = null)
        {
            throw new NotImplementedException();
        }

        public void Update(ApplicationOnlineNpckSignFile entity)
        {
            throw new NotImplementedException();
        }
    }
}
