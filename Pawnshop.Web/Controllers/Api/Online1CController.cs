using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Pawnshop.Services.Integrations.Online1C;
using Pawnshop.Data.Models.Online1C;
using System.Threading.Tasks;

namespace Pawnshop.Web.Controllers.Online1C
{
    [Authorize]
    public class Online1CController : Controller
    {
        private readonly IOnline1CService _online1CService;

        public Online1CController(IOnline1CService online1CService)
        {
            _online1CService = online1CService;
        }

        [HttpPost]
        public async Task<IActionResult> SendReportManual([FromBody] Online1CReportData reportData)
        {
            var result = await _online1CService.SendReportManual(reportData);
            return Ok(result.Item2);
        }
    }
}