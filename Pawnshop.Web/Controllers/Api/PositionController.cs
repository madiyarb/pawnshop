using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Pawnshop.Core.Queries;
using Pawnshop.Core;
using Pawnshop.Data.Models.Dictionaries;
using Pawnshop.Services.Positions;
using System.Threading.Tasks;
using DocumentFormat.OpenXml.Drawing.Charts;
using System.Collections.Generic;
using Pawnshop.Web.Engine;

namespace Pawnshop.Web.Controllers.Api
{
    [Authorize]
    public class PositionController : Controller
    {
        private readonly IPositionService _positionService;
        public PositionController(IPositionService positionService) 
        {
            _positionService = positionService;
        }

        [HttpPost("/api/positions/additionalInfo"), Authorize(Permissions.ContractView)]
        public async Task<IActionResult> AdditionalInfo([FromBody] int positionId)
        {
            return Ok(await _positionService.GetPositionAdditionalInfo(positionId));
        }

        [HttpPost("/api/positions/activeContracts"), Authorize(Permissions.ContractView)]
        public async Task<IActionResult> ActiveContractsForPosition([FromBody] IList<int> positionsIds)
        { 

            ModelState.Validate();
            return Ok(await _positionService.GetActiveContractsForPosition(positionsIds));
        }
    }
}
