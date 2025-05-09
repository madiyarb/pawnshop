using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Pawnshop.Core;
using Pawnshop.Core.Exceptions;
using Pawnshop.Core.Queries;
using Pawnshop.Data.Models.Audit;
using Pawnshop.Data.Models.Dictionaries;
using Pawnshop.Services.Models.List;
using Pawnshop.Services.Realties;
using Pawnshop.Web.Engine;
using Pawnshop.Web.Engine.Middleware;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Pawnshop.Web.Controllers.Api
{
    [Authorize]
    public class RealtyController : Controller
    {
        private readonly IRealtyService _realtyService;

        public RealtyController(IRealtyService realtyService)
        {
            _realtyService = realtyService;
        }

        [HttpPost("/api/realty/list"), Authorize(Permissions.RealtyView)]
        public ListModel<Realty> List([FromBody] ListQuery listQuery)
        {
            return _realtyService.ListWithCount(listQuery);
        }

        [HttpPost("/api/realty/save"), Authorize(Permissions.RealtyManage)]
        [Event(EventCode.DictRealtySaved, EventMode = EventMode.Response, EntityType = EntityType.Position)]
        public IActionResult Save([FromBody] Realty realty)
        {
            ModelState.Validate();
            _realtyService.Validate(realty);

            try
            {
                return Ok(_realtyService.Save(realty));
            }
            catch (Exception e)
            {
                throw new PawnshopApplicationException(e.Message);
            }
        }

        [HttpPost("/api/realty/delete"), Authorize(Permissions.RealtyManage)]
        [Event(EventCode.DictRealtyDeleted, EventMode = EventMode.Request)]
        public IActionResult Delete([FromBody] int id)
        {
            if (id <= 0) throw new ArgumentOutOfRangeException(nameof(id));

            _realtyService.Delete(id);
            return Ok();
        }

        [HttpPost("/api/realty/card"), Authorize(Permissions.RealtyView)]
        public IActionResult Card([FromBody] int id)
        {
            if (id <= 0) throw new ArgumentOutOfRangeException(nameof(id));

            return Ok(_realtyService.Get(id));
        }
    }
}
