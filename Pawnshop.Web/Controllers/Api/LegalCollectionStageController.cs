using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Pawnshop.Core.Queries;
using Pawnshop.Data.Models.LegalCollection.Dtos;
using Pawnshop.Services.LegalCollection.HttpServices.Dtos.LegalCaseStage;
using Pawnshop.Web.Models.List;
using Pawnshop.Services.LegalCollection.HttpServices.Interfaces;

namespace Pawnshop.Web.Controllers.Api
{
    public class LegalCollectionStageController : Controller
    {
        private readonly ILegalCollectionStagesHttpService _legalCollectionStagesHttpService;

        public LegalCollectionStageController(ILegalCollectionStagesHttpService legalCollectionStagesHttpService)
        {
            _legalCollectionStagesHttpService = legalCollectionStagesHttpService;
        }
        
        [HttpPost("api/legal-collection-stage/create")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(LegalCaseStageDto))]
        public async Task<IActionResult> Create([FromBody] CreateLegalCaseStageCommand query)
        {
            var result = await _legalCollectionStagesHttpService.Create(query);
            return Ok(result);
        }
        
        [HttpPost("api/legal-collection-stage/details")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(LegalCaseStageDto))]
        public async Task<IActionResult> Get([FromBody] DetailsLegalCaseStageQuery query)
        {
            var result = await _legalCollectionStagesHttpService.Details(query);
            return Ok(result);
        }
        
        [HttpPost("api/legal-collection-stage/list")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(LegalCaseStagesList))]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> List([FromBody] ListQuery query)
        {
            if (query is null) query = new ListQuery();

            var result = await _legalCollectionStagesHttpService.List();
            return Ok(new ListModel<LegalCaseStageDto>()
            {
                Count = result.Count,
                List = result.List.Skip(query.Page.Offset).Take(query.Page.Limit).ToList()
            });
        }
        
        [HttpPost("api/legal-collection-stage/update")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(LegalCaseStageDto))]
        public async Task<IActionResult> Update([FromBody] UpdateLegalCaseStageCommand request)
        {
            var result = await _legalCollectionStagesHttpService.Update(request);
            return Ok(result);
        }
        
        [HttpPost("api/legal-collection-stage/delete")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(int))]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Delete([FromBody] DeleteLegalCaseStageCommand request)
        {
            var result = await _legalCollectionStagesHttpService.Delete(request);
            return Ok(result);
        }
    }
}