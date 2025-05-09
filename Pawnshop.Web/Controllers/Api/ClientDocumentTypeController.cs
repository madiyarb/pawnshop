using System;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Pawnshop.Core;
using Pawnshop.Core.Queries;
using Pawnshop.Data.Access;
using Pawnshop.Data.Models.Audit;
using Pawnshop.Data.Models.Dictionaries;
using Pawnshop.Web.Engine;
using Pawnshop.Web.Engine.Middleware;
using Pawnshop.Web.Models.List;

namespace Pawnshop.Web.Controllers.Api
{
    [Authorize(Permissions.ClientDocumentTypeView)]
    public class ClientDocumentTypeController : Controller
    {
        private readonly ClientDocumentTypeRepository _repository;

        public ClientDocumentTypeController(ClientDocumentTypeRepository repository)
        {
            _repository = repository;
        }

        [HttpPost]
        public ListModel<ClientDocumentType> List([FromBody] ListQuery listQuery)
        {
            return new ListModel<ClientDocumentType>
            {
                List = _repository.List(listQuery),
                Count = _repository.Count(listQuery)
            };
        }

        [HttpPost]
        public ClientDocumentType Card([FromBody] int id)
        {
            if (id <= 0) throw new ArgumentOutOfRangeException(nameof(id));

            var ClientDocumentType = _repository.Get(id);
            if (ClientDocumentType == null) throw new InvalidOperationException();

            return ClientDocumentType;
        }

        [HttpPost, Authorize(Permissions.ClientDocumentTypeManage)]
        [Event(EventCode.DictClientDocumentTypeSaved, EventMode = EventMode.Response)]
        public ClientDocumentType Save([FromBody] ClientDocumentType ClientDocumentType)
        {
            ModelState.Validate();

            if (ClientDocumentType.Id > 0)
            {
                _repository.Update(ClientDocumentType);
            }
            else
            {
                _repository.Insert(ClientDocumentType);
            }
            return ClientDocumentType;
        }

        [HttpPost, Authorize(Permissions.ClientDocumentTypeManage)]
        [Event(EventCode.DictClientDocumentTypeDeleted, EventMode = EventMode.Request)]
        public IActionResult Delete([FromBody] int id)
        {
            if (id <= 0) throw new ArgumentOutOfRangeException(nameof(id));

            _repository.Delete(id);
            return Ok();
        }
    }
}