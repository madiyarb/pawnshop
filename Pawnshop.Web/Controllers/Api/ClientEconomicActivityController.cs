using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Pawnshop.Core;
using Pawnshop.Data.Models.Clients;
using Pawnshop.Services.Clients;
using Pawnshop.Web.Engine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Pawnshop.Web.Controllers.Api
{
    [Route("api/economicActivities/{clientId:int}"), Authorize(Permissions.ClientView)]
    public class ClientEconomicActivityController : Controller
    {
        private readonly IClientEconomicActivityService _clientEconomicActivityService;

        public ClientEconomicActivityController(IClientEconomicActivityService clientEconomicActivityService)
        {
            _clientEconomicActivityService = clientEconomicActivityService;
        }

        [HttpPost("list"), ProducesResponseType(typeof(List<ClientEconomicActivity>), 200)]
        public IActionResult List([FromRoute] int clientId) => Ok(_clientEconomicActivityService.GetList(clientId));

        [HttpPost("save"), Authorize(Permissions.ClientManage), ProducesResponseType(typeof(List<ClientEconomicActivity>), 200)]
        public IActionResult Save([FromRoute] int clientId, [FromBody] List<ClientEconomicActivity> clientEconomicActivitiesRequest)
        {
            ModelState.Validate();
            List<ClientEconomicActivity> clientEconomicActivities = _clientEconomicActivityService.Save(clientId, clientEconomicActivitiesRequest);
            return Ok(clientEconomicActivities);
        }
    }
}
