using Dapper;
using Pawnshop.Core;
using Pawnshop.Core.Impl;
using Pawnshop.Core.Queries;
using Pawnshop.Data.Models.Transfers;
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

namespace Pawnshop.Data.Access
{
    public class ContractTransferRepository : RepositoryBase, IRepository<ContractTransfer>
    {
        public ContractTransferRepository(IUnitOfWork unitOfWork) : base(unitOfWork)
        {
        }

        public void Insert(ContractTransfer entity)
        {
            using (var transaction = BeginTransaction())
            {
                entity.Id = UnitOfWork.Session.QuerySingleOrDefault<int>(@"
INSERT INTO ContractTransfers ( ContractId, TransferDate, BackTransferDate, PoolNumber)
VALUES ( @ContractId, @TransferDate, @BackTransferDate, @PoolNumber )
SELECT SCOPE_IDENTITY()", entity, UnitOfWork.Transaction);

                transaction.Commit();
            }
        }

        public void Update(ContractTransfer entity)
        {
            using (var transaction = BeginTransaction())
            {
                UnitOfWork.Session.Execute(@"
UPDATE ContractTransfers
SET ContractId = @ContractId, TransferDate = @TransferDate, BackTransferDate = @BackTransferDate, PoolNumber = @PoolNumber
WHERE Id = @Id", entity, UnitOfWork.Transaction);

                transaction.Commit();
            }
        }

        public ContractTransfer Get(int id)
        {
            return UnitOfWork.Session.QuerySingleOrDefault<ContractTransfer>(@"
SELECT * 
FROM ContractTransfers
WHERE Id=@id", new { id }, UnitOfWork.Transaction);
        }

        public List<ContractTransfer> GetContactTransfersByContractId(int contractId)
        {
            return UnitOfWork.Session.Query<ContractTransfer>(@"
SELECT *
FROM ContractTransfers 
WHERE ContractId = @contractId", new { contractId }, UnitOfWork.Transaction).ToList();
        }

        public void Delete(int id)
        {
            using (var transaction = UnitOfWork.BeginTransaction())
            {
                UnitOfWork.Session.Execute("DELETE FROM ContractTransfers WHERE Id = @id", new { id = id }, UnitOfWork.Transaction);
                transaction.Commit();
            }
        }

        public ContractTransfer Find(object query)
        {
            if (query == null) throw new ArgumentNullException(nameof(query));

            var contractId = query?.Val<int>("ContractId");

            return UnitOfWork.Session.QuerySingleOrDefault<ContractTransfer>(@"
                SELECT TOP 1 * FROM ContractTransfers WHERE ContractId=@contractId AND BackTransferDate IS NULL", new { contractId }, UnitOfWork.Transaction);
        }

        public List<ContractTransfer> List(ListQuery listQuery, object query = null)
        {
            throw new NotImplementedException();
        }

        public int Count(ListQuery listQuery, object query = null)
        {
            throw new NotImplementedException();
        }
    }
}
