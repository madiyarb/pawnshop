using Microsoft.AspNetCore.Mvc;
using Pawnshop.Core.Exceptions;
using Pawnshop.Data.Models.Collection;
using Pawnshop.Web.Engine;
using System.Collections.Generic;
using System;
using Pawnshop.Web.Models.List;
using Pawnshop.Services.Collection.http;

namespace Pawnshop.Web.Controllers.Api
{
    [Route("api/collection/status")]
    public class CollectionStatusController : Controller
    {
        private readonly ICollectionHttpService<CollectionStatus> _httpService;
        public CollectionStatusController(ICollectionHttpService<CollectionStatus> httpService)
        {
            _httpService = httpService;
        }

        [HttpPost("list"), ProducesResponseType(typeof(List<CollectionStatus>), 200)]
        public ListModel<CollectionStatus> List()
        {
            var list = _httpService.List().Result;

            return new ListModel<CollectionStatus>
            {
                List = list,
                Count = list.Count
            };
        }
        [HttpPost("save")]
        public IActionResult Save([FromBody] CollectionStatus item)
        {
            ModelState.Validate();
            int result = 0;
            var itemAction = item.Id.HasValue ? _httpService.Get(item.Id.Value.ToString()).Result : null;
            if (itemAction == null)
                item.Id = _httpService.Create(item).Result;
            else if (item.Id.HasValue)
                result = _httpService.Update(item).Result;

            if (result > 0 || item.Id > 0)
                itemAction = _httpService.Get(item.Id.Value.ToString()).Result;
            else
                throw new PawnshopApplicationException("Ошибка сервиса");

            return Ok(itemAction);
        }

        [HttpPost("card")]
        public IActionResult Card([FromBody] int id)
        {
            if (id <= 0) throw new ArgumentOutOfRangeException(nameof(id));

            var result = _httpService.Get(id.ToString()).Result;
            if (result == null) throw new InvalidOperationException();

            return Ok(result);
        }

        [HttpPost("delete")]
        public IActionResult Delete([FromBody] int id)
        {
            if (id <= 0) throw new ArgumentOutOfRangeException(nameof(id));

            var result = _httpService.Delete(id.ToString());

            return Ok(result);
        }
    }
}
