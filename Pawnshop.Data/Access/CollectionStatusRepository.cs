using Pawnshop.Core.Impl;
using Pawnshop.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using Pawnshop.Data.Models.Collection;
using Dapper;
using Pawnshop.Core.Queries;
using System.Threading.Tasks;
using Pawnshop.Core.Exceptions;
using Pawnshop.Data.Models.Contracts;

namespace Pawnshop.Data.Access
{
    public class CollectionStatusRepository : RepositoryBase, IRepository<CollectionContractStatus>
    {
        public CollectionStatusRepository(IUnitOfWork unitOfWork) : base(unitOfWork)
        {
            
        }

        public void Insert(CollectionContractStatus entity)
        {
            using (var transaction = BeginTransaction())
            {
                entity.Id = UnitOfWork.Session.ExecuteScalar<int>(@"
                        INSERT INTO CollectionContractStatuses (ContractId, FincoreStatusId, CollectionStatusCode, isActive, StartDelayDate)
                        VALUES(@ContractId, @FincoreStatusId, @CollectionStatusCode, @isActive, @StartDelayDate)
                        SELECT SCOPE_IDENTITY()", entity, UnitOfWork.Transaction);
                transaction.Commit();
            }
        }

        public void Update(CollectionContractStatus entity)
        {
            if(entity.StartDelayDate == DateTime.MinValue)
            {
                entity.StartDelayDate = DateTime.Now;
            }
            using (var transaction = BeginTransaction())
            {
                UnitOfWork.Session.Execute(@"
                        UPDATE CollectionContractStatuses SET 
                            ContractId = @ContractId, 
                            FincoreStatusId = @FincoreStatusId,
                            CollectionStatusCode = @CollectionStatusCode, 
                            isActive = @isActive,
                            StartDelayDate = @StartDelayDate
                        WHERE Id=@id AND DeleteDate IS NULL", entity, UnitOfWork.Transaction);
                transaction.Commit();
            }
        }

        public void Delete(int id)
        {
            using (var transaction = BeginTransaction())
            {
                UnitOfWork.Session.Execute(@"
                        UPDATE CollectionContractStatuses SET DeleteDate = dbo.GETASTANADATE()
                        WHERE Id=@id", new { id }, UnitOfWork.Transaction);
                transaction.Commit();
            }
        }

        public CollectionContractStatus Get(int id)
        {
            return UnitOfWork.Session.QueryFirstOrDefault<CollectionContractStatus>(@"
                            SELECT * 
                            FROM CollectionContractStatuses
                            WHERE Id=@id AND DeleteDate IS NULL", new { id }, UnitOfWork.Transaction);
        }
        public CollectionContractStatus GetByContractId(int contractId)
        {
            return UnitOfWork.Session.QueryFirstOrDefault<CollectionContractStatus>(@"
                            SELECT * 
                            FROM CollectionContractStatuses
                            WHERE contractId=@contractId AND DeleteDate IS NULL", new { contractId }, UnitOfWork.Transaction);
        }

        public async Task<CollectionContractStatus> GetByContractIdAsync(int contractId)
        {
            return await UnitOfWork.Session.QueryFirstOrDefaultAsync<CollectionContractStatus>(@"
                            SELECT * 
                            FROM CollectionContractStatuses
                            WHERE contractId=@contractId AND DeleteDate IS NULL", new { contractId }, UnitOfWork.Transaction);
        }

        public async Task<int> GetDelayDaysAsync(int contractId)
        {
            return await UnitOfWork.Session.QueryFirstOrDefaultAsync<int>(@"
                            SELECT DATEDIFF(Day,ccs.StartDelayDate,dbo.GetAstanaDate()) 
                              FROM CollectionContractStatuses ccs
                              WHERE ccs.ContractId = @contractId AND DeleteDate IS NULL", new { contractId }, UnitOfWork.Transaction);
        }

        public List<int> GetContractIdsByCollectionStatus(string collectionStatus)
        {
            if (collectionStatus is null)
            {
                throw new PawnshopApplicationException("При попытке поиска договоров по Collection status не передан статус");
            }
            
            var parameters = new { CollectionStatus = collectionStatus };
            var sqlQuery = @"
                SELECT C.Id
                FROM Contracts C
                    JOIN dbo.CollectionContractStatuses CCS on C.Id = CCS.ContractId
                WHERE C.DeleteDate IS NULL
                    AND CCS.CollectionStatusCode = @CollectionStatus";
            
            return UnitOfWork.Session.Query<int>(sqlQuery, parameters, UnitOfWork.Transaction).ToList();
        }

        public CollectionContractStatus Find(object query)
        {
            throw new NotImplementedException();
        }

        public List<CollectionContractStatus> List(ListQuery listQuery, object query = null)
        {
            throw new NotImplementedException();
        }

        public int Count(ListQuery listQuery, object query = null)
        {
            throw new NotImplementedException();
        }

        public List<Contract> GetOverdueContractsByClientId(int clientId)
        {
            return UnitOfWork.Session.Query<Contract>($@"
                            SELECT c.* FROM Contracts c
	                        LEFT JOIN CollectionContractStatuses ccs ON ccs.ContractId = c.Id
	                        WHERE ccs.DeleteDate IS NULL
	                        AND ccs.IsActive = 1
	                        AND c.ClientId = {clientId}", UnitOfWork.Transaction).AsList();
        }
    }
}
