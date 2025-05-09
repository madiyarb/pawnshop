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
    public class VehicleMarkController : Controller
    {
        private readonly VehicleMarkRepository _repository;

        public VehicleMarkController(VehicleMarkRepository repository)
        {
            _repository = repository;
        }

        [HttpPost]
        public ListModel<VehicleMark> List([FromBody] ListQuery listQuery)
        {
            return new ListModel<VehicleMark>
            {
                List = _repository.List(listQuery),
                Count = _repository.Count(listQuery)
            };
        }

        [HttpPost]
        public VehicleMark Card([FromBody] int id)
        {
            if (id <= 0) throw new ArgumentOutOfRangeException(nameof(id));

            var VehicleMark = _repository.Get(id);
            if (VehicleMark == null) throw new InvalidOperationException();

            return VehicleMark;
        }

        [HttpPost]
        [Event(EventCode.DictVehicleMarksSaved, EventMode = EventMode.Response)]
        public VehicleMark Save([FromBody] VehicleMark Vehiclemark)
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
        [Event(EventCode.DictVehicleMarksDeleted, EventMode = EventMode.Request)]
        public IActionResult Delete([FromBody] int id)
        {
            if (id <= 0) throw new ArgumentOutOfRangeException(nameof(id));

            var count = _repository.RelationCount(id);
            if (count > 0)
            {
                throw new Exception("Невозможно удалить марку, так как она привязана к модели или машине или индексу изготовителей машин");
            }

            _repository.Delete(id);
            return Ok();
        }
    }
}