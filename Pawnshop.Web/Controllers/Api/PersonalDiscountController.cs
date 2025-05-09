using System;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Pawnshop.Core;
using Pawnshop.Core.Queries;
using Pawnshop.Data.Access;
using Pawnshop.Data.Models.Audit;
using Pawnshop.Web.Engine;
using Pawnshop.Web.Engine.Middleware;
using Pawnshop.Web.Models.List;
using Pawnshop.Data.Models.Dictionaries;
using Pawnshop.Core.Exceptions;

namespace Pawnshop.Web.Controllers.Api
{
    [Authorize(Permissions.PersonalDiscountView)]
    public class PersonalDiscountController : Controller
    {
        private readonly ISessionContext _sessionContext;
        private readonly PersonalDiscountRepository _repository;

        public PersonalDiscountController(PersonalDiscountRepository repository, ISessionContext sessionContext)
        {
            _sessionContext = sessionContext;
            _repository = repository;
        }

        [HttpPost]
        public ListModel<PersonalDiscount> List([FromBody] ListQuery listQuery)
        {
            return new ListModel<PersonalDiscount>
            {
                List = _repository.List(listQuery),
                Count = _repository.Count(listQuery)
            };
        }

        [HttpPost]
        public PersonalDiscount Card([FromBody] int id)
        {
            if (id <= 0) throw new ArgumentOutOfRangeException(nameof(id));

            var PersonalDiscount = _repository.Get(id);
            if (PersonalDiscount == null) throw new InvalidOperationException();

            return PersonalDiscount;
        }

        [HttpPost, Authorize(Permissions.PersonalDiscountManage)]
        [Event(EventCode.PersonalDiscountSaved, EventMode = EventMode.Response)]
        public PersonalDiscount Save([FromBody] PersonalDiscount model)
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
    }
}