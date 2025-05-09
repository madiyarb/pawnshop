using System;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Pawnshop.Core;
using Pawnshop.Core.Queries;
using Pawnshop.Data.Access;
using Pawnshop.Data.Models.Dictionaries;
using Pawnshop.Web.Engine;
using Pawnshop.Web.Models.List;

namespace Pawnshop.Web.Controllers.Api
{
    [Authorize(Permissions.ExpenseGroupView)]
    public class ExpenseGroupController : Controller
    {
        private readonly ExpenseGroupRepository _repository;

        public ExpenseGroupController(ExpenseGroupRepository repository)
        {
            _repository = repository;
        }

        [HttpPost]
        public ListModel<ExpenseGroup> List([FromBody] ListQuery listQuery)
        {
            return new ListModel<ExpenseGroup>
            {
                List = _repository.List(listQuery),
                Count = _repository.Count(listQuery)
            };
        }

        [HttpPost]
        public ExpenseGroup Card([FromBody] int id)
        {
            if (id <= 0) throw new ArgumentOutOfRangeException(nameof(id));

            var model = _repository.Get(id);
            if (model == null) throw new InvalidOperationException();

            return model;
        }

        [HttpPost, Authorize(Permissions.ExpenseGroupManage)]
        public ExpenseGroup Save([FromBody] ExpenseGroup model)
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

        [HttpPost, Authorize(Permissions.ExpenseGroupManage)]
        public IActionResult Delete([FromBody] int id)
        {
            if (id <= 0) throw new ArgumentOutOfRangeException(nameof(id));

            _repository.Delete(id);
            return Ok();
        }
    }
}