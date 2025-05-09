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
using Pawnshop.Web.Models.Dictionary;
using Pawnshop.Web.Models.List;

namespace Pawnshop.Web.Controllers.Api
{
    [Authorize(Permissions.HolidayView)]
    public class HolidayController : Controller
    {
        private readonly HolidayRepository _repository;
        private readonly ISessionContext _sessionContext;

        public HolidayController(HolidayRepository repository, ISessionContext sessionContext)
        {
            _repository = repository;
            _sessionContext = sessionContext;
        }

        [HttpPost]
        public ListModel<Holiday> List([FromBody] ListQueryModel<HolidayListQueryModel> listQuery)
        {
            if (listQuery == null) listQuery = new ListQueryModel<HolidayListQueryModel>();
            if (listQuery.Model == null) listQuery.Model = new HolidayListQueryModel();

            if (!listQuery.Model.BeginDate.HasValue)
            {
                listQuery.Model.BeginDate = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
            }

            if (!listQuery.Model.EndDate.HasValue)
            {
                listQuery.Model.EndDate = new DateTime(DateTime.Now.AddMonths(1).Year, DateTime.Now.AddMonths(1).Month, 1);
            }

            return new ListModel<Holiday>
            {
                List = _repository.List(listQuery, listQuery.Model),
                Count = _repository.Count(listQuery, listQuery.Model)
            };
        }

        [HttpPost]
        public Holiday Card([FromBody] int id)
        {
            if (id <= 0) throw new ArgumentOutOfRangeException(nameof(id));

            var Holiday = _repository.Get(id);
            if (Holiday == null) throw new InvalidOperationException();

            return Holiday;
        }

        [HttpPost, Authorize(Permissions.HolidayManage)]
        [Event(EventCode.DictHolidaySaved, EventMode = EventMode.Response)]
        public Holiday Save([FromBody] Holiday holiday)
        {
            ModelState.Validate();

            if (holiday.Id > 0)
            {
                _repository.Update(holiday);
            }
            else
            {
                holiday.AuthorId = _sessionContext.UserId;
                holiday.CreateDate = DateTime.Now;
                _repository.Insert(holiday);
            }
            return holiday;
        }

        [HttpPost, Authorize(Permissions.HolidayManage)]
        [Event(EventCode.DictHolidayDeleted, EventMode = EventMode.Request)]
        public IActionResult Delete([FromBody] int id)
        {
            if (id <= 0) throw new ArgumentOutOfRangeException(nameof(id));

            _repository.Delete(id);
            return Ok();
        }
    }
}