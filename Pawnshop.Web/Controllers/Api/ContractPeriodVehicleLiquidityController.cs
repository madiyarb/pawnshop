using System;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Pawnshop.Core;
using Pawnshop.Core.Queries;
using Pawnshop.Data.Models.Audit;
using Pawnshop.Data.Models.Dictionaries;
using Pawnshop.Web.Engine.Middleware;
using Pawnshop.Services.Models.List;
using Pawnshop.Services.Contracts;

namespace Pawnshop.Web.Controllers.Api
{
    //[Authorize(Permissions.VehicleLiquidityView)]
    [Authorize]
    public class ContractPeriodVehicleLiquidityController : Controller
    {
        private readonly IContractPeriodVehicleLiquidityService _service;

        public ContractPeriodVehicleLiquidityController(IContractPeriodVehicleLiquidityService service)
        {
            _service = service;
        }

        [HttpPost]
        public ListModel<ContractPeriodVehicleLiquidity> List([FromBody] ListQuery listQuery)
        {
            return _service.List(listQuery);
        }

        [HttpGet]
        public ContractPeriodVehicleLiquidity Card(int id)
        {
            if (id <= 0) throw new ArgumentOutOfRangeException(nameof(id));
            return _service.Get(id);
        }

        [HttpGet]
        public ContractPeriodVehicleLiquidity GetPeriod(int markId, int modelId, int releaseYear)
        {
            if (markId <= 0 || modelId <= 0) throw new ArgumentOutOfRangeException(nameof(markId), nameof(modelId));
            return _service.GetPeriodByLiquidity(releaseYear, markId, modelId);
        }

        [HttpPost]
        [Event(EventCode.DictContractPeriodVehicleLiquiditySaved, EventMode = EventMode.Response)]
        public ContractPeriodVehicleLiquidity Save([FromBody] ContractPeriodVehicleLiquidity model)
        {
            return _service.Save(model);
        }

        [HttpDelete]
        [Event(EventCode.DictContractPeriodVehicleLiquidityDeleted, EventMode = EventMode.Request)]
        public IActionResult Delete([FromBody] int id)
        {
            _service.Delete(id);
            return Ok();
        }
    }
}