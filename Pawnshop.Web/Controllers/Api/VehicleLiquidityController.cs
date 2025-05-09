using System;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Pawnshop.Core;
using Pawnshop.Core.Queries;
using Pawnshop.Data.Access;
using Pawnshop.Data.Models.Audit;
using Pawnshop.Data.Models.Dictionaries;
using Pawnshop.Web.Engine;
using Pawnshop.Web.Engine.Middleware;
using Pawnshop.Services.Models.List;
using Pawnshop.Services.Cars;
using Pawnshop.Services.Models.Filters;
using Pawnshop.Services.Contracts;
using System.Threading.Tasks;

namespace Pawnshop.Web.Controllers.Api
{
    [Authorize]
    public class VehicleLiquidityController : Controller
    {
        private readonly IVehicleLiquidityService _service;
        private readonly IContractPeriodVehicleLiquidityService _contractPeriodVehicleLiquidityService;

        public VehicleLiquidityController(IVehicleLiquidityService service, IContractPeriodVehicleLiquidityService contractPeriodVehicleLiquidityService)
        {
            _service = service;
            _contractPeriodVehicleLiquidityService = contractPeriodVehicleLiquidityService;
        }

        [HttpPost]
        public ListModel<VehicleLiquidity> List([FromBody] ListQueryModel<VehicleLiquidityFilter> listQuery)
        {
            return _service.List(listQuery);
        }

        [HttpGet]
        public VehicleLiquidity Card(int id)
        {
            if (id <= 0) throw new ArgumentOutOfRangeException(nameof(id));
            return _service.Get(id);
        }

        [HttpGet]
        public int GetLiquidity(int markId, int modelId, int releaseYear)
        {
            if (markId <= 0 || modelId <= 0) throw new ArgumentOutOfRangeException(nameof(markId), nameof(modelId));
            return _service.Get(markId, modelId, releaseYear);
        }

        [HttpGet]
        public int GetMaxMonths(int markId, int modelId, int releaseYear)
        {
            if (markId <= 0 || modelId <= 0) throw new ArgumentOutOfRangeException(nameof(markId), nameof(modelId));
            var maxPossibleContractPeriod = _contractPeriodVehicleLiquidityService.GetPeriodByLiquidity(releaseYear, markId, modelId);
            return maxPossibleContractPeriod.MaxMonthsCount;
        }

        [HttpPost]
        [Event(EventCode.DictVehicleLiquiditySaved, EventMode = EventMode.Response)]
        public VehicleLiquidity Save([FromBody] VehicleLiquidity model)
        {
            return _service.Save(model);
        }

        [HttpDelete]
        [Event(EventCode.DictVehicleLiquidityDeleted, EventMode = EventMode.Request)]
        public IActionResult Delete([FromBody] int id)
        {
            _service.Delete(id);
            return Ok();
        }
    }
}