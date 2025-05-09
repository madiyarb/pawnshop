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
using BusinessOperationSetting = Pawnshop.AccountingCore.Models.BusinessOperationSetting;

namespace Pawnshop.Web.Controllers.Api
{
    [Authorize(Permissions.BusinessOperationView)]
    public class BusinessOperationSettingController : Controller
    {
        private readonly IDictionaryWithSearchService<BusinessOperationSetting, BusinessOperationSettingFilter> _service;
        private readonly ISessionContext _sessionContext;

        public BusinessOperationSettingController(IDictionaryWithSearchService<BusinessOperationSetting, BusinessOperationSettingFilter> service, ISessionContext sessionContext)
        {
            _service = service;
            _sessionContext = sessionContext;
        }

        [HttpPost("/api/BusinessOperationSetting/list")]
        public ListModel<BusinessOperationSetting> List([FromBody] ListQueryModel<BusinessOperationSettingFilter> listQuery) => _service.List(listQuery);

        [HttpPost("/api/BusinessOperationSetting/card")]
        public async Task<BusinessOperationSetting> Card([FromBody] int id)
        {
            if (id <= 0) throw new ArgumentOutOfRangeException(nameof(id));

            return await _service.GetAsync(id);
        }

        [HttpPost("/api/BusinessOperationSetting/save"), Authorize(Permissions.BusinessOperationManage)]
        [Event(EventCode.DictBusinessOperationSettingSaved, EventMode = EventMode.Response)]
        public BusinessOperationSetting Save([FromBody] BusinessOperationSetting model)
        {
            ModelState.Validate();

            if (model.Id == 0)
            {
                model.AuthorId = _sessionContext.UserId;
                model.CreateDate = DateTime.Now;
            }

            return _service.Save(model);
        }

        [HttpPost("/api/BusinessOperationSetting/delete"), Authorize(Permissions.BusinessOperationManage)]
        [Event(EventCode.DictBusinessOperationSettingDeleted, EventMode = EventMode.Request)]
        public IActionResult Delete([FromBody] int id)
        {
            if (id <= 0) throw new ArgumentOutOfRangeException(nameof(id));

            _service.Delete(id);

            return Ok();
        }
    }
}