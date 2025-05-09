using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Pawnshop.Core.Queries;
using Pawnshop.Data.Models.LegalCollection.Dtos;
using Pawnshop.Services.LegalCollection;
using Pawnshop.Services.LegalCollection.HttpServices.Dtos;
using Pawnshop.Web.Models.List;
using Pawnshop.Services.LegalCollection.HttpServices.Interfaces;

namespace Pawnshop.Web.Controllers.Api
{
    public class LegalCollectionCourseController : Controller
    {
        private readonly ILegalCollectionCoursesHttpService _legalCollectionCoursesHttpService;

        public LegalCollectionCourseController(ILegalCollectionCoursesHttpService legalCollectionCoursesHttpService)
        {
            _legalCollectionCoursesHttpService = legalCollectionCoursesHttpService;
        }
        
        [HttpPost("api/legal-collection-course/create")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(LegalCaseCourseDto))]
        public async Task<IActionResult> Create([FromBody] CreateLegalCaseCourseCommand query)
        {
            var result = await _legalCollectionCoursesHttpService.Create(query);
            return Ok(result);
        }
        
        [HttpPost("api/legal-collection-course/details")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(LegalCaseCourseDto))]
        public async Task<IActionResult> Get([FromBody] LegalCaseCourseDetailsQuery query)
        {
            var result = await _legalCollectionCoursesHttpService.Details(query.LegalCaseCourseId);
            return Ok(result);
        }
        
        [HttpPost("api/legal-collection-course/list")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(LegalCaseCourseList))]
        public async Task<IActionResult> List([FromBody] ListQuery query)
        {
            if (query is null) query = new ListQuery();

            var result = await _legalCollectionCoursesHttpService.List();
            return Ok(new ListModel<LegalCaseCourseDto>()
            {
                Count = result.Count,
                List = result.List.Skip(query.Page.Offset).Take(query.Page.Limit).ToList()
            });
        }
        
        [HttpPost("api/legal-collection-course/update")]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Update([FromBody] UpdateLegalCaseCourseCommand request)
        {
            var result = await _legalCollectionCoursesHttpService.Update(request);
            return Ok(result);
        }
        
        [HttpPost("api/legal-collection-course/delete")]
        public async Task<IActionResult> Delete([FromBody] DeleteLegalCaseCourseCommand request)
        {
            var result = await _legalCollectionCoursesHttpService.Delete(request);
            return Ok(result);
        }
    }
}