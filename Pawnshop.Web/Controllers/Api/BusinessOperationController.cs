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
using BusinessOperation = Pawnshop.AccountingCore.Models.BusinessOperation;

namespace Pawnshop.Web.Controllers.Api
{
    [Authorize(Permissions.BusinessOperationView)]
    public class BusinessOperationController : Controller
    {
        private readonly IBusinessOperationService _service;
        private readonly ISessionContext _sessionContext;

        public BusinessOperationController(IBusinessOperationService service, ISessionContext sessionContext)
        {
            _service = service;
            _sessionContext = sessionContext;
        }

        [HttpPost("/api/businessOperation/list")]
        public ListModel<BusinessOperation> List([FromBody] ListQuery listQuery)
        {
            return _service.List(listQuery);
        }

        [HttpPost("/api/businessOperation/card")]
        public async Task<BusinessOperation> Card([FromBody] int id)
        {
            if (id <= 0) throw new ArgumentOutOfRangeException(nameof(id));

            return await _service.GetAsync(id);
        }

        [HttpPost("/api/businessOperation/save"), Authorize(Permissions.BusinessOperationManage)]
        [Event(EventCode.DictBusinessOperationSaved, EventMode = EventMode.Response)]
        public BusinessOperation Save([FromBody] BusinessOperation model)
        {
            ModelState.Validate();
            
            if (model.Id == 0)
            {
                model.AuthorId = _sessionContext.UserId;
                model.CreateDate = DateTime.Now;
            }

            return _service.Save(model);
        }

        [HttpPost("/api/businessOperation/delete"), Authorize(Permissions.BusinessOperationManage)]
        [Event(EventCode.DictBusinessOperationDeleted, EventMode = EventMode.Request)]
        public IActionResult Delete([FromBody] int id)
        {
            if (id <= 0) throw new ArgumentOutOfRangeException(nameof(id));

            _service.Delete(id);

            return Ok();
        }
    }
}