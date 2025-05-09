using System;
using System.Collections.Generic;
using System.Linq;
using Dapper;
using Pawnshop.AccountingCore.Models;
using Pawnshop.Core;
using Pawnshop.Core.Impl;
using Pawnshop.Core.Queries;
using Pawnshop.Data.Models.Contracts;
using Pawnshop.Data.Models.Dictionaries;

namespace Pawnshop.Data.Access
{
    public class MachineryRepository : RepositoryBase, IRepository<Machinery>
    {
        private readonly VehicleLiquidityRepository _vehicleLiquidityRepository;
        private readonly ContractPeriodVehicleLiquidityRepository _contractPeriodVehicleLiquidityRepository;

        public MachineryRepository(IUnitOfWork unitOfWork, VehicleLiquidityRepository vehicleLiquidityRepository, ContractPeriodVehicleLiquidityRepository contractPeriodVehicleLiquidityRepository) : base(unitOfWork)
        {
            _vehicleLiquidityRepository = vehicleLiquidityRepository;
            _contractPeriodVehicleLiquidityRepository = contractPeriodVehicleLiquidityRepository;
        }

        public void Insert(Machinery entity)
        {
            using (var transaction = BeginTransaction())
            {
                entity.Id = UnitOfWork.Session.QuerySingleOrDefault<int>(@"
INSERT INTO Positions ( Name, CollateralType, ClientId ) VALUES ( @Name, @CollateralType, @ClientId )
SELECT SCOPE_IDENTITY()",
                    new
                    {
                        Name = entity.TransportNumber,
                        CollateralType = CollateralType.Machinery
                    }, UnitOfWork.Transaction);

                UnitOfWork.Session.Execute(@"
INSERT INTO Machineries ( Id, Mark, Model, ReleaseYear, TransportNumber, MotorNumber, BodyNumber, TechPassportNumber, TechPassportDate, Color, VehicleMarkId, VehicleModelId )
VALUES ( @Id, @Mark, @Model, @ReleaseYear, @TransportNumber, @MotorNumber, @BodyNumber, @TechPassportNumber, @TechPassportDate, @Color, @VehicleMarkId, @VehicleModelId )",
                   entity, UnitOfWork.Transaction);

                transaction.Commit();
            }
        }

        public void Update(Machinery entity)
        {
            using (var transaction = BeginTransaction())
            {
                UnitOfWork.Session.Execute(@"UPDATE Positions SET Name = @Name, ClientId = @ClientId WHERE Id = @Id",
                    new
                    {
                        Id = entity.Id,
                        Name = entity.TransportNumber,
                    }, UnitOfWork.Transaction);

                UnitOfWork.Session.Execute(@"
UPDATE Machineries SET Mark = @Mark, Model = @Model, ReleaseYear = @ReleaseYear, TransportNumber = @TransportNumber,
MotorNumber = @MotorNumber, BodyNumber = @BodyNumber, TechPassportNumber = @TechPassportNumber, Color = @Color,
TechPassportDate = @TechPassportDate, ParkingStatusId=@ParkingStatusId, VehicleMarkId = @VehicleMarkId, VehicleModelId = @VehicleModelId
WHERE Id = @Id", entity, UnitOfWork.Transaction);

                transaction.Commit();
            }
        }

        public void Delete(int id)
        {
            using (var transaction = BeginTransaction())
            {
                UnitOfWork.Session.Execute("DELETE FROM Machineries WHERE Id = @id", new { id }, UnitOfWork.Transaction);
                UnitOfWork.Session.Execute("DELETE FROM Positions WHERE Id = @id", new { id }, UnitOfWork.Transaction);
                transaction.Commit();
            }
        }

        public Machinery Get(int id)
        {
            var entity = UnitOfWork.Session.Query<Machinery, VehicleMark, VehicleModel, Machinery>(@"
                SELECT ma.*, p.*, m.*, vm.*
                FROM Machineries ma
                JOIN Positions p ON p.Id = ma.Id
                LEFT JOIN VehicleMarks m ON ma.VehicleMarkId = m.Id
                LEFT JOIN VehicleModels vm ON ma.VehicleModelId = vm.Id
                WHERE ma.Id = @id",
                (ma, m, mod) =>
                {
                    ma.VehicleMark = m;
                    ma.VehicleModel = mod;
                    return ma;
                }, 
                new { id }
            ).FirstOrDefault();

            return entity;
        }

        public Machinery Find(object query)
        {
            throw new NotImplementedException();
        }

        public List<Machinery> List(ListQuery listQuery, object query = null)
        {
            if (listQuery == null) throw new ArgumentNullException(nameof(listQuery));

            var condition = listQuery.Like(string.Empty, "p.Name", "ma.Mark", "ma.Model");
            var order = listQuery.Order(string.Empty, new Sort
            {
                Name = "p.Name",
                Direction = SortDirection.Asc
            });
            var page = listQuery.Page();

            return UnitOfWork.Session.Query<Machinery, VehicleMark, VehicleModel, Machinery>($@"
SELECT *
FROM Machineries ma
JOIN Positions p ON p.Id = ma.Id
LEFT JOIN VehicleMarks m ON ma.VehicleMarkId = m.Id
LEFT JOIN VehicleModels vm ON ma.VehicleModelId = vm.Id
{condition} {order} {page}",
(ma, m, mod) =>
{
    ma.VehicleMark = m;
    ma.VehicleModel = mod;
    return ma;
}, new
        {
            listQuery.Page?.Offset,
            listQuery.Page?.Limit,
            listQuery.Filter
        }).ToList();
        }

        public int Count(ListQuery listQuery, object query = null)
        {
            if (listQuery == null) throw new ArgumentNullException(nameof(listQuery));

            var condition = listQuery.Like(string.Empty, "Name", "Mark", "Model");

            return UnitOfWork.Session.ExecuteScalar<int>($@"
SELECT COUNT(*)
FROM Machineries
JOIN Positions ON Positions.Id = Machineries.Id
{condition}", new
            {
                listQuery.Filter
            });
        }

        public int RelationCount(int positionId)
        {
            return UnitOfWork.Session.ExecuteScalar<int>(@"
SELECT COUNT(*)
FROM ContractPositions
WHERE PositionId = @positionId", new { positionId = positionId });
        }

        public List<string> Colors()
        {
            return UnitOfWork.Session.Query<string>(@"
SELECT Color
FROM Machineries
GROUP BY Color").ToList();
        }
    }
}
