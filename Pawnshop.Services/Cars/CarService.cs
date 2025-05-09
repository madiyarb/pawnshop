using Pawnshop.Core.Queries;
using Pawnshop.Data.Access;
using Pawnshop.Data.Models.Dictionaries;
using Pawnshop.Services.Models.List;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Pawnshop.Data.Models.Auction.Dtos.Car;

namespace Pawnshop.Services.Cars
{
    public class CarService : ICarService
    {
        private readonly CarRepository _repository;
        private readonly IVehcileService _vehcileService;

        public CarService(CarRepository repository, IVehcileService vehcileService)
        {
            _repository = repository;
            _vehcileService = vehcileService;
        }

        public ListModel<Car> ListWithCount(ListQuery listQuery)
        {
            return new ListModel<Car>
            {
                List = _repository.List(listQuery),
                Count = _repository.Count(listQuery)
            };
        }

        public Car Get(int id)
        {
            var car = _repository.Get(id);
            if (car == null)
                throw new NullReferenceException($"Автомобиль с Id {id} не найден");
            return car;
        }

        public Car Save(Car car)
        {
            if (car.Id > 0) _repository.Update(car);
            else _repository.Insert(car);

            return car;
        }

        public void Delete(int id)
        {
            var count = _repository.RelationCount(id);
            if (count > 0)
            {
                throw new Exception("Невозможно удалить позицию, так как она привязана к позиции договора");
            }
            _repository.Delete(id);
        }

        public List<string> Colors()
        {
            return _repository.Colors();
        }

        public void Validate(Car car)
        {
            _vehcileService.BodyNumberValidate(car.BodyNumber);
            _vehcileService.TechPassportNumberValidate(car.TechPassportNumber);
            _vehcileService.TechPassportDateValidate(car.TechPassportDate);
            _vehcileService.ReleaseYearValidate(car.ReleaseYear);
            _vehcileService.MarkValidate(car.VehicleMark);
            _vehcileService.ModelValidate(car.VehicleModel);
        }

        public Car Find(object query)
        {
            return _repository.Find(query);
        }

        public async Task<IEnumerable<Car>> ListByClientId(int clientid)
        {
            return await _repository.ListByClientId(clientid);
        }

        public async Task<List<AuctionCarDto>> GetByTransportNumber(string transportNumber)
        {
            var cars = await _repository.GetByTransportNumberAsync(transportNumber);

            return cars;
        }
    }
}
