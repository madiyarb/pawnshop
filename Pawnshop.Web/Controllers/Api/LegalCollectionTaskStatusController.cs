using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Pawnshop.Core;
using Pawnshop.Data.Models.LegalCollection;
using Pawnshop.Data.Models.LegalCollection.Dtos;
using Pawnshop.Services.LegalCollection.Inerfaces;

namespace Pawnshop.Web.Controllers.Api
{
    [Authorize(Permissions.LegalCollectionView)]
    [Route("api/legal-collection/task-status")]
    public class LegalCollectionTaskStatusController : Controller
    {
        private readonly ILegalCollectionTaskStatusService _legalCaseTaskStatusService;

        public LegalCollectionTaskStatusController(ILegalCollectionTaskStatusService legalCaseTaskStatusService)
        {
            _legalCaseTaskStatusService = legalCaseTaskStatusService;
        }
        
        [HttpPost("list")]
        [ProducesResponseType(typeof(PagedResponse<LegalCaseTaskStatusDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> List()
        {
            var result = await _legalCaseTaskStatusService.List();
            return Ok(result);
        }
    }
}