using System;
using System.Collections.Generic;
using System.Linq;
using Dapper;
using Pawnshop.Core;
using Pawnshop.Core.Exceptions;
using Pawnshop.Core.Impl;
using Pawnshop.Core.Queries;
using Pawnshop.Data.Models.Dictionaries;

namespace Pawnshop.Data.Access
{
    public class VehicleLiquidityRepository : RepositoryBase, IRepository<VehicleLiquidity>
    {
        public VehicleLiquidityRepository(IUnitOfWork unitOfWork) : base(unitOfWork)
        {

        }

        public void Insert(VehicleLiquidity entity)
        {
            using (var transaction = BeginTransaction())
            {
                entity.CreateDate = DateTime.Now;
                if (entity.LiquidByAdditionCondition is null) entity.LiquidByAdditionCondition = 0;

                entity.Id = UnitOfWork.Session.QuerySingleOrDefault<int>(@"
                    INSERT INTO VehicleLiquid ( VehicleMarkId, VehicleModelId, LiquidDefault, LiquidByAdditionCondition,  YearCondition, AuthorId, CreateDate)
                    VALUES ( @VehicleMarkId, @VehicleModelId, @LiquidDefault, @LiquidByAdditionCondition, @YearCondition, @AuthorId, @CreateDate )
                    SELECT SCOPE_IDENTITY()",
                    entity, UnitOfWork.Transaction);
                transaction.Commit();
            }
        }

        public void Update(VehicleLiquidity entity)
        {
            using (var transaction = BeginTransaction())
            {
                UnitOfWork.Session.Execute(@"
                    UPDATE VehicleLiquid
                    SET VehicleMarkId = @VehicleMarkId, VehicleModelId = @VehicleModelId, LiquidDefault = @LiquidDefault, 
                    LiquidByAdditionCondition = @LiquidByAdditionCondition, YearCondition = @YearCondition, AuthorId = @AuthorId
                    WHERE Id = @Id",
                    entity, UnitOfWork.Transaction);
                transaction.Commit();
            }
        }

        public void Delete(int id)
        {
            using (var transaction = BeginTransaction())
            {
                UnitOfWork.Session.Execute("DELETE FROM VehicleLiquid WHERE Id = @id", new { id }, UnitOfWork.Transaction);
                transaction.Commit();
            }
        }

        public void DeleteByModelId(int modelId)
        {
            using (var transaction = BeginTransaction())
            {
                UnitOfWork.Session.Execute(@"UPDATE VehicleLiquid SET DeleteDate = dbo.GETASTANADATE() WHERE VehicleModelId = @modelId", new { modelId }, UnitOfWork.Transaction);
                transaction.Commit();
            }
        }

        public List<VehicleLiquidity> GetVehicleLiquidityByModelId(int vehicleModelId)
        {
            var entity = UnitOfWork.Session.Query<VehicleLiquidity>(@"
                SELECT vl.*
                FROM VehicleLiquid vl
                WHERE vl.VehicleModelId = @vehicleModelId",
                new { vehicleModelId },
                UnitOfWork.Transaction).ToList();

            return entity;
        }

        public VehicleLiquidity Get(int id)
        {
            return UnitOfWork.Session.Query<VehicleLiquidity, VehicleMark, VehicleModel, VehicleLiquidity>(@"
            SELECT vl.*, vma.* , vmo.*
            FROM VehicleLiquid vl
            LEFT JOIN VehicleMarks vma ON vma.Id = vl.VehicleMarkId
            LEFT JOIN VehicleModels vmo ON vmo.Id = vl.VehicleModelId
            WHERE vl.Id = @id",
            (vl, vma, vmo) =>
            {
                vl.VehicleMark = vma;
                vl.VehicleModel = vmo;
                return vl;
            }, new { id }, UnitOfWork.Transaction).FirstOrDefault();
        }

        public VehicleLiquidity Find(object query)
        {
            if (query == null)
                throw new ArgumentNullException(nameof(query));

            var vehicleMarkId = query?.Val<int>("VehicleMarkId");
            var vehicleModelId = query?.Val<int>("VehicleModelId");

            var entity = UnitOfWork.Session.Query<VehicleLiquidity, VehicleMark, VehicleModel, VehicleLiquidity>(@"
                SELECT vl.*, vma.* , vmo.*
                FROM VehicleLiquid vl
                LEFT JOIN VehicleMarks vma ON vma.Id = @vehicleMarkId
                LEFT JOIN VehicleModels vmo ON vmo.Id = @vehicleModelId
                WHERE vl.VehicleMarkId = @vehicleMarkId AND vl.VehicleModelId = @vehicleModelId",
                (vl, vma, vmo) =>
                {
                    vl.VehicleMark = vma;
                    vl.VehicleModel = vmo;
                    return vl;
                }, new { vehicleMarkId, vehicleModelId },
                UnitOfWork.Transaction).FirstOrDefault();

            return entity;
        }

        public List<VehicleLiquidity> List(ListQuery listQuery, object query = null)
        {
            if (listQuery == null) throw new ArgumentNullException(nameof(listQuery));

            var vehicleMarkId = query?.Val<int?>("VehicleMarkId");
            var vehicleModelId = query?.Val<int?>("VehicleModelId");
            var liquidDefault = query?.Val<int?>("LiquidDefault");
            var liquidByAdditionCondition = query?.Val<int?>("LiquidByAdditionCondition");

            var pre = "vl.DeleteDate IS NULL";
            pre += vehicleMarkId.HasValue ? " AND vl.VehicleMarkId = @vehicleMarkId" : string.Empty;
            pre += vehicleModelId.HasValue ? " AND vl.vehicleModelId = @vehicleModelId" : string.Empty;
            pre += liquidDefault.HasValue ? " AND vl.LiquidDefault = @liquidDefault" : string.Empty;
            pre += liquidByAdditionCondition.HasValue ? " AND vl.LiquidByAdditionCondition = @liquidByAdditionCondition" : string.Empty;

            var condition = listQuery.Like(pre, "vma.Name", "vmo.Name");
            var order = listQuery.Order(string.Empty, new Sort
            {
                Name = "vma.Name",
                Direction = SortDirection.Asc
            });
            var page = listQuery.Page();

            return UnitOfWork.Session.Query<VehicleLiquidity, VehicleMark, VehicleModel, VehicleLiquidity>($@"
                SELECT vl.*, vma.* , vmo.*
                FROM VehicleLiquid vl
                LEFT JOIN VehicleMarks vma ON vma.Id = vl.VehicleMarkId
                LEFT JOIN VehicleModels vmo ON vmo.Id = vl.VehicleModelId
                {condition} {order} {page}",
                (vl, vma, vmo) =>
                {
                    vl.VehicleMark = vma;
                    vl.VehicleModel = vmo;
                    return vl;
                },
                new
                {
                    vehicleMarkId,
                    vehicleModelId,
                    liquidDefault,
                    liquidByAdditionCondition,
                    listQuery.Page?.Offset,
                    listQuery.Page?.Limit,
                    listQuery.Filter
                }).ToList();
        }

        public int Count(ListQuery listQuery, object query = null)
        {
            if (listQuery == null) throw new ArgumentNullException(nameof(listQuery));

            var vehicleMarkId = query?.Val<int?>("VehicleMarkId");
            var vehicleModelId = query?.Val<int?>("VehicleModelId");
            var liquidDefault = query?.Val<int?>("LiquidDefault");
            var liquidByAdditionCondition = query?.Val<int?>("LiquidByAdditionCondition");

            var pre = "vl.DeleteDate IS NULL";
            pre += vehicleMarkId.HasValue ? " AND vl.VehicleMarkId = @vehicleMarkId" : string.Empty;
            pre += vehicleModelId.HasValue ? " AND vl.VehicleModelId = @vehicleModelId" : string.Empty;
            pre += liquidDefault.HasValue ? " AND vl.LiquidDefault = @liquidDefault" : string.Empty;
            pre += liquidByAdditionCondition.HasValue ? " AND vl.LiquidByAdditionCondition = @liquidByAdditionCondition" : string.Empty;

            var condition = listQuery.Like(pre, "vma.Name");

            return UnitOfWork.Session.ExecuteScalar<int>($@"
                SELECT COUNT(*)
                FROM VehicleLiquid vl
                LEFT JOIN VehicleMarks vma ON vl.VehicleMarkId = vma.Id
                {condition}",
                new
                {
                    vehicleMarkId,
                    vehicleModelId,
                    liquidDefault,
                    liquidByAdditionCondition,
                    listQuery.Filter
                });
        }

        public int Get(int vehicleMarkId, int vehicleModelId, int releaseYear)
        {
            var entity = UnitOfWork.Session.Query<VehicleLiquidity>(@"
            SELECT *
            FROM VehicleLiquid
            WHERE VehicleMarkId = @vehicleMarkId AND VehicleModelId = @vehicleModelId",
            new { vehicleMarkId, vehicleModelId }, UnitOfWork.Transaction).FirstOrDefault();

            if (entity is null) return 0;

            return GetByYearCondition(entity, releaseYear);
        }

        public int GetByYearCondition(VehicleLiquidity entity, int releaseYear)
        {
            if (entity.YearCondition.HasValue && releaseYear >= entity.YearCondition)
                return (int)entity.LiquidByAdditionCondition.Value;

            return (int)entity.LiquidDefault;
        }
    }
}