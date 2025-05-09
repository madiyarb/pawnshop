using System;
using Pawnshop.Data.Models;
using System.Threading.Tasks;

namespace Pawnshop.Data.Access.Interfaces
{
    public interface IAuctionRepository
    {
        public Task<CarAuction> CreateAsync(CarAuction entity);

        Task<int> WithdrawAsync(
            int contractId,
            DateTime transactionDate,
            decimal amount,
            int boId,
            int bosId,
            int creditAccountId,
            string reason,
            int? actionId = null,
            int? debitAccountId = null);
        
        public Task<CarAuction> GetAsync(int id);
        public Task<CarAuction> GetByContractIdAsync(int contractId);
        public CarAuction GetByContractId(int contractId);
        public Task<CarAuction> GetByAuctionIdAsync(int auctionId);
        public Task DeleteAsync(int id);
    }
}