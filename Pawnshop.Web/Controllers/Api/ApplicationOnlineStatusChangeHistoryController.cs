using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Pawnshop.Data.Models.ApplicationsOnline;
using Pawnshop.Web.Models;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;
using Pawnshop.Data.Access;
using Pawnshop.Data.Models.ApplicationOnlineStatusChangeHistories.Views;

namespace Pawnshop.Web.Controllers.Api
{
    [ApiController]
    [Authorize]
    public class ApplicationOnlineStatusChangeHistoryController : Controller
    {
        [HttpGet("/api/applicationonline/{id}/history")]
        [ProducesResponseType(typeof(ApplicationOnlineStatusChangeHistoryView), 200)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> GetApplicationOnlineStatusHistory(
            [FromRoute] Guid id,
            [FromQuery] PageBinding pageBinding,
            [FromServices] ApplicationOnlineStatusChangeHistoryRepository repository)
        {
            var view = repository.GetListView(id, pageBinding.Offset, pageBinding.Limit);
            if (view == null)
            {
                return NotFound();
            }

            return Ok(view);
        }
    }
}
