using System;
using System.Threading.Tasks;
using Pawnshop.AccountingCore.Models;
using Pawnshop.Data.Access;
using Pawnshop.Data.Models.Dictionaries;

namespace Pawnshop.Services.ApplicationsOnline
{
    public sealed class ApplicationOnlineCarService : IApplicationOnlineCarService
    {
        private readonly ApplicationOnlineCarRepository _applicationOnlineCarRepository;
        private readonly CarRepository _carRepository;
        public ApplicationOnlineCarService(ApplicationOnlineCarRepository applicationOnlineCarRepository, CarRepository carRepository)
        {
            _applicationOnlineCarRepository = applicationOnlineCarRepository;
            _carRepository = carRepository;
        }

        public async Task<bool> ActualizeCarInfo(Guid id, int clientId)
        {
            var applicationCar = _applicationOnlineCarRepository.Get(id);
            if (applicationCar == null)
            {
                throw new ArgumentNullException($"Автомобиль по заявке с идентификатором {id} не найден");
            }
            if (!applicationCar.CarId.HasValue)
            {
                return true;
            }

            var car = _carRepository.Get(applicationCar.CarId.Value);
            if (car.ClientId.HasValue)
            {
                if (car.ClientId.Value != clientId)//Залог на данный момент оформлен на другого клиента 
                {
                    var carCanBeUpdated = await CanChangeClientIdentifierForClient(car, clientId);
                    if (!carCanBeUpdated)
                    {
                        return false;
                    }
                }
            }

            bool needUpdate = false;

            if (car.VehicleMarkId != applicationCar.VehicleMarkId)
            {
                needUpdate = true;
                car.VehicleMarkId = applicationCar.VehicleMarkId.Value;
                car.Mark = applicationCar.Mark;
            }

            if (car.VehicleModelId != applicationCar.VehicleModelId)
            {
                needUpdate = true;
                car.VehicleModelId = applicationCar.VehicleModelId.Value;
                car.Model = applicationCar.Model;
            }

            if (car.ReleaseYear != applicationCar.ReleaseYear)
            {
                needUpdate = true;
                car.ReleaseYear = applicationCar.ReleaseYear.Value;
            }

            if (car.TransportNumber != applicationCar.TransportNumber)
            {
                needUpdate = true;
                car.TransportNumber = applicationCar.TransportNumber;
            }

            if (car.MotorNumber != applicationCar.MotorNumber)
            {
                needUpdate = true;
                car.MotorNumber = applicationCar.MotorNumber;
            }

            if (car.ClientId != clientId)
            {
                needUpdate = true;
                car.ClientId = clientId;
            }

            if (car.BodyNumber != applicationCar.BodyNumber)
            {
                needUpdate = true;
                car.BodyNumber = applicationCar.BodyNumber;
            }

            if (car.TechPassportNumber != applicationCar.TechPassportNumber)
            {
                needUpdate = true;
                car.TechPassportNumber = applicationCar.TechPassportNumber;
            }

            if (car.Color != applicationCar.Color)
            {
                needUpdate = true;
                car.Color .Equals(applicationCar.Color);
            }

            if (car.TechPassportDate != applicationCar.TechPassportDate)
            {
                needUpdate = true;
                car.TechPassportDate = applicationCar.TechPassportDate;
            }
            if (needUpdate)
            {
                _carRepository.Update(car);
            }
            return true;
        }

        private async Task<bool> CanChangeClientIdentifierForClient(Car car, int clientId)
        {
            var contractsInfo = await _carRepository.GetShortContractByCarId(car.Id);
            foreach (var contractinfo in contractsInfo)
            {
                if (contractinfo.ClientId != clientId && contractinfo.Status != ContractStatus.BoughtOut )
                {
                    return false;
                }
            }

            return true;
        }
    }
}
