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
    public class AccountController : Controller
    {
        private readonly IAccountService _service;
        private readonly IAccountRecordService _recordService;
        private readonly ISessionContext _sessionContext;

        public AccountController(IAccountService service,
            IAccountRecordService recordService,
            ISessionContext sessionContext)
        {
            _service = service;
            _recordService = recordService;
            _sessionContext = sessionContext;
        }

        [HttpPost("/api/account/list")]
        public ListModel<Account> List([FromBody] ListQueryModel<AccountFilter> listQuery) => _service.List(listQuery);

        [HttpPost("/api/account/card")]
        public async Task<Account> Card([FromBody] int id)
        {
            if (id <= 0) throw new ArgumentOutOfRangeException(nameof(id));

            return await _service.GetAsync(id);
        }

        [HttpPost("/api/account/save"), Authorize(Permissions.AccountManage)]
        [Event(EventCode.DictAccountSaved, EventMode = EventMode.All)]
        public Account Save([FromBody] Account model)
        {
            ModelState.Validate();

            if (model.Id == 0)
            {
                model.AuthorId = _sessionContext.UserId;
                model.CreateDate = DateTime.Now;
            }

            return _service.Save(model);
        }

        [HttpPost("/api/account/delete"), Authorize(Permissions.AccountManage)]
        [Event(EventCode.DictAccountDeleted, EventMode = EventMode.All)]
        public IActionResult Delete([FromBody] int id)
        {
            if (id <= 0) throw new ArgumentOutOfRangeException(nameof(id));

            _service.Delete(id);

            return Ok();
        }

        [HttpPost("/api/account/recalculateBalance")]
        [ProducesResponseType(typeof(Account), 200)]
        [Event(EventCode.DictAccountDeleted, EventMode = EventMode.All)]
        public async Task<IActionResult> RecalculateBalance([FromBody] int id)
        {
            if (id <= 0) throw new ArgumentOutOfRangeException(nameof(id));

            await Task.Run(() => _recordService.RecalculateBalanceOnAccount(id));
            Account account = await _service.GetAsync(id);
            return Ok(account);
        }
    }
}