using Microsoft.AspNetCore.Mvc;
using Pawnshop.Core;
using Pawnshop.Core.Exceptions;
using Pawnshop.Core.Queries;
using Pawnshop.Data.Access;
using Pawnshop.Data.Models.Dictionaries;
using Pawnshop.Services.Models.Filters;
using Pawnshop.Services.Models.List;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;
using System.Text.RegularExpressions;

namespace Pawnshop.Services.Cars
{
    public class VehicleLiquidityService : IVehicleLiquidityService
    {
        private readonly VehicleLiquidityRepository _vehicleLiquidityRepository;

        public VehicleLiquidityService(VehicleLiquidityRepository vehicleLiquidityRepository)
        {
            _vehicleLiquidityRepository = vehicleLiquidityRepository;
        }

        public ListModel<VehicleLiquidity> List(ListQuery listQuery)
        {
            return new ListModel<VehicleLiquidity>()
            {
                List = _vehicleLiquidityRepository.List(listQuery),
                Count = _vehicleLiquidityRepository.Count(listQuery)
            };
        }

        public ListModel<VehicleLiquidity> List(ListQueryModel<VehicleLiquidityFilter> listQuery)
        {
            return new ListModel<VehicleLiquidity>()
            {
                List = _vehicleLiquidityRepository.List(listQuery, listQuery.Model),
                Count = _vehicleLiquidityRepository.Count(listQuery, listQuery.Model)
            };
        }

        public VehicleLiquidity Get(int id)
        {
            var vehicleLiquid = _vehicleLiquidityRepository.Get(id);
            if (vehicleLiquid == null) throw new NullReferenceException($"Ликвидность с Id {id} не найден");
            return vehicleLiquid;
        }

        public int Get(int vehicleMarkId, int vehicleModelId, int releaseYear)
        {
            return _vehicleLiquidityRepository.Get(vehicleMarkId, vehicleModelId, releaseYear);
        }

        public int GetByYearCondition(VehicleLiquidity entity, int releaseYear)
        {
            if (entity is null) return 0;
            return _vehicleLiquidityRepository.GetByYearCondition(entity, releaseYear);
        }

        public VehicleLiquidity Save(VehicleLiquidity entity)
        {
            if (entity.Id > 0)
            {
                _vehicleLiquidityRepository.Update(entity);
            }
            else
            {
                var exist = _vehicleLiquidityRepository.Find(new { VehicleMarkId = entity.VehicleMarkId, VehicleModelId = entity.VehicleModelId });
                if (exist != null) throw new PawnshopApplicationException($"Ликвидность для {exist.VehicleMark.Name} {exist.VehicleModel.Name} уже заведена в справочник");
                _vehicleLiquidityRepository.Insert(entity);
            }
            return entity;
        }

        public void Delete(int id)
        {
            if (id <= 0) throw new ArgumentOutOfRangeException(nameof(id));
            _vehicleLiquidityRepository.Delete(id);
        }
    }
}
