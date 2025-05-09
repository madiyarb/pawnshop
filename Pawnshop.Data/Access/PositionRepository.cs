using System;
using System.Collections.Generic;
using Pawnshop.Core;
using Pawnshop.Core.Impl;
using Pawnshop.Core.Queries;
using Pawnshop.Data.Models.Dictionaries;
using Dapper;
using System.Linq;
using Pawnshop.AccountingCore.Models;
using Pawnshop.Data.Models.Contracts;
using Pawnshop.Data.Models.Positions;
using Pawnshop.Data.Models.Clients;
using System.Threading.Tasks;

namespace Pawnshop.Data.Access
{
    public class PositionRepository : RepositoryBase, IRepository<Position>
    {
        private readonly ClientRepository _clientRepository;
        public PositionRepository(IUnitOfWork unitOfWork, ClientRepository clientRepository) : base(unitOfWork)
        {
            _clientRepository = clientRepository;
        }

        public void Insert(Position entity)
        {
            throw new NotImplementedException();
        }

        public void Update(Position entity)
        {
            throw new NotImplementedException();
        }

        public void Delete(int id)
        {
            throw new NotImplementedException();
        }

        public Position Get(int id)
        {
            return UnitOfWork.Session.QuerySingleOrDefault<Position>(@"
SELECT *
FROM Positions
WHERE Id = @id", new { id }, UnitOfWork.Transaction);
        }

        public Position Find(object query)
        {
            throw new NotImplementedException();
        }

        public List<Position> List(ListQuery listQuery, object query = null)
        {
            if (listQuery == null) throw new ArgumentNullException(nameof(listQuery));

            var collateralType = query?.Val<CollateralType?>("CollateralType");
            var pre = collateralType.HasValue ? "CollateralType = @collateralType" : string.Empty;

            var condition = listQuery.Like(pre, "name");
            var order = listQuery.Order(string.Empty, new Sort
            {
                Name = "name",
                Direction = SortDirection.Asc
            });
            var page = listQuery.Page();

            return UnitOfWork.Session.Query<Position>($@"
SELECT *
FROM Positions
{condition} {order} {page}", new
            {
                collateralType,
                listQuery.Page?.Offset,
                listQuery.Page?.Limit,
                listQuery.Filter
            }).ToList();
        }

        public int Count(ListQuery listQuery, object query = null)
        {
            if (listQuery == null) throw new ArgumentNullException(nameof(listQuery));

            var collateralType = query?.Val<CollateralType?>("CollateralType");
            var pre = collateralType.HasValue ? "CollateralType = @collateralType" : string.Empty;

            var condition = listQuery.Like(pre, "name");

            return UnitOfWork.Session.ExecuteScalar<int>($@"
SELECT COUNT(*)
FROM Positions
{condition}", new
            {
                collateralType,
                listQuery.Filter
            });
        }

        public async Task<Client> GetActivePositionClient(int positionId)
        {

            var clientId = UnitOfWork.Session.QuerySingleOrDefault<int>(@"
SELECT cl.Id
FROM Positions p
JOIN Clients cl ON cl.Id = p.ClientId
WHERE p.Id = @positionId", new { positionId }, UnitOfWork.Transaction);
            var client = _clientRepository.Get(clientId);
            return client;

        }

        public async Task<(DateTime?, DateTime?)> GetSignAndContractDateForSignedPosition(int positionId)
        {
            var result = (await UnitOfWork.Session.QueryAsync<(DateTime?, DateTime?)>(@"
SELECT TOP 1 c.SignDate, c.ContractDate
FROM Contracts c
JOIN ContractPositions cp ON cp.ContractId = c.Id
JOIN Positions p ON p.Id = cp.PositionId
WHERE cp.PositionId = @positionId
AND c.Status IN (30,40,50,60)
AND c.DeleteDate IS NULL
AND cp.DeleteDate IS NULL
ORDER BY c.ContractDate ASC", new { positionId }, UnitOfWork.Transaction)).FirstOrDefault();

            return result;
        }

        public async Task<int> GetCountForActiveContractsForPosition(int positionId)
        {
            var count = await UnitOfWork.Session.QuerySingleOrDefaultAsync<int>(@"
SELECT COUNT(*)
FROM ContractPositions cp
JOIN Contracts c ON c.Id = cp.ContractId
WHERE cp.PositionId = @positionId
AND c.Status IN (@awaitForMoney,@signed)
AND cp.DeleteDate IS NULL
AND c.DeleteDate IS NULL", new { awaitForMoney = (int)ContractStatus.AwaitForMoneySend, signed = (int)ContractStatus.Signed, positionId }, UnitOfWork.Transaction);
            return count;
        }

        public async Task<IEnumerable<int>> GetActiveContractIdsForPositionAsync(int positionId)
        {
            var contractIds = await UnitOfWork.Session.QueryAsync<int>(@"
SELECT DISTINCT cp.ContractId
FROM ContractPositions cp
JOIN Contracts c ON c.Id = cp.ContractId
WHERE cp.DeleteDate IS NULL
AND c.DeleteDate IS NULL
AND c.Status IN (30,50)
AND cp.PositionId = @positionId",
                new { positionId }, UnitOfWork.Transaction);

            return contractIds;
        }

        public async Task<IEnumerable<Position>> GetByIdentityNumberAsync(string iin)
        {
            return await UnitOfWork.Session.QueryAsync<Position>(@"SELECT DISTINCT p.*
  FROM Positions p
  JOIN ContractPositions cp ON cp.PositionId = p.Id
  JOIN Contracts c ON c.Id = cp.ContractId
  JOIN Clients cl ON cl.Id = c.ClientId
 WHERE cp.DeleteDate IS NULL
   AND c.DeleteDate IS NULL
   AND (c.Status IN (20, 24, 25, 26, 27, 29, 30, 50) OR c.BuyoutDate >= DATEADD(MONTH, -3, GETDATE()))
   AND cl.IdentityNumber = @iin",
                new { iin }, UnitOfWork.Transaction);
        }
    }
}