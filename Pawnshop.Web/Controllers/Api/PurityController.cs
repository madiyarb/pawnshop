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
    [Authorize(Permissions.PurityView)]
    public class PurityController : Controller
    {
        private readonly PurityRepository _repository;

        public PurityController(PurityRepository repository)
        {
            _repository = repository;
        }

        [HttpPost]
        public ListModel<Purity> List([FromBody] ListQuery listQuery)
        {
            return new ListModel<Purity>
            {
                List = _repository.List(listQuery),
                Count = _repository.Count(listQuery)
            };
        }

        [HttpPost]
        public Purity Card([FromBody] int id)
        {
            if (id <= 0) throw new ArgumentOutOfRangeException(nameof(id));

            var purity = _repository.Get(id);
            if (purity == null) throw new InvalidOperationException();

            return purity;
        }

        [HttpPost, Authorize(Permissions.PurityManage)]
        [Event(EventCode.DictPuritySaved, EventMode = EventMode.Response)]
        public Purity Save([FromBody] Purity purity)
        {
            ModelState.Validate();

            if (purity.Id > 0)
            {
                _repository.Update(purity);
            }
            else
            {
                _repository.Insert(purity);
            }
            return purity;
        }

        [HttpPost, Authorize(Permissions.PurityManage)]
        [Event(EventCode.DictPurityDeleted, EventMode = EventMode.Request)]
        public IActionResult Delete([FromBody] int id)
        {
            if (id <= 0) throw new ArgumentOutOfRangeException(nameof(id));
            var count = _repository.RelationCount(id);
            if (count > 0)
            {
                throw new PawnshopApplicationException("Невозможно удалить пробу, так как она привязана к позиции договора либо реализации");
            }

            _repository.Delete(id);
            return Ok();
        }
    }
}