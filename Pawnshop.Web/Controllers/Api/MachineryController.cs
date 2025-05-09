using System;
using System.Collections.Generic;
using Microsoft.Data.SqlClient;
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
using Pawnshop.Services.Models.List;
using Pawnshop.Services.Cars;

namespace Pawnshop.Web.Controllers.Api
{
    [Authorize]
    public class MachineryController : Controller
    {
        private readonly IMachineryService _machineryService;

        public MachineryController(IMachineryService machineryService)
        {
            _machineryService = machineryService;
        }

        [HttpPost, Authorize(Permissions.MachineryView)]
        public ListModel<Machinery> List([FromBody] ListQuery listQuery)
        {
            return _machineryService.ListWithCount(listQuery);
        }

        [HttpPost, Authorize(Permissions.MachineryView)]
        public Machinery Card([FromBody] int id)
        {
            if (id <= 0) throw new ArgumentOutOfRangeException(nameof(id));

            return _machineryService.Get(id);
        }

        [HttpPost, Authorize(Permissions.MachineryManage)]
        [Event(EventCode.DictMachinerySaved, EventMode = EventMode.Response)]
        public Machinery Save([FromBody] Machinery entity)
        {
            ModelState.Validate();
            _machineryService.Validate(entity);

            try
            {
                _machineryService.Save(entity);
            }
            catch (SqlException e)
            {
                if (e.Number == 2627)
                {
                    throw new PawnshopApplicationException("Поле номер техпаспорта должно быть уникальным");
                }
                throw new PawnshopApplicationException(e.Message);
            }
            catch (Exception e)
            {
                throw new PawnshopApplicationException(e.Message);
            }

            return entity;
        }

        [HttpPost, Authorize(Permissions.MachineryManage)]
        [Event(EventCode.DictMachineryDeleted, EventMode = EventMode.Request)]
        public IActionResult Delete([FromBody] int id)
        {
            if (id <= 0) throw new ArgumentOutOfRangeException(nameof(id));

            _machineryService.Delete(id);
            return Ok();
        }

        [HttpPost]
        public List<string> Colors()
        {
            return _machineryService.Colors();
        }
    }
}
