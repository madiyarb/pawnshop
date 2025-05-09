using Pawnshop.Core.Impl;
using Pawnshop.Core;
using System;
using System.Collections.Generic;
using Dapper;
using Pawnshop.Core.Queries;
using System.Threading.Tasks;
using Pawnshop.Data.Models.MobileApp.HardCollection.Entities;
using Pawnshop.Data.Models.MobileApp.HardCollection.Models;

namespace Pawnshop.Data.Access
{
    public class HCContractStatusRepository : RepositoryBase, IRepository<HCContractStatus>
    {
        public HCContractStatusRepository(IUnitOfWork unitOfWork) : base(unitOfWork)
        {

        }

        public void Insert(HCContractStatus entity)
        {
            using (var transaction = BeginTransaction())
            {
                entity.Id = UnitOfWork.Session.ExecuteScalar<int>(@"
                        INSERT INTO HCContractStatus (ContractId, StageId, IsActive)
                        VALUES(@ContractId, @StageId, @IsActive)
                        SELECT SCOPE_IDENTITY()", entity, UnitOfWork.Transaction);
                transaction.Commit();
            }
        }

        public async void InsertAsync(HCContractStatus entity)
        {
            using (var transaction = BeginTransaction())
            {
                await UnitOfWork.Session.ExecuteScalarAsync<int>(@"
                        INSERT INTO HCContractStatus (ContractId, StageId, IsActive)
                        VALUES(@ContractId, @StageId, @IsActive)
                        SELECT SCOPE_IDENTITY()", entity, UnitOfWork.Transaction);
                transaction.Commit();
            }
        }

        public void Update(HCContractStatus entity)
        {
            using (var transaction = BeginTransaction())
            {
                UnitOfWork.Session.Execute(@"
                        UPDATE HCContractStatus SET 
                            ContractId = @ContractId, 
                            StageId = @StageId,
                            IsActive = @IsActive
                        WHERE Id=@id AND DeleteDate IS NULL", entity, UnitOfWork.Transaction);
                transaction.Commit();
            }
        }

        public async Task UpdateAsync(HCContractStatus entity)
        {
            using (var transaction = BeginTransaction())
            {
                await UnitOfWork.Session.ExecuteAsync(@"
                        UPDATE HCContractStatus SET 
                            ContractId = @ContractId, 
                            StageId = @StageId,
                            IsActive = @IsActive
                        WHERE Id=@id AND DeleteDate IS NULL", entity, UnitOfWork.Transaction);
                transaction.Commit();
            }
        }

        public void Delete(int id)
        {
            using (var transaction = BeginTransaction())
            {
                UnitOfWork.Session.Execute(@"
                        UPDATE HCContractStatus SET DeleteDate = dbo.GETASTANADATE()
                        WHERE Id=@id", new { id }, UnitOfWork.Transaction);
                transaction.Commit();
            }
        }

        public async void DeleteAsync(int id)
        {
            using (var transaction = BeginTransaction())
            {
                await UnitOfWork.Session.ExecuteAsync(@"
                        UPDATE HCContractStatus SET DeleteDate = dbo.GETASTANADATE()
                        WHERE Id=@id", new { id }, UnitOfWork.Transaction);
                transaction.Commit();
            }
        }

        public async Task<HCContractStatus> GetAsync(int id)
        {
            return await UnitOfWork.Session.QueryFirstOrDefaultAsync<HCContractStatus>(@"
                            SELECT * 
                              FROM HCContractStatus
                              WHERE Id=@id AND DeleteDate IS NULL", new { id }, UnitOfWork.Transaction);
        }

        public HCContractStatus Get(int id)
        {
            return UnitOfWork.Session.QueryFirstOrDefault<HCContractStatus>(@"
                            SELECT * 
                              FROM HCContractStatus
                              WHERE Id=@id AND DeleteDate IS NULL", new { id }, UnitOfWork.Transaction);
        }

        public async Task<HCContractStatus> GetByContractIdAsync(int contractId)
        {
            return await UnitOfWork.Session.QueryFirstOrDefaultAsync<HCContractStatus>(@"
                            SELECT * 
                              FROM HCContractStatus
                              WHERE contractId=@contractId AND DeleteDate IS NULL", new { contractId }, UnitOfWork.Transaction);
        }

        public HCContractStatus GetByContractId(int contractId)
        {
            return UnitOfWork.Session.QueryFirstOrDefault<HCContractStatus>(@"
                            SELECT * 
                              FROM HCContractStatus
                              WHERE contractId=@contractId AND DeleteDate IS NULL", new { contractId }, UnitOfWork.Transaction);
        }

        public async Task<IEnumerable<HCContractStatus>> GetByClientIdAsync(int clientId)
        {
            return await UnitOfWork.Session.QueryAsync<HCContractStatus>(@"
                            SELECT * FROM Contracts c
                              INNER JOIN HCContractStatus hc ON hc.ContractId = c.Id
                              WHERE c.ClientId = @clientId
                              AND hc.IsActive = 1
                              AND hc.DeleteDate is null", new { clientId }, UnitOfWork.Transaction);
        }

        public IEnumerable<HCContractStatus> GetByClientId(int clientId)
        {
            return UnitOfWork.Session.Query<HCContractStatus>(@"
                            SELECT * FROM Contracts c
                              INNER JOIN HCContractStatus hc ON hc.ContractId = c.Id
                              WHERE c.ClientId = @clientId
                              AND hc.IsActive = 1
                              AND hc.DeleteDate is null", new { clientId }, UnitOfWork.Transaction);
        }

        public HCContractStatus Find(object query)
        {
            throw new NotImplementedException();
        }

        public List<HCContractStatus> List(ListQuery listQuery, object query = null)
        {
            throw new NotImplementedException();
        }

        public async Task<IEnumerable<HCContractStatus>> ListAsync()
        {
            return await UnitOfWork.Session.QueryAsync<HCContractStatus>(@"
                            SELECT * FROM HCContractStatus hc
                              WHERE hc.DeleteDate IS NULL AND hc.IsActive = 1", UnitOfWork.Transaction);
        }

        public async Task<IEnumerable<HardCollectionOverdue>> ListAllAsync()
        {
            return await UnitOfWork.Session.QueryAsync<HardCollectionOverdue>(@"
                            SELECT ContractId, CollectionStatusCode as StatusCode FROM CollectionContractStatuses
                              WHERE CollectionStatusCode in ('LEGALHARD_COLLECTION', 'HARD_COLLECTION')
                              AND IsActive = 1 AND DeleteDate IS NULL");
        }

        public int Count(ListQuery listQuery, object query = null)
        {
            throw new NotImplementedException();
        }

        public int GetDelayDays(int contractId)
        {
            return UnitOfWork.Session.QueryFirstOrDefault<int>(@"
                            SELECT DATEDIFF(Day,ccs.StartDelayDate,dbo.GetAstanaDate()) 
                              FROM CollectionContractStatuses ccs
                              WHERE ccs.ContractId = @contractId", new { contractId }, UnitOfWork.Transaction);
        }

        public Task<int> GetDelayDaysAsync(int contractId)
        {
            return UnitOfWork.Session.QueryFirstOrDefaultAsync<int>(@"
                            SELECT DATEDIFF(Day,ccs.StartDelayDate,dbo.GetAstanaDate()) 
                              FROM CollectionContractStatuses ccs
                              WHERE ccs.ContractId = @contractId", new { contractId }, UnitOfWork.Transaction);
        }
    }
}
