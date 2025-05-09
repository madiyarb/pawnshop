using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Pawnshop.Core;
using Pawnshop.Core.Exceptions;
using Pawnshop.Core.Queries;
using Pawnshop.Data.Access;
using Pawnshop.Data.Models.Audit;
using Pawnshop.Data.Models.Dictionaries;
using Pawnshop.Services.Cars;
using Pawnshop.Web.Engine;
using Pawnshop.Web.Engine.Middleware;
using Pawnshop.Web.Models.List;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Pawnshop.Web.Controllers.Api
{
    [Authorize(Permissions.VehiclesBlackListView)]
    public class VehiclesBlackListController : Controller
    {
        private readonly VehicleBlackListRepository _repository;
        private readonly ISessionContext _sessionContext;
        private readonly IVehcileBlackListService _vehcileBlackListService;

        public VehiclesBlackListController(VehicleBlackListRepository repository, ISessionContext sessionContext, IVehcileBlackListService vehcileBlackListService)
        {
            _repository = repository;
            _sessionContext = sessionContext;
            _vehcileBlackListService = vehcileBlackListService;
        }

        [HttpPost]
        public ListModel<VehiclesBlackListItem> List([FromBody] ListQuery listQuery)
        {
            return new ListModel<VehiclesBlackListItem>
            {
                List = _repository.List(listQuery),
                Count = _repository.Count(listQuery)
            };
        }

        [HttpPost]
        public VehiclesBlackListItem Card([FromBody] int id)
        {
            if (id <= 0) throw new ArgumentOutOfRangeException(nameof(id));

            var carInBlackList = _repository.Get(id);
            if (carInBlackList == null) throw new InvalidOperationException();

            return carInBlackList;
        }

        [HttpPost, Authorize(Permissions.VehiclesBlackListManage)]
        [Event(EventCode.DictVehiclesBlackListItemSaved, EventMode = EventMode.Response)]
        public VehiclesBlackListItem Save([FromBody] VehiclesBlackListItem model)
        {
            ModelState.Validate();
            _vehcileBlackListService.Validate(model);

            if (model.BodyNumber != model.ConfirmedBodyNumber)
                throw new PawnshopApplicationException("VIN-кода не совпадают");

            if (model.Id > 0)
            {
                _repository.Update(model);
            }
            else
            {
                model.AuthorId = _sessionContext.UserId;
                model.CreateDate = DateTime.Now;
                _repository.Insert(model);
            }
            return model;
        }

        [HttpPost, Authorize(Permissions.VehiclesBlackListManage)]
        [Event(EventCode.DictVehiclesBlackListItemDeleted, EventMode = EventMode.Request)]
        public IActionResult Delete([FromBody] int id)
        {
            if (id <= 0) throw new ArgumentOutOfRangeException(nameof(id));

            _repository.Delete(id);
            return Ok();
        }
    }
}
