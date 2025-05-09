using System;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Pawnshop.Core;
using Pawnshop.Core.Queries;
using Pawnshop.Data.Models.Audit;
using Pawnshop.Data.Models.Clients;
using Pawnshop.Services;
using Pawnshop.Services.Models.List;
using Pawnshop.Web.Engine;
using Pawnshop.Web.Engine.Middleware;

namespace Pawnshop.Web.Controllers.Api
{
    [Authorize(Permissions.ClientSignersAllowedDocumentTypeView)]
    public class ClientSignersAllowedDocumentTypeController : Controller
    {
        private readonly IBaseService<ClientSignersAllowedDocumentType> _service;

        public ClientSignersAllowedDocumentTypeController(IBaseService<ClientSignersAllowedDocumentType> service)
        {
            _service = service;
        }

        [HttpPost]
        public ListModel<ClientSignersAllowedDocumentType> List([FromBody] ListQuery listQuery) => _service.List(listQuery);

        [HttpPost]
        public ClientSignersAllowedDocumentType Card([FromBody] int id)
        {
            if (id <= 0) throw new ArgumentOutOfRangeException(nameof(id));

            var ClientSignersAllowedDocumentType = _service.Get(id);
            if (ClientSignersAllowedDocumentType == null) throw new InvalidOperationException();

            return ClientSignersAllowedDocumentType;
        }

        [HttpPost, Authorize(Permissions.ClientSignersAllowedDocumentTypeManage)]
        [Event(EventCode.DictClientSignersAllowedDocumentTypeSaved, EventMode = EventMode.Response)]
        public ClientSignersAllowedDocumentType Save([FromBody] ClientSignersAllowedDocumentType model)
        {
            ModelState.Validate();

            return _service.Save(model);
        }

        [HttpPost, Authorize(Permissions.ClientSignersAllowedDocumentTypeManage)]
        [Event(EventCode.DictClientSignersAllowedDocumentTypeSaved, EventMode = EventMode.Response)]
        public IActionResult Delete([FromBody] int id)
        {
            if (id <= 0) throw new ArgumentOutOfRangeException(nameof(id));

            _service.Delete(id);
            return Ok();
        }
    }
}