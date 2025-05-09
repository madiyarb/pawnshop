using Pawnshop.Core.Impl;
using Pawnshop.Core;
using System;
using System.Collections.Generic;
using System.Text;
using Dapper;
using Pawnshop.Core.Queries;
using System.Threading.Tasks;
using Pawnshop.Data.Models.MobileApp.HardCollection.Entities;
using System.Linq;

namespace Pawnshop.Data.Access
{
    public class HCActionHistoryRepository : RepositoryBase, IRepository<HCActionHistory>
    {
        public HCActionHistoryRepository(IUnitOfWork unitOfWork) : base(unitOfWork)
        {

        }
        public void Insert(HCActionHistory entity)
        {
            using (var transaction = BeginTransaction())
            {
                entity.Id = UnitOfWork.Session.ExecuteScalar<int>(@"
                        INSERT INTO HCActionHistory (HCContractStatusId, ActionName, ActionId, Value, Comment, AuthorId, CreateDate)
                        VALUES(@HCContractStatusId, @ActionName, @ActionId, @Value, @Comment, @AuthorId, @CreateDate)
                        SELECT SCOPE_IDENTITY()", entity, UnitOfWork.Transaction);
                transaction.Commit();
            }
        }

        public async Task InsertAsync(HCActionHistory entity)
        {
            using (var transaction = BeginTransaction())
            {
                entity.Id =  await UnitOfWork.Session.ExecuteScalarAsync<int>(@"
                        INSERT INTO HCActionHistory (HCContractStatusId, ActionName, ActionId, Value, Comment,AuthorId, CreateDate)
                        VALUES(@HCContractStatusId, @ActionName, @ActionId, @Value, @Comment, @AuthorId, @CreateDate)
                        SELECT SCOPE_IDENTITY()", entity, UnitOfWork.Transaction);
                transaction.Commit();
            }
        }

        public void Update(HCActionHistory entity)
        {
            throw new NotImplementedException();
        }

        public async void UpdateAsync(HCActionHistory entity)
        {
            throw new NotImplementedException();
        }

        public void Delete(int id)
        {
            throw new NotImplementedException();
        }

        public async void DeleteAsync(int id)
        {
            throw new NotImplementedException();
        }

        public async Task<HCActionHistory> GetAsync(int id)
        {
            return await UnitOfWork.Session.QueryFirstOrDefaultAsync<HCActionHistory>(@"
                            SELECT * 
                              FROM HCActionHistory
                              WHERE Id=@id AND DeleteDate IS NULL", new { id }, UnitOfWork.Transaction);
        }

        public HCActionHistory Get(int id)
        {
            return UnitOfWork.Session.QueryFirstOrDefault<HCActionHistory>(@"
                            SELECT * 
                              FROM HCActionHistory
                              WHERE Id=@id AND DeleteDate IS NULL", new { id }, UnitOfWork.Transaction);
        }

        public List<HCActionHistory> GetByContractId(int contractId)
        {
            return UnitOfWork.Session.Query<HCActionHistory>(@"
                            SELECT hcah.* 
                              FROM HCContractStatus hccs
                              INNER JOIN HCActionHistory hcah ON hcah.HCContractStatusId = hccs.Id
                              WHERE hccs.contractId=@contractId AND hccs.DeleteDate IS NULL AND hcah.DeleteDate IS NULL", new { contractId }, UnitOfWork.Transaction).ToList();
        }

        public HCActionHistory GetByClientId(int clientId)
        {
            return UnitOfWork.Session.QueryFirstOrDefault<HCActionHistory>(@"
                            SELECT hh.* FROM Contracts c
                              INNER JOIN HCContractStatus hc ON hc.ContractId = c.Id
                              INNER JOIN HCActionHistory hh ON hh.HCContractStatusId = hc.Id
                              WHERE c.ClientId = @clientId
                              AND hc.IsActive = 1
                              AND hc.DeleteDate IS NULL
                              AND hh.DeleteDate IS NULL", new { clientId }, UnitOfWork.Transaction);
        }

        public HCActionHistory Find(object query)
        {
            throw new NotImplementedException();
        }

        public List<HCActionHistory> List(ListQuery listQuery, object query = null)
        {
            throw new NotImplementedException();
        }

        public int Count(ListQuery listQuery, object query = null)
        {
            throw new NotImplementedException();
        }
    }
}
