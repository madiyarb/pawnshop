using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Pawnshop.Core.Queries;
using Pawnshop.Data.Models.LegalCollection;
using Pawnshop.Data.Models.LegalCollection.DocumentType.HttpServie;
using Pawnshop.Data.Models.LegalCollection.Dtos;
using Pawnshop.Services.LegalCollection.Inerfaces;
using Pawnshop.Web.Models.List;

namespace Pawnshop.Web.Controllers.Api
{
    [Route("api/legal-collection/document-type")]
    public class LegalCollectionDocumentTypesController : Controller
    {
        private readonly ILegalCollectionDocumentTypeService _legalCollectionDocumentTypeService;

        public LegalCollectionDocumentTypesController(ILegalCollectionDocumentTypeService legalCollectionDocumentTypeService)
        {
            _legalCollectionDocumentTypeService = legalCollectionDocumentTypeService;
        }

        [HttpPost("create")]
        [ProducesResponseType(typeof(LegalCollectionDocumentTypeDto), StatusCodes.Status200OK)]
        public async Task<IActionResult> Create([FromBody] CreateDocumentTypeHttpRequest request)
        {
            var result = await _legalCollectionDocumentTypeService.Create(request);
            return Ok(result);
        }

        [HttpPost("details")]
        [ProducesResponseType(typeof(LegalCollectionDocumentTypeDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Details([FromBody] DetailsDocumentTypeHttRequest request)
        {
            var result = await _legalCollectionDocumentTypeService.Details(request);
            return Ok(result);
        }
        
        [HttpPost("list")]
        [ProducesResponseType(typeof(ListModel<LegalCollectionDocumentTypeDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> List([FromBody] ListQuery query)
        {
            var result = await _legalCollectionDocumentTypeService.List(query);
            return Ok(result);
        }
        
        [HttpPost("update")]
        [ProducesResponseType(typeof(LegalCollectionDocumentTypeDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Update([FromBody] UpdateDocumentTypeHttpRequest request)
        {
            var result = await _legalCollectionDocumentTypeService.Update(request);
            return Ok(result);
        }
        
        [HttpPost("delete")]
        [ProducesResponseType(typeof(LegalCollectionDocumentTypeDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Delete([FromBody] DeleteDocumentTypeHttpRequest request)
        {
            var result = await _legalCollectionDocumentTypeService.Delete(request);
            return Ok(result);
        }
    }
}