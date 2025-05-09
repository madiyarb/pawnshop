using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Pawnshop.Core;
using Pawnshop.Data.Access;
using Pawnshop.Data.Models.ApplicationOnlineInsurances;
using Pawnshop.Services.ApplicationsOnline;
using Pawnshop.Services.Insurance;
using Pawnshop.Web.Models;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using System;
using Pawnshop.Services.TasOnlinePermissionValidator;

namespace Pawnshop.Web.Controllers.Api
{
    [ApiController]
    [Authorize]
    [Route("api/[controller]")]
    public class ApplicationOnlineInsurancesController : Controller
    {
        private readonly IApplicationOnlineService _applicationOnlineService;
        private readonly ISessionContext _sessionContext;
        private readonly ITasOnlinePermissionValidatorService _permissionValidator;

        public ApplicationOnlineInsurancesController(
            IApplicationOnlineService applicationOnlineService,
            ITasOnlinePermissionValidatorService permissionValidator,
            ISessionContext sessionContext)
        {
            _applicationOnlineService = applicationOnlineService;
            _sessionContext = sessionContext;
            _permissionValidator = permissionValidator;
        }

        [HttpPost("applications/{id}")]
        [ProducesResponseType(typeof(BaseResponse), 404)]
        [ProducesResponseType(typeof(BaseResponse), 400)]
        [ProducesResponseType(204)]
        public async Task<IActionResult> Create([FromRoute] Guid id,
            [FromServices] ApplicationOnlineInsuranceRepository insuranceRepository,
            [FromServices] ApplicationOnlineRepository applicationOnlineRepository,
            [FromServices] IInsurancePremiumCalculator insurancePremiumCalculator)
        {
            var application = applicationOnlineRepository.Get(id);
            if (application == null)
            {
                return NotFound(new BaseResponse(HttpStatusCode.NotFound, $"Заявка {id} не найдена"));
            }

            if (!_permissionValidator.ValidateApplicationOnlineStatusWithUserRole(application))
            {
                return StatusCode((int)HttpStatusCode.Forbidden,
                    new BaseResponse(HttpStatusCode.Forbidden, "У Вас не достаточно прав."));
            }

            var insurance = await insuranceRepository.GetByApplicationId(id);
            if (insurance != null)
            {
                return BadRequest(new BaseResponse(HttpStatusCode.BadRequest, $"Страховка для заявки {id}. Уже предоставлена"));
            }

            var product = await _applicationOnlineService.GetProduct(application.ProductId, true);
            if (product == null)
            {
                return BadRequest(new BaseResponse(HttpStatusCode.BadRequest, $"У продукта нет возможности получить страховку"));
            }

            var maxApplicationAmount = await _applicationOnlineService.GetMaxApplicationAmount(application.Id, application);
            var maxApplicationAmountWithoutPremium = insurancePremiumCalculator.GetLoanCostWithoutInsurancePremium(maxApplicationAmount);

            if (application.ApplicationAmount > maxApplicationAmountWithoutPremium)
                application.ApplicationAmount = maxApplicationAmountWithoutPremium;

            await _applicationOnlineService.CreateInsurance(application, application.ApplicationAmount, product, _sessionContext.UserId);

            return NoContent();
        }

        [HttpDelete("applications/{id}")]
        [ProducesResponseType(typeof(BaseResponse), 404)]
        [ProducesResponseType(204)]
        public async Task<IActionResult> Delete([FromRoute] Guid id,
            [FromServices] ApplicationOnlineInsuranceRepository insuranceRepository,
            [FromServices] ApplicationOnlineRepository applicationOnlineRepository)
        {
            var application = applicationOnlineRepository.Get(id);
            if (application == null)
            {
                return NotFound(new BaseResponse(HttpStatusCode.NotFound, $"Заявка {id} не найдена"));
            }

            if (!_permissionValidator.ValidateApplicationOnlineStatusWithUserRole(application))
            {
                return StatusCode((int)HttpStatusCode.Forbidden,
                    new BaseResponse(HttpStatusCode.Forbidden, "У Вас не достаточно прав."));
            }

            var insurance = await insuranceRepository.GetByApplicationId(id);
            if (insurance == null)
            {
                return NotFound(new BaseResponse(HttpStatusCode.NotFound, $"Страховка для заявки {id} не найдена"));
            }

            var product = await _applicationOnlineService.GetProduct(application.ProductId, false);
            if (product == null)
            {
                return BadRequest(new BaseResponse(HttpStatusCode.BadRequest, $"У продукта нет возможности убрать страховку"));
            }

            await _applicationOnlineService.DeleteInsurance(application, insurance, _sessionContext.UserId);

            return NoContent();
        }

        [HttpGet("applications/{id}")]
        [ProducesResponseType(typeof(BaseResponse), 404)]
        [ProducesResponseType(204)]
        public async Task<IActionResult> Get([FromRoute] Guid id,
            [FromServices] ApplicationOnlineInsuranceRepository repository)
        {
            var insurance = await repository.GetViewByApplicationId(id);
            if (insurance == null)
            {
                return NotFound(new BaseResponse(HttpStatusCode.NotFound, $"Страховка для заявки {id} не найдена"));
            }

            return Ok(insurance);
        }

        [HttpGet("ApplicationOnlineInsuranceStatuses/list")]
        [ProducesResponseType(typeof(List<EnumView>), 200)]
        public async Task<IActionResult> GetApplicationOnlineInsuranceStatuses()
        {
            List<EnumView> estimationStatuses = new List<EnumView>();

            foreach (ApplicationOnlineInsuranceStatus status in Enum.GetValues(typeof(ApplicationOnlineInsuranceStatus)))
            {
                estimationStatuses.Add(new EnumView
                {
                    Id = (int)status,
                    Name = status.ToString(),
                    DisplayName = status.GetDisplayName()
                });
            }

            return Ok(estimationStatuses);
        }
    }
}
