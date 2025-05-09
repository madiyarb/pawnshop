using Pawnshop.AccountingCore.Models;
using Pawnshop.Data.Access;
using Pawnshop.Data.Models.AbsOnline;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Pawnshop.Services.AbsOnline
{
    public class AbsOnlineClientsService : IAbsOnlineClientsService
    {
        private readonly CarRepository _carRepository;
        private readonly PositionRepository _positionRepository;
        private readonly RealtyRepository _realtyRepository;

        public AbsOnlineClientsService(
            CarRepository carRepository,
            PositionRepository positionRepository,
            RealtyRepository realtyRepository
            )
        {
            _carRepository = carRepository;
            _positionRepository = positionRepository;
            _realtyRepository = realtyRepository;
        }

        public async Task<List<AbsOnlineClientPositionView>> GetClientPositionsAsync(string iin)
        {
            var response = new List<AbsOnlineClientPositionView>();

            var positions = await _positionRepository.GetByIdentityNumberAsync(iin);

            if (positions == null)
                return response;

            var cars = positions
                .Where(x => x.CollateralType == CollateralType.Car)
                .Select(x => _carRepository.Get(x.Id));

            var realties = positions
                .Where(x => x.CollateralType == CollateralType.Realty)
                .Select(x => _realtyRepository.Get(x.Id));

            if (cars.Any())
                response.AddRange(cars.Select(x => new AbsOnlineClientPositionView
                {
                    CarBrand = x.VehicleMark?.Name ?? x.Mark,
                    CarModel = x.VehicleModel?.Name ?? x.Model,
                    CarNumber = x.TransportNumber,
                    Vin = x.BodyNumber,
                    Year = x.ReleaseYear,
                    PositionType = x.CollateralType.ToString(),
                }));

            if (realties.Any())
                response.AddRange(realties.Select(x => new AbsOnlineClientPositionView
                {
                    Address = x.Address?.FullPathRus,
                    Rka = x.Rca,
                    Year = x.Year,
                    PositionType = x.CollateralType.ToString(),
                }));

            return response;
        }
    }
}
