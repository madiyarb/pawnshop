using System;
using System.Data.Common;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using DocumentFormat.OpenXml.Office2010.Excel;
using Microsoft.AspNetCore.Mvc;
using Pawnshop.Core;
using Pawnshop.Core.Exceptions;
using Pawnshop.Data.Access;
using Pawnshop.Data.Models.ClientsMobilePhoneContacts;
using Pawnshop.Data.Models.ClientsMobilePhoneContacts.Views;
using Pawnshop.Web.Models;
using Pawnshop.Web.Models.ClientsMobilePhoneContacts;
using Serilog;

namespace Pawnshop.Web.Controllers.Api
{
    [ApiController]
    [ProducesResponseType(typeof(ProblemDetails), 401)]
    public sealed class ClientsMobilePhoneContactsController : Controller
    {
        private readonly ILogger _logger;
        public ClientsMobilePhoneContactsController(ILogger logger)
        {
            _logger = logger;
        }

        [ProducesResponseType(typeof(Guid), 200)]
        [ProducesResponseType(typeof(ProblemDetails), 400)]
        [ProducesResponseType(typeof(string), 400)]
        [ProducesResponseType(typeof(string), 409)]
        [HttpPost("api/clients/{clientid}/mobilephonecontact")]
        public async Task<IActionResult> AddClientContact(
            [FromServices] ClientsMobilePhoneContactsRepository repository,
            [FromServices] ClientRepository clientRepository,
            [FromRoute] int clientid,
            [FromBody] ClientsMobilePhoneContactBinding binding)
        {
            var client = clientRepository.GetOnlyClient(clientid);
            if (client == null)
                return BadRequest($"Client with id : {clientid} not found.");

            var contact = repository.GetClientContact(clientid, binding.PhoneNumber, binding.Name);

            if (contact != null)
            {
                return Conflict(
                    $"Contract for client : {clientid} , with name {binding.Name} and phonenumber {binding.PhoneNumber}");
            }

            ClientsMobilePhoneContact newContact = new ClientsMobilePhoneContact(binding.PhoneNumber, binding.Name, clientid);

            repository.Insert(newContact);
            return Ok(newContact.Id);
        }


        [ProducesResponseType(typeof(string), 200)]
        [ProducesResponseType(typeof(ProblemDetails), 400)]
        [ProducesResponseType(typeof(string), 400)]
        [HttpPost("api/clients/{clientid}/mobilephonecontacts")]
        public async Task<IActionResult> AddClientContacts(
            [FromServices] ClientsMobilePhoneContactsRepository repository,
            [FromServices] ClientRepository clientRepository,
            [FromRoute] int clientid,
            [FromBody] ClientsMobilePhoneContactListBinding binding)
        {
            try 
            { 
                var client = clientRepository.GetOnlyClient(clientid);
                if (client == null)
                    return NotFound(new BaseResponseDRPP(HttpStatusCode.NotFound, $"Client {clientid} not found",
                        DRPPResponseStatusCode.ClientNotFound));

                var clientContacts = repository.GetClientsMobilePhoneContacts(clientid);

                var insertedList = binding.MobileContactsList
                    .Where(contact => !clientContacts.Any(existedContact =>
                        contact.Name == existedContact.Name && contact.PhoneNumber == existedContact.PhoneNumber));

                if (!insertedList.Any())
                {
                    return Ok();
                }

                foreach (var contact in insertedList)
                {
                    repository.Insert(new ClientsMobilePhoneContact(contact.PhoneNumber, contact.Name, clientid));
                }

                return Ok($"Added {insertedList.Count()} contacts");
            }
            catch (PawnshopApplicationException exception)
            {
                _logger.Error(exception, exception.Message);
                return StatusCode((int)HttpStatusCode.Locked,
                    new BaseResponseDRPP(HttpStatusCode.Locked, exception.Message,
                        DRPPResponseStatusCode.UnspecifiedPawnshopApplicationException));
            }
            catch (DbException exception)
            {
                _logger.Error(exception, exception.Message);
                return StatusCode((int)HttpStatusCode.InsufficientStorage,
                    new BaseResponseDRPP(HttpStatusCode.InsufficientStorage, exception.Message,
                        DRPPResponseStatusCode.UnspecifiedDatabaseProblems));
            }
            catch (Exception exception)
            {
                _logger.Error(exception, exception.Message);
                return StatusCode((int)HttpStatusCode.InternalServerError,
                    new BaseResponseDRPP(HttpStatusCode.InternalServerError, exception.Message,
                        DRPPResponseStatusCode.UnspecifiedProblem));
            }
        }


        [ProducesResponseType(typeof(ClientsMobilePhoneContactListView), 200)]
        [ProducesResponseType(typeof(ProblemDetails), 400)]
        [ProducesResponseType(typeof(ProblemDetails), 404)]
        [HttpGet("api/clients/{clientid}/mobilephonecontactslist")]
        public async Task<IActionResult> GetMobileContactsList(
            [FromServices] ClientsMobilePhoneContactsRepository repository,
            [FromRoute] int clientid,
            [FromQuery] PageBinding pageBinding)
        {
            var clientContacts = repository.GetClientsMobilePhoneContacts(clientid, pageBinding.Offset, pageBinding.Limit);
            if (clientContacts == null)
                return NotFound();

            if (clientContacts.Count == 0)
                return NotFound();

            return Ok(clientContacts);
        }

    }
}
