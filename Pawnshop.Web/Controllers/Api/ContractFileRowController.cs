using System;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Pawnshop.Core;
using Pawnshop.Data.Access;
using Pawnshop.Data.Models.Contracts;
using Pawnshop.Web.Engine;

namespace Pawnshop.Web.Controllers.Api
{
    [Authorize(Permissions.ContractManage)]
    public class ContractFileRowController : Controller
    {
        private readonly ContractFileRowRepository _repository;

        public ContractFileRowController(ContractFileRowRepository repository)
        {
            _repository = repository;
        }

        [HttpPost]
        public ContractFileRow Save([FromBody] ContractFileRow fileRow)
        {
            ModelState.Validate();
            _repository.Insert(fileRow);
            return fileRow;
        }

        [HttpPost]
        public IActionResult Delete([FromBody] ContractFileRow fileRow)
        {
            var model = _repository.Find(fileRow);
            if (model == null) throw new InvalidOperationException();

            _repository.Delete(model.Id);
            return Ok();            
        }
    }
}