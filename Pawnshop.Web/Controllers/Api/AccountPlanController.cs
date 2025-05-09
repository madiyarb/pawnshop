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
    [Authorize(Permissions.AccountPlanView)]
    public class AccountPlanController : Controller
    {
        private readonly IDictionaryService<AccountPlan> _service;
        private readonly ISessionContext _sessionContext;

        public AccountPlanController(IDictionaryService<AccountPlan> service, ISessionContext sessionContext)
        {
            _service = service;
            _sessionContext = sessionContext;
        }

        [HttpPost("/api/accountPlan/list")]
        public ListModel<AccountPlan> List([FromBody] ListQuery listQuery) => _service.List(listQuery);

        [HttpPost("/api/accountPlan/card")]
        public async Task<AccountPlan> Card([FromBody] int id)
        {
            if (id <= 0) throw new ArgumentOutOfRangeException(nameof(id));

            return await _service.GetAsync(id);
        }

        [HttpPost("/api/accountPlan/save"), Authorize(Permissions.AccountPlanManage)]
        [Event(EventCode.DictAccountPlanSaved, EventMode = EventMode.Response)]
        public AccountPlan Save([FromBody] AccountPlan model)
        {
            ModelState.Validate();

            if (model.Id == 0)
            {
                model.AuthorId = _sessionContext.UserId;
                model.CreateDate = DateTime.Now;
            }

            return _service.Save(model);
        }

        [HttpPost("/api/accountPlan/delete"), Authorize(Permissions.AccountPlanManage)]
        [Event(EventCode.DictAccountPlanDeleted, EventMode = EventMode.Request)]
        public IActionResult Delete([FromBody] int id)
        {
            if (id <= 0) throw new ArgumentOutOfRangeException(nameof(id));

            _service.Delete(id);

            return Ok();
        }
    }
}