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
using Pawnshop.Services.Models.Filters;
using Pawnshop.Web.Engine;
using Pawnshop.Web.Engine.Middleware;
using Pawnshop.Services.Models.List;

namespace Pawnshop.Web.Controllers.Api
{
    [Authorize(Permissions.AccountSettingView)]
    public class AccountSettingController : Controller
    {
        private readonly IDictionaryWithSearchService<AccountSetting, AccountSettingFilter> _service;
        private readonly ISessionContext _sessionContext;

        public AccountSettingController(IDictionaryWithSearchService<AccountSetting, AccountSettingFilter> service, ISessionContext sessionContext)
        {
            _service = service;
            _sessionContext = sessionContext;
        }

        [HttpPost("/api/accountSetting/list")]
        public ListModel<AccountSetting> List([FromBody] ListQueryModel<AccountSettingFilter> listQuery) => _service.List(listQuery);

        [HttpPost("/api/accountSetting/card")]
        public async Task<AccountSetting> Card([FromBody] int id)
        {
            if (id <= 0) throw new ArgumentOutOfRangeException(nameof(id));

            return await _service.GetAsync(id);
        }

        [HttpPost("/api/accountSetting/save"), Authorize(Permissions.AccountSettingManage)]
        [Event(EventCode.DictAccountSaved, EventMode = EventMode.Response)]
        public AccountSetting Save([FromBody] AccountSetting model)
        {
            ModelState.Validate();

            if (model.Id == 0)
            {
                model.AuthorId = _sessionContext.UserId;
                model.CreateDate = DateTime.Now;
            }

            return _service.Save(model);
        }

        [HttpPost("/api/accountSetting/delete"), Authorize(Permissions.AccountSettingManage)]
        [Event(EventCode.DictAccountDeleted, EventMode = EventMode.Request)]
        public IActionResult Delete([FromBody] int id)
        {
            if (id <= 0) throw new ArgumentOutOfRangeException(nameof(id));

            _service.Delete(id);

            return Ok();
        }
    }
}