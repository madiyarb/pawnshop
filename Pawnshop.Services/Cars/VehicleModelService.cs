using Newtonsoft.Json;
using Pawnshop.Core.Exceptions;
using Pawnshop.Core;
using Pawnshop.Data.Access;
using Pawnshop.Data.Models.Audit;
using Pawnshop.Data.Models.Dictionaries;
using Pawnshop.Services.Models.List;
using Pawnshop.Services.Models.Vehicle;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System;

namespace Pawnshop.Services.Cars
{
    public class VehicleModelService : IVehicleModelService
    {
        private readonly VehicleModelRepository _vehicleModelRepository;
        private readonly ISessionContext _sessionContext;
        private readonly IEventLog _eventLog;

        public VehicleModelService(VehicleModelRepository vehicleModelRepository,
                                   ISessionContext sessionContext,
                                   IEventLog eventLog)
        {
            _vehicleModelRepository = vehicleModelRepository;
            _sessionContext = sessionContext;
            _eventLog = eventLog;
        }

        public ListModel<VehicleModelDto> List(ListQueryModel<VehicleModelListQueryModel> listQuery)
        {
            if (listQuery == null) listQuery = new ListQueryModel<VehicleModelListQueryModel>();
            if (listQuery.Model == null) listQuery.Model = new VehicleModelListQueryModel();

            return new ListModel<VehicleModelDto>
            {
                List = GetVehicleModelDtoList(_vehicleModelRepository.List(listQuery, listQuery.Model)),
                Count = _vehicleModelRepository.Count(listQuery, listQuery.Model)
            };
        }

        public VehicleModelDto Card(int id)
        {
            if (id <= 0) throw new ArgumentOutOfRangeException(nameof(id));

            var model = _vehicleModelRepository.Get(id);
            if (model == null) throw new InvalidOperationException();

            return GetVehicleModelDto(model);
        }

        public VehicleModelDto Save(VehicleModelDto vehicleModelDto)
        {
            var model = GetVehicleModel(vehicleModelDto);

            VehicleModel prevModel = null;
            if (model.Id > 0)
            {
                prevModel = _vehicleModelRepository.Get(model.Id);
                _vehicleModelRepository.Update(model);

                _eventLog.Log(EventCode.ChangeModelLiquidity, EventStatus.Success, EntityType.VehicleLiquidity,
                            model.Id, JsonConvert.SerializeObject(model), JsonConvert.SerializeObject(prevModel), userId: model.AuthorId);
            }
            else
            {
                _vehicleModelRepository.Insert(model);

                _eventLog.Log(EventCode.AddModelLiquidity, EventStatus.Success, EntityType.VehicleLiquidity,
                            model.Id, JsonConvert.SerializeObject(model), null, userId: model.AuthorId);
            }

            return GetVehicleModelDto(model);
        }

        public void Delete(int id)
        {
            if (id <= 0) throw new ArgumentOutOfRangeException(nameof(id));

            var count = _vehicleModelRepository.RelationCount(id);
            if (count > 0)
            {
                throw new Exception("Невозможно удалить модель, так как она привязана к машине");
            }
            _vehicleModelRepository.Delete(id);
        }

        public async Task<VehicleModel> GetOrCreateModelNameWithMarkIdAsync(string name, int markId)
        {
            var model = await _vehicleModelRepository.FindByNameOrCodeWithMarkIdAsync(name, markId);

            if (model == null)
            {
                model = new VehicleModel
                {
                    AuthorId = 1,
                    Code = name.Replace(" ", "_"),
                    Name = name,
                    VehicleMarkId = markId,
                    VehicleLiquidities = new List<VehicleLiquidity>
                    {
                        new VehicleLiquidity
                        {
                            AuthorId = 1,
                            CreateDate= DateTime.Today,
                            LiquidDefault = LiquidDefaultType.Low,
                            LiquidByAdditionCondition = 0,
                        }
                    },
                };

                _vehicleModelRepository.Insert(model);
            }

            return model;
        }


        private List<VehicleModelDto> GetVehicleModelDtoList(List<VehicleModel> vehicleModelList)
        {
            return vehicleModelList.Select(x => GetVehicleModelDto(x)).ToList();
        }

        private VehicleLiquidity GetLiquidityInfo(VehicleModel vehicleModel)
        {
            var vehicleLiquidity = vehicleModel.VehicleLiquidities?.FirstOrDefault();
            return vehicleLiquidity;
        }

        private VehicleModelDto GetVehicleModelDto(VehicleModel vehicleModel)
        {
            return new VehicleModelDto()
            {
                Id = vehicleModel.Id,
                Name = vehicleModel.Name,
                Code = vehicleModel.Code,
                VehicleMarkId = vehicleModel.VehicleMarkId,
                VehicleMark = vehicleModel.VehicleMark,
                IsDisabled = vehicleModel.IsDisabled,
                VehicleLiquidity = GetLiquidityInfo(vehicleModel)
            };
        }

        private VehicleModel GetVehicleModel(VehicleModelDto vehicleModelDto)
        {
            if (vehicleModelDto.VehicleLiquidity is null)
                throw new PawnshopApplicationException($"Значение VehicleLiquidity объязательно к заполнению");

            if (vehicleModelDto.VehicleLiquidity.YearCondition.HasValue && !vehicleModelDto.VehicleLiquidity.LiquidByAdditionCondition.HasValue)
                throw new PawnshopApplicationException($"Значение LiquidByAdditionCondition объязательно к заполнению при заполненном YearCondition");

            if (vehicleModelDto.VehicleLiquidity.LiquidByAdditionCondition.HasValue &&
                vehicleModelDto.VehicleLiquidity.LiquidByAdditionCondition != 0 &&
                !vehicleModelDto.VehicleLiquidity.YearCondition.HasValue)
                throw new PawnshopApplicationException($"Значение YearCondition объязательно к заполнению при заполненном LiquidByAdditionCondition");

            return new VehicleModel()
            {
                Id = vehicleModelDto.Id,
                Name = vehicleModelDto.Name,
                Code = vehicleModelDto.Code,
                VehicleMarkId = vehicleModelDto.VehicleMarkId,
                VehicleMark = vehicleModelDto.VehicleMark,
                IsDisabled = vehicleModelDto.IsDisabled,
                VehicleLiquidities = GetVehicleLiquidities(vehicleModelDto),
                AuthorId = _sessionContext.IsInitialized ? _sessionContext.UserId : Constants.ADMINISTRATOR_IDENTITY
            };
        }

        private List<VehicleLiquidity> GetVehicleLiquidities(VehicleModelDto vehicleModelDto)
        {
            var vehicleModelList = new List<VehicleLiquidity>();
            vehicleModelList.Add(new VehicleLiquidity()
            {
                Id = vehicleModelDto.VehicleLiquidity.Id,
                VehicleMarkId = vehicleModelDto.VehicleMarkId,
                VehicleModelId = vehicleModelDto.Id,
                LiquidDefault = vehicleModelDto.VehicleLiquidity.LiquidDefault,
                LiquidByAdditionCondition = vehicleModelDto.VehicleLiquidity.LiquidByAdditionCondition,
                YearCondition = vehicleModelDto.VehicleLiquidity.YearCondition,
                AuthorId = _sessionContext.IsInitialized ? _sessionContext.UserId : Constants.ADMINISTRATOR_IDENTITY,
                CreateDate = DateTime.Now
            });
            return vehicleModelList;
        }
    }
}
