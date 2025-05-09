using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Pawnshop.Data.Access;
using Pawnshop.Data.Models.ApplicationOnlineLog.Views;
using System.Threading.Tasks;
using System;
using Pawnshop.Web.Models;

namespace Pawnshop.Web.Controllers.Api
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class ClientLogItemsController : Controller
    {
        [HttpGet("{id}/log")]
        [ProducesResponseType(typeof(ApplicationOnlineLogItemListView), 200)]
        public async Task<IActionResult> GetList(
            [FromRoute] int id,
            [FromServices] ClientLogItemsRepository repository,
            [FromQuery] PageBinding pageBinding
        )
        {
            return Ok(await repository.GetListView(id, pageBinding.Offset, pageBinding.Limit));
        }

    }
}
