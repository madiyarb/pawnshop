using System;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Pawnshop.Core;
using Pawnshop.Data.Access;
using Pawnshop.Data.Models.Dictionaries;
using Pawnshop.Web.Engine;
using Pawnshop.Web.Models.Dictionary;
using Pawnshop.Web.Models.List;

namespace Pawnshop.Web.Controllers.Api
{
    [Authorize(Permissions.ExpenseTypeView)]
    public class ExpenseTypeController : Controller
    {
        private readonly ExpenseTypeRepository _repository;

        public ExpenseTypeController(ExpenseTypeRepository repository)
        {
            _repository = repository;
        }

        [HttpPost]
        public ListModel<ExpenseType> List([FromBody] ListQueryModel<ExpenseTypeListQueryModel> listQuery)
        {
            if (listQuery == null) listQuery = new ListQueryModel<ExpenseTypeListQueryModel>();
            if (listQuery.Model == null) listQuery.Model = new ExpenseTypeListQueryModel();
            
            return new ListModel<ExpenseType>
            {
                List = _repository.List(listQuery, listQuery.Model),
                Count = _repository.Count(listQuery, listQuery.Model)
            };
        }

        [HttpPost]
        public ExpenseType Card([FromBody] int id)
        {
            if (id <= 0) throw new ArgumentOutOfRangeException(nameof(id));

            var model = _repository.Get(id);
            if (model == null) throw new InvalidOperationException();

            return model;
        }

        [HttpPost, Authorize(Permissions.ExpenseTypeManage)]
        public ExpenseType Save([FromBody] ExpenseType model)
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

        [HttpPost, Authorize(Permissions.ExpenseTypeManage)]
        public IActionResult Delete([FromBody] int id)
        {
            if (id <= 0) throw new ArgumentOutOfRangeException(nameof(id));

            _repository.Delete(id);
            return Ok();
        }
    }
}