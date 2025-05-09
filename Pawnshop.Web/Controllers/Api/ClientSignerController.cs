using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Pawnshop.Data.Models.Clients;
using Pawnshop.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Pawnshop.Services.Models.List;
using Pawnshop.Core.Queries;
using Pawnshop.Web.Engine;
using Microsoft.AspNetCore.Authorization;
using Pawnshop.Core;
using Pawnshop.Core.Exceptions;
using Pawnshop.Services.Clients;

namespace Pawnshop.Web.Controllers.Api
{
    [Route("api/clientSigners/{clientId:int}"), Authorize(Permissions.ClientView)]
    public class ClientSignerController : Controller
    {
        private readonly IClientSignerService _clientSignerService;

        public ClientSignerController(IClientSignerService clientSignerService)
        {
            _clientSignerService = clientSignerService;
        }

        [HttpPost("list"), ProducesResponseType(typeof(List<ClientSigner>), 200)]
        public IActionResult List([FromRoute] int clientId) => Ok(_clientSignerService.GetList(clientId));

        [HttpPost("save"), Authorize(Permissions.ClientManage), ProducesResponseType(typeof(List<ClientSigner>), 200)]
        public IActionResult Save([FromRoute] int clientId, [FromBody] List<ClientSigner> companySignersRequest)
        {
            ModelState.Validate();
            List<ClientSigner> companySigners = _clientSignerService.Save(clientId, companySignersRequest);
            return Ok(companySigners);
        }
    }
}
