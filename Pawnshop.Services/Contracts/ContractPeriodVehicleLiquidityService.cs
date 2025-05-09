using Pawnshop.Core.Queries;
using Pawnshop.Data.Access;
using Pawnshop.Data.Models.Contracts;
using Pawnshop.Data.Models.Dictionaries;
using Pawnshop.Services.Models.List;
using System;

namespace Pawnshop.Services.Contracts
{
    public class ContractPeriodVehicleLiquidityService : IContractPeriodVehicleLiquidityService
    {
        private readonly ContractPeriodVehicleLiquidityRepository _contractPeriodVehicleLiquidityRepository;
        private readonly VehicleLiquidityRepository _vehicleLiquidityRepository;

        public ContractPeriodVehicleLiquidityService(ContractPeriodVehicleLiquidityRepository contractPeriodVehicleLiquidityRepository,
                                                     VehicleLiquidityRepository vehicleLiquidityRepository)
        {
            _contractPeriodVehicleLiquidityRepository = contractPeriodVehicleLiquidityRepository;
            _vehicleLiquidityRepository = vehicleLiquidityRepository;
        }

        public ListModel<ContractPeriodVehicleLiquidity> List(ListQuery listQuery)
        {
            return new ListModel<ContractPeriodVehicleLiquidity>()
            {
                List = _contractPeriodVehicleLiquidityRepository.List(listQuery),
                Count = _contractPeriodVehicleLiquidityRepository.Count(listQuery)
            };
        }

        public ContractPeriodVehicleLiquidity Get(int id)
        {
            var contractPeriodVehicleLiquidity = _contractPeriodVehicleLiquidityRepository.Get(id);
            if (contractPeriodVehicleLiquidity == null) throw new NullReferenceException($"Максимальный срок от ликвидности автомобиля с Id {id} не найден");
            return contractPeriodVehicleLiquidity;
        }

        public int GetLiquidityByVehicleMarkAndModel(int releaseYear, int vehicleMarkId, int vehicleModelId)
        {
            return _vehicleLiquidityRepository.Get(vehicleMarkId, vehicleModelId, releaseYear);
        }

        public ContractPeriodVehicleLiquidity GetPeriodByLiquidity(int releaseYear, int vehicleMarkId, int vehicleModelId)
        {
            var liquidValue = GetLiquidityByVehicleMarkAndModel(releaseYear, vehicleMarkId, vehicleModelId);

            if (liquidValue == 0) throw new NullReferenceException($"Ликвидность автотранспорта не найден. Обратитесь в службу технической поддержки");

            var contractPeriodVehicleLiquidity = _contractPeriodVehicleLiquidityRepository.GetPeriod(releaseYear, liquidValue);
            return contractPeriodVehicleLiquidity;
        }

        public ContractPeriodVehicleLiquidity Save(ContractPeriodVehicleLiquidity entity)
        {
            if (entity.Id > 0) _contractPeriodVehicleLiquidityRepository.Update(entity);
            else _contractPeriodVehicleLiquidityRepository.Insert(entity);
            return entity;
        }

        public void Delete(int id)
        {
            if (id <= 0) throw new ArgumentOutOfRangeException(nameof(id));
            _contractPeriodVehicleLiquidityRepository.Delete(id);
        }
    }
}
