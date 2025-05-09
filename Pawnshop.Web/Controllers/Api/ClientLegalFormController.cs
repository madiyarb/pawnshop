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
    [Authorize]
    public class ClientLegalFormController : Controller
    {
        private readonly ClientLegalFormRepository _repository;

        public ClientLegalFormController(ClientLegalFormRepository repository)
        {
            _repository = repository;
        }

        [HttpPost("/api/ClientLegalForm/list"), Authorize(Permissions.ClientLegalFormView)]
        public ListModel<ClientLegalForm> List([FromBody] ListQuery listQuery)
        {
            return new ListModel<ClientLegalForm>
            {
                List = _repository.List(listQuery),
                Count = _repository.Count(listQuery)
            };
        }

        [HttpPost("/api/ClientLegalForm/card"), Authorize(Permissions.ClientLegalFormView)]
        public ClientLegalForm Card([FromBody] int id)
        {
            if (id <= 0) throw new ArgumentOutOfRangeException(nameof(id));

            var ClientLegalForm = _repository.Get(id);
            if (ClientLegalForm == null) throw new InvalidOperationException();

            return ClientLegalForm;
        }

        [HttpPost("/api/ClientLegalForm/save"), Authorize(Permissions.ClientLegalFormManage)]
        [Event(EventCode.DictClientLegalFormSaved, EventMode = EventMode.Response)]
        public ClientLegalForm Save([FromBody] ClientLegalForm ClientLegalForm)
        {
            ModelState.Validate();

            if (ClientLegalForm.Id > 0)
            {
                _repository.Update(ClientLegalForm);
            }
            else
            {
                _repository.Insert(ClientLegalForm);
            }
            return ClientLegalForm;
        }

        [HttpPost("/api/ClientLegalForm/delete"), Authorize(Permissions.ClientLegalFormManage)]
        [Event(EventCode.DictClientLegalFormDeleted, EventMode = EventMode.Request)]
        public IActionResult Delete([FromBody] int id)
        {
            if (id <= 0) throw new ArgumentOutOfRangeException(nameof(id));

            _repository.Delete(id);
            return Ok();
        }
    }
}