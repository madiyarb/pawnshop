using System;
using System.Data.Common;
using System.Net;
using System.Threading.Tasks;
using DocumentFormat.OpenXml.Office2010.Excel;
using Microsoft.AspNetCore.Mvc;
using Pawnshop.Core.Exceptions;
using Pawnshop.Data.Access;
using Pawnshop.Data.Models.ClientsGeoPositions;
using Pawnshop.Data.Models.ClientsGeoPositions.Views;
using Pawnshop.Web.Models;
using Pawnshop.Web.Models.ClientsGeoPositions;

namespace Pawnshop.Web.Controllers.Api
{
    [ApiController]
    public class ClientsGeoPositionsController : Controller
    {
        public ClientsGeoPositionsController()
        {
            
        }

        [ProducesResponseType(typeof(Guid), 200)]
        [ProducesResponseType(typeof(ProblemDetails), 400)]
        [ProducesResponseType(typeof(string), 400)]
        [HttpPost("api/clients/{clientid}/geoposition")]
        public async Task<IActionResult> AddClientGeoPosition(
            [FromServices] ClientsGeoPositionsRepository repository,
            [FromServices] ClientRepository clientRepository,
            [FromRoute] int clientid,
            [FromBody] ClientGeoPositionBinding binding)
        {
            try 
            { 
                var client = clientRepository.GetOnlyClient(clientid);
                if (client == null)
                    return BadRequest(new BaseResponseDRPP(HttpStatusCode.BadRequest, $"Client {clientid} not found",
                        DRPPResponseStatusCode.ClientNotFound));
                ClientGeoPosition clientGeoPosition = new ClientGeoPosition(binding.Latitude, binding.Longitude,clientid, binding.Date);
                await repository.Insert(clientGeoPosition);
                return Ok(clientGeoPosition.Id);
            }
            catch (PawnshopApplicationException exception)
            {
                return StatusCode((int)HttpStatusCode.Locked,
                    new BaseResponseDRPP(HttpStatusCode.Locked, exception.Message,
                        DRPPResponseStatusCode.UnspecifiedPawnshopApplicationException));
            }
            catch (DbException exception)
            {
                return StatusCode((int)HttpStatusCode.InsufficientStorage,
                    new BaseResponseDRPP(HttpStatusCode.InsufficientStorage, exception.Message,
                        DRPPResponseStatusCode.UnspecifiedDatabaseProblems));
            }
            catch (Exception exception)
            {
                return StatusCode((int)HttpStatusCode.InternalServerError,
                    new BaseResponseDRPP(HttpStatusCode.InternalServerError, exception.Message,
                        DRPPResponseStatusCode.UnspecifiedProblem));
            }
        }

        [ProducesResponseType(typeof(Guid), 200)]
        [ProducesResponseType(typeof(ProblemDetails), 400)]
        [ProducesResponseType(typeof(ProblemDetails), 404)]
        [HttpGet("api/clients/{clientid}/geoposition/last")]
        public async Task<IActionResult> GetLastClientGeoPosition(
            [FromRoute] int clientid,
            [FromServices] ClientsGeoPositionsRepository repository)
        {
            var clientGeoPosition = repository.GetLastClientGeoPosition(clientid);

            if (clientGeoPosition == null)
                return NotFound();

            return Ok(clientGeoPosition);
        }


        [ProducesResponseType(typeof(ClientGeoPositionListView), 200)]
        [ProducesResponseType(typeof(ProblemDetails), 400)]
        [ProducesResponseType(typeof(ProblemDetails), 404)]
        [HttpGet("api/clients/{clientid}/geopositions/list")]
        public async Task<IActionResult> GetClientGeoPositionsList(
            [FromRoute] int clientid,
            [FromQuery] PageBinding pageBinding,
            [FromServices] ClientsGeoPositionsRepository repository)
        {
            var geoPositions = repository.GetClientGeoPositions(clientid, pageBinding.Offset, pageBinding.Limit);
            if (geoPositions == null)
                return NotFound();

            if (geoPositions.Count == 0)
                return NotFound();

            return Ok(geoPositions);
        }
    }
}
