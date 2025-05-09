using Microsoft.AspNetCore.Mvc;
using Pawnshop.Data.Access;
using Pawnshop.Data.Models.ApplicationsOnline;
using Pawnshop.Data.Models.ApplicationsOnlineCar.Views;
using Pawnshop.Data.Models.ApplicationsOnlineCar;
using Pawnshop.Services.Cars;
using Pawnshop.Services.MaximumLoanTermDetermination;
using Pawnshop.Web.Models.ApplicationOnlineCar;
using System.Threading.Tasks;
using System;
using Microsoft.AspNetCore.Authorization;
using Pawnshop.Core;
using Pawnshop.Data.Models.Dictionaries;

namespace Pawnshop.Web.Controllers.Api
{
    [Authorize]
    [ApiController]
    public class ApplicationOnlineCarController : Controller
    {
        private readonly ISessionContext _sessionContext;
        public ApplicationOnlineCarController(ISessionContext sessionContext)
        {
            _sessionContext = sessionContext;
        }

        [HttpGet("/api/appicaliononlinecars/{id}")]
        public async Task<IActionResult> Get(
            [FromRoute] Guid id,
            [FromServices] ApplicationOnlineCarRepository repository
        )
        {
            var car = repository.Get(id);

            return Ok(new ApplicationOnlineCarView(car.Id, car.Mark, car.Model, car.TechPassportModel, car.Category,
                car.TechCategory, car.ReleaseYear,
                car.TransportNumber, car.MotorNumber, car.BodyNumber, car.TechPassportNumber, car.TechPassportDate,
                car.Color, car.VehicleMarkId, car.VehicleModelId,
                car.OwnerRegionName, car.OwnerRegionNameEn, car.Notes, car.Liquidity, car.CarId));
        }

        [HttpPost("/api/appicaliononlinecars/{id}/update")]
        public async Task<IActionResult> Update(
            [FromRoute] Guid id,
            [FromServices] ApplicationOnlineCarRepository repository,
            [FromBody] ApplicationOnlineCarBinding binding,
            [FromServices] VehicleModelRepository vehicleModelRepository,
            [FromServices] VehicleMarkRepository vehicleMarkRepository,
            [FromServices] IMaximumLoanTermDeterminationService maximumLoanTermDeterminationService,
            [FromServices] ApplicationOnlineRepository applicationOnlineRepository,
            [FromServices] VehicleLiquidityService vehicleLiquidityService
        )
        {
            var car = repository.Get(id);

            if (car == null)
            {
                return NotFound();
            }

            var applicationOnline = applicationOnlineRepository.GetByApplicationOnlinePositionId(id);
            if (!applicationOnline.CanEditing(_sessionContext.UserId))
            {
                return BadRequest($"Заявка принадлежит пользователю с id : {applicationOnline.ResponsibleManagerId} назначте заявку себя");
            }

            if (applicationOnline.Status == ApplicationOnlineStatus.OnEstimation.ToString())
            {
                return BadRequest("Нельзя изменять информацию об авто в текущем статусе.");
            }

            VehicleModel model = null;
            VehicleMark mark = null;

            if (binding.VehicleModelId.HasValue)
            {
                model = vehicleModelRepository.Get(binding.VehicleModelId.Value);
            }

            if (binding.VehicleMarkId.HasValue)
            {
                mark = vehicleMarkRepository.Get(binding.VehicleMarkId.Value);
            }

            car.Update(mark?.Name, model?.Name, binding.TechPassportModel, binding.Category, binding.TechCategory, binding.ReleaseYear, binding.TransportNumber,
                binding.MotorNumber, binding.BodyNumber, binding.TechPassportNumber, binding.TechPassportDate, binding.Color, binding.VehicleMarkId, binding.VehicleModelId,
                binding.OwnerRegionName, binding.OwnerRegionNameEn, binding.Notes);

            if (car.VehicleMarkId != null && car.VehicleModelId != null && car.ReleaseYear != null)
            {
                var carLiquidity = vehicleLiquidityService.Get(car.VehicleMarkId.Value, car.VehicleModelId.Value, car.ReleaseYear.Value);

                CarLiquidity liquidity;
                switch (carLiquidity)
                {
                    case 1:
                        liquidity = CarLiquidity.Low;
                        break;
                    case 2:
                        liquidity = CarLiquidity.Middle;
                        break;
                    case 3:
                        liquidity = CarLiquidity.High;
                        break;
                    default:
                        liquidity = CarLiquidity.Low;
                        break;
                }

                car.UpdateLiquidity(liquidity.ToString());

                var maxLoanTerm = maximumLoanTermDeterminationService.Determinate(
                    applicationOnline.ProductId, new MaximumLoanTermCarDeterminationModel
                    {
                        CarMarkId = car.VehicleMarkId.Value,
                        CarModelId = car.VehicleModelId.Value,
                        ReleaseYear = car.ReleaseYear.Value
                    });

                applicationOnline.SetMaximumAvailableLoanTerm(maxLoanTerm);
                await applicationOnlineRepository.Update(applicationOnline);
            }

            repository.Update(car);
            return Ok(car);
        }
    }
}
