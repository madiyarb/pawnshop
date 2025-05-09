using Dapper;
using Pawnshop.Core;
using Pawnshop.Core.Impl;
using Pawnshop.Data.Models.CardCashOutTransaction;
using System;
using System.Threading.Tasks;

namespace Pawnshop.Data.Access
{
    public class CardCashOutJetPayTransactionRepository : RepositoryBase
    {
        public CardCashOutJetPayTransactionRepository(IUnitOfWork unitOfWork) : base(unitOfWork) { }

        public async Task DeleteAsync(int id)
        {
            using (var transaction = BeginTransaction())
            {
                await UnitOfWork.Session.ExecuteAsync(@"UPDATE CardCashOutJetPayTransactions
   SET DeleteDate = @deleteDate
 WHERE Id = @id",
                    new { id, deleteDate = DateTime.Now }, UnitOfWork.Transaction);

                transaction.Commit();
            }
        }

        public async Task<CardCashOutJetPayTransaction> GetAsync(int Id)
        {
            return await UnitOfWork.Session.QueryFirstOrDefaultAsync<CardCashOutJetPayTransaction>(@"SELECT *
  FROM CardCashOutJetPayTransactions
 WHERE Id = @Id",
                new { Id }, UnitOfWork.Transaction);
        }

        public async Task<CardCashOutJetPayTransaction> GetByPaymentIdAsync(Guid paymentId)
        {
            return await UnitOfWork.Session.QueryFirstOrDefaultAsync<CardCashOutJetPayTransaction>(@"SELECT *
  FROM CardCashOutJetPayTransactions
 WHERE PaymentId = @paymentId",
    new { paymentId }, UnitOfWork.Transaction);
        }

        public async Task<CardCashOutJetPayTransaction> GetLastByContractIdAsync(int contractId)
        {
            return await UnitOfWork.Session.QueryFirstOrDefaultAsync<CardCashOutJetPayTransaction>(@"SELECT TOP 1 *
  FROM CardCashOutJetPayTransactions
 WHERE ContractId = @contractId
 ORDER BY CreateDate DESC",
                new { contractId }, UnitOfWork.Transaction);
        }

        public async Task InsertAsync(CardCashOutJetPayTransaction entity)
        {
            using (var transaction = BeginTransaction())
            {
                entity.CreateDate = DateTime.Now;
                entity.Id = await UnitOfWork.Session.QuerySingleOrDefaultAsync<int>(@"
INSERT INTO CardCashOutJetPayTransactions ( CreateDate, ClientId, ContractId, JetPayCardPayoutInformationId, Status, PaymentId, Amount )
VALUES ( @CreateDate, @ClientId, @ContractId, @JetPayCardPayoutInformationId, @Status, @PaymentId, @Amount )

SELECT SCOPE_IDENTITY()",
                entity, UnitOfWork.Transaction);

                transaction.Commit();
            }
        }

        public async Task UpdateAsync(CardCashOutJetPayTransaction entity)
        {
            using (var transaction = BeginTransaction())
            {
                entity.UpdateDate = DateTime.Now;
                await UnitOfWork.Session.ExecuteAsync(@"UPDATE CardCashOutJetPayTransactions
   SET UpdateDate = @UpdateDate,
       Message = @Message,
       Status = @Status
 WHERE Id = @Id",
                    entity, UnitOfWork.Transaction);

                transaction.Commit();
            }
        }
    }
}
