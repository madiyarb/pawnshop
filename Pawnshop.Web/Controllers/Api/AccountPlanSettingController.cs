using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Pawnshop.Core;
using Pawnshop.Core.Queries;
using Pawnshop.Data.Models.Audit;
using Pawnshop.Services;
using Pawnshop.Services.AccountingCore;
using Pawnshop.Services.Models.Filters;
using Pawnshop.Web.Engine;
using Pawnshop.Web.Engine.Middleware;
using Pawnshop.Services.Models.List;
using AccountPlanSetting = Pawnshop.AccountingCore.Models.AccountPlanSetting;

namespace Pawnshop.Web.Controllers.Api
{
    [Authorize(Permissions.AccountPlanSettingView)]
    public class AccountPlanSettingController : Controller
    {
        private readonly IDictionaryWithSearchService<AccountPlanSetting, AccountPlanSettingFilter> _service;
        private readonly ISessionContext _sessionContext;

        public AccountPlanSettingController(IDictionaryWithSearchService<AccountPlanSetting, AccountPlanSettingFilter> service,
            ISessionContext sessionContext)
        {
            _service = service;
            _sessionContext = sessionContext;
        }

        [HttpPost("/api/accountPlanSetting/list")]
        public ListModel<AccountPlanSetting> List([FromBody] ListQueryModel<AccountPlanSettingFilter> listQuery) => _service.List(listQuery);

        [HttpPost("/api/accountPlanSetting/card")]
        public async Task<AccountPlanSetting> Card([FromBody] int id)
        {
            if (id <= 0) throw new ArgumentOutOfRangeException(nameof(id));

            return await _service.GetAsync(id);
        }

        [HttpPost("/api/accountPlanSetting/save"), Authorize(Permissions.AccountPlanSettingManage)]
        [Event(EventCode.DictAccountPlanSettingSaved, EventMode = EventMode.Response)]
        public AccountPlanSetting Save([FromBody] AccountPlanSetting model)
        {
            ModelState.Validate();

            if (model.Id == 0)
            {
                model.AuthorId = _sessionContext.UserId;
                model.CreateDate = DateTime.Now;
            }

            return _service.Save(model);
        }

        [HttpPost("/api/accountPlanSetting/delete"), Authorize(Permissions.AccountPlanSettingManage)]
        [Event(EventCode.DictAccountPlanSettingDeleted, EventMode = EventMode.Request)]
        public IActionResult Delete([FromBody] int id)
        {
            if (id <= 0) throw new ArgumentOutOfRangeException(nameof(id));

            _service.Delete(id);

            return Ok();
        }
    }
}