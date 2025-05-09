using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Pawnshop.Core;
using Pawnshop.Core.Impl;
using Pawnshop.Data.Access;
using Pawnshop.Data.Models.Clients;
using Pawnshop.Web.Models;
using Pawnshop.Web.Models.ClientAddresses;
using Pawnshop.Web.Models.ClientFamilyStatus;

namespace Pawnshop.Web.Controllers.Api
{
    [ApiController]
    [Authorize]
    [Route("api/[controller]")]
    public class ClientFamilyStatusController : Controller
    {
        private readonly ISessionContext _sessionContext;
        public ClientFamilyStatusController(ISessionContext sessionContext)
        {
            _sessionContext = sessionContext;
        }

        [HttpPost("clients/{id}/ClientFamilyStatus")]
        [ProducesResponseType(typeof(ClientFamilyStatusView), 200)]
        [ProducesResponseType(typeof(BaseResponse), 404)]
        public async Task<IActionResult> SetClientFamilyStatus(
            [FromRoute] int id,
            [FromServices] ClientProfileRepository repository,
            [FromServices] ClientRepository clientRepository,
            [FromBody] ClientFamilyStatusBinding binding)
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
                    ClientId = id,
                    AuthorId = _sessionContext.UserId
                });
                profile = repository.Get(id);
            }
            profile.SetFamilyStatus(binding.MaritalStatusId, binding.SpouseFullname, binding.SpouseIncome,
                binding.ChildrenCount, binding.AdultDependentsCount, binding.UnderageDependentsCount);
            repository.Update(profile);
            return Ok(new ClientFamilyStatusView
            {
                MaritalStatusId = profile.MaritalStatusId,
                SpouseFullname = profile.SpouseFullname,
                AdultDependentsCount = profile.AdultDependentsCount,
                ChildrenCount = profile.ChildrenCount,
                SpouseIncome = profile.SpouseIncome,
                UnderageDependentsCount = profile.UnderageDependentsCount
            });
        }

        [HttpGet("clients/{id}/ClientFamilyStatus")]
        [ProducesResponseType(typeof(ClientFamilyStatusView), 200)]
        [ProducesResponseType(typeof(BaseResponse), 404)]
        public async Task<IActionResult> GetClientFamilyStatus(
            [FromRoute] int id,
            [FromServices] ClientProfileRepository repository)
        {
            var profile = repository.Get(id);
            if (profile == null)
            {
                return NotFound(new BaseResponse(HttpStatusCode.NotFound, $"Не найден клиентский профиль с идентификатором {id}"));
            }
            return Ok(new ClientFamilyStatusView
            {
                MaritalStatusId = profile.MaritalStatusId,
                SpouseFullname = profile.SpouseFullname,
                AdultDependentsCount = profile.AdultDependentsCount,
                ChildrenCount = profile.ChildrenCount,
                SpouseIncome = profile.SpouseIncome,
                UnderageDependentsCount = profile.UnderageDependentsCount
            });
        }
    }
}
