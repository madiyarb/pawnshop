using Dapper;
using Pawnshop.Core;
using Pawnshop.Core.Impl;
using Pawnshop.Data.Access.Interfaces;
using Pawnshop.Data.Models;
using System;
using System.Data;
using System.Threading.Tasks;

namespace Pawnshop.Data.Access
{
    public class AuctionRepository : RepositoryBase, IAuctionRepository
    {
        public AuctionRepository(IUnitOfWork unitOfWork) : base(unitOfWork)
        {
        }

        public async Task<CarAuction> CreateAsync(CarAuction entity)
        {
            var parameters = new
            {
                ContractId = entity.ContractId,
                AuctionId = entity.AuctionId,
                Cost = entity.Cost,
                OrderRequestId = entity.OrderRequestId,
                WithdrawCost = entity.WithdrawCost
            };

            using (var transaction = BeginTransaction())
            {
                entity.Id = await UnitOfWork.Session.ExecuteScalarAsync<int>(@"
                    INSERT INTO CarAuctions (ContractId, AuctionId, Cost, WithdrawCost, OrderRequestId)
                    VALUES (@ContractId, @AuctionId, @Cost, @WithdrawCost, @OrderRequestId);
                    SELECT SCOPE_IDENTITY();", parameters, UnitOfWork.Transaction);

                transaction.Commit();
            }

            return entity;
        }

        public async Task<int> WithdrawAsync(
            int contractId,
            DateTime transactionDate,
            decimal amount,
            int boId,
            int bosId,
            int creditAccountId,
            string reason,
            int? actionId = null,
            int? debitAccountId = null)
        {
            var parameters = new DynamicParameters();
            parameters.Add("contractId", contractId);
            parameters.Add("transactionDate", transactionDate);
            parameters.Add("transactionAmount", amount);
            parameters.Add("boId", boId);
            parameters.Add("bosId", bosId);
            parameters.Add("debitAccountId", debitAccountId);
            parameters.Add("creditAccountId", creditAccountId);
            parameters.Add("reason", reason, DbType.String, size: 500);
            parameters.Add("specialBranchId", null, DbType.Int32);
            parameters.Add("actionId", actionId, DbType.Int32);
            
            parameters.Add("orderId", dbType: DbType.Int32, direction: ParameterDirection.Output);
            
            var orderId = await UnitOfWork.Session.ExecuteAsync("dbo.sp_UTLCreateContractTransaction",
                parameters,
                UnitOfWork.Transaction,
                commandType: CommandType.StoredProcedure
            );
            
            return orderId;
        }

        public async Task<CarAuction> GetAsync(int id)
        {
            var parameters = new { Id = id };
            var sqlQuery = @"
                SELECT TOP 1 * FROM CarAuctions
                WHERE Id = @Id
                  AND DeleteDate IS NULL";

            var result = await UnitOfWork.Session
                .QueryFirstOrDefaultAsync<CarAuction>(sqlQuery, parameters, UnitOfWork.Transaction);

            return result;
        }

        public async Task<CarAuction> GetByContractIdAsync(int contractId)
        {
            var parameters = new { ContractId = contractId };
            var sqlQuery = @"
                SELECT TOP 1 * FROM CarAuctions
                WHERE ContractId = @ContractId
                  AND DeleteDate IS NULL";

            var result = await UnitOfWork.Session
                .QuerySingleOrDefaultAsync<CarAuction>(sqlQuery, parameters, UnitOfWork.Transaction);

            return result;
        }

        public CarAuction GetByContractId(int contractId)
        {
            var parameters = new { ContractId = contractId };
            var sqlQuery = @"
                SELECT TOP 1 * FROM CarAuctions
                WHERE ContractId = @ContractId
                  AND DeleteDate IS NULL";

            var result = UnitOfWork.Session.QueryFirstOrDefault<CarAuction>(sqlQuery, parameters, UnitOfWork.Transaction);

            return result;
        }

        public async Task<CarAuction> GetByAuctionIdAsync(int auctionId)
        {
            var parameters = new { AuctionId = auctionId };
            var sqlQuery = @"
                SELECT TOP 1 * FROM CarAuctions
                WHERE AuctionId = @AuctionId
                  AND DeleteDate IS NULL";

            var result = await UnitOfWork.Session
                .QueryFirstOrDefaultAsync<CarAuction>(sqlQuery, parameters, UnitOfWork.Transaction);

            return result;
        }

        public async Task DeleteAsync(int id)
        {
            var parameters = new { Id = id, DeleteDate = DateTimeOffset.Now };
            var sqlUpdate = @"
                UPDATE CarAuctions
                SET DeleteDate = @DeleteDate
                WHERE Id = @Id";

            await UnitOfWork.Session.ExecuteAsync(sqlUpdate, parameters, UnitOfWork.Transaction);
        }
    }
}