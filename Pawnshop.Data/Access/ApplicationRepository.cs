using System;
using System.Collections.Generic;
using System.Linq;
using Dapper;
using Microsoft.Data.SqlClient;
using Pawnshop.AccountingCore.Models;
using Pawnshop.Core;
using Pawnshop.Core.Exceptions;
using Pawnshop.Core.Impl;
using Pawnshop.Core.Queries;
using Pawnshop.Data.Models.Clients;
using Pawnshop.Data.Models.Dictionaries;
using Pawnshop.Data.Models.MobileApp;

namespace Pawnshop.Data.Access
{
    public class ApplicationRepository : RepositoryBase, IRepository<Application>
    {
        private readonly ClientRepository _clientRepository;
        private readonly CarRepository _carRepository;
        private readonly MachineryRepository _machineryRepository;

        public ApplicationRepository(IUnitOfWork unitOfWork, ClientRepository clientRepository,
            CarRepository carRepository, MachineryRepository machineryRepository) : base(unitOfWork) {
            _clientRepository = clientRepository;
            _carRepository = carRepository;
            _machineryRepository = machineryRepository;
        }

        public void Insert(Application entity)
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));

            using (var transaction = BeginTransaction())
            {
                entity.Id = UnitOfWork.Session.ExecuteScalar<int>(@"
                INSERT INTO Applications (ClientId, PositionId, AppId, ApplicationDate, EstimatedCost, RequestedSum, AuthorId, DebtorsRegisterSum, LightCost, TurboCost, MotorCost, LimitSum, Status, ManagerGuarentee, WithoutDriving, IsAddition, ParentContractId, BitrixId, PrePayment, IsAutocredit, ApplicationMerchantId, ContractClass )
                VALUES(@ClientId, @PositionId, @AppId, @ApplicationDate, @EstimatedCost, @RequestedSum, @AuthorId, @DebtorsRegisterSum, @LightCost, @TurboCost, @MotorCost, @LimitSum, @Status, @ManagerGuarentee, @WithoutDriving, @IsAddition, @ParentContractId, @BitrixId, @PrePayment, @IsAutocredit, @ApplicationMerchantId, @ContractClass )
                SELECT SCOPE_IDENTITY()", entity, UnitOfWork.Transaction);

                transaction.Commit();
            }
        }

        public void Update(Application entity)
        {
            using (var transaction = BeginTransaction())
            {
                UnitOfWork.Session.Execute(@"
                    UPDATE Applications SET Status = @Status
                    WHERE Id = @Id",
                    entity, UnitOfWork.Transaction);

                transaction.Commit();
            }
        }

        public void UpdateByAppId(Application entity)
        {
            using (var transaction = BeginTransaction())
            {
                UnitOfWork.Session.Execute(@"
                    UPDATE Applications
                        SET
                            EstimatedCost = @EstimatedCost,
                            RequestedSum = @RequestedSum,
                            AuthorId = @AuthorId,
                            DebtorsRegisterSum = @DebtorsRegisterSum,
                            LightCost = @LightCost,
                            TurboCost = @TurboCost,
                            MotorCost = @MotorCost,
                            LimitSum = @LimitSum,
                            Status = @Status,
                            ManagerGuarentee = @ManagerGuarentee,
                            WithoutDriving = @WithoutDriving,
                            IsAddition = @IsAddition,
                            ParentContractId = @ParentContractId,
                            BitrixId = @BitrixId,
                            PrePayment = @PrePayment,
                            IsAutocredit = @IsAutocredit,
                            ApplicationMerchantId = @ApplicationMerchantId,
                            ContractClass = @ContractClass 
                        WHERE
                            AppId = @AppId
                        ",
                    entity, UnitOfWork.Transaction);

                transaction.Commit();
            }
        }

        public Application? FindByContractId(int Id)
        {
            try
            {
                return UnitOfWork.Session.Query<Application>($@"
                    SELECT * FROM Applications app
	                    JOIN ApplicationDetails apd on apd.ApplicationId = app.Id
	                    WHERE apd.ContractId = @Id",
                    new { Id }, UnitOfWork.Transaction).FirstOrDefault();
            }
            catch
            {
                return null;
            }
        }

        public void Delete(int id)
        {
            throw new System.NotImplementedException();
        }

        public Application Get(int id)
        {
            var entity = UnitOfWork.Session.Query<Application, Client, Position, Application>(@"
                SELECT app.*, c.*, p.*
                FROM Applications app
                LEFT JOIN Positions p ON p.Id = app.PositionId
                LEFT JOIN Clients c ON c.Id = app.ClientId
                WHERE app.Id = @id",
                (app, c, p) =>
                {
                    app.Client = c;
                    app.Position = p;
                    return app;
                }, new { id }, UnitOfWork.Transaction).FirstOrDefault();

            if (entity == null)
                throw new NullReferenceException($"Заявка МП с Id {id} не найдена");

            entity.Client = _clientRepository.Get(entity.ClientId);

            switch (entity.Position.CollateralType)
            {
                case CollateralType.Car:
                    entity.Position = _carRepository.Get(entity.PositionId);
                    break;
                case CollateralType.Machinery:
                    entity.Position = _machineryRepository.Get(entity.PositionId);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(CollateralType));
            }

            return entity;
        }

        public int? GetAppIdBy(int ContractId)
        {
            return UnitOfWork.Session.Query<int?>($@"
                SELECT app.AppId
                FROM Applications app
                JOIN ApplicationDetails apd ON app.Id = apd.ApplicationId
                JOIN Contracts cnc ON cnc.Id = apd.ContractId
                WHERE cnc.Id = @ContractId",
            new { ContractId }, UnitOfWork.Transaction).FirstOrDefault();
        }

        public Application Find(object query)
        {
            var parentContractId = query?.Val<int?>("ParentContractId");
            var applicationStatuses = query?.Val<List<int>>("ApplicationStatuses");

            var condition = applicationStatuses != null && applicationStatuses.Any() ? " AND Status IN @applicationStatuses" : string.Empty;

            return UnitOfWork.Session.Query<Application>($@"
                SELECT *
                FROM Applications app
                WHERE app.ParentContractId = @parentContractId
                {condition}
                ORDER BY app.Id DESC",
                new { parentContractId, applicationStatuses }, UnitOfWork.Transaction).FirstOrDefault();
        }

        public Application FindByAppId(int AppId)
        {
            return UnitOfWork.Session.Query<Application>($@"
                SELECT *
                FROM Applications app
                WHERE AppId = @AppId
                ORDER BY app.Id DESC",
            new { AppId }, UnitOfWork.Transaction).FirstOrDefault();
        }

        public List<Application> List(ListQuery listQuery, object query = null)
        {
            var authorId = query?.Val<int?>("AuthorId");
            var beginDate = query?.Val<DateTime?>("BeginDate");
            var endDate = query?.Val<DateTime?>("EndDate");
            var status = query?.Val<ApplicationStatus?>("Status");
            var isAddition = query?.Val<bool?>("IsAddition");
            var showAllApplications = query?.Val<bool?>("ShowAllApplications");

            var pre = "EXISTS(SELECT* FROM MemberRelations WHERE RightMemberId = mr.RightMemberId";
            pre += authorId.HasValue ? " AND LeftMemberId = @authorId) " : ") ";
            pre += showAllApplications.HasValue && showAllApplications == true && beginDate.HasValue ? " AND app.ApplicationDate >= @beginDate" : string.Empty;
            pre += showAllApplications.HasValue && showAllApplications == true && endDate.HasValue ? " AND app.ApplicationDate <= @endDate" : string.Empty;
            pre += status.HasValue ? " AND app.Status = @status" : string.Empty;
            pre += isAddition.HasValue ? " AND app.IsAddition = @isAddition" : string.Empty;
            pre += showAllApplications.HasValue && showAllApplications != true ? " AND convert(DATE, app.ApplicationDate) = convert(DATE, dbo.GETASTANADATE())" : string.Empty;

            var condition = listQuery.Like(pre, "c.FullName", "app.AppId", "p.Name");
            var order = listQuery.Order(string.Empty, new Sort
            {
                Name = "app.Id",
                Direction = SortDirection.Asc
            });
            var page = listQuery.Page();

            var applications = UnitOfWork.Session.Query<Application, Client, Position, Application>($@"
                SELECT distinct app.*, c.*, p.*
                FROM Applications app WITH (NOLOCK)
                LEFT JOIN Clients c ON app.ClientId = c.Id
                LEFT JOIN Positions p ON app.PositionId = p.Id
                LEFT JOIN MemberRelations mr ON mr.LeftMemberId = app.AuthorId AND mr.RelationType=10
                {condition}
                {order} {page}",
                (app, c, p) =>
                {
                    app.Client = c;
                    app.Position = p;
                    return app;
                },
                new
                {
                    listQuery.Page?.Offset,
                    listQuery.Page?.Limit,
                    listQuery.Filter,
                    authorId,
                    beginDate,
                    endDate,
                    status,
                    isAddition
                }, UnitOfWork.Transaction).AsList();

            applications.ForEach(x => { 
                if (x.Position != null)
                {
                    switch (x.Position.CollateralType)
                    {
                        case CollateralType.Car:
                            x.Position = _carRepository.Get(x.PositionId);
                            break;
                        case CollateralType.Machinery:
                            x.Position = _machineryRepository.Get(x.PositionId);
                            break;
                        default:
                            throw new ArgumentOutOfRangeException(nameof(CollateralType));
                    }
                }
            });

            return applications;
        }

        public int Count(ListQuery listQuery, object query = null)
        {
            var authorId = query?.Val<int?>("AuthorId");
            var beginDate = query?.Val<DateTime?>("BeginDate");
            var endDate = query?.Val<DateTime?>("EndDate");
            var status = query?.Val<ApplicationStatus?>("Status");
            var isAddition = query?.Val<bool?>("IsAddition");
            var showAllApplications = query?.Val<bool?>("ShowAllApplications");

            var pre = "EXISTS(SELECT* FROM MemberRelations WHERE RightMemberId = mr.RightMemberId";
            pre += authorId.HasValue ? " AND LeftMemberId = @authorId) " : ") ";
            pre += showAllApplications.HasValue && showAllApplications == true && beginDate.HasValue ? " AND app.ApplicationDate >= @beginDate" : string.Empty;
            pre += showAllApplications.HasValue && showAllApplications == true && endDate.HasValue ? " AND app.ApplicationDate <= @endDate" : string.Empty;
            pre += status.HasValue ? " AND app.Status = @status" : string.Empty;
            pre += isAddition.HasValue ? " AND app.IsAddition = @isAddition" : string.Empty;
            pre += showAllApplications.HasValue && showAllApplications != true ? " AND convert(DATE, app.ApplicationDate) = convert(DATE, dbo.GETASTANADATE())" : string.Empty;

            var condition = listQuery.Like(pre, "c.FullName", "app.AppId", "p.Name");

            return UnitOfWork.Session.ExecuteScalar<int>($@"
                SELECT COUNT(DISTINCT app.Id)
                FROM Applications app WITH (NOLOCK)
                LEFT JOIN Clients c ON app.ClientId = c.Id
                LEFT JOIN Positions p ON app.PositionId = p.Id
                LEFT JOIN MemberRelations mr ON mr.LeftMemberId = app.AuthorId AND mr.RelationType=10
                {condition}",
                new
                {
                    listQuery.Filter,
                    authorId,
                    beginDate,
                    endDate,
                    status,
                    isAddition
                }, UnitOfWork.Transaction);
        }

        public SameContractList GetSameContracts(MobileAppModel contract)
        {
            if (contract == null)
                throw new ArgumentNullException(nameof(contract));

            var list = new SameContractList();
            List<int> releaseYears = new List<int>();

            foreach (var property in list.GetType().GetProperties())
            {
                var condition = "WHERE c.DeleteDate IS NULL";
                switch (property.Name)
                {
                    case "Models":

                        condition += " AND vmo.Code = @model AND vma.Code = @mark";
                        if (contract.ReleaseYear > 0)
                        {
                            releaseYears.AddRange(new List<int> { contract.ReleaseYear, contract.ReleaseYear + 1, contract.ReleaseYear - 1 });
                            condition += " AND car.ReleaseYear IN @releaseYears";
                        }
                        break;
                    case "Clients":
                        condition += " AND cl.IdentityNumber = @identityNumber";
                        break;
                    case "Vehicles":
                        condition += " AND car.BodyNumber = @bodyNumber";
                        break;
                }

                condition += " AND c.ContractDate >= DATEADD(MONTH, -1, CAST(dbo.GETASTANADATE() AS DATE))";

                property.SetValue(list,
                    UnitOfWork.Session.Query<MobileAppModel>($@"
                SELECT 
                    c.ContractNumber, 
                    c.LoanCost, 
                    vma.Name as Mark, 
                    vmo.Name as Model, 
                    car.ReleaseYear, 
                    car.BodyNumber, 
                    cl.IdentityNumber 
                FROM Contracts c
                    JOIN ContractPositions cp ON cp.ContractId = c.Id
                    JOIN Cars car ON car.Id = cp.PositionId
                    JOIN VehicleMarks vma ON vma.Id = car.VehicleMarkId
                    JOIN VehicleModels vmo ON vmo.Id = car.VehicleModelId
                    JOIN Clients cl ON cl.Id = c.ClientId
                    {condition}",
                    new
                    {
                        model = contract.Model,
                        mark = contract.Mark,
                        releaseYears,
                        identityNumber = contract.IdentityNumber,
                        bodyNumber = contract.BodyNumber
                    }, UnitOfWork.Transaction, commandTimeout: 90).ToList());
            }

            return list;
        }

        public List<int> GetBranchCodeByUserId(int userId)
        {
            return UnitOfWork.Session.Query<int>($@"
                SELECT mr.RightMemberId
                FROM MemberRelations mr
                WHERE mr.RelationType=10
                AND mr.LeftMemberId = @userId",
                new
                {
                    userId
                }, UnitOfWork.Transaction).AsList();
        }

        public List<Application> GetApplicationsForReject(object query)
        {
            var applicationStatuses = query?.Val<List<int>>("ApplicationStatuses");

            var condition = "WHERE convert(DATE, app.ApplicationDate) = convert(DATE, dbo.GETASTANADATE())";
            condition += applicationStatuses != null && applicationStatuses.Any() ? " AND app.Status IN @applicationStatuses" : string.Empty;

            return UnitOfWork.Session.Query<Application>($@"
                SELECT app.*
                FROM Applications app WITH (NOLOCK)
                {condition}",
                new
                {
                    applicationStatuses
                }, UnitOfWork.Transaction).AsList();
        }

        public int GetBitrixId(int contractId)
        {
            try
            {
                var result = UnitOfWork.Session.Query<int>($@"
    SELECT BitrixId FROM ApplicationDetails ad
    LEFT JOIN Applications a on a.Id = ad.ApplicationId
    WHERE ad.ContractId = @contractId", new { contractId }).FirstOrDefault();

                return result == 0 ? -999 : result;
            }
            catch
            {
                return -999;
            }
        }
    }
}