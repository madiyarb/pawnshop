using Dapper;
using Pawnshop.Core.Impl;
using Pawnshop.Data.Models.Auction.Mapping;
using Pawnshop.Data.Access.Auction.Mapping.Interfaces;
using System.Threading.Tasks;
using Pawnshop.Core;

// удалить после успешного маппинга
namespace Pawnshop.Data.Access.Auction.Mapping
{
    public class AuctionMappingRepository : RepositoryBase, IAuctionMappingRepository
    {
        public AuctionMappingRepository(IUnitOfWork unitOfWork) : base(unitOfWork) { }

        public async Task<AuctionCarDetails> GetCarDetailsByContractIdAsync(int contractId)
        {
            var parameters = new { ContractId = contractId };
            var sqlQuery = @"
        SELECT
            cl.FullName,
            cl.IdentityNumber AS IIN,
            cl.Id AS ClientId,
            c.Id AS ContractId,
            c.ContractNumber,
            c.BuyoutDate AT TIME ZONE 'West Asia Standard Time' AS AuctionDate,
            c.BuyoutDate AT TIME ZONE 'West Asia Standard Time' AS AuctionContractDate,
            c.ContractNumber AS AuctionContractNumber,
            g.DisplayName AS Branch,
            cr.Id AS CarId,
            cr.Mark,
            cr.Model,
            cr.ReleaseYear,
            cr.Color,
            cr.BodyNumber,
            cr.TransportNumber
        FROM ContractPositions cp
        JOIN Contracts c ON c.Id = cp.ContractId
        JOIN Clients cl ON c.ClientId = cl.Id
        JOIN Groups g ON c.BranchId = g.Id
        JOIN Cars cr ON cp.PositionId = cr.Id
        WHERE cp.ContractId = @ContractId";

            var result = await UnitOfWork.Session
                  .QueryFirstOrDefaultAsync<AuctionCarDetails>(sqlQuery, parameters, UnitOfWork.Transaction);

            return result;
        }

        public async Task<int> GetBranchIdForExpenseByContractIdAsync(int contractId)
        {
            var parameters = new { ContractId = contractId };
            var sqlQuery = @"
        SELECT 
            BranchId
        FROM Contracts c
        JOIN Groups g ON c.BranchId = g.Id
        WHERE c.Id = @ContractId";

            var result = await UnitOfWork.Session
                .QueryFirstOrDefaultAsync<int>(sqlQuery, parameters, UnitOfWork.Transaction);

            return result;
        }
    }
}
