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
    [Authorize(Permissions.PayTypeView)]
    public class PayTypeController : Controller
    {
        private readonly PayTypeRepository _repository;
        private readonly ISessionContext _sessionContext;

        public PayTypeController(PayTypeRepository repository, ISessionContext sessionContext)
        {
            _repository = repository;
            _sessionContext = sessionContext;
        }

        [HttpPost]
        public ListModel<PayType> List([FromBody] ListQuery listQuery)
        {
            return new ListModel<PayType>
            {
                List = _repository.List(listQuery),
                Count = _repository.Count(listQuery)
            };
        }

        [HttpPost]
        public PayType Card([FromBody] int id)
        {
            if (id <= 0) throw new ArgumentOutOfRangeException(nameof(id));

            var model = _repository.Get(id);
            if (model == null) throw new InvalidOperationException();

            return model;
        }

        [HttpPost, Authorize(Permissions.PayTypeManage)]
        [Event(EventCode.DictPayTypeSaved, EventMode = EventMode.Response)]
        public PayType Save([FromBody] PayType model)
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

        [HttpPost, Authorize(Permissions.PayTypeManage)]
        [Event(EventCode.DictPayTypeDeleted, EventMode = EventMode.Request)]
        public IActionResult Delete([FromBody] int id)
        {
            if (id <= 0) throw new ArgumentOutOfRangeException(nameof(id));
            var count = _repository.RelationCount(id);
            if (count > 0)
            {
                throw new Exception("Невозможно удалить вид оплаты, так как он привязан к существующим действиям по договору");
            }

            _repository.Delete(id);
            return Ok();
        }
    }
}