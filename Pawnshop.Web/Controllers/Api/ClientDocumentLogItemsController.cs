using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Pawnshop.Data.Access;
using System.Threading.Tasks;
using Pawnshop.Web.Models;
using Pawnshop.Data.Models.ClientDocumentLogItems.Views;

namespace Pawnshop.Web.Controllers.Api
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class ClientDocumentLogItemsController : Controller
    {
        [HttpGet("{id}/log")]
        [ProducesResponseType(typeof(ClientDocumentLogListItemView), 200)]
        public async Task<IActionResult> GetList(
            [FromRoute] int id,
            [FromServices] ClientDocumentLogItemsRepository repository,
            [FromQuery] PageBinding pageBinding
        )
        {
            return Ok(await repository.GetListView(id, pageBinding.Offset, pageBinding.Limit));
        }

    }
}
