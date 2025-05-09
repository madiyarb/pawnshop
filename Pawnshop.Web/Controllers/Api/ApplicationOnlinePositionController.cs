using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Pawnshop.Core;
using Pawnshop.Data.Access;
using Pawnshop.Web.Models.ApplicationOnlinePosition;

namespace Pawnshop.Web.Controllers.Api
{
    [Authorize]
    public class ApplicationOnlinePositionController : Controller
    {
        private readonly ISessionContext _sessionContext;

        public ApplicationOnlinePositionController(ISessionContext sessionContext)
        {
            _sessionContext = sessionContext;
        }
        [HttpGet("/api/appicaliononlineposition/{id}")]
        public async Task<IActionResult> Get(
            [FromRoute] Guid id,
            [FromServices] ApplicationOnlinePositionRepository repository
        )
        {
            return Ok(repository.Get(id));
        }

        [HttpPost("/api/appicaliononlineposition/{id}/update")]
        public async Task<IActionResult> Update(
            [FromRoute] Guid id,
            [FromBody] ApplicationOnlinePositionBinding binding,
            [FromServices] ApplicationOnlinePositionRepository repository,
            [FromServices] ApplicationOnlineRepository applicationOnlineRepository
        )
        {
            var applicationOnlinePosition = repository.Get(id);
            var applicationOnline =
                applicationOnlineRepository.GetByApplicationOnlinePositionId(applicationOnlinePosition.Id);

            if (!applicationOnline.CanEditing(_sessionContext.UserId))
            {
                return BadRequest($"Заявка принадлежит пользователю с id : {applicationOnline.ResponsibleManagerId} назначте заявку себя");
            }
            if (applicationOnlinePosition == null)
            {
                return NotFound();
            }
            applicationOnlinePosition.Update(binding.LoanCost, binding.EstimatedCost);
            repository.Update(applicationOnlinePosition);
            return Ok(applicationOnlinePosition);
        }
    }
}
