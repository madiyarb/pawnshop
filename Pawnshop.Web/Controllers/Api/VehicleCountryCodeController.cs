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
    public class VehicleCountryCodeController : Controller
    {
        private readonly VehicleCountryCodeRepository _repository;

        public VehicleCountryCodeController(VehicleCountryCodeRepository repository)
        {
            _repository = repository;
        }

        [HttpPost]
        public ListModel<VehicleCountryCode> List([FromBody] ListQuery listQuery)
        {
            return new ListModel<VehicleCountryCode>
            {
                List = _repository.List(listQuery),
                Count = _repository.Count(listQuery)
            };
        }

        [HttpPost]
        public VehicleCountryCode Card([FromBody] int id)
        {
            if (id <= 0) throw new ArgumentOutOfRangeException(nameof(id));

            var VehicleCountryCode = _repository.Get(id);
            if (VehicleCountryCode == null) throw new InvalidOperationException();

            return VehicleCountryCode;
        }

        [HttpPost]
        [Event(EventCode.DictVehicleCountryCodesSaved, EventMode = EventMode.Response)]
        public VehicleCountryCode Save([FromBody] VehicleCountryCode Vehiclemark)
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
        [Event(EventCode.DictVehicleCountryCodesDeleted, EventMode = EventMode.Request)]
        public IActionResult Delete([FromBody] int id)
        {
            if (id <= 0) throw new ArgumentOutOfRangeException(nameof(id));

            var count = _repository.RelationCount(id);
            if (count > 0)
            {
                throw new Exception("Невозможно удалить код страны, так как он привязан к индексу изготовителей машин");
            }

            _repository.Delete(id);
            return Ok();
        }
    }
}