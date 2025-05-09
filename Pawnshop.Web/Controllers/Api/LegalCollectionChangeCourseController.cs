using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Pawnshop.Data.Models.LegalCollection.Dtos;
using Pawnshop.Services.LegalCollection.HttpServices.Interfaces;
using Pawnshop.Services.LegalCollection.Inerfaces;

namespace Pawnshop.Web.Controllers.Api
{
    [Route("api/legal-collection-change-course")]
    public class LegalCollectionChangeCourseController : Controller
    {
        private readonly ILegalCollectionChangeCourseHttpService _changeCourseHttpService;

        public LegalCollectionChangeCourseController(ILegalCollectionChangeCourseHttpService changeCourseHttpService)
        {
            _changeCourseHttpService = changeCourseHttpService;
        }
        
        [HttpPost("list")]
        [ProducesResponseType(typeof(List<ChangeCourseActionDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> List()
        {
            var result = await _changeCourseHttpService.List();
            return Ok(result);
        }
    }
}
