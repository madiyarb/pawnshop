using System;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Pawnshop.Core;
using Pawnshop.Core.Exceptions;
using Pawnshop.Core.Queries;
using Pawnshop.Data.Access;
using Pawnshop.Data.Models.Audit;
using Pawnshop.Data.Models.Dictionaries;
using Pawnshop.Data.Models.Postponements;
using Pawnshop.Web.Engine;
using Pawnshop.Web.Engine.Middleware;
using Pawnshop.Web.Models.List;

namespace Pawnshop.Web.Controllers.Api
{
    [Authorize(Permissions.PostponementView)]
    public class PostponementController : Controller
    {
        private readonly PostponementRepository _repository;

        public PostponementController(PostponementRepository repository)
        {
            _repository = repository;
        }

        [HttpPost]
        public ListModel<Postponement> List([FromBody] ListQuery listQuery)
        {
            return new ListModel<Postponement>
            {
                List = _repository.List(listQuery),
                Count = _repository.Count(listQuery)
            };
        }

        [HttpPost]
        public Postponement Card([FromBody] int id)
        {
            if (id <= 0) throw new ArgumentOutOfRangeException(nameof(id));

            var model = _repository.Get(id);
            if (model == null) throw new InvalidOperationException();

            return model;
        }

        [HttpPost, Authorize(Permissions.PostponementManage)]
        [Event(EventCode.DictPostponementSaved, EventMode = EventMode.Response)]
        public Postponement Save([FromBody] Postponement model)
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

        [HttpPost, Authorize(Permissions.PostponementManage)]
        [Event(EventCode.DictPostponementDeleted, EventMode = EventMode.Request)]
        public IActionResult Delete([FromBody] int id)
        {
            if (id <= 0) throw new ArgumentOutOfRangeException(nameof(id));
            var count = _repository.RelationCount(id);
            if (count > 0)
            {
                throw new PawnshopApplicationException("Невозможно удалить, так как есть привязка к договору");
            }

            _repository.Delete(id);
            return Ok();
        }
    }
}