using System;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Pawnshop.Core;
using Pawnshop.Core.Queries;
using Pawnshop.Data.Access;
using Pawnshop.Data.Models.Audit;
using Pawnshop.Data.Models.Dictionaries;
using Pawnshop.Services.Cars;
using Pawnshop.Services.Models.List;
using Pawnshop.Services.Models.Vehicle;
using Pawnshop.Web.Engine;
using Pawnshop.Web.Engine.Middleware;

namespace Pawnshop.Web.Controllers.Api
{
    public class VehicleModelController : Controller
    {
        private readonly IVehicleModelService _vehicleModelService;

        public VehicleModelController(IVehicleModelService vehicleModelService)
        {
            _vehicleModelService = vehicleModelService;
        }

        [HttpPost]
        public IActionResult List([FromBody] ListQueryModel<VehicleModelListQueryModel> listQuery)
        {
            return Ok(_vehicleModelService.List(listQuery));
        }

        [HttpPost]
        public IActionResult Card([FromBody] int id)
        {
            if (id <= 0) throw new ArgumentOutOfRangeException(nameof(id));
            return Ok(_vehicleModelService.Card(id));
        }

        [HttpPost]
        [Event(EventCode.DictVehicleModelsSaved, EventMode = EventMode.Response)]
        public IActionResult Save([FromBody] VehicleModelDto model)
        {
            ModelState.Validate();

            return Ok(_vehicleModelService.Save(model));
        }

        [HttpPost]
        [Event(EventCode.DictVehicleModelsDeleted, EventMode = EventMode.Request)]
        public IActionResult Delete([FromBody] int id)
        {
            _vehicleModelService.Delete(id);
            return Ok();
        }
    }
}