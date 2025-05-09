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
using Pawnshop.Data.Models.Clients;

namespace Pawnshop.Data.Access
{
    public class CrmSyncContactQueueRepository : RepositoryBase, IRepository<CrmSyncContact>
    {
        public CrmSyncContactQueueRepository(IUnitOfWork unitOfWork) : base(unitOfWork)
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

        public CrmSyncContact Get(int id)
        {
            throw new NotImplementedException();
        }

        public void Insert(CrmSyncContact entity)
        {
            using (var transaction = BeginTransaction())
            {
                entity.Id = UnitOfWork.Session.Execute(@"				
INSERT INTO CrmSyncContactsQueue ( ClientId, CreateDate ) VALUES 
( @ClientId, @CreateDate ) SELECT SCOPE_IDENTITY()", entity, UnitOfWork.Transaction);

                transaction.Commit();
            }
        }

        public List<CrmSyncContact> List(ListQuery listQuery, object query = null)
        {
            throw new NotImplementedException();
        }

        public void Update(CrmSyncContact entity)
        {
            using (var transaction = BeginTransaction())
            {
                UnitOfWork.Session.Execute(@"
UPDATE CrmSyncContactsQueue SET UploadDate = @UploadDate WHERE Id = @Id", entity, UnitOfWork.Transaction);

                transaction.Commit();
            }
        }

        public List<CrmSyncContact> GenerateQueue()
        {
            return UnitOfWork.Session.Query<CrmSyncContact>(@"
                INSERT INTO CrmSyncContactsQueue (ClientId, CreateDate)
OUTPUT Inserted.*
SELECT DISTINCT Clients.Id, dbo.GETASTANADATE() as CreateDate
FROM Clients 
LEFT JOIN Contracts ON Contracts.ClientId=Clients.Id
WHERE Contracts.Status IN (30, 50)
AND Clients.Id NOT IN (SELECT ClientId FROM CrmSyncContactsQueue) ORDER BY Clients.Id").ToList();
        }
    }
}
