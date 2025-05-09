using System;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Pawnshop.Data.Access;
using Pawnshop.Services.DebtorRegistry;
using Pawnshop.Web.Models;
using Serilog;

namespace Pawnshop.Web.Controllers.Api
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public sealed class DebtorRegistryController : ControllerBase
    {
        private readonly ILogger _logger;

        public DebtorRegistryController(ILogger logger)
        {
            _logger = logger;
        }


        [HttpGet("client/{id}/refresh")]
        public async Task<IActionResult> RefreshInfo(
            [FromServices] IDebtorRegistryService debtorRegistryService,
            [FromServices] ClientRepository clientRepository,
            [FromRoute] int id,
            CancellationToken cancellationToken
        )
        {
            try
            {
                var client = clientRepository.GetOnlyClient(id);
                if (client == null)
                {
                    return NotFound(new BaseResponse(HttpStatusCode.NotFound,
                        $"Клиент с идентификатором {id} не найден"));
                }


                return Ok(await debtorRegistryService.GetInfoFromDebtorRegistry(client.IdentityNumber, client.Id,  cancellationToken));
            }
            catch (Exception exception)
            {
                _logger.Error(exception, exception.Message);
                return StatusCode((int)HttpStatusCode.InternalServerError,
                    new BaseResponse(HttpStatusCode.InternalServerError, exception.Message));
            }
        }
        [HttpGet("client/{id}")]
        public async Task<IActionResult> GetActualInfo(
            [FromServices] ClientDebtorRegistryDataRepository clientDebtorRegistryDataRepository,
            [FromServices] ClientDebtorRegistryRequestsRepository clientDebtorRegistryRequestsRepository,
            [FromServices] ClientRepository clientRepository,
            [FromRoute] int id,
            CancellationToken cancellationToken
        )
        {
            try
            {
                var client = clientRepository.GetOnlyClient(id);
                if (client == null)
                {
                    return NotFound(new BaseResponse(HttpStatusCode.NotFound,
                        $"Клиент с идентификатором {id} не найден"));
                }

                var request = await clientDebtorRegistryRequestsRepository.GetLastRequest(id);
                if (request == null)
                {
                    return NotFound(new BaseResponse(HttpStatusCode.NotFound,
                        $"Нет актуального запроса в реестр должников для клиента {id}"));
                }
                return Ok((await clientDebtorRegistryDataRepository.GetAllDataByRequestId(request.Id)).ToList());
            }
            catch (Exception exception)
            {
                _logger.Error(exception, exception.Message);
                return StatusCode((int)HttpStatusCode.InternalServerError,
                    new BaseResponse(HttpStatusCode.InternalServerError, exception.Message));
            }
        }

    }
}
