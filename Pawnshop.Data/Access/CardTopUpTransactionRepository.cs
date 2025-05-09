using Pawnshop.Core.Impl;
using Pawnshop.Core;
using Pawnshop.Data.Models.CardTopUpTransaction;
using Pawnshop.Core.Queries;
using System.Collections.Generic;
using System.Linq;
using Dapper;
using System;

namespace Pawnshop.Data.Access
{
    public sealed class CardTopUpTransactionRepository : RepositoryBase, IRepository<CardTopUpTransaction>
    {
        public CardTopUpTransactionRepository(IUnitOfWork unitOfWork) : base(unitOfWork)
        {
            
        }
        public int Count(ListQuery listQuery, object query = null)
        {
            throw new System.NotImplementedException();
        }

        public void Delete(int id)
        {
            throw new System.NotImplementedException();
        }

        public CardTopUpTransaction Find(object query)
        {
            throw new System.NotImplementedException();
        }

        public CardTopUpTransaction Get(int id)
        {
            throw new System.NotImplementedException();
        }

        public CardTopUpTransaction GetByCustomerReference(string CustomerReference)
        {
            return UnitOfWork.Session.Query<CardTopUpTransaction>(@"SELECT c.* from CardTopUPTransactions c WHERE c.CustomerReference = @CustomerReference", new { CustomerReference }, UnitOfWork.Transaction).FirstOrDefault();
        }

        public List<CardTopUpTransaction> GetTopUpTransactionsForPeriod(DateTime from, DateTime to)
        {
            return UnitOfWork.Session.Query<CardTopUpTransaction>(@"SELECT c.* from CardTopUPTransactions c WHERE c.UpdateDate BETWEEN @From AND @To AND c.Status = 'Created'", new { from, to }, UnitOfWork.Transaction).ToList();
        }

        public void Insert(CardTopUpTransaction entity)
        {
            using (var transaction = BeginTransaction())
            {
                entity.CreateDate = DateTime.UtcNow;
                entity.Id = UnitOfWork.Session.QuerySingleOrDefault<int>(@"
                INSERT INTO CardTopUPTransactions ( ClientId, ContractId, Amount, CustomerReference, Url, CreateDate, UpdateDate, Status, OrderId)
                VALUES ( @ClientId, @ContractId, @Amount, @CustomerReference, @Url, @CreateDate, @UpdateDate, @Status, @OrderId)
                SELECT SCOPE_IDENTITY()",
                    entity, UnitOfWork.Transaction);
                transaction.Commit();
            }
        }

        public List<CardTopUpTransaction> List(ListQuery listQuery, object query = null)
        {
            throw new System.NotImplementedException();
        }

        public void Update(CardTopUpTransaction entity)
        {
            UnitOfWork.Session.Execute(@"
            UPDATE CardTopUPTransactions
               SET ClientId = @ClientId, ContractId = @ContractId, Amount = @Amount, CustomerReference = @CustomerReference, 
                   Url = @Url, UpdateDate = @UpdateDate, Status = @Status, OrderId = @OrderId
             WHERE Id = @Id",
                entity, UnitOfWork.Transaction);
        }
    }
}
