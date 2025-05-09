using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Pawnshop.Core.Queries;
using Pawnshop.Services.LegalCollection.HttpServices.Dtos.LegalCaseNotificationHistory;
using Pawnshop.Services.LegalCollection.Inerfaces;
using System.Threading.Tasks;

namespace Pawnshop.Web.Controllers.Api
{
    public class LegalCollectionNotificationHistoryController : Controller
    {
        private readonly ILegalCollectionNotificationHttpService _httpService;

        public LegalCollectionNotificationHistoryController(ILegalCollectionNotificationHttpService httpService)
        {
            _httpService = httpService;
        }

        [HttpPost("api/legal-collection-notificationHistory/pagedList")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ListQuery))]
        public async Task<IActionResult> List([FromBody] ListQuery query)
        {
            if (query is null) query = new ListQuery();
            var result = await _httpService.PagedList(query.Page.Offset, query.Page.Limit);
            return Ok(result);
        }

        [HttpPost("api/legal-collection-notificationHistory/list")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ListQuery))]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> List()
        {
            var result = await _httpService.List();
            return Ok(result);
        }

        [HttpPost("api/legal-collection-notificationHistory/update")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(int))]
        public async Task<IActionResult> Update([FromBody] UpdateLegalCaseNotificationHistoryCommand command)
        {
            var result = await _httpService.Update(command);
            return Ok(result);
        }
    }
}
