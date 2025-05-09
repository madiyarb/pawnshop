using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Pawnshop.Core;
using Pawnshop.Data.Models.DebtorRegistry.CourtOfficer;
using Pawnshop.Data.Models.LegalCollection;
using Pawnshop.Services.DebtorRegisrty.CourtOfficer.Interfaces;

namespace Pawnshop.Web.Controllers.Api
{
    [Authorize(Permissions.LegalCollectionView)]
    [Route("api/court-officer")]
    public class CourtOfficerController : Controller
    {
        private readonly IFilteredCourtOfficersService _filteredCourtOfficersService;

        public CourtOfficerController(IFilteredCourtOfficersService filteredCourtOfficersService)
        {
            _filteredCourtOfficersService = filteredCourtOfficersService;
        }

        [HttpPost("list")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(PagedResponse<CourtOfficerDto>))]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> List([FromBody] CourtOfficersQuery request)
        {
            var result = await _filteredCourtOfficersService.List(request);
            return Ok(result);
        }
    }
}