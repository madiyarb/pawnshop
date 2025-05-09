using Pawnshop.Core.Impl;
using Pawnshop.Core;
using Pawnshop.Data.Models.CardCashOutTransaction;
using Pawnshop.Core.Queries;
using System;
using System.Collections.Generic;
using System.Linq;
using Dapper;

namespace Pawnshop.Data.Access
{
    public sealed class CardCashOutTransactionRepository : RepositoryBase, IRepository<CardCashOutTransaction>
    {
        public CardCashOutTransactionRepository(IUnitOfWork unitOfWork) : base(unitOfWork)
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

        public CardCashOutTransaction Find(object query)
        {
            throw new NotImplementedException();
        }

        public CardCashOutTransaction Get(int id)
        {
            return UnitOfWork.Session.Query<CardCashOutTransaction>(@"SELECT c.* from CardCashOutTransactions c WHERE c.Id = @id", new { id }, UnitOfWork.Transaction).FirstOrDefault();
        }

        public IEnumerable<CardCashOutTransaction> GetByContractId(int contractId)
        {
            return UnitOfWork.Session.Query<CardCashOutTransaction>(@"SELECT c.* from CardCashOutTransactions c WHERE c.ContractId = @ContractId Order by CreateDate Desc", new { contractId }, UnitOfWork.Transaction);
        }
        public List<CardCashOutTransaction> GetListByContractId(int contractId)
        {
            return UnitOfWork.Session.Query<CardCashOutTransaction>(@"SELECT c.* from CardCashOutTransactions c WHERE c.ContractId = @ContractId Order by CreateDate Desc", new { contractId }, UnitOfWork.Transaction).ToList();
        }

        public List<CardCashOutTransaction> GetListByStatus(string status, int skip, int take)
        {
            return UnitOfWork.Session.Query<CardCashOutTransaction>(@"SELECT c.* from CardCashOutTransactions c WHERE c.Status = @Status Order by CreateDate Desc OFFSET (@Skip) ROWS FETCH NEXT (@Take) ROWS ONLY", new { status, skip, take }, UnitOfWork.Transaction).ToList();
        }

        public CardCashOutTransaction GetByCustomerReference(string CustomerReference)
        {
            return UnitOfWork.Session.Query<CardCashOutTransaction>(@"SELECT c.* from CardCashOutTransactions c WHERE c.CustomerReference = @CustomerReference", new { CustomerReference }, UnitOfWork.Transaction).FirstOrDefault();
        }
        public void Insert(CardCashOutTransaction entity)
        {
            using (var transaction = BeginTransaction())
            {
                entity.CreateDate = DateTime.UtcNow;
                entity.Id = UnitOfWork.Session.QuerySingleOrDefault<int>(@"
INSERT INTO CardCashOutTransactions ( ClientId, ContractId, CardNumber, CardHolderName, Amount, CustomerReference, Url, TranGUID, Status, CreateDate)
VALUES ( @ClientId, @ContractId, @CardNumber, @CardHolderName, @Amount, @CustomerReference, @Url, @TranGUID, @Status, @CreateDate)
SELECT SCOPE_IDENTITY()",
                    entity, UnitOfWork.Transaction);
                transaction.Commit();
            }
        }

        public List<CardCashOutTransaction> List(ListQuery listQuery, object query = null)
        {
            throw new NotImplementedException();
        }

        public void Update(CardCashOutTransaction entity)
        {
            UnitOfWork.Session.Execute(@"
UPDATE CardCashOutTransactions
   SET ClientId = @ClientId, ContractId = @ContractId, CardNumber = @CardNumber, CardHolderName = @CardHolderName, 
       Amount = @Amount, CustomerReference = @CustomerReference, Url = @Url, TranGUID = @TranGUID, Status = @Status,
       CreateDate = @CreateDate
 WHERE Id = @Id",
                entity, UnitOfWork.Transaction);
        }

    }
}
