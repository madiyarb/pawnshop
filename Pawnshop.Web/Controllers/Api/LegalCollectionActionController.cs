using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Pawnshop.Core.Queries;
using Pawnshop.Data.Models.LegalCollection.Dtos;
using Pawnshop.Services.LegalCollection;
using Pawnshop.Services.LegalCollection.HttpServices.Dtos.LegalCaseAction;
using Pawnshop.Web.Models.List;
using Pawnshop.Services.LegalCollection.HttpServices.Interfaces;

namespace Pawnshop.Web.Controllers.Api
{
    public class LegalCollectionActionController : Controller
    {
        private readonly ILegalCollectionActionsHttpService _legalCollectionActionsHttpService;

        public LegalCollectionActionController(ILegalCollectionActionsHttpService legalCollectionActionsHttpService)
        {
            _legalCollectionActionsHttpService = legalCollectionActionsHttpService;
        }
        
        [HttpPost("api/legal-collection-actions/create")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(LegalCaseActionDto))]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Create([FromBody] CreateLegalCaseActionsCommand query)
        {
            var result = await _legalCollectionActionsHttpService.Create(query);
            return Ok(result);
        }
        
        [HttpPost("api/legal-collection-actions/details")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(LegalCaseActionDto))]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Get([FromBody] LegalCaseActionDetailsQuery query)
        {
            var result = await _legalCollectionActionsHttpService.Details(query);
            return Ok(result);
        }
        
        [HttpPost("api/legal-collection-actions/list")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(LegalCaseActionsList))]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> List([FromBody] ListQuery query)
        {
            if (query is null) query = new ListQuery();

            var result = await _legalCollectionActionsHttpService.List();
            return Ok(new ListModel<LegalCaseActionDto>()
            {
                Count = result.Count,
                List = result.List.Skip(query.Page.Offset).Take(query.Page.Limit).ToList()
            });
        }
        
        [HttpPost("api/legal-collection-actions/update")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(int))]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Update([FromBody] UpdateLegalCaseActionsCommand request)
        {
            var result = await _legalCollectionActionsHttpService.Update(request);
            return Ok(result);
        }
        
        [HttpPost("api/legal-collection-actions/delete")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(int))]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Delete([FromBody] DeleteLegalCaseActionCommand request)
        {
            var result = await _legalCollectionActionsHttpService.Delete(request);
            return Ok(result);
        }
    }
}