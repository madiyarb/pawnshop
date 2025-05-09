using Microsoft.AspNetCore.Mvc;
using Pawnshop.Data.Models.Dictionaries;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Pawnshop.Core.Queries;
using Pawnshop.Data.Access;
using Microsoft.AspNetCore.Authorization;
using Pawnshop.Core;
using Pawnshop.Web.Models.List;
using Pawnshop.Web.Engine.Middleware;
using Pawnshop.Data.Models.Audit;
using Pawnshop.Web.Engine;

namespace Pawnshop.Web.Controllers.Api
{
    [Authorize]
    public class BlackListReasonController : Controller
    {
        private readonly BlackListReasonRepository _repository;
        public BlackListReasonController(BlackListReasonRepository repository)
        {
            _repository = repository;
        }

        [HttpPost("/api/BlackListReason/list")]
        public ListModel<BlackListReason> List([FromBody]ListQuery listQuery)
        {
            return new ListModel<BlackListReason>
            {
                Count = _repository.Count(listQuery),
                List = _repository.List(listQuery)
            };
        }

        [HttpPost("/api/BlackListReason/card")]
        public BlackListReason Card([FromBody]int id)
        {
            if (id <= 0) throw new ArgumentOutOfRangeException(nameof(id));

            var reason = _repository.Get(id);
            if(reason==null) throw new InvalidOperationException();

            return reason;
        }

        [HttpPost("/api/BlackListReason/save"), Authorize(Permissions.BlackListReasonManage)]
        [Event(EventCode.DictBlackListReasonSaved, EventMode = EventMode.Response)]
        public BlackListReason Save([FromBody] BlackListReason reason)
        {
            ModelState.Validate();

            if (reason.Id > 0)
            {
                _repository.Update(reason);
            }
            else
            {
                _repository.Insert(reason);
            }
            return reason;
        }

        [HttpPost("/api/BlackListReason/delete"), Authorize(Permissions.BlackListReasonManage)]
        [Event(EventCode.DictBlackListReasonDeleted, EventMode = EventMode.Request)]
        public IActionResult Delete([FromBody] int id)
        {
            if (id <= 0) throw new ArgumentOutOfRangeException(nameof(id));

            var count = _repository.RelationCount(id);
            if (count > 0)
            {
                throw new Exception("Невозможно удалить причину, так как она привязана к клиентам или авто в чс");
            }

            _repository.Delete(id);
            return Ok();
        }
    }
}
