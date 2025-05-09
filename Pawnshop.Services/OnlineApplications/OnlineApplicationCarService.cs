using Pawnshop.Core;
using Pawnshop.Core.Extensions;
using Pawnshop.Data.Access;
using Pawnshop.Data.Models.OnlineApplications;
using Pawnshop.Services.Cars;
using System.Threading.Tasks;

namespace Pawnshop.Services.OnlineApplications
{
    public class OnlineApplicationCarService : IOnlineApplicationCarService
    {
        private readonly OnlineApplicationCarRepository _onlineApplicationCarRepository;
        private readonly OnlineApplicationPositionRepository _onlineApplicationPositionRepository;
        private readonly IVehicleMarkService _vehicleMarkService;
        private readonly IVehicleModelService _vehicleModelService;

        public OnlineApplicationCarService(OnlineApplicationCarRepository onlineApplicationCarRepository,
            OnlineApplicationPositionRepository onlineApplicationPositionRepository,
            IVehicleMarkService vehicleMarkService,
            IVehicleModelService vehicleModelService
            )
        {
            _onlineApplicationCarRepository = onlineApplicationCarRepository;
            _onlineApplicationPositionRepository = onlineApplicationPositionRepository;
            _vehicleMarkService = vehicleMarkService;
            _vehicleModelService = vehicleModelService;
        }


        public async Task<OnlineApplicationCar> GetEntityForCreateAsync(OnlineApplicationCar car)
        {
            if (car == null)
                return null;

            if (!string.IsNullOrEmpty(car.Mark) && !string.IsNullOrEmpty(car.Model))
            {
                var mark = await _vehicleMarkService.GetOrCreateMarkNameAsync(car.Mark);
                var model = await _vehicleModelService.GetOrCreateModelNameWithMarkIdAsync(car.Model, mark.Id);

                car.VehicleMarkId = mark.Id;
                car.VehicleModelId = model.Id;
            }

            if (car.Id == 0)
                return car;

            var result = await _onlineApplicationCarRepository.GetAsync(car.Id);

            car.ReleaseYear = car.ReleaseYear ?? result.ReleaseYear;
            car.TransportNumber = car.TransportNumber.IsNullOrEmpty(result.TransportNumber);
            car.MotorNumber = car.MotorNumber.IsNullOrEmpty(result.MotorNumber);
            car.BodyNumber = car.BodyNumber.IsNullOrEmpty(result.BodyNumber);
            car.TechPassportNumber = car.TechPassportNumber.IsNullOrEmpty(result.TechPassportNumber);
            car.TechPassportDate = car.TechPassportDate.CompareResultForDb(result.TechPassportDate);
            car.Color = car.Color.IsNullOrEmpty(result.Color);

            return car;
        }
    }
}
