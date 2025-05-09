using System;
using System.Collections.Generic;
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
    [Authorize(Permissions.ContractActionCheckView)]
    public class ContractActionCheckController : Controller
    {
        private readonly ContractActionCheckRepository _repository;
        private readonly ISessionContext _sessionContext;

        public ContractActionCheckController(ContractActionCheckRepository repository, ISessionContext sessionContext)
        {
            _repository = repository;
            _sessionContext = sessionContext;
        }

        [HttpPost]
        public ListModel<ContractActionCheck> List([FromBody] ListQuery listQuery)
        {
            return new ListModel<ContractActionCheck>
            {
                List = _repository.List(listQuery),
                Count = _repository.Count(listQuery)
            };
        }

        [HttpPost]
        public ContractActionCheck Card([FromBody] int id)
        {
            if (id <= 0) throw new ArgumentOutOfRangeException(nameof(id));

            var model = _repository.Get(id);
            if (model == null) throw new InvalidOperationException();

            return model;
        }

        [HttpPost, Authorize(Permissions.ContractActionCheckManage)]
        [Event(EventCode.DictContractActionCheckSaved, EventMode = EventMode.Response)]
        public ContractActionCheck Save([FromBody] ContractActionCheck model)
        {
            ModelState.Validate();

            if (model.Id > 0)
            {
                _repository.Update(model);
            }
            else
            {
                model.CreateDate = DateTime.Now;
                model.AuthorId = _sessionContext.UserId;
                _repository.Insert(model);
            }
            return model;
        }

        [HttpPost, Authorize(Permissions.ContractActionCheckManage)]
        [Event(EventCode.DictContractActionCheckDeleted, EventMode = EventMode.Request)]
        public IActionResult Delete([FromBody] int id)
        {
            if (id <= 0) throw new ArgumentOutOfRangeException(nameof(id));

            _repository.Delete(id);
            return Ok();
        }
    }
}