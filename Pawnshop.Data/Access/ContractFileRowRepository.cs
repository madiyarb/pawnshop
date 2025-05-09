using System;
using System.Collections.Generic;
using Dapper;
using Pawnshop.Core;
using Pawnshop.Core.Impl;
using Pawnshop.Core.Queries;
using Pawnshop.Data.Models.Contracts;

namespace Pawnshop.Data.Access
{
    public class ContractFileRowRepository : RepositoryBase, IRepository<ContractFileRow>
    {
        public ContractFileRowRepository(IUnitOfWork unitOfWork) : base(unitOfWork)
        {
        }

        public void Insert(ContractFileRow entity)
        {
            using (var transaction = BeginTransaction())
            {
                UnitOfWork.Session.Execute(@"
                    INSERT INTO ContractFileRows
                        (ContractId, FileRowId)
                    VALUES 
                        (@ContractId, @FileRowId)", 
                entity, UnitOfWork.Transaction);
                transaction.Commit();
            }
        }

        public void Update(ContractFileRow entity)
        {
            throw new System.NotImplementedException();
        }

        public void Delete(int id)
        {
            using (var transaction = BeginTransaction())
            {
                UnitOfWork.Session.Execute(@"UPDATE ContractFileRows SET DeleteDate=dbo.GETASTANADATE() WHERE Id = @id", new { id }, UnitOfWork.Transaction);
                transaction.Commit();
            }
        }

        public void DeleteFileRow(int id)
        {
            using (var transaction = BeginTransaction())
            {
                UnitOfWork.Session.Execute(@"UPDATE ContractFileRows SET DeleteDate=dbo.GETASTANADATE() WHERE FileRowId = @id", new { id }, UnitOfWork.Transaction);
                transaction.Commit();
            }
        }

        public ContractFileRow Get(int id)
        {
            throw new System.NotImplementedException();
        }

        public ContractFileRow Find(object query)
        {
            if (query == null) throw new ArgumentNullException(nameof(query));

            var contractId = query?.Val<int?>("ContractId");
            var fileRowId = query?.Val<int?>("FileRowId");

            return UnitOfWork.Session.QuerySingleOrDefault<ContractFileRow>(@"
SELECT TOP 1 *
FROM ContractFileRows
WHERE ContractId = @contractId AND DeleteDate IS NULL
    AND FileRowId = @fileRowId", new { contractId, fileRowId });
        }

        public List<ContractFileRow> List(ListQuery listQuery, object query = null)
        {
            throw new System.NotImplementedException();
        }

        public int Count(ListQuery listQuery, object query = null)
        {
            throw new System.NotImplementedException();
        }
    }
}