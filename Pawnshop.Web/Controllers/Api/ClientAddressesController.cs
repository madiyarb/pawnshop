using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Pawnshop.Core;
using Pawnshop.Core.Exceptions;
using Pawnshop.Data.Access;
using Pawnshop.Data.Models.Clients;
using Pawnshop.Web.Models;
using Pawnshop.Web.Models.ClientAddresses;

namespace Pawnshop.Web.Controllers.Api
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public sealed class ClientAddressesController : Controller
    {
        private readonly ISessionContext _sessionContext;
        public ClientAddressesController(ISessionContext sessionContext)
        {
            _sessionContext = sessionContext;
        }

        [HttpGet("clients/{id}/addresses/{clientaddressid}")]
        [ProducesResponseType(typeof(ClientAddress), 200)]
        [ProducesResponseType(typeof(BaseResponse), 404)]
        public async Task<IActionResult> GetAddress(
            [FromRoute] int id,
            [FromRoute] int clientaddressid,
            [FromServices] ClientRepository repository)
        {
            var client = repository.Get(id);
            if (client == null)
            {
                return NotFound(new BaseResponse(HttpStatusCode.NotFound, $"Клиент c идентификатором : {id} не найден"));
            }
            var addresses = client.Addresses.Where(address => address.DeleteDate == null).ToList();
            if (addresses.Count == 0)
                return NotFound(new BaseResponse(HttpStatusCode.NotFound, $"Не найдено адресов клиента"));

            var clientAddress = addresses.SingleOrDefault(address => address.Id == clientaddressid);

            if (clientAddress == null)
            {
                return NotFound(new BaseResponse(HttpStatusCode.NotFound, $"У клиента : {id} нету адресса с идентификатором {clientaddressid}"));
            }

            return Ok(clientAddress);
        }

        [HttpGet("clients/{id}/addresses/list")]
        [ProducesResponseType(typeof(List<ClientAddress>), 200)]
        [ProducesResponseType(typeof(BaseResponse), 404)]
        public async Task<IActionResult> GetAdressesList(
            [FromRoute] int id,
            [FromServices] ClientRepository repository)
        {
            var client = repository.Get(id);
            if (client == null)
            {
                return NotFound(new BaseResponse(HttpStatusCode.NotFound, $"Клиент c идентификатором : {id} не найден"));
            }
            var addresses = client.Addresses.Where(address => address.DeleteDate == null).ToList();
            if (addresses.Count == 0)
                return NotFound(new BaseResponse(HttpStatusCode.NotFound, $"Не найдено адресов клиента"));

            return Ok(addresses);
        }


        [HttpPost("clients/{id}/addresses")]
        [ProducesResponseType(204)]
        [ProducesResponseType(typeof(BaseResponse), 400)]
        [ProducesResponseType(typeof(BaseResponse), 404)]
        public async Task<IActionResult> CreateClientAddresses(
            [FromRoute] int id,
            [FromServices] ClientRepository repository,
            [FromBody] ClientAddressBinding binding)
        {
            var client = repository.Get(id);
            if (client == null)
            {
                return NotFound(new BaseResponse(HttpStatusCode.NotFound, $"Клиент c идентификатором : {id} не найден"));
            }

            try
            {
                var clientAddress = new ClientAddress(id, binding.AddressTypeId, binding.CountryId, binding.ATEId,
                    binding.GeonimId, binding.BuildingNumber, binding.RoomNumber,
                    _sessionContext.UserId, binding.IsActual, binding.Note);
                repository.InsertAddress(clientAddress);
            }
            catch (PawnshopApplicationException exception)
            {
                return BadRequest(new BaseResponse(HttpStatusCode.BadRequest, string.Join("\r\n", exception.Messages)));
            }
            return NoContent();
        }

        [HttpPost("clients/{id}/addresses/{clientaddressid}")]
        [ProducesResponseType(204)]
        [ProducesResponseType(typeof(BaseResponse), 400)]
        [ProducesResponseType(typeof(BaseResponse), 404)]
        public async Task<IActionResult> UpdateClientAddress(
            [FromRoute] int id,
            [FromRoute] int clientaddressid,
            [FromServices] ClientRepository repository,
            [FromBody] ClientAddressBinding binding)
        {
            var client = repository.Get(id);
            if (client == null)
            {
                return NotFound(new BaseResponse(HttpStatusCode.NotFound, $"Клиент c идентификатором : {id} не найден"));
            }
            var addresses = client.Addresses.Where(address => address.DeleteDate == null).ToList();
            if (addresses.Count == 0)
                return NotFound(new BaseResponse(HttpStatusCode.NotFound, $"Не найдено адресов клиента"));

            var clientAddress = addresses.FirstOrDefault(address => address.Id == clientaddressid);

            if (clientAddress == null)
            {
                return NotFound(new BaseResponse(HttpStatusCode.NotFound, $"Адресс клиента : {id} с идентификатором : {clientaddressid} не найден"));
            }

            try
            {
                clientAddress.Update(binding.AddressTypeId, binding.CountryId, binding.ATEId, binding.GeonimId,
                    binding.BuildingNumber, binding.RoomNumber, binding.IsActual, binding.Note);
                repository.UpdateAddress(clientAddress);
            }
            catch (PawnshopApplicationException exception)
            {
                return BadRequest(new BaseResponse(HttpStatusCode.BadRequest, string.Join("\r\n", exception.Messages)));
            }
            return NoContent();
        }


        [HttpDelete("clients/{id}/addresses/{clientaddressid}")]
        [ProducesResponseType(204)]
        [ProducesResponseType(typeof(BaseResponse), 404)]
        public async Task<IActionResult> DeleteClientAddress(
          [FromRoute] int id,
          [FromRoute] int clientaddressid,
          [FromServices] ClientRepository repository,
          [FromServices] ClientAddressesRepository addressesRepository)
        {
            var client = repository.Get(id);
            if (client == null)
            {
                return NotFound(new BaseResponse(HttpStatusCode.NotFound, $"Клиент c идентификатором : {id} не найден"));
            }
            var addresses = client.Addresses.Where(address => address.DeleteDate == null).ToList();
            if (addresses.Count == 0)
                return NotFound(new BaseResponse(HttpStatusCode.NotFound, $"Не найдено адресов клиента"));

            var clientAddress = addresses.FirstOrDefault(address => address.Id == clientaddressid);

            if (clientAddress == null)
            {
                return NotFound(new BaseResponse(HttpStatusCode.NotFound, $"Аддресс клиента : {id} с идентификатором : {clientaddressid} не найден"));
            }

            clientAddress.Delete();
            addressesRepository.Delete(clientAddress);

            return NoContent();
        }

        [HttpPost("clients/{id}/residenceaddresstype")]
        [ProducesResponseType(typeof(ResidenceAddressTypeView), 200)]
        [ProducesResponseType(typeof(BaseResponse), 404)]
        public async Task<IActionResult> SetResidenceAddressType(
            [FromRoute] int id,
            [FromServices] ClientProfileRepository repository,
            [FromServices] ClientRepository clientRepository,
            [FromBody] ResidenceAddressTypeBinding binding)
        {
            var profile = repository.Get(id);
            if (profile == null)
            {
                var client = clientRepository.GetOnlyClient(id);
                if (client == null)
                {
                    return NotFound(new BaseResponse(HttpStatusCode.NotFound, $"Не найден клиент с идентификатором {id}"));
                }
                repository.Insert(new ClientProfile
                {
                    ClientId = id
                });
                profile = repository.Get(id);
            }
            profile.SetResidenceAddressTypeId(binding.ResidenceAddressTypeId);
            repository.Update(profile);
            return Ok(new ResidenceAddressTypeView
            {
                ResidenceAddressTypeId = profile.ResidenceAddressTypeId
            });
        }

        [HttpGet("clients/{id}/residenceaddresstype")]
        [ProducesResponseType(typeof(ResidenceAddressTypeView), 200)]
        [ProducesResponseType(typeof(BaseResponse), 404)]
        public async Task<IActionResult> GetResidenceaddresstype(
            [FromRoute] int id,
            [FromServices] ClientProfileRepository repository)
        {
            var profile = repository.Get(id);
            return Ok(new ResidenceAddressTypeView
            {
                ResidenceAddressTypeId = profile.ResidenceAddressTypeId
            });
        }
    }
}
