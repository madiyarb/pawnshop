using System.Collections.Generic;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Pawnshop.Data.Access;
using Pawnshop.Data.Models.ClientSUSNStatuses.Views;
using Pawnshop.Services.SUSN;
using Pawnshop.Web.Models;

namespace Pawnshop.Web.Controllers.Api
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class SUSNRequestsController : ControllerBase
    {
        private readonly ClientRepository _clientRepository;
        private readonly SUSNRequestsRepository _susnRequestsRepository;
        private readonly ClientSUSNStatusesRepository _clientSusnStatusesRepository;
        public SUSNRequestsController(SUSNRequestsRepository susnRequestsRepository,
            ClientSUSNStatusesRepository clientSusnStatusesRepository,
            ClientRepository clientRepository)
        {
            _susnRequestsRepository = susnRequestsRepository;
            _clientRepository = clientRepository;
            _clientSusnStatusesRepository = clientSusnStatusesRepository;
        }
        [HttpGet("{id}")]
        [ProducesResponseType( 200)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> GetSusnStatus(
            [FromRoute] int id)
        {
            var client = _clientRepository.GetOnlyClient(id);

            if (client == null)
            {
                return NotFound(new BaseResponse(HttpStatusCode.NotFound,
                    $"Статус сусн с идентификатором {id} не найден"));
            }

            var susnRequest =  await _susnRequestsRepository.GetLastRequestByClientId(id);
            if (susnRequest == null)
            {
                return NotFound(new BaseResponse(HttpStatusCode.NotFound,
                    $"Для клиента {client.Id} статус СУСН ни разу не запрашивался. Либо ни разу не завершился успешно"));
            }

            if (!susnRequest.AnyAsp)
            {
                return Ok(new ClientSUSNStatusesView
                {
                    AnySUSNStatus = false,
                    Count = 0,
                    List = new List<ClientSUSNStatusView>(),
                });
            }

            return Ok(await _clientSusnStatusesRepository.GetStatusesView(id, susnRequest.Id));
        }

        [HttpGet("{id}/refreshSUSNStatus")]
        public async Task<IActionResult> GetSusn([FromRoute] int id,
            [FromServices] ClientRepository clientRepository,
            [FromServices] ITasLabSUSNService sUSNService,
            CancellationToken cancellationToken)
        {
            var client = clientRepository.Get(id);
            await sUSNService.GetSUSNStatus(client.IdentityNumber, client.Id, cancellationToken);
            return NoContent();
        }
    }
}
