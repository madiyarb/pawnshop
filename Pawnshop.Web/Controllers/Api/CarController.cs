using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Pawnshop.Core.Exceptions;
using Pawnshop.Core.Queries;
using Pawnshop.Core;
using Pawnshop.Data.Models.AbsOnline;
using Pawnshop.Data.Models.Audit;
using Pawnshop.Data.Models.Dictionaries;
using Pawnshop.Services.Cars;
using Pawnshop.Services.Models.List;
using Pawnshop.Web.Engine.Middleware;
using Pawnshop.Web.Engine;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System;
using Microsoft.AspNetCore.Http;
using Pawnshop.Data.Access;
using Pawnshop.Data.Models.ApplicationsOnlineCar;
using Pawnshop.Data.Models.Auction.Dtos.Car;
using Pawnshop.Web.Models.CarModel;

namespace Pawnshop.Web.Controllers.Api
{
    [Authorize]
    public class CarController : Controller
    {
        private readonly ICarService _carService;

        public CarController(ICarService carService)
        {
            _carService = carService;
        }

        [HttpGet]
        [Route("api/car/transport-number/{transportNumber}")]
        [ProducesResponseType(typeof(List<AuctionCarDto>), StatusCodes.Status200OK)]
        [Authorize(Permissions.LegalCollectionManage)]
        public async Task<IActionResult> GetByCarNumber(string transportNumber)
        {
            var cars = await _carService.GetByTransportNumber(transportNumber);
            return Ok(cars);
        }

        [HttpPost("/api/car/list"), Authorize(Permissions.CarView)]
        public ListModel<Car> List([FromBody] ListQuery listQuery)
        {
            return _carService.ListWithCount(listQuery);
        }

        [HttpPost("/api/car/card"), Authorize(Permissions.CarView)]
        public Car Card([FromBody] int id)
        {
            if (id <= 0) throw new ArgumentOutOfRangeException(nameof(id));

            return _carService.Get(id);
        }

        [HttpPost("/api/car/save"), Authorize(Permissions.CarManage)]
        [Event(EventCode.DictCarSaved, EventMode = EventMode.Response)]
        public Car Save([FromBody] Car car)
        {
            ModelState.Validate();
            _carService.Validate(car);

            try
            {
                _carService.Save(car);
            }
            catch (SqlException e)
            {
                if (e.Number == 2627)
                {
                    throw new PawnshopApplicationException("Поле номер техпаспорта должно быть уникальным");
                }
                throw new PawnshopApplicationException(e.Message);
            }
            catch (Exception e)
            {
                throw new PawnshopApplicationException(e.Message);
            }

            return car;
        }

        [HttpPost("/api/car/delete"), Authorize(Permissions.CarManage)]
        [Event(EventCode.DictCarDeleted, EventMode = EventMode.Request)]
        public IActionResult Delete([FromBody] int id)
        {
            if (id <= 0) throw new ArgumentOutOfRangeException(nameof(id));

            _carService.Delete(id);
            return Ok();
        }

        [HttpPost("/api/car/colors")]
        public List<string> Colors()
        {
            return _carService.Colors();
        }

        [HttpGet("/api/car/from-mobile")]
        public async Task<IActionResult> GetCarsByClientId([FromQuery] int clientid)
        {
            var cars = await _carService.ListByClientId(clientid);

            var carViewList = cars.Select(x => new CarView
            {
                Brand = x.VehicleMark?.Name ?? x.Mark,
                Model = x.VehicleModel?.Name ?? x.Model,
                CarNumber = x.TransportNumber,
                Vin = x.BodyNumber,
                Year = x.ReleaseYear,
                CarId = x.Id
            });

            return Ok(carViewList);
        }

        [AllowAnonymous]
        [HttpGet("/api/cars/liquidity")]
        public async Task<IActionResult> GetCarLiquidity(
            //[FromQuery] string? mark, 
            //[FromQuery] string? model,
            //[FromQuery] int? year,
            //[FromServices] VehicleLiquidityService vehicleLiquidityService,
            //[FromServices] VehicleModelRepository vehicleModelRepository,
            //[FromServices] VehicleMarkRepository vehicleMarkRepository,
            //[FromServices] ContractPeriodVehicleLiquidityRepository contractPeriodVehicleLiquidityRepository)
            )
        {
            //if (string.IsNullOrEmpty(mark) || string.IsNullOrEmpty(model) || year == null)
            //{
            //    return BadRequest(
            //        $"Some of params is null or empty. Recieved mark : {mark} model : {model} , year : {year} ");
            //}

            //if (DateTime.Now.Year < year)
            //{
            //    return BadRequest(
            //        $"Release year greater then now date ");
            //}

            //var carMark = await vehicleMarkRepository.FindByNameAsync(mark);
            //if (carMark == null)
            //{
            //    return Ok(new CarLiquidityMobileApplicationView());
            //}

            //var carModel = await vehicleModelRepository.FindByNameOrCodeWithMarkIdAsync(model, carMark.Id);

            //if (carModel == null)
            //{
            //    return Ok(new CarLiquidityMobileApplicationView());
            //}

            //var carLiquidity = vehicleLiquidityService.Get(carMark.Id, carModel.Id, year.Value);

            //var contractPeriodVehicleLiquidity = contractPeriodVehicleLiquidityRepository.GetPeriod(year.Value, carLiquidity);

            //CarLiquidity liquidity;
            //switch (carLiquidity)
            //{
            //    case 1:
            //        liquidity = CarLiquidity.Low;
            //        break;
            //    case 2:
            //        liquidity = CarLiquidity.Middle;
            //        break;
            //    case 3:
            //        liquidity = CarLiquidity.High;
            //        break;
            //    default:
            //        liquidity = CarLiquidity.Low;
            //        break;
            //}

            //return Ok(new CarLiquidityMobileApplicationView(liquidity, contractPeriodVehicleLiquidity.MaxMonthsCount));

            return Ok(new CarLiquidityMobileApplicationView(CarLiquidity.Middle, 36));
        }
    }
}