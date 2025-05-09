using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Pawnshop.Core.Queries;
using Pawnshop.Data.Models.LegalCollection.Dtos.LegalCase;
using Pawnshop.Services.LegalCollection.HttpServices.Dtos.LegalCaseNotificationTemplate;
using Pawnshop.Services.LegalCollection.Inerfaces;
using Pawnshop.Web.Models.List;
using System.Linq;
using System.Threading.Tasks;

namespace Pawnshop.Web.Controllers.Api
{
    public class LegalCollectionNotificationTemplateController : Controller
    {
        private readonly ILegalCollectionNotificationTemplateHttpService _httpService;

        public LegalCollectionNotificationTemplateController(ILegalCollectionNotificationTemplateHttpService httpService)
        {
            _httpService = httpService;
        }

        [HttpPost("api/legal-collection-notificationTemplate/create")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(LegalCaseNotificationTemplateDto))]
        public async Task<IActionResult> Create([FromBody] CreateLegalCaseNotificationTemplateCommand command)
        {
            var result = await _httpService.Create(command);
            return Ok(result);
        }

        [HttpPost("api/legal-collection-notificationTemplate/card")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(LegalCaseNotificationTemplateDto))]
        public async Task<IActionResult> Card([FromBody] LegalCaseNotificationTemplateCardQuery request)
        {
            var result = await _httpService.Card(request);
            return Ok(result);
        }

        [HttpPost("api/legal-collection-notificationTemplate/update")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(LegalCaseNotificationTemplateDto))]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Update([FromBody] UpdateLegalCaseNotificationTemplateCommand query)
        {
            var result = await _httpService.Update(query);
            return Ok(result);
        }

        [HttpPost("api/legal-collection-notificationTemplate/delete")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(int))]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Delete([FromBody] DeleteLegalCaseNotificationTemplateCommand request)
        {
            var result = await _httpService.Delete(request);
            return Ok(result);
        }

        [HttpPost("api/legal-collection-notificationTemplate/list")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ListModel<LegalCaseNotificationTemplateDto>))]
        public async Task<IActionResult> List([FromBody] ListQuery query)
        {
            if (query is null) query = new ListQuery();
            var resultList = await _httpService.List();
            return Ok(new ListModel<LegalCaseNotificationTemplateDto>
            {
                List = resultList.Skip(query.Page.Offset).Take(query.Page.Limit).ToList(),
                Count = resultList.Count
            });
        }
    }
}
