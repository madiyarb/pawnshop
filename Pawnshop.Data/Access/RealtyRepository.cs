using Dapper;
using Pawnshop.AccountingCore.Models;
using Pawnshop.Core;
using Pawnshop.Core.Impl;
using Pawnshop.Core.Queries;
using Pawnshop.Data.Models.Dictionaries;
using Pawnshop.Data.Models.Domains;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pawnshop.Data.Access
{
    public class RealtyRepository : RepositoryBase, IRepository<Realty>
    {
        public RealtyRepository(IUnitOfWork unitOfWork) : base(unitOfWork)
        {
        }

        public void Delete(int id)
        {
            using (var transaction = BeginTransaction())
            {
                UnitOfWork.Session.Execute("DELETE FROM Realties WHERE Id = @id", new { id = id }, UnitOfWork.Transaction);
                UnitOfWork.Session.Execute("DELETE FROM Positions WHERE Id = @id", new { id = id }, UnitOfWork.Transaction);
                transaction.Commit();
            }
        }

        public Realty Find(object query)
        {
            throw new NotImplementedException();
        }

        public Realty Get(int id)
        {
            return UnitOfWork.Session.Query<Realty, RealtyAddress, Realty>(@"SELECT r.*, p.*, ra.*
  FROM Realties r
  JOIN Positions p ON p.Id = r.Id
  LEFT JOIN RealtyAddress ra ON ra.Id = r.Id
 WHERE r.Id = @id",
                (r, ra) =>
                {
                    if (r != null)
                        r.Address = ra;
                    return r;
                },
                new { id }, UnitOfWork.Transaction).FirstOrDefault();
        }

        public void Insert(Realty entity)
        {
            using (var transaction = BeginTransaction())
            {
                entity.Id = UnitOfWork.Session.QuerySingleOrDefault<int>(@"
                    INSERT INTO Positions ( Name, CollateralType, ClientId ) VALUES ( @Name, @CollateralType, @ClientId )
                    SELECT SCOPE_IDENTITY()",
                    new
                    {
                        Name = entity.Rca,
                        CollateralType = CollateralType.Realty,
                        ClientId = entity.ClientId
                    }, UnitOfWork.Transaction);

                UnitOfWork.Session.Execute(@"
                    INSERT INTO Realties ( Id, RealtyTypeId, CadastralNumber, Rca, Year, TotalArea, LivingArea, StoreysNumber, WallMaterial, RoomsNumber, LocationStoreysNumber,LandArea, LandAreaRatio, PurposeId, LightingId, ColdWaterSupplyId, GasSupplyId, SanitationId, HotWaterSupplyId, HeatingId, PhoneConnectionId, CadastralNumberAdditional )
                    VALUES ( @Id, @RealtyTypeId, @CadastralNumber, @Rca, @Year, @TotalArea, @LivingArea, @StoreysNumber, @WallMaterial, @RoomsNumber, @LocationStoreysNumber, @LandArea, @LandAreaRatio, @PurposeId, @LightingId, @ColdWaterSupplyId, @GasSupplyId, @SanitationId, @HotWaterSupplyId, @HeatingId, @PhoneConnectionId, @CadastralNumberAdditional )",
                    entity, UnitOfWork.Transaction);

                transaction.Commit();
            }
        }

        public void Update(Realty entity)
        {
            using (var transaction = BeginTransaction())
            {
                UnitOfWork.Session.Execute(@"UPDATE Positions SET Name = @Name, ClientId = @ClientId WHERE Id = @Id",
                    new
                    {
                        Id = entity.Id,
                        Name = entity.Rca,
                        ClientId = entity.ClientId
                    }, UnitOfWork.Transaction);

                UnitOfWork.Session.Execute(@"
                    UPDATE Realties SET RealtyTypeId = @RealtyTypeId, CadastralNumber = @CadastralNumber, Year = @Year, TotalArea = @TotalArea, LivingArea = @LivingArea, StoreysNumber = @StoreysNumber, WallMaterial = @WallMaterial, RoomsNumber = @RoomsNumber, LocationStoreysNumber = @LocationStoreysNumber, LandArea = @LandArea, LandAreaRatio = @LandAreaRatio, PurposeId = @PurposeId, LightingId = @LightingId, ColdWaterSupplyId = @ColdWaterSupplyId, GasSupplyId = @GasSupplyId, SanitationId = @SanitationId, HotWaterSupplyId = @HotWaterSupplyId, HeatingId = @HeatingId, PhoneConnectionId = @PhoneConnectionId, CadastralNumberAdditional = @CadastralNumberAdditional
                    WHERE Id = @Id",
                    entity, UnitOfWork.Transaction);

                transaction.Commit();
            }
        }

        public List<Realty> List(ListQuery listQuery, object query = null)
        {
            if (listQuery == null)
                throw new ArgumentNullException(nameof(listQuery));

            var condition = listQuery.Like(string.Empty, "r.Rca");
            var order = listQuery.Order(string.Empty, new Sort
            {
                Name = "r.Rca",
                Direction = SortDirection.Asc
            });
            var page = listQuery.Page();

            return UnitOfWork.Session.Query<Realty, DomainValue, Realty>($@"
                SELECT r.*,p.*, dv.*
                FROM Realties r
                JOIN Positions p ON p.Id = r.Id
                JOIN DomainValues dv ON dv.Id = r.RealtyTypeId
                {condition} {order} {page}",
                (r, dv) =>
                {
                    r.RealtyType = dv;
                    return r;
                },
                new
                {
                    listQuery.Page?.Offset,
                    listQuery.Page?.Limit,
                    listQuery.Filter
                }, UnitOfWork.Transaction).ToList();
        }

        public int Count(ListQuery listQuery, object query = null)
        {
            if (listQuery == null)
                throw new ArgumentNullException(nameof(listQuery));

            var condition = listQuery.Like(string.Empty, "r.Rca");

            return UnitOfWork.Session.ExecuteScalar<int>($@"
                SELECT COUNT(*)
                FROM Realties r
                JOIN Positions p ON p.Id = r.Id
                {condition}",
                new
                {
                    listQuery.Filter
                }, UnitOfWork.Transaction);
        }

        public int RelationCount(int positionId)
        {
            return UnitOfWork.Session.ExecuteScalar<int>(@"
                SELECT COUNT(*)
                FROM ContractPositions
                WHERE PositionId = @positionId", new { positionId = positionId }, UnitOfWork.Transaction);
        }

        public async Task<Realty> GetByContractIdAsync(int contractId)
        {
            return await UnitOfWork.Session.QueryFirstOrDefaultAsync<Realty>(@"SELECT r.*, cp.ContractId, p.CollateralType
  FROM Realties r
  JOIN Positions p ON p.Id = r.Id
  JOIN ContractPositions cp ON cp.PositionId = r.Id
 WHERE cp.ContractId = @contractId",
                new { contractId }, UnitOfWork.Transaction);
        }
    }
}
