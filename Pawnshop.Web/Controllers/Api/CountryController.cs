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
    [Authorize(Permissions.CountryView)]
    public class CountryController : Controller
    {
        private readonly CountryRepository _repository;

        public CountryController(CountryRepository repository)
        {
            _repository = repository;
        }

        [HttpPost]
        public ListModel<Country> List([FromBody] ListQuery listQuery)
        {
            return new ListModel<Country>
            {
                List = _repository.List(listQuery),
                Count = _repository.Count(listQuery)
            };
        }

        [HttpPost]
        public Country Card([FromBody] int id)
        {
            if (id <= 0) throw new ArgumentOutOfRangeException(nameof(id));

            var Country = _repository.Get(id);
            if (Country == null) throw new InvalidOperationException();

            return Country;
        }

        [HttpPost, Authorize(Permissions.CountryManage)]
        [Event(EventCode.DictCountrySaved, EventMode = EventMode.Response)]
        public Country Save([FromBody] Country Country)
        {
            ModelState.Validate();

            if (Country.Id > 0)
            {
                _repository.Update(Country);
            }
            else
            {
                _repository.Insert(Country);
            }
            return Country;
        }

        [HttpPost, Authorize(Permissions.CountryManage)]
        [Event(EventCode.DictCountryDeleted, EventMode = EventMode.Request)]
        public IActionResult Delete([FromBody] int id)
        {
            if (id <= 0) throw new ArgumentOutOfRangeException(nameof(id));

            var count = _repository.RelationCount(id);
            if (count > 0)
            {
                throw new Exception("Невозможно удалить страну, так как она привязана к коду стран-производителей машин или клиенту или адресу клиента");
            }

            _repository.Delete(id);
            return Ok();
        }
    }
}