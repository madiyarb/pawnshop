using Dapper;
using Pawnshop.AccountingCore.Models;
using Pawnshop.Core.Impl;
using Pawnshop.Core.Queries;
using Pawnshop.Core;
using Pawnshop.Data.Models.AbsOnline;
using Pawnshop.Data.Models.Dictionaries;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System;
using Pawnshop.Data.Models.Auction.Dtos.Car;
using Pawnshop.Data.Models.Auction.Dtos.Client;
using Pawnshop.Data.Models.Auction.Dtos.Contract;
using Pawnshop.Data.Models.Contracts;

namespace Pawnshop.Data.Access
{
    public class CarRepository : RepositoryBase, IRepository<Car>
    {
        public CarRepository(IUnitOfWork unitOfWork) : base(unitOfWork)
        {

        }

        public void Insert(Car entity)
        {
            using (var transaction = BeginTransaction())
            {
                entity.Id = UnitOfWork.Session.QuerySingleOrDefault<int>(@"
                    INSERT INTO Positions ( Name, CollateralType, ClientId ) VALUES ( @Name, @CollateralType, @ClientId )
                    SELECT SCOPE_IDENTITY()",
                    new
                    {
                        Name = entity.TransportNumber,
                        CollateralType = CollateralType.Car,
                        ClientId = entity.ClientId
                    }, UnitOfWork.Transaction);

                UnitOfWork.Session.Execute(@"
                    INSERT INTO Cars ( Id, Mark, Model, ReleaseYear, TransportNumber, MotorNumber, BodyNumber, TechPassportNumber, Color, TechPassportDate, VehicleMarkId, VehicleModelId, ParkingStatusId )
                    VALUES ( @Id, @Mark, @Model, @ReleaseYear, @TransportNumber, @MotorNumber, @BodyNumber, @TechPassportNumber, @Color, @TechPassportDate, @VehicleMarkId, @VehicleModelId, @ParkingStatusId )",
                    entity, UnitOfWork.Transaction);

                transaction.Commit();
            }
        }

        public void Update(Car entity)
        {
            using (var transaction = BeginTransaction())
            {
                UnitOfWork.Session.Execute(@"UPDATE Positions SET Name = @Name, ClientId = @ClientId WHERE Id = @Id",
                    new
                    {
                        Id = entity.Id,
                        Name = entity.TransportNumber,
                        ClientId = entity.ClientId
                    }, UnitOfWork.Transaction);

                UnitOfWork.Session.Execute(@"
                    UPDATE Cars SET Mark = @Mark, Model = @Model, ReleaseYear = @ReleaseYear, TransportNumber = @TransportNumber,
                    MotorNumber = @MotorNumber, BodyNumber = @BodyNumber, TechPassportNumber = @TechPassportNumber, Color = @Color,
                    TechPassportDate = @TechPassportDate, ParkingStatusId=@ParkingStatusId, VehicleMarkId = @VehicleMarkId, VehicleModelId = @VehicleModelId
                    WHERE Id = @Id",
                    entity, UnitOfWork.Transaction);

                transaction.Commit();
            }
        }

        public void UpdatePosition(Position entity)
        {
            UnitOfWork.Session.Execute(@"UPDATE Positions SET Name = @Name, ClientId = @ClientId WHERE Id = @Id", entity, UnitOfWork.Transaction);
        }

        public void Delete(int id)
        {
            using (var transaction = BeginTransaction())
            {
                UnitOfWork.Session.Execute("DELETE FROM Cars WHERE Id = @id", new { id = id }, UnitOfWork.Transaction);
                UnitOfWork.Session.Execute("DELETE FROM Positions WHERE Id = @id", new { id = id }, UnitOfWork.Transaction);
                transaction.Commit();
            }
        }

        public Car Get(int id)
        {
            var entity = UnitOfWork.Session.Query<Car, VehicleMark, VehicleModel, Car>(@"
                SELECT c.*, p.*, m.*, vm.*
                FROM Cars c
                JOIN Positions p ON p.Id = c.Id
                LEFT JOIN VehicleMarks m ON c.VehicleMarkId = m.Id
                LEFT JOIN VehicleModels vm ON c.VehicleModelId = vm.Id
                WHERE c.Id = @id",
                (c, m, mod) =>
                {
                    c.VehicleMark = m;
                    c.VehicleModel = mod;
                    return c;
                },
                new { id }, UnitOfWork.Transaction
            ).FirstOrDefault();

            return entity;
        }

        public Car Find(object query)
        {
            if (query == null)
                throw new ArgumentNullException(nameof(query));

            var techPassportNumber = query?.Val<string>("TechPassportNumber");

            var car = UnitOfWork.Session.Query<Car>(@"
                    SELECT * FROM Cars
                    WHERE REPLACE(REPLACE(TechPassportNumber, N'№', ''), ' ', '') = @techPassportNumber",
                new { techPassportNumber }, UnitOfWork.Transaction).FirstOrDefault();

            if (car is null)
                car = UnitOfWork.Session.Query<Car>(@$"
                    SELECT * FROM Cars
                    WHERE TechPassportNumber = @techPassportNumber",
                    new { techPassportNumber }, UnitOfWork.Transaction).FirstOrDefault();

            return car;
        }

        public List<Car> List(ListQuery listQuery, object query = null)
        {
            if (listQuery == null)
                throw new ArgumentNullException(nameof(listQuery));

            var condition = listQuery.Like(string.Empty, "BodyNumber", "p.Name", "Mark", "Model");
            var order = listQuery.Order(string.Empty, new Sort
            {
                Name = "m.Name",
                Direction = SortDirection.Asc
            });
            var page = listQuery.Page();

            return UnitOfWork.Session.Query<Car, VehicleMark, VehicleModel, Car>($@"
                SELECT *
                FROM Cars c
                JOIN Positions p ON p.Id = c.Id
                LEFT JOIN VehicleMarks m ON c.VehicleMarkId = m.Id
                LEFT JOIN VehicleModels vm ON c.VehicleModelId = vm.Id
                {condition} {order} {page}",
                (c, m, mod) =>
                {
                    c.VehicleMark = m;
                    c.VehicleModel = mod;
                    return c;
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

            var condition = listQuery.Like(string.Empty, "BodyNumber", "TransportNumber", "Mark", "Model");

            return UnitOfWork.Session.ExecuteScalar<int>($@"
                SELECT COUNT(*)
                FROM Cars
                JOIN Positions ON Positions.Id = Cars.Id
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

        public List<string> Colors()
        {
            return UnitOfWork.Session.Query<string>(@"
                SELECT Color
                FROM Cars
                GROUP BY Color", UnitOfWork.Transaction).ToList();
        }

        public async Task<Car> GetByContractIdAsync(int contractId)
        {
            return await UnitOfWork.Session.QueryFirstOrDefaultAsync<Car>(@"SELECT cr.*, cp.ContractId, p.CollateralType
  FROM Cars cr
  JOIN Positions p ON p.Id = cr.Id
  JOIN ContractPositions cp ON cp.PositionId = cr.Id
 WHERE cp.ContractId = @contractId",
                new { contractId }, UnitOfWork.Transaction);
        }

        public async Task<IEnumerable<Car>> GetListByContractIdsAsync(List<int> contractIds)
        {
            return await UnitOfWork.Session.QueryAsync<Car>(@"SELECT cr.*, cp.ContractId, p.CollateralType
  FROM Cars cr
  JOIN Positions p ON p.Id = cr.Id
  JOIN ContractPositions cp ON cp.PositionId = cr.Id
 WHERE cp.ContractId IN @contractIds",
                new { contractIds }, UnitOfWork.Transaction);
        }

        public async Task<CarStatusView> GetCarStatus(int contractId)
        {
            return await UnitOfWork.Session.QueryFirstOrDefaultAsync<CarStatusView>(@"SELECT cr.BodyNumber AS Vin,
       cr.TransportNumber AS CarNumber,
       ps.StatusName AS Status,
       ps.StatusCode AS StatusCode,
       (SELECT TOP 1 psh.CreateDate FROM ParkingHistories psh WHERE psh.PositionId = cr.Id ORDER BY psh.CreateDate DESC) AS UpdateDate
  FROM ContractPositions cp
  JOIN Cars cr ON cr.Id = cp.PositionId
  JOIN ParkingStatuses ps ON ps.Id = cr.ParkingStatusId
 WHERE cp.ContractId = @contractId
 ORDER BY cp.Id desc",
                new { contractId }, UnitOfWork.Transaction);
        }

        public async Task<IEnumerable<Car>> ListByClientId(int clientId)
        {
            return await UnitOfWork.Session.QueryAsync<Car>(@"SELECT cr.*
  FROM Cars cr
  JOIN Positions p ON p.Id = cr.Id
  JOIN ContractPositions cp ON cp.PositionId = cr.Id
  JOIN Contracts c ON c.Id = cp.ContractId
 WHERE c.DeleteDate IS NULL
   AND c.Status IN (20, 24, 25, 26, 27, 29, 30, 50)
   AND c.ClientId = @clientId",
                new { clientId }, UnitOfWork.Transaction);
        }

        public async Task<IEnumerable<Contract>> GetShortContractByCarId(int carId)
        {
            return await UnitOfWork.Session.QueryAsync<Contract>(
                @"SELECT Contracts.ClientId, Contracts.Status, Contracts.Id, Contracts.ContractNumber, Contracts.SignDate 
                FROM Cars LEFT JOIN ContractPositions ON ContractPositions.PositionId = Cars.Id
                LEFT JOIN Contracts ON Contracts.Id = ContractPositions.ContractId 
                WHERE Cars.Id = @carId AND Contracts.DeleteDate IS NULL ", new { carId }, UnitOfWork.Transaction);
        }

        /// <summary>
        /// Получить авто по номеру по действующему договору
        /// </summary>
        /// <param name="transportNumber">Номер авто</param>
        /// <returns></returns>
        public async Task<List<AuctionCarDto>> GetByTransportNumberAsync(string transportNumber)
        {
            var parameters = new { TransportNumber = transportNumber };
            var sqlQuery = @"
                SELECT CRS.Id              as CarId,
                       CRS.TransportNumber as TransportNumber,
                       CRS.BodyNumber      as BodyNumber,
                       CRS.Mark            as Mark,
                       CRS.Model           as Model,
                       CRS.Color           as Color,
                       CRS.ReleaseYear     as ReleaseYear,
                       CLN.IdentityNumber  as IdentityNumber,
                       CLN.Id              as ClientId,
                       CLN.Surname         as Surname,
                       CLN.Name            as Name,
                       CLN.Patronymic      as Patronymic,
                       CLN.FullName        as FullName,
                       CLN.LegalFormId     as LegalFormId,
                       CNT.id              as ContractId,
                       CNT.ContractNumber  as ContractNumber,
                       G.DisplayName       as DisplayName
                FROM Cars CRS
                         JOIN ContractPositions cp on cp.PositionId = CRS.Id
                         JOIN Contracts CNT ON CNT.Id = cp.ContractId
                         JOIN Groups G ON CNT.BranchId = G.Id
                         JOIN Clients CLN on CNT.ClientId = CLN.Id
                WHERE CNT.DeleteDate IS NULL
                  AND CNT.Status = 30
                  AND CRS.TransportNumber = @TransportNumber";

            var result = await UnitOfWork.Session.QueryAsync(sqlQuery, parameters, UnitOfWork.Transaction);

            return result.Select(row => new AuctionCarDto
            {
                Id = row.CarId,
                TransportNumber = row.TransportNumber,
                BodyNumber = row.BodyNumber,
                Mark = row.Mark,
                Model = row.Model,
                Color = row.Color,
                ReleaseYear = row.ReleaseYear,
                Client = new AuctionClientDto
                {
                    Id = row.ClientId,
                    IdentityNumber = row.IdentityNumber,
                    Surname = row.Surname,
                    Name = row.Name,
                    Patronymic = row.Patronymic,
                    FullName = row.FullName,
                    LegalFormId = row.LegalFormId
                },
                Contract = new AuctionContractDto
                {
                    Id = row.ContractId,
                    ContractNumber = row.ContractNumber,
                    DisplayName = row.DisplayName
                }
            }).ToList();
        }
    }
}