using DocumentFormat.OpenXml.Office2010.ExcelAc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Pawnshop.Core;
using Pawnshop.Core.Exceptions;
using Pawnshop.Core.Queries;
using Pawnshop.Data.Access;
using Pawnshop.Data.Models.Audit;
using Pawnshop.Data.Models.Clients;
using Pawnshop.Data.Models.Collection;
using Pawnshop.Web.Engine;
using Pawnshop.Web.Engine.Middleware;
using Pawnshop.Web.Engine.Services;
using Pawnshop.Web.Models.Clients;
using Pawnshop.Web.Models.List;
using System;
using System.Collections.Generic;
using System.Linq;
using Pawnshop.Services.Collection.http;

namespace Pawnshop.Web.Controllers.Api
{
    [Route("api/collection/actions")]
    public class CollectionActionController : Controller
    {
        private readonly ICollectionHttpService<CollectionActions> _httpService;
        public CollectionActionController(ICollectionHttpService<CollectionActions> httpService)
        {
            _httpService = httpService;
        }

        [HttpPost("list"), ProducesResponseType(typeof(List<CollectionActions>), 200)]
        public ListModel<CollectionActions> List()
        {
            var list = _httpService.List().Result;;
            return new ListModel<CollectionActions>
            {
                List = list,
                Count = list.Count
            };
        }
        [HttpPost("save")]
        public IActionResult Save([FromBody] CollectionActions item)
        {
            ModelState.Validate();
            int result = 0;
            var itemAction = item.Id.HasValue ? _httpService.Get(item.Id.Value.ToString()).Result : null;
            if (itemAction == null)
                item.Id = _httpService.Create(item).Result;
            else
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
            //if (result == null) throw new InvalidOperationException();

            return Ok(result);
        }

        [HttpPost("delete")]
        public IActionResult Delete([FromBody] int id)
        {
            if (id <= 0) throw new ArgumentOutOfRangeException(nameof(id));

            var result = _httpService.Delete(id.ToString());

            return Ok();
        }
    }
}
