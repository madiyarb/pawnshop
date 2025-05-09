using System;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Pawnshop.Core;
using Pawnshop.Data.Access;
using Pawnshop.Services.ApplicationOnlineRefinances;
using Pawnshop.Services.TasOnlinePermissionValidator;
using Pawnshop.Web.Models;
using Pawnshop.Web.Models.ApplicationOnlineRefinances;

namespace Pawnshop.Web.Controllers.Api
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class ApplicationOnlineRefinancesController : Controller
    {
        private readonly ISessionContext _sessionContext;
        private readonly ITasOnlinePermissionValidatorService _permissionValidator;
        public ApplicationOnlineRefinancesController(ISessionContext sessionContext, ITasOnlinePermissionValidatorService permissionValidator)
        {
            _sessionContext = sessionContext;
            _permissionValidator = permissionValidator;
        }

        [HttpGet("applications/{id}")]
        [ProducesResponseType( 200)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> GetApplicationOnline(
            [FromRoute] Guid id,
            [FromServices] IApplicationOnlineRefinancesService service)
        {
            return Ok(await service.GetRefinancedContractInfo(id));
        }

        [HttpPost("applications/{id}")]
        [ProducesResponseType(200)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> UpdateApplicationsOnlineRefinances(
            [FromRoute] Guid id,
            [FromBody] ApplicationOnlineRefinancesBinding binding,
            [FromServices] IApplicationOnlineRefinancesService service)
        {
            await service.UpdateApplicationOnlineRefinancesList(id, binding.RefinancedContracts.ToList());
            return NoContent();
        }

        [HttpPost("applications/{id}/correctApplicationAmountForRefinance")]
        [ProducesResponseType(200)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> CorrectApplicationAmountForRefinance(
            [FromRoute] Guid id,
            [FromServices] IApplicationOnlineRefinancesService service,
            [FromServices] ApplicationOnlineRepository applicationOnlineRepository)
        {
            var applicationOnline = applicationOnlineRepository.Get(id);
            if (!_permissionValidator.ValidateApplicationOnlineStatusWithUserRole(applicationOnline))
            {
                return StatusCode((int)HttpStatusCode.Forbidden,
                    new BaseResponse(HttpStatusCode.Forbidden, "У Вас не достаточно прав."));
            }
            await service.CorrectApplicationAmountSumForRefinancedContracts(id, _sessionContext.UserId);
            return NoContent();
        }

        [Authorize(Permissions.TasOnlineManager)]
        [HttpPost("applications/{id}/movePrepaymentForRefinancedContracts")]
        [ProducesResponseType(200)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> MovePrepaymentForRefinancedContracts(
            [FromRoute] Guid id,
            [FromServices] IApplicationOnlineRefinancesService service,
            [FromServices] ApplicationOnlineRepository repository)
        {
            var application = repository.Get(id);
            if (application.ContractId.HasValue)
            {
               var success =  await service.MovePrepaymentForRefinance(application.ContractId.Value, application.ContractBranchId.Value);
               if (success)
               {
                   return NoContent();
               }
            }
            else
            {
                return BadRequest();
            }

            return NoContent();
        }

    }
}
