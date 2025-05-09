using Dapper;
using Pawnshop.Core;
using Pawnshop.Core.Exceptions;
using Pawnshop.Core.Impl;
using Pawnshop.Core.Queries;
using Pawnshop.Data.Models.Contracts;
using Pawnshop.Data.Models.Dictionaries;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Pawnshop.Data.Models.Contracts.Actions;

namespace Pawnshop.Data.Access
{
    public class VehicleBlackListRepository : RepositoryBase, IRepository<VehiclesBlackListItem>
    {
        public VehicleBlackListRepository(IUnitOfWork unitOfWork) : base(unitOfWork)
        {
        }

        public void Insert(VehiclesBlackListItem entity)
        {
            using (var transaction = BeginTransaction())
            {
                entity.Id = UnitOfWork.Session.QuerySingleOrDefault<int>(@"
INSERT INTO VehiclesBlackList ( BodyNumber, Number, ReasonId, AuthorId, Note, CreateDate )
VALUES ( @BodyNumber, @Number, @ReasonId, @AuthorId, @Note, @CreateDate )
SELECT SCOPE_IDENTITY()", entity, UnitOfWork.Transaction);

                transaction.Commit();
            }
        }

        public void Update(VehiclesBlackListItem entity)
        {
            using (var transaction = BeginTransaction())
            {
                UnitOfWork.Session.Execute(@"
UPDATE VehiclesBlackList
SET BodyNumber = @BodyNumber, Number = @Number, ReasonId = @ReasonId, AuthorId = @AuthorId, Note = @Note
WHERE Id = @Id", entity, UnitOfWork.Transaction);

                transaction.Commit();
            }
        }

        public void Delete(int id)
        {
            using (var transaction = UnitOfWork.BeginTransaction())
            {
                UnitOfWork.Session.Execute(@"UPDATE VehiclesBlackList SET DeleteDate = dbo.GETASTANADATE() WHERE Id = @id", new { id }, UnitOfWork.Transaction);
                transaction.Commit();
            }
        }

        public VehiclesBlackListItem Get(int id)
        {
            return UnitOfWork.Session.Query<VehiclesBlackListItem, BlackListReason, VehiclesBlackListItem>(@"
SELECT v.*, b.* FROM VehiclesBlackList v 
LEFT JOIN BlackListReasons b ON v.ReasonId=b.Id
WHERE v.Id=@id", (vb, b) =>
            {
                vb.BlackListReason = b;
                return vb;
            }, new { id }, UnitOfWork.Transaction).FirstOrDefault();
        }

        public VehiclesBlackListItem Find(object query)
        {
            if (query == null) throw new ArgumentNullException(nameof(query));

            var BodyNumber = query?.Val<string>("BodyNumber");

            return UnitOfWork.Session.Query<VehiclesBlackListItem, BlackListReason, VehiclesBlackListItem>(@"
                    SELECT v.*, b.* FROM VehiclesBlackList v 
                    LEFT JOIN BlackListReasons b ON v.ReasonId=b.Id
                    WHERE v.DeleteDate IS NULL AND v.BodyNumber = REPLACE(@BodyNumber, '-', '')", (vb, b) =>
                    {
                        vb.BlackListReason = b;
                        return vb;
                    }, new { BodyNumber }, UnitOfWork.Transaction).FirstOrDefault();
        }

        public List<VehiclesBlackListItem> List(ListQuery listQuery, object query = null)
        {
            if (listQuery == null) throw new ArgumentNullException(nameof(listQuery));

            var pre = "v.DeleteDate IS NULL";

            var condition = listQuery.Like(pre, "v.BodyNumber", "v.Number");
            var order = listQuery.Order(string.Empty, new Sort
            {
                Name = "v.BodyNumber",
                Direction = SortDirection.Asc
            });
            var page = listQuery.Page();

            return UnitOfWork.Session.Query<VehiclesBlackListItem, BlackListReason, VehiclesBlackListItem>($@"
                    SELECT v.*, b.* FROM VehiclesBlackList v 
                        LEFT JOIN BlackListReasons b ON v.ReasonId=b.Id
                    {condition} {order} {page}",
                    (vb, b) =>
                    {
                        vb.BlackListReason = b;
                        return vb;
                    },
                    new
                    {
                        listQuery.Page?.Offset,
                        listQuery.Page?.Limit,
                        listQuery.Filter
                    }).ToList();
        }

        public int Count(ListQuery listQuery, object query = null)
        {
            if (listQuery == null) throw new ArgumentNullException(nameof(listQuery));

            var pre = "DeleteDate IS NULL";

            var condition = listQuery.Like(pre, "BodyNumber", "Number");

            return UnitOfWork.Session.ExecuteScalar<int>($@"
SELECT COUNT(*)
FROM VehiclesBlackList
{condition}", new
            {
                listQuery.Filter
            });
        }

        public void SearchVehicleBlackListByBodyNumber(IBodyNumber bodyNumber, int categoryId, ContractActionType actionType)
        {
            VehiclesBlackListItem vehicleBlackList = Find(bodyNumber);

            if (vehicleBlackList != null)
            {
                if(vehicleBlackList.BlackListReason == null)
                    throw new PawnshopApplicationException("Не найдена причина списка черных VIN-кодов");

                var canCreateNewContracts = actionType == ContractActionType.PartialPayment;

                if (!canCreateNewContracts && vehicleBlackList.BlackListReason.AllowNewContracts && categoryId != Constants.WITHOUT_DRIVE_RIGHT_CATEGORY)
                    throw new PawnshopApplicationException("Данная машина находится в списке черных VIN-кодов, разрешена только категория без права вождения");

                if (!canCreateNewContracts && !vehicleBlackList.BlackListReason.AllowNewContracts)
                    throw new PawnshopApplicationException("Данная машина находится в списке черных VIN-кодов, действие запрещено");
            }
        }

        public List<ContractPosition> GetContractPositionsByContractId(int contractId)
        {
            var contractPositions = UnitOfWork.Session.Query<ContractPosition, Car, ContractPosition>(@"
                    SELECT cp.*, car.*
                        FROM Contracts c
                        JOIN ContractPositions cp ON cp.ContractId = c.Id
                        JOIN Cars car ON car.Id = cp.PositionId
                        JOIN VehiclesBlackList vb ON vb.BodyNumber = car.BodyNumber
                            WHERE vb.DeleteDate IS NULL
                                AND c.Id = @contractId", (cp, car) => {
                cp.Position = car;
                return cp;
            }, new { contractId }, UnitOfWork.Transaction).ToList();

            return contractPositions;
        }

        public void CheckContractPositionsInBlackList(int contractId)
        {
            foreach (var contractPosition in GetContractPositionsByContractId(contractId))
            {
                SearchVehicleBlackListByBodyNumber((Car) contractPosition.Position,
                    contractPosition.CategoryId, ContractActionType.Sign);
            }
        }
    }
}

