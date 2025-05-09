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
using AccrualBase = Pawnshop.AccountingCore.Models.AccrualBase;

namespace Pawnshop.Web.Controllers.Api
{
    [Authorize(Permissions.AccrualBaseView)]
    public class AccrualBaseController : Controller
    {
        private readonly IDictionaryWithSearchService<AccrualBase, AccrualBaseFilter> _service;
        private readonly ISessionContext _sessionContext;

        public AccrualBaseController(IDictionaryWithSearchService<AccrualBase, AccrualBaseFilter> service, ISessionContext sessionContext)
        {
            _service = service;
            _sessionContext = sessionContext;
        }

        [HttpPost("/api/AccrualBase/list")]
        public ListModel<AccrualBase> List([FromBody] ListQueryModel<AccrualBaseFilter> listQuery) => _service.List(listQuery);

        [HttpPost("/api/AccrualBase/card")]
        public async Task<AccrualBase> Card([FromBody] int id)
        {
            if (id <= 0) throw new ArgumentOutOfRangeException(nameof(id));

            return await _service.GetAsync(id);
        }

        [HttpPost("/api/AccrualBase/save"), Authorize(Permissions.AccrualBaseManage)]
        [Event(EventCode.DictAccrualBaseSaved, EventMode = EventMode.Response)]
        public AccrualBase Save([FromBody] AccrualBase model)
        {
            ModelState.Validate();

            if (model.Id == 0)
            {
                model.AuthorId = _sessionContext.UserId;
                model.CreateDate = DateTime.Now;
            }

            return _service.Save(model);
        }

        [HttpPost("/api/AccrualBase/delete"), Authorize(Permissions.AccrualBaseManage)]
        [Event(EventCode.DictAccrualBaseSaved, EventMode = EventMode.Request)]
        public IActionResult Delete([FromBody] int id)
        {
            if (id <= 0) throw new ArgumentOutOfRangeException(nameof(id));

            _service.Delete(id);

            return Ok();
        }
    }
}