using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Pawnshop.Core;
using Pawnshop.Data.Access;
using Pawnshop.Data.Models.ApplicationOnlineRejectionReasons;
using Pawnshop.Data.Models.ApplicationOnlineRejectionReasons.Bindings;
using Pawnshop.Data.Models.ApplicationOnlineRejectionReasons.Views;
using Pawnshop.Services.Clients;
using Pawnshop.Web.Models;
using Pawnshop.Web.Models.ApplicationOnlineRejectionReasons;

namespace Pawnshop.Web.Controllers.Api
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public sealed class ApplicationOnlineRejectionReasonsController : Controller
    {
        public ApplicationOnlineRejectionReasonsController()
        {
            
        }
        [Authorize(Permissions.TasOnlineAdministrator)]
        [HttpPost("create")]
        [ProducesResponseType(typeof(int),200)]
        public async Task<IActionResult> CreateRejectionReason(
            [FromServices] ApplicationOnlineRejectionReasonsRepository repository,
            [FromBody] ApplicationOnlineRejectionReasonCreationBinding binding)
        {
            return Ok(
                await repository.Insert(new ApplicationOnlineRejectionReason(binding.Code,
                    binding.InternalReason, binding.ExternalReasonEn, binding.ExternalReasonKz, binding.ExternalReasonRu, 
                    binding.AvailableToChoiseForClient, binding.AvailableToChoiseForManager)));
        }

        [HttpGet("{id}")]
        [ProducesResponseType(204)]
        public async Task<IActionResult> Get(
            [FromRoute] int id,
            [FromServices] ApplicationOnlineRejectionReasonsRepository repository)
        {
            var rejectionReason = await repository.Get(id);
            if (rejectionReason == null)
            {
                return NotFound(new BaseResponse(HttpStatusCode.NotFound, $"Причина отказа с идентификатором {id} не найдена"));
            }
            return Ok(rejectionReason);
        }

        [Authorize(Permissions.TasOnlineAdministrator)]
        [HttpPost("{id}")]
        [ProducesResponseType(204)]
        public async Task<IActionResult> Update(
            [FromRoute] int id,
            [FromServices] ApplicationOnlineRejectionReasonsRepository repository,
            [FromBody] ApplicationOnlineRejectionReasonUpdatingBinding binding)
        {
            var rejectionReason = await repository.Get(id);
            if (rejectionReason == null)
            {
                return NotFound(new BaseResponse(HttpStatusCode.NotFound, $"Причина отказа с идентификатором {id} не найдена"));
            }
            rejectionReason.Update(binding.Code, binding.InternalReason, binding.ExternalReasonEn,
                binding.ExternalReasonKz, binding.ExternalReasonRu, binding.AvailableToChoiceForClient,
                binding.AvailableToChoiceForManager, binding.Enabled);
            await repository.Update(rejectionReason);
            return NoContent();
        }

        [Authorize(Permissions.TasOnlineAdministrator)]
        [HttpDelete("{id}")]
        [ProducesResponseType(204)]
        public async Task<IActionResult> Disable(
            [FromRoute] int id,
            [FromServices] ApplicationOnlineRejectionReasonsRepository repository)
        {
            var rejectionReason = await repository.Get(id);
            if (rejectionReason == null)
            {
                return NotFound(new BaseResponse(HttpStatusCode.NotFound, $"Причина отказа с идентификатором {id} не найдена"));
            }
            rejectionReason.Disable();
            await repository.Update(rejectionReason);
            return NoContent();
        }

        [HttpGet("list")]
        [ProducesResponseType(typeof(ApplicationOnlineRejectionReasonListView),200)]
        public async Task<IActionResult> GetList(
            [FromServices] ApplicationOnlineRejectionReasonsRepository repository,
            [FromQuery] ApplicationOnlineRejectionReasonQuery query,
            [FromQuery] PageBinding pageBinding)
        {
            return Ok(await repository.GetFiltredRejectionReasons(offset: pageBinding.Offset, limit: pageBinding.Limit,
                forClient: query.ForClient, forManager:query.ForManager, enabled: query.Enabled, code: query.Code  ));
        }

    }
}
