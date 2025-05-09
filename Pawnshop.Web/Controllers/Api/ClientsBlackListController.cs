using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Pawnshop.Core;
using Pawnshop.Core.Queries;
using Pawnshop.Data.Access;
using Pawnshop.Data.Models.Audit;
using Pawnshop.Data.Models.Clients;
using Pawnshop.Data.Models.Dictionaries;
using Pawnshop.Services.Clients;
using Pawnshop.Web.Engine;
using Pawnshop.Web.Engine.Middleware;
using Pawnshop.Web.Engine.Services.Interfaces;
using Pawnshop.Web.Models.Clients;
using Pawnshop.Web.Models.List;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Pawnshop.Web.Controllers.Api
{
    [Route("api/clients/{clientId:int}/blackList")]
    public class ClientsBlackListController : Controller
    {
        private readonly IClientBlackListService _clientBlackListService;

        public ClientsBlackListController(IClientBlackListService clientBlackListService, ISessionContext sessionContext)
        {
            _clientBlackListService = clientBlackListService;
        }

        [HttpPost("addedList"), Authorize(Permissions.ClientBlackListView), ProducesResponseType(typeof(List<ClientsBlackListDto>), 200)]
        public IActionResult GetAddedList([FromRoute] int clientId)
        {
            List<ClientsBlackListDto> clientsBlackLists = _clientBlackListService.GetAddedList(clientId);
            return Ok(clientsBlackLists);
        }

        [HttpPost("displayList"), ProducesResponseType(typeof(List<ClientsBlackListDto>), 200)]
        public IActionResult GetDisplayedList([FromRoute] int clientId)
        {
            List<ClientsBlackListDisplayedDto> clientsBlackLists = _clientBlackListService.GetDisplayedList(clientId);
            return Ok(clientsBlackLists);
        }

        [HttpPost("save"), Authorize(Permissions.ClientBlackListManage), ProducesResponseType(typeof(List<ClientsBlackListDto>), 200)]
        public IActionResult SaveAddedList([FromRoute] int clientId, [FromBody] SaveClientBlackListRequest request)
        {
            ModelState.Validate();

            List<ClientsBlackList> clientsBlackList = _clientBlackListService.SaveBlackList(clientId, request.BlackList);

            var blackListSaved = clientsBlackList.Select(p => new ClientsBlackListDto
            {
                Id = p.Id,
                AddReason = p.AddReason,
                ReasonId = p.ReasonId,
                AddedAt = p.AddedAt,
                AddedBy = p.AddedBy,
                AddedFile = p.AddedFile,
                RemoveReason = p.RemoveReason,
                RemoveDate = p.RemoveDate,
                RemovedBy = p.RemovedBy,
                RemovedFile = p.RemovedFile
            });
            return Ok(blackListSaved);
        }
    }
}
