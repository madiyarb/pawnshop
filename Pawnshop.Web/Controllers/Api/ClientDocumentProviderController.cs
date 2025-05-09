using System;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Pawnshop.Core;
using Pawnshop.Core.Exceptions;
using Pawnshop.Core.Queries;
using Pawnshop.Data.Access;
using Pawnshop.Data.Models.Audit;
using Pawnshop.Data.Models.Dictionaries;
using Pawnshop.Web.Engine;
using Pawnshop.Web.Engine.Middleware;
using Pawnshop.Web.Models.List;

namespace Pawnshop.Web.Controllers.Api
{
    [Authorize(Permissions.ClientDocumentProviderView)]
    public class ClientDocumentProviderController : Controller
    {
        private readonly ClientDocumentProviderRepository _repository;

        public ClientDocumentProviderController(ClientDocumentProviderRepository repository)
        {
            _repository = repository;
        }

        [HttpPost]
        public ListModel<ClientDocumentProvider> List([FromBody] ListQuery listQuery)
        {
            return new ListModel<ClientDocumentProvider>
            {
                List = _repository.List(listQuery),
                Count = _repository.Count(listQuery)
            };
        }

        [HttpPost]
        public ClientDocumentProvider Card([FromBody] int id)
        {
            if (id <= 0) throw new ArgumentOutOfRangeException(nameof(id));

            var ClientDocumentProvider = _repository.Get(id);
            if (ClientDocumentProvider == null) throw new InvalidOperationException();

            return ClientDocumentProvider;
        }

        [HttpPost, Authorize(Permissions.ClientDocumentProviderManage)]
        [Event(EventCode.DictClientDocumentProviderSaved, EventMode = EventMode.Response)]
        public ClientDocumentProvider Save([FromBody] ClientDocumentProvider ClientDocumentProvider)
        {
            ModelState.Validate();

            if (ClientDocumentProvider.Id > 0)
            {
                _repository.Update(ClientDocumentProvider);
            }
            else
            {
                _repository.Insert(ClientDocumentProvider);
            }
            return ClientDocumentProvider;
        }

        [HttpPost, Authorize(Permissions.ClientDocumentProviderManage)]
        [Event(EventCode.DictClientDocumentProviderDeleted, EventMode = EventMode.Request)]
        public IActionResult Delete([FromBody] int id)
        {
            if (id <= 0) throw new ArgumentOutOfRangeException(nameof(id));

            _repository.Delete(id);
            return Ok();
        }
    }
}