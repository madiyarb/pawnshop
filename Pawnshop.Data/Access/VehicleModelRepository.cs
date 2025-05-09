using Dapper;
using Microsoft.Data.SqlClient;
using Pawnshop.Core.Exceptions;
using Pawnshop.Core.Impl;
using Pawnshop.Core.Queries;
using Pawnshop.Core;
using Pawnshop.Data.Models.Dictionaries;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System;

namespace Pawnshop.Data.Access
{
    public class VehicleModelRepository : RepositoryBase, IRepository<VehicleModel>
    {
        private readonly VehicleLiquidityRepository _vehicleLiquidityRepository;

        public VehicleModelRepository(IUnitOfWork unitOfWork,
                                      VehicleLiquidityRepository vehicleLiquidityRepository) : base(unitOfWork)
        {
            _vehicleLiquidityRepository = vehicleLiquidityRepository;
        }

        private void UpdateVehicleLiquid(VehicleLiquidity vehicleLiquidity)
        {
            try
            {
                _vehicleLiquidityRepository.Update(vehicleLiquidity);
            }
            catch (SqlException e)
            {
                throw new PawnshopApplicationException(e.Message);
            }
        }

        private void InsertVehicleLiquid(VehicleLiquidity vehicleLiquidity)
        {
            try
            {
                _vehicleLiquidityRepository.Insert(vehicleLiquidity);
            }
            catch (SqlException e)
            {
                throw new PawnshopApplicationException(e.Message);
            }
        }

        public void Insert(VehicleModel entity)
        {
            using (var transaction = BeginTransaction())
            {
                entity.Id = UnitOfWork.Session.QuerySingleOrDefault<int>(@"
                    INSERT INTO VehicleModels ( Name, VehicleMarkId, Code, IsDisabled )
                    VALUES ( @Name, @VehicleMarkId, @Code, @IsDisabled )
                    SELECT SCOPE_IDENTITY()", entity, UnitOfWork.Transaction);

                entity.VehicleLiquidities?.ForEach(x =>
                {
                    x.VehicleModelId = entity.Id;
                    x.VehicleMarkId = entity.VehicleMarkId;

                    InsertVehicleLiquid(x);
                });

                transaction.Commit();
            }
        }

        public void Update(VehicleModel entity)
        {
            using (var transaction = BeginTransaction())
            {
                Get(entity.Id);

                UnitOfWork.Session.Execute(@"
                    UPDATE VehicleModels
                    SET Name = @Name, VehicleMarkId = @VehicleMarkId, Code = @Code, IsDisabled = @IsDisabled
                    WHERE Id = @Id", entity, UnitOfWork.Transaction);

                entity.VehicleLiquidities?.ForEach(x =>
                {
                    if (x.Id > 0)
                    {
                        UpdateVehicleLiquid(x);
                    }
                    else
                    {
                        x.VehicleModelId = entity.Id;
                        x.VehicleMarkId = entity.VehicleMarkId;

                        InsertVehicleLiquid(x);
                    }
                });

                transaction.Commit();
            }
        }

        public void Delete(int id)
        {
            using (var transaction = UnitOfWork.BeginTransaction())
            {
                UnitOfWork.Session.Execute(@"UPDATE VehicleModels SET DeleteDate = dbo.GETASTANADATE() WHERE Id = @id", new { id }, UnitOfWork.Transaction);
                _vehicleLiquidityRepository.DeleteByModelId(id);
                transaction.Commit();
            }
        }

        public VehicleModel Get(int id)
        {
            var entity = UnitOfWork.Session.Query<VehicleModel, VehicleMark, VehicleModel>(@"
                SELECT vm.*, m.* 
                FROM VehicleModels vm
                LEFT JOIN VehicleMarks m ON m.Id = vm.VehicleMarkId
                WHERE vm.Id=@id",
            (vm, m) =>
            {
                vm.VehicleMark = m;
                vm.VehicleLiquidities = _vehicleLiquidityRepository.GetVehicleLiquidityByModelId(vm.Id);
                return vm;
            }, new { id }, UnitOfWork.Transaction).FirstOrDefault();

            if (entity is null)
                throw new PawnshopApplicationException($"Не найдена модель авто с Id {id}");

            return entity;
        }

        public VehicleModel Find(object query)
        {
            if (query == null)
                throw new ArgumentNullException(nameof(query));

            var code = query?.Val<string>("Code");

            var modelId = UnitOfWork.Session.ExecuteScalar<int?>($@"
            SELECT TOP 1 id FROM VehicleModels WHERE DeleteDate IS NULL AND Code = @code", new { code });

            if (!modelId.HasValue)
                return null;

            return Get(modelId.Value);
        }

        public List<VehicleModel> List(ListQuery listQuery, object query = null)
        {
            if (listQuery == null) throw new ArgumentNullException(nameof(listQuery));

            var pre = "v.DeleteDate IS NULL";

            var vehicleMarkId = query?.Val<int?>("VehicleMarkId");
            var vehicleModelId = query?.Val<int?>("VehicleModelId");
            var liquidDefault = query?.Val<int?>("LiquidDefault");
            var liquidByAdditionCondition = query?.Val<int?>("LiquidByAdditionCondition");

            if (vehicleMarkId.HasValue) pre += " AND v.VehicleMarkId = @vehicleMarkId";
            if (vehicleModelId.HasValue) pre += " AND v.Id = @vehicleModelId";

            var condition = listQuery.Like(pre, "v.Name", "m.Name", "m.Code");
            var order = listQuery.Order(string.Empty, new Sort
            {
                Name = "v.Name",
                Direction = SortDirection.Asc
            });
            var page = listQuery.Page();

            if (liquidDefault.HasValue || liquidByAdditionCondition.HasValue)
            {
                var vehicleLiquidities = _vehicleLiquidityRepository.List(listQuery, new { VehicleMarkId = vehicleMarkId, VehicleModelId = vehicleModelId, LiquidDefault = liquidDefault, LiquidByAdditionCondition = liquidByAdditionCondition });

                return vehicleLiquidities.Select(x => new VehicleModel()
                {
                    Id = x.VehicleModel.Id,
                    Name = x.VehicleModel.Name,
                    Code = x.VehicleModel.Code,
                    VehicleMarkId = x.VehicleMarkId,
                    VehicleMark = x.VehicleMark,
                    IsDisabled = x.VehicleModel.IsDisabled,
                    VehicleLiquidities = vehicleLiquidities.Where(z => z.VehicleMarkId == x.VehicleMarkId && z.VehicleModelId == x.VehicleModelId).ToList()

                }).ToList();
            }
            else
            {
                return UnitOfWork.Session.Query<VehicleModel, VehicleMark, VehicleModel>($@"
                    SELECT v.*, m.*
                        FROM VehicleModels v
                        LEFT JOIN VehicleMarks m ON v.VehicleMarkId=m.Id
                    {condition} {order} {page}",
                    (vm, m) =>
                    {
                        vm.VehicleMark = m;
                        vm.VehicleLiquidities = _vehicleLiquidityRepository.GetVehicleLiquidityByModelId(vm.Id);
                        return vm;
                    },
                    new
                    {
                        vehicleMarkId,
                        vehicleModelId,
                        listQuery.Page?.Offset,
                        listQuery.Page?.Limit,
                        listQuery.Filter
                    }).ToList();
            }
        }

        public int Count(ListQuery listQuery, object query = null)
        {
            if (listQuery == null) throw new ArgumentNullException(nameof(listQuery));

            var pre = "v.DeleteDate IS NULL";

            var vehicleMarkId = query?.Val<int?>("VehicleMarkId");
            var vehicleModelId = query?.Val<int?>("VehicleModelId");
            var liquidDefault = query?.Val<int?>("LiquidDefault");
            var liquidByAdditionCondition = query?.Val<int?>("LiquidByAdditionCondition");

            if (vehicleMarkId.HasValue) pre += " AND v.VehicleMarkId = @vehicleMarkId";
            if (vehicleModelId.HasValue) pre += " AND v.Id = @vehicleModelId";

            var condition = listQuery.Like(pre, "v.Name", "m.Name", "m.Code");

            if (liquidDefault.HasValue || liquidByAdditionCondition.HasValue)
            {
                var vehicleLiquidities = _vehicleLiquidityRepository.List(listQuery, new { VehicleMarkId = vehicleMarkId, VehicleModelId = vehicleModelId, LiquidDefault = liquidDefault, LiquidByAdditionCondition = liquidByAdditionCondition });

                return vehicleLiquidities.Select(x => new VehicleModel()
                {
                    Id = x.VehicleModel.Id,
                    Name = x.VehicleModel.Name,
                    Code = x.VehicleModel.Code,
                    VehicleMarkId = x.VehicleMarkId,
                    VehicleMark = x.VehicleMark,
                    IsDisabled = x.VehicleModel.IsDisabled,
                    VehicleLiquidities = vehicleLiquidities.Where(z => z.VehicleMarkId == x.VehicleMarkId && z.VehicleModelId == x.VehicleModelId).ToList()

                }).ToList().Count();
            }
            else
            {
                return UnitOfWork.Session.Query<VehicleModel, VehicleMark, VehicleModel>($@"
                    SELECT v.*, m.*
                        FROM VehicleModels v
                        LEFT JOIN VehicleMarks m ON v.VehicleMarkId=m.Id
                    {condition}",
                    (vm, m) =>
                    {
                        vm.VehicleMark = m;
                        vm.VehicleLiquidities = _vehicleLiquidityRepository.GetVehicleLiquidityByModelId(vm.Id);
                        return vm;
                    },
                    new
                    {
                        vehicleMarkId,
                        vehicleModelId,
                        listQuery.Filter
                    }).ToList().Count();
            }
        }

        public int RelationCount(int modelId)
        {
            return UnitOfWork.Session.ExecuteScalar<int>(@"
            SELECT SUM(c)
            FROM (
            SELECT COUNT(*) as c
            FROM Cars
            WHERE VehicleModelId = @modelId
            UNION ALL
            SELECT COUNT(*) as c
            FROM Machineries
            WHERE VehicleModelId = @modelId
            ) as t",
        new { modelId = modelId });
        }

        public List<VehicleModel> GetVehicleModelsByMarkId(int markId)
        {
            return UnitOfWork.Session.Query<VehicleModel>($@"
            SELECT Id, Name, VehicleMarkId
            FROM VehicleModels
            WHERE VehicleMarkId = @markId
            AND IsDisabled = 0", new { markId },
                UnitOfWork.Transaction).ToList();
        }

        public async Task<VehicleModel> FindByNameWithMarkIdAsync(string name, int markId)
        {
            return await UnitOfWork.Session.QueryFirstOrDefaultAsync<VehicleModel>(
                "SELECT * FROM VehicleModels WHERE Name = @name AND VehicleMarkId = @markId AND DeleteDate IS NULL AND IsDisabled = 0",
                new { name, markId }, UnitOfWork.Transaction);
        }

        public async Task<VehicleModel> FindByNameOrCodeWithMarkIdAsync(string value, int markId)
        {
            return await UnitOfWork.Session.QueryFirstOrDefaultAsync<VehicleModel>(@"SELECT *
  FROM VehicleModels
 WHERE DeleteDate IS NULL
   AND VehicleMarkId = @markId
   AND (Name = @value OR Code = @value)
 ORDER BY Id DESC",
                new { value, markId }, UnitOfWork.Transaction);
        }
    }
}