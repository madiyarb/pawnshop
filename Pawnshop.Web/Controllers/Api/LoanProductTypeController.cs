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
    [Authorize(Permissions.LoanProductTypeView)]
    public class LoanProductTypeController : Controller
    {
        private readonly LoanProductTypeRepository _repository;
        private readonly ISessionContext _sessionContext;

        public LoanProductTypeController(LoanProductTypeRepository repository, ISessionContext sessionContext)
        {
            _repository = repository;
            _sessionContext = sessionContext;
        }

        [HttpPost]
        public ListModel<LoanProductType> List([FromBody] ListQuery listQuery)
        {
            return new ListModel<LoanProductType>
            {
                List = _repository.List(listQuery),
                Count = _repository.Count(listQuery)
            };
        }

        [HttpPost]
        public LoanProductType Card([FromBody] int id)
        {
            if (id <= 0) throw new ArgumentOutOfRangeException(nameof(id));

            var model = _repository.Get(id);
            if (model == null) throw new InvalidOperationException();

            return model;
        }

        [HttpPost, Authorize(Permissions.LoanProductTypeManage)]
        [Event(EventCode.DictLoanProductTypeSaved, EventMode = EventMode.Response)]
        public LoanProductType Save([FromBody] LoanProductType model)
        {
            ModelState.Validate();

            if (model.Id > 0)
            {
                var count = _repository.RelationCount(model.Id);
                if (count > 0)
                {
                    throw new Exception("Невозможно внести изменения, так как уже используется");
                }
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

        [HttpPost, Authorize(Permissions.LoanProductTypeManage)]
        [Event(EventCode.DictLoanProductTypeDeleted, EventMode = EventMode.Request)]
        public IActionResult Delete([FromBody] int id)
        {
            if (id <= 0) throw new ArgumentOutOfRangeException(nameof(id));
            var count = _repository.RelationCount(id);
            if (count > 0)
            {
                throw new Exception("Невозможно удалить, так как уже используется");
            }

            _repository.Delete(id);
            return Ok();
        }
    }
}