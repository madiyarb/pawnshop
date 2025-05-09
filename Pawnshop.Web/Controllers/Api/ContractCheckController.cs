using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Pawnshop.AccountingCore.Models;
using Pawnshop.Core;
using Pawnshop.Core.Queries;
using Pawnshop.Data.Access;
using Pawnshop.Data.Models.Audit;
using Pawnshop.Data.Models.Contracts;
using Pawnshop.Data.Models.Dictionaries;
using Pawnshop.Web.Engine;
using Pawnshop.Web.Engine.Middleware;
using Pawnshop.Web.Models.List;

namespace Pawnshop.Web.Controllers.Api
{
    [Authorize(Permissions.ContractCheckView)]
    public class ContractCheckController : Controller
    {
        private readonly ContractCheckRepository _repository;
        private readonly ISessionContext _sessionContext;

        public ContractCheckController(ContractCheckRepository repository, ISessionContext sessionContext)
        {
            _repository = repository;
            _sessionContext = sessionContext;
        }

        [HttpPost]
        public ListModel<ContractCheck> List([FromBody] ListQuery listQuery)
        {
            return new ListModel<ContractCheck>
            {
                List = _repository.List(listQuery),
                Count = _repository.Count(listQuery)
            };
        }

        [HttpPost]
        public ContractCheck Card([FromBody] int id)
        {
            if (id <= 0) throw new ArgumentOutOfRangeException(nameof(id));

            var model = _repository.Get(id);
            if (model == null) throw new InvalidOperationException();

            return model;
        }

        [HttpPost]
        public List<ContractCheck> Find(CollateralType collateralType)
        {
            return _repository.Find(new ListQuery() {Page = null}, new { CollateralType = collateralType });
        }

        [HttpPost, Authorize(Permissions.ContractCheckManage)]
        [Event(EventCode.DictContractCheckSaved, EventMode = EventMode.Response)]
        public ContractCheck Save([FromBody] ContractCheck model)
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

        [HttpPost, Authorize(Permissions.ContractCheckManage)]
        [Event(EventCode.DictContractCheckDeleted, EventMode = EventMode.Request)]
        public IActionResult Delete([FromBody] int id)
        {
            if (id <= 0) throw new ArgumentOutOfRangeException(nameof(id));

            _repository.Delete(id);
            return Ok();
        }
    }
}