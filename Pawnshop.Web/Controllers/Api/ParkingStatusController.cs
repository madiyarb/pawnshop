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
    public class ParkingStatusController : Controller
    {
        private readonly ParkingStatusRepository _repository;

        public ParkingStatusController(ParkingStatusRepository repository)
        {
            _repository = repository;
        }

        [HttpPost]
        public ListModel<ParkingStatus> List([FromBody] ListQuery listQuery)
        {
            return new ListModel<ParkingStatus>
            {
                List = _repository.List(listQuery),
                Count = _repository.Count(listQuery)
            };
        }

        [HttpPost]
        public ParkingStatus Card([FromBody] int id)
        {
            if (id <= 0) throw new ArgumentOutOfRangeException(nameof(id));

            var parkingStatus = _repository.Get(id);
            if (parkingStatus == null) throw new InvalidOperationException();

            return parkingStatus;
        }

        [HttpPost, Authorize(Permissions.ParkingManage)]
        [Event(EventCode.DictParkingActionSaved, EventMode = EventMode.Response)]
        public ParkingStatus Save([FromBody] ParkingStatus parkingStatus)
        {
            ModelState.Validate();

            if (parkingStatus.Id > 0)
            {
                _repository.Update(parkingStatus);
            }
            else
            {
                _repository.Insert(parkingStatus);
            }
            return parkingStatus;
        }

        [HttpPost, Authorize(Permissions.ParkingManage)]
        [Event(EventCode.DictParkingActionDeleted, EventMode = EventMode.Request)]
        public IActionResult Delete([FromBody] int id)
        {
            if (id <= 0) throw new ArgumentOutOfRangeException(nameof(id));

            var count = _repository.RelationCount(id);
            if (count > 0)
            {
                throw new Exception("Невозможно удалить статус, так как он привязан к позиции");
            }

            _repository.Delete(id);
            return Ok();
        }
    }
}
