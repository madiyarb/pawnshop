using System;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Pawnshop.Core;
using Pawnshop.Core.Exceptions;
using Pawnshop.Core.Queries;
using Pawnshop.Data.Access;
using Pawnshop.Data.Models.Audit;
using Pawnshop.Data.Models.Dictionaries;
using Pawnshop.Web.Engine;
using Pawnshop.Web.Engine.Middleware;
using Pawnshop.Web.Models.List;

namespace Pawnshop.Web.Controllers.Api
{
    public class VehicleWMIController : Controller
    {
        private readonly VehicleWMIRepository _repository;

        public VehicleWMIController(VehicleWMIRepository repository)
        {
            _repository = repository;
        }

        [HttpPost]
        public ListModel<VehicleWMI> List([FromBody] ListQuery listQuery)
        {
            return new ListModel<VehicleWMI>
            {
                List = _repository.List(listQuery),
                Count = _repository.Count(listQuery)
            };
        }

        [HttpPost]
        public VehicleWMI Card([FromBody] int id)
        {
            if (id <= 0) throw new ArgumentOutOfRangeException(nameof(id));

            var VehicleWMI = _repository.Get(id);
            if (VehicleWMI == null) throw new InvalidOperationException();

            return VehicleWMI;
        }

        [HttpPost]
        [Event(EventCode.DictVehicleWMIsSaved, EventMode = EventMode.Response)]
        public VehicleWMI Save([FromBody] VehicleWMI model)
        {
            ModelState.Validate();

            if (model.Id > 0)
            {
                _repository.Update(model);
            }
            else
            {
                _repository.Insert(model);
            }
            return model;
        }

        [HttpPost]
        [Event(EventCode.DictVehicleWMIsDeleted, EventMode = EventMode.Request)]
        public IActionResult Delete([FromBody] int id)
        {
            if (id <= 0) throw new ArgumentOutOfRangeException(nameof(id));

            var count = _repository.RelationCount(id);
            if (count > 0)
            {
                throw new Exception("Невозможно удалить индекс изготовителя машин, так как он привязан к машине");
            }

            _repository.Delete(id);
            return Ok();
        }
    }
}