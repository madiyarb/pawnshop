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
    public class CrmUploadContractRepository : RepositoryBase, IRepository<CrmUploadContract>
    {
        public CrmUploadContractRepository(IUnitOfWork unitOfWork) : base(unitOfWork)
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

        public CrmUploadContract Find(object query)
        {
            throw new NotImplementedException();
        }
        public List<CrmUploadContract> Find()
        {
            return UnitOfWork.Session.Query<CrmUploadContract>($@"
SELECT cup.*
FROM CrmUploadContracts cup
JOIN Contracts c ON c.Id = cup.ContractId
WHERE cup.UploadDate IS NULL
AND c.CollateralType <> 60
AND c.Status >= 30
AND c.CreatedInOnline = 0
AND c.ContractClass in (1, 2)").ToList();
        }

        public CrmUploadContract Get(int id)
        {
            return UnitOfWork.Session.Query<CrmUploadContract>($@"
SELECT *
FROM CrmUploadContracts WHERE Id = @id", new {id}).FirstOrDefault();
        }

        public void Insert(CrmUploadContract entity)
        {
            using (var transaction = BeginTransaction())
            {
                entity.Id = UnitOfWork.Session.Execute(@"
INSERT INTO CrmUploadContracts ( ContractId, CreateDate, UserId, ContractCrmId, ClientCrmId, BitrixId ) VALUES ( @ContractId, @CreateDate, @UserId, @ContractCrmId, @ClientCrmId, @BitrixId )
SELECT SCOPE_IDENTITY()", entity, UnitOfWork.Transaction);

                transaction.Commit();
            }
        }

        public List<CrmUploadContract> List(ListQuery listQuery, object query = null)
        {
            throw new NotImplementedException();
        }

        public void Update(CrmUploadContract entity)
        {
            using (var transaction = BeginTransaction())
            {
                UnitOfWork.Session.Execute(@"
UPDATE CrmUploadContracts
SET UploadDate = @UploadDate, ContractCrmId = @ContractCrmId, ClientCrmId = @ClientCrmId, BitrixId = @BitrixId
WHERE Id = @Id", entity, UnitOfWork.Transaction);
                transaction.Commit();
            }
        }
    }
}
