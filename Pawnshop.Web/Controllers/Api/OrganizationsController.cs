using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Pawnshop.Data.Access;
using Pawnshop.Data.Models.ApplicationsOnline.Views;
using Pawnshop.Data.Models.Membership;
using Pawnshop.Services.CreditLines;

namespace Pawnshop.Web.Controllers.Api
{
    [Route("api/[controller]")]
    [ApiController]
    public sealed class OrganizationsController : Controller
    {
        [HttpGet("{name}")]
        [ProducesResponseType(typeof(Organization), 200)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> GetOrganization(
            [FromRoute] string name,
            [FromServices] OrganizationRepository repository)
        {
            return Ok(await repository.GetByName(name));
        }
    }
}
