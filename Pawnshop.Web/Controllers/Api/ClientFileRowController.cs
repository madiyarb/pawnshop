using System;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Pawnshop.Core;
using Pawnshop.Data.Access;
using Pawnshop.Data.Models.Clients;
using Pawnshop.Web.Engine;

namespace Pawnshop.Web.Controllers.Api
{
    [Authorize]
    public class ClientFileRowController : Controller
    {
        private readonly ClientFileRowRepository _repository;
        
        public ClientFileRowController(ClientFileRowRepository repository)
        {
            _repository = repository;
        }

        [HttpPost]
        public ClientFileRow Save([FromBody] ClientFileRow fileRow)
        {
            ModelState.Validate();
            _repository.Insert(fileRow);
            return fileRow;
        }

        [HttpPost]
        public IActionResult Delete([FromBody] ClientFileRow fileRow)
        {
            var model = _repository.Find(fileRow);
            if (model == null) throw new InvalidOperationException();

            _repository.Delete(model.Id);
            return Ok();            
        }
    }
}