using System.Net.Http.Headers;
using System.Diagnostics.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Dapper;
using Pawnshop.Core;
using Pawnshop.Data.Models.Crm;
using Pawnshop.Core.Impl;
using Pawnshop.Core.Queries;

namespace Pawnshop.Data.Access
{
    public class CrmSyncContactRepository : RepositoryBase, IRepository<CrmSyncContact>
    {
        public CrmSyncContactRepository(IUnitOfWork unitOfWork) : base(unitOfWork)
        {
        }

        public void Dispose()
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

        public CrmSyncContact Find(object query)
        {
            throw new NotImplementedException();
        }

        public List<CrmSyncContact> FindByClientId(int id)
        {
            return UnitOfWork.Session.Query<CrmSyncContact>($@"
SELECT * FROM CrmSyncContacts WHERE ClientId = @id").ToList();
        }

        public CrmSyncContact Get(int id)
        {
            return UnitOfWork.Session.QuerySingleOrDefault<CrmSyncContact>(@"
SELECT * FROM CrmSyncContacts WHERE Id = @id", new { id }, UnitOfWork.Transaction);

        }

        public void Insert(CrmSyncContact entity)
        {
            using (var transaction = BeginTransaction())
            {
                entity.Id = UnitOfWork.Session.Execute(@"				
INSERT INTO CrmSyncContacts ( ClientId, ClientCrmId, CrmId, CrmName, CrmIdentityNumber, CreateDate, UploadDate ) VALUES 
( @ClientId, @ClientCrmId, @CrmId, @CrmName, @CrmIdentityNumber, @CreateDate, @UploadDate )
SELECT SCOPE_IDENTITY()", entity, UnitOfWork.Transaction);

                transaction.Commit();
            }
        }

        public List<CrmSyncContact> List(ListQuery listQuery, object query = null)
        {
            throw new NotImplementedException();
        }

        public void Update(CrmSyncContact entity)
        {
            throw new NotImplementedException();
        }
    }
}
