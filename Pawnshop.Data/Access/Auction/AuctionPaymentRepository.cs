using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using Pawnshop.Core;
using Pawnshop.Core.Impl;
using Pawnshop.Data.Access.Auction.Interfaces;
using Pawnshop.Data.Models.Auction;

namespace Pawnshop.Data.Access.Auction
{
    public class AuctionPaymentRepository : RepositoryBase, IAuctionPaymentRepository
    {
        public AuctionPaymentRepository(IUnitOfWork unitOfWork) : base(unitOfWork)
        {
        }
        
        public IDbTransaction BeginTransaction()
        {
            return base.BeginTransaction();
        }

        public async Task InsertAsync(AuctionPayment auctionPayment)
        {
            var parameters = new
            {
                RequestId = auctionPayment.RequestId,
                CashOrderId = auctionPayment.CashOrderId,
                CreateDate = DateTime.Now,
                AuthorId = auctionPayment.AuthorId
            };
    
            var sqlQuery = @"
                INSERT INTO AuctionPaymentRecords (RequestId, CashOrderId, CreateDate, AuthorId)
                OUTPUT INSERTED.Id
                VALUES (@RequestId, @CashOrderId, @CreateDate, @AuthorId)";

            auctionPayment.Id = await UnitOfWork.Session.ExecuteScalarAsync<int>(sqlQuery, parameters, UnitOfWork.Transaction);
        }

        public async Task InsertMultipleAsync(List<AuctionPayment> auctionPaymentRecords)
        {
            if (auctionPaymentRecords == null || auctionPaymentRecords.Count == 0)
            {
                throw new ArgumentException("Список auctionPayments не должен быть пустым или равным null.");
            }
            
            using (var transaction = BeginTransaction())
            {
                try
                {
                    var sqlQuery = @"
                        INSERT INTO AuctionPayments (RequestId, CashOrderId, CreateDate, AuthorId)
                        SELECT @RequestId, @CashOrderId, @CreateDate, @AuthorId
                        WHERE NOT EXISTS (
                            SELECT 1
                            FROM AuctionPayments
                            WHERE RequestId = @RequestId AND CashOrderId = @CashOrderId)";

                    foreach (var payment in auctionPaymentRecords)
                    {
                        var parameters = new DynamicParameters();
                        parameters.Add("@RequestId", payment.RequestId);
                        parameters.Add("@CashOrderId", payment.CashOrderId);
                        parameters.Add("@CreateDate", payment.CreateDate);
                        parameters.Add("@AuthorId", payment.AuthorId);

                        await UnitOfWork.Session.ExecuteAsync(sqlQuery, parameters, UnitOfWork.Transaction);
                    }

                    transaction.Commit();
                }
                catch (Exception ex)
                {
                    transaction.Rollback();
                    throw new Exception("Ошибка при вставке данных в AuctionPayments", ex);
                }
            }
        }

        public async Task<IEnumerable<AuctionPayment>> GetByRequestIdAsync(Guid requestId)
        {
            var parameters = new { RequestId = requestId };
            var sqlQuery = @"
                SELECT *
                    FROM AuctionPayments
                    WHERE DeleteDate IS NULL
                      AND RequestId = @RequestId";

            var result = await UnitOfWork.Session
                .QueryAsync<AuctionPayment>(sqlQuery, parameters, UnitOfWork.Transaction);

            return result?.ToList();
        }

        public async Task<IEnumerable<AuctionPayment>> GetByCashOrderIdAsync(int cashOrderId)
        {
            var parameters = new { CashOrderId = cashOrderId };
            var sqlQuery = @"
                SELECT *
                    FROM AuctionPayments
                    WHERE DeleteDate IS NULL
                      AND CashOrderId = @CashOrderId";
            
            var result = await UnitOfWork.Session
                .QueryAsync<AuctionPayment>(sqlQuery, parameters, UnitOfWork.Transaction);

            return result;
        }
    }
}
