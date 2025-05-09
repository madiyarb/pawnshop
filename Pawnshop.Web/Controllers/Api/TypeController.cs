using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Pawnshop.Core;
using Pawnshop.Core.Queries;
using Pawnshop.Data.Models.Audit;
using Pawnshop.Services;
using Pawnshop.Services.AccountingCore;
using Pawnshop.Web.Engine;
using Pawnshop.Web.Engine.Middleware;
using Pawnshop.Services.Models.List;
using Type = Pawnshop.AccountingCore.Models.Type;

namespace Pawnshop.Web.Controllers.Api
{
    [Authorize(Permissions.TypeView)]
    public class TypeController : Controller
    {
        private readonly IDictionaryService<Type> _service;
        private readonly ISessionContext _sessionContext;

        public TypeController(IDictionaryService<Type> service, ISessionContext sessionContext)
        {
            _service = service;
            _sessionContext = sessionContext;
        }

        [HttpPost("/api/type/list")]
        public ListModel<Type> List([FromBody] ListQuery listQuery)
        {
            return _service.List(listQuery);
        }

        [HttpPost("/api/type/card")]
        public async Task<Type> Card([FromBody] int id)
        {
            if (id <= 0) throw new ArgumentOutOfRangeException(nameof(id));

            return await _service.GetAsync(id);
        }

        [HttpPost("/api/type/save"), Authorize(Permissions.TypeManage)]
        [Event(EventCode.DictTypeSaved, EventMode = EventMode.Response)]
        public Type Save([FromBody] Type model)
        {
            ModelState.Validate();
            if (model.Id == 0)
            {
                model.AuthorId = _sessionContext.UserId;
                model.CreateDate = DateTime.Now;
            }

            return _service.Save(model);
        }

        [HttpPost("/api/type/delete"), Authorize(Permissions.TypeManage)]
        [Event(EventCode.DictTypeDeleted, EventMode = EventMode.Request)]
        public IActionResult Delete([FromBody] int id)
        {
            if (id <= 0) throw new ArgumentOutOfRangeException(nameof(id));

            _service.Delete(id);

            return Ok();
        }
    }
}