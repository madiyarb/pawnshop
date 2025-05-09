using Dapper;
using Pawnshop.Core;
using Pawnshop.Core.Impl;
using Pawnshop.Core.Queries;
using Pawnshop.Data.Models.Contracts;
using Pawnshop.Data.Models.Membership;
using Pawnshop.Data.Models.Transfers.TransferContracts;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Pawnshop.Data.Access
{
    public class TransferContractRepository : RepositoryBase, IRepository<TransferContract>
    {
        public TransferContractRepository(IUnitOfWork unitOfWork) : base(unitOfWork)
        {
        }

        public void TransferContracts(string Contracts, int PoolNumber)
        {
            var setter = $"set PoolNumber={PoolNumber}";
            var condition = $"WHERE contracts.DeleteDate IS NULL AND contracts.ContractNumber in({ Contracts})";
            UnitOfWork.Session.Query<Contract>($@"
            update contracts {setter} {condition}");
        }
        
        public List<int> PoolList()
        {
            return UnitOfWork.Session.Query<int>(
                @"SELECT DISTINCT PoolNumber FROM ContractTransfers WHERE PoolNumber IS NOT NULL ORDER BY 1").ToList();
        }

        public int PoolCount()
        {
            return UnitOfWork.Session.ExecuteScalar<int>(
                @"SELECT COUNT(DISTINCT PoolNumber) FROM ContractTransfers WHERE PoolNumber IS NOT NULL");
        }

        public List<Contract> GetContractsForTransfer(Dictionary<string,string> contractNumbers)
        {
            if (contractNumbers == null) throw new ArgumentNullException(nameof(contractNumbers));
            if (contractNumbers.Count==0) throw new ArgumentNullException(nameof(contractNumbers));

            List<Contract> contracts = new List<Contract>();

            foreach (var contract in contractNumbers)
            {
                var contractNumber = contract.Key;
                var clientIdentityNumber = contract.Value;

                var condition = $"WHERE c.DeleteDate IS NULL AND c.ContractNumber = {contractNumber} AND (cl.IdentityNumber={clientIdentityNumber} OR JSON_VALUE(c.ContractData, '$.Client.IdentityNumber')={clientIdentityNumber})";
                var from = "FROM Contracts c JOIN Clients cl ON cl.Id=c.ClientId ";

                string Query = $@"
                WITH ContractPaged AS (
                    SELECT DISTINCT c.Id,c.ContractNumber,c.ContractDate
                    {from}
                    JOIN MemberRelations mr ON mr.RightMemberId = c.OwnerId
                    {condition} 
                )
                SELECT DISTINCT c.*,
                    (CASE 
                        WHEN c.DeleteDate IS NOT NULL THEN 60
                        WHEN c.Status = 0 THEN 0
                        WHEN c.Status = 30 AND c.MaturityDate >= CONVERT(DATE, dbo.GETASTANADATE()) AND c.ProlongDate IS NULL THEN 10
                        WHEN c.Status = 30 AND c.MaturityDate < CONVERT(DATE, dbo.GETASTANADATE()) THEN 20
                        WHEN c.Status = 30 AND c.MaturityDate >= CONVERT(DATE, dbo.GETASTANADATE()) AND c.ProlongDate IS NOT NULL THEN 30
                        WHEN c.Status = 40 THEN 40
                        WHEN c.Status = 50 THEN 50
                        ELSE 0
                    END) AS DisplayStatus,
                    g.*,
                    u.*
                FROM ContractPaged cp
                JOIN Contracts c ON cp.Id = c.Id
                JOIN Groups g ON c.BranchId = g.Id
                JOIN Users u ON c.AuthorId = u.Id
                ORDER BY 1 DESC";

                var result = UnitOfWork.Session.Query<Contract, Group, User, Contract>(Query,
                    (c, g, u) =>
                    {
                        c.Branch = g;
                        c.Author = u;
                        return c;
                    }
                ).ToList().FirstOrDefault();

                contracts.Add(result);
            }
            return contracts;
        }

        public List<TransferContract> List(ListQuery listQuery)
        {
            if (listQuery == null) throw new ArgumentNullException(nameof(listQuery));
            var order = listQuery.Order(string.Empty, new Sort
            {
                Name = "EntryPosition",
                Direction = SortDirection.Asc
            });
            var page = listQuery.Page();

            return UnitOfWork.Session.Query<TransferContract, Contract, TransferContract>($@"
                SELECT *
                  FROM TransferContracts
                  LEFT JOIN Contracts ON Contracts.Id = TransferContracts.contractId
                WHERE PoolTransferId={listQuery.Filter} {order}",
                            (t, u) =>
                            {
                                t.Contract = u;
                                return t;
                            }, new
                            {
                                listQuery.Filter
                            }, UnitOfWork.Transaction).ToList();
        }

        public int Count(ListQuery listQuery)
        {
            if (listQuery == null) throw new ArgumentNullException(nameof(listQuery));
            
            return UnitOfWork.Session.ExecuteScalar<int>($@"
                SELECT COUNT(*)
                  FROM TransferContracts
                  LEFT JOIN Contracts ON Contracts.Id = TransferContracts.contractId
                WHERE PoolTransferId={listQuery.Filter}", new
                            {
                                listQuery.Filter
                            }, UnitOfWork.Transaction);
        }

        public void Insert(TransferContract entity)
        {
            using (var transaction = BeginTransaction())
            {
                entity.Id = UnitOfWork.Session.ExecuteScalar<int>($@"
                    INSERT INTO TransferContracts
                           ( PoolTransferId, ContractId, Status, ActionId, ErrorMessages, CreateDate, EntryPosition, EntryСontractNumber, EntryСlientIdentityNumber, Amount )
                    VALUES ( @PoolTransferId, @ContractId, @Status, @ActionId, @ErrorMessages, @CreateDate, @EntryPosition, @EntryСontractNumber, @EntryСlientIdentityNumber, @Amount )
                    SELECT SCOPE_IDENTITY()", entity, UnitOfWork.Transaction);

                transaction.Commit();
            }
        }

        public void Update(TransferContract entity)
        {
            using (var transaction = BeginTransaction())
            {
                UnitOfWork.Session.Execute(@"
                    UPDATE TransferContracts
                    SET Status=@Status, ActionId=@ActionId, ErrorMessages=@ErrorMessages, Amount=@Amount
                    WHERE Id = @Id", entity, UnitOfWork.Transaction);

                transaction.Commit();
            }
        }

        public TransferContract Get(int id) //TODO: check we need it?
        {
            return UnitOfWork.Session.QuerySingleOrDefault<TransferContract>(@"
                SELECT *
                  FROM TransferContracts t
                  WHERE t.PoolTransferId = @Id", new { id }, UnitOfWork.Transaction);
        }

        public List<TransferContract> List(ListQuery listQuery, object query = null)
        {
            throw new NotImplementedException();
        }

        public int Count(ListQuery listQuery, object query = null)
        {
            throw new NotImplementedException();
        }

        public TransferContract Find(object query)
        {
            throw new NotImplementedException();
        }

        public void Delete(int id)
        {
            throw new NotImplementedException();
        }
    }
}
