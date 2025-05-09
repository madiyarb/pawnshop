using System;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Pawnshop.Core;
using Pawnshop.Core.Queries;
using Pawnshop.Data.Access;
using Pawnshop.Data.Models.Audit;
using Pawnshop.Data.Models.Insurances;
using Pawnshop.Services.Models.List;
using Pawnshop.Web.Engine;
using Pawnshop.Web.Engine.Middleware;

namespace Pawnshop.Web.Controllers.Api
{
    [Authorize(Permissions.InsuranceRateView)]
    public class InsuranceRateController : Controller
    {
        private readonly InsuranceRateRepository _repository;
        private readonly ISessionContext _sessionContext;

        public InsuranceRateController(InsuranceRateRepository repository, ISessionContext sessionContext)
        {
            _repository = repository;
            _sessionContext = sessionContext;
        }

        [HttpPost]
        public ListModel<InsuranceRate> List([FromBody] ListQuery listQuery)
        {
            return new ListModel<InsuranceRate>
            {
                List = _repository.List(listQuery),
                Count = _repository.Count(listQuery)
            };
        }

        [HttpPost]
        public InsuranceRate Card([FromBody] int id)
        {
            if (id <= 0) throw new ArgumentOutOfRangeException(nameof(id));

            var model = _repository.Get(id);
            if (model == null) throw new InvalidOperationException();

            return model;
        }

        [HttpPost, Authorize(Permissions.InsuranceRateManage)]
        [Event(EventCode.DictInsuranceRateSaved, EventMode = EventMode.Response)]
        public InsuranceRate Save([FromBody] InsuranceRate model)
        {
            ModelState.Validate();

            if (model.Id > 0)
                _repository.Update(model);
            else
            {
                model.CreateDate = DateTime.Now;
                model.AuthorId = _sessionContext.UserId;
                _repository.Insert(model);
            }
            return model;
        }

        [HttpPost, Authorize(Permissions.InsuranceRateManage)]
        [Event(EventCode.DictInsuranceRateDeleted, EventMode = EventMode.Request)]
        public IActionResult Delete([FromBody] int id)
        {
            if (id <= 0) throw new ArgumentOutOfRangeException(nameof(id));

            _repository.Delete(id);
            return Ok();
        }
    }
}