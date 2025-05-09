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
using Pawnshop.Web.Models.List;

namespace Pawnshop.Web.Controllers.Api
{
    public class VehicleManufacturerController : Controller
    {
        private readonly VehicleManufacturerRepository _repository;

        public VehicleManufacturerController(VehicleManufacturerRepository repository)
        {
            _repository = repository;
        }

        [HttpPost]
        public ListModel<VehicleManufacturer> List([FromBody] ListQuery listQuery)
        {
            return new ListModel<VehicleManufacturer>
            {
                List = _repository.List(listQuery),
                Count = _repository.Count(listQuery)
            };
        }

        [HttpPost]
        public VehicleManufacturer Card([FromBody] int id)
        {
            if (id <= 0) throw new ArgumentOutOfRangeException(nameof(id));

            var VehicleManufacturer = _repository.Get(id);
            if (VehicleManufacturer == null) throw new InvalidOperationException();

            return VehicleManufacturer;
        }

        [HttpPost]
        [Event(EventCode.DictVehicleManufacturersSaved, EventMode = EventMode.Response)]
        public VehicleManufacturer Save([FromBody] VehicleManufacturer Vehiclemark)
        {
            ModelState.Validate();

            if (Vehiclemark.Id > 0)
            {
                _repository.Update(Vehiclemark);
            }
            else
            {
                _repository.Insert(Vehiclemark);
            }
            return Vehiclemark;
        }

        [HttpPost]
        [Event(EventCode.DictVehicleManufacturersDeleted, EventMode = EventMode.Request)]
        public IActionResult Delete([FromBody] int id)
        {
            if (id <= 0) throw new ArgumentOutOfRangeException(nameof(id));

            var count = _repository.RelationCount(id);
            if (count > 0)
            {
                throw new Exception("Невозможно удалить производителя машин, так как он привязан к индексу изготовителей машин");
            }

            _repository.Delete(id);
            return Ok();
        }
    }
}