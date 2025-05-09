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
    [Authorize(Permissions.ParkingView)]
    public class ParkingActionController : Controller
    {
        private readonly ParkingActionRepository _repository;

        public ParkingActionController(ParkingActionRepository repository)
        {
            _repository = repository;
        }

        [HttpPost]
        public ListModel<ParkingAction> List([FromBody] ListQuery listQuery)
        {
            return new ListModel<ParkingAction>
            {
                List = _repository.List(listQuery),
                Count = _repository.Count(listQuery)
            };
        }

        [HttpPost]
        public ParkingAction Card([FromBody] int id)
        {
            if (id <= 0) throw new ArgumentOutOfRangeException(nameof(id));

            var parkingAction = _repository.Get(id);
            if (parkingAction == null) throw new InvalidOperationException();

            return parkingAction;
        }

        [HttpPost, Authorize(Permissions.ParkingManage)]
        [Event(EventCode.DictParkingActionSaved, EventMode = EventMode.Response)]
        public ParkingAction Save([FromBody] ParkingAction parkingAction)
        {
            ModelState.Validate();

            if (parkingAction.Id > 0)
            {
                _repository.Update(parkingAction);
            }
            else
            {
                _repository.Insert(parkingAction);
            }
            return parkingAction;
        }

        [HttpPost, Authorize(Permissions.ParkingManage)]
        [Event(EventCode.DictParkingActionDeleted, EventMode = EventMode.Request)]
        public IActionResult Delete([FromBody] int id)
        {
            if (id <= 0) throw new ArgumentOutOfRangeException(nameof(id));
            _repository.Delete(id);
            return Ok();
        }
    }
}
