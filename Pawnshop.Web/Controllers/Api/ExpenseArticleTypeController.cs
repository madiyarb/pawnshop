using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Pawnshop.AccountingCore.Models;
using Pawnshop.Core;
using Pawnshop.Core.Queries;
using Pawnshop.Data.Models.Audit;
using Pawnshop.Services;
using Pawnshop.Services.AccountingCore;
using Pawnshop.Web.Engine;
using Pawnshop.Web.Engine.Middleware;
using Pawnshop.Services.Models.List;

namespace Pawnshop.Web.Controllers.Api
{
    [Authorize(Permissions.ExpenseArticleTypeView)]
    public class ExpenseArticleTypeController : Controller
    {
        private readonly IDictionaryService<ExpenseArticleType> _service;
        private readonly ISessionContext _sessionContext;

        public ExpenseArticleTypeController(IDictionaryService<ExpenseArticleType> service, ISessionContext sessionContext)
        {
            _service = service;
            _sessionContext = sessionContext;
        }

        [HttpPost("/api/expenseArticleType/list")]
        public ListModel<ExpenseArticleType> List([FromBody] ListQuery listQuery) => _service.List(listQuery);

        [HttpPost("/api/expenseArticleType/card")]
        public async Task<ExpenseArticleType> Card([FromBody] int id)
        {
            if (id <= 0) throw new ArgumentOutOfRangeException(nameof(id));

            return await _service.GetAsync(id);
        }

        [HttpPost("/api/expenseArticleType/save"), Authorize(Permissions.ExpenseArticleTypeManage)]
        [Event(EventCode.DictExpenseArticleTypeSaved, EventMode = EventMode.Response)]
        public ExpenseArticleType Save([FromBody] ExpenseArticleType model)
        {
            ModelState.Validate();

            if (model.Id == 0)
            {
                model.AuthorId = _sessionContext.UserId;
                model.CreateDate = DateTime.Now;
            }

            return _service.Save(model);
        }

        [HttpPost("/api/expenseArticleType/delete"), Authorize(Permissions.ExpenseArticleTypeManage)]
        [Event(EventCode.DictExpenseArticleTypeDeleted, EventMode = EventMode.Request)]
        public IActionResult Delete([FromBody] int id)
        {
            if (id <= 0) throw new ArgumentOutOfRangeException(nameof(id));

            _service.Delete(id);

            return Ok();
        }
    }
}