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
    [Authorize(Permissions.CategoryView)]
    public class CategoryController : Controller
    {
        private readonly CategoryRepository _repository;

        public CategoryController(CategoryRepository repository)
        {
            _repository = repository;
        }

        [HttpPost("/api/category/list")]
        public ListModel<Category> List([FromBody] ListQuery listQuery)
        {
            return new ListModel<Category>
            {
                List = _repository.List(listQuery),
                Count = _repository.Count(listQuery)
            };
        }

        [HttpPost("/api/category/card")]
        public Category Card([FromBody] int id)
        {
            if (id <= 0) throw new ArgumentOutOfRangeException(nameof(id));

            var category = _repository.Get(id);
            if (category == null) throw new InvalidOperationException();

            return category;
        }

        [HttpPost("/api/category/save"), Authorize(Permissions.CategoryManage)]
        [Event(EventCode.DictCategorySaved, EventMode = EventMode.Response)]
        public Category Save([FromBody] Category category)
        {
            ModelState.Validate();

            if (category.Id > 0)
            {
                _repository.Update(category);
            }
            else
            {
                _repository.Insert(category);
            }
            return category;
        }

        [HttpPost("/api/category/delete"), Authorize(Permissions.CategoryManage)]
        [Event(EventCode.DictCategoryDeleted, EventMode = EventMode.Request)]
        public IActionResult Delete([FromBody] int id)
        {
            if (id <= 0) throw new ArgumentOutOfRangeException(nameof(id));
            var count = _repository.RelationCount(id);
            if (count > 0)
            {
                throw new Exception("Невозможно удалить категорию, так как она привязана к позиции договора");
            }

            _repository.Delete(id);
            return Ok();
        }
    }
}