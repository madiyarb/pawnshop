using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Pawnshop.Core;
using Pawnshop.Data.Models.LegalCollection;
using Pawnshop.Data.Models.LegalCollection.Action;
using Pawnshop.Data.Models.LegalCollection.ChangeCourse;
using Pawnshop.Data.Models.LegalCollection.Create;
using Pawnshop.Data.Models.LegalCollection.Details;
using Pawnshop.Data.Models.LegalCollection.Documents;
using Pawnshop.Data.Models.LegalCollection.Dtos;
using Pawnshop.Data.Models.LegalCollection.PrintTemplates;
using Pawnshop.Services.CollateralTypes;
using Pawnshop.Data.Models.Regions;
using Pawnshop.Services.Dictionaries;
using Pawnshop.Services.LegalCollection.HttpServices.Interfaces;
using Pawnshop.Services.LegalCollection.Inerfaces;

namespace Pawnshop.Web.Controllers.Api
{
    [Authorize(Permissions.LegalCollectionView)]
    [Route("api/legal-collection")]
    public class LegalCollectionController : Controller
    {
        private readonly ILegalCollectionsFilteringService _legalCollectionsFilteringService;
        private readonly ICollateralTypeService _collateralTypeService;
        private readonly IGetRegionsService _getRegionsService;
        private readonly ILegalCollectionUpdateService _legalCollectionUpdateService;
        private readonly ILegalCollectionActionOptionsService _legalCollectionActionOptionsService;
        private readonly ILegalCollectionDetailsService _legalCollectionDetailsService;
        private readonly ILegalCollectionPrintTemplateService _legalCollectionPrintTemplateService;
        private readonly ILegalCollectionDocumentsService _legalCollectionDocumentsService;
        private readonly ILegalCollectionChangeCourseService _legalCollectionChangeCourseService;
        private readonly ILegalCaseHttpService _legalCaseHttpService;
        private readonly IHolidayService _holidayService;
        private readonly IContractExpensesService _contractExpensesService;
        private readonly ILegalCollectionCreateService _legalCollectionCreateService;
        
        public LegalCollectionController(
            IGetRegionsService getRegionsService,
            ILegalCollectionUpdateService legalCollectionUpdateService,
            ILegalCollectionActionOptionsService legalCollectionActionOptionsService,
            ILegalCollectionDetailsService legalCollectionDetailsService,
            ILegalCollectionsFilteringService legalCollectionsFilteringService,
            ILegalCollectionPrintTemplateService legalCollectionPrintTemplateService,
            ILegalCollectionDocumentsService legalCollectionDocumentsService,
            ILegalCollectionChangeCourseService legalCollectionChangeCourseService,
            ILegalCaseHttpService legalCaseHttpService,
            ICollateralTypeService collateralTypeService,
            IHolidayService holidayService,
            IContractExpensesService contractExpensesService,
            ILegalCollectionCreateService legalCollectionCreateService)
        {
            _getRegionsService = getRegionsService;
            _collateralTypeService = collateralTypeService;
            _legalCollectionUpdateService = legalCollectionUpdateService;
            _legalCollectionActionOptionsService = legalCollectionActionOptionsService;
            _legalCollectionDetailsService = legalCollectionDetailsService;
            _legalCollectionsFilteringService = legalCollectionsFilteringService;
            _legalCollectionPrintTemplateService = legalCollectionPrintTemplateService;
            _legalCollectionDocumentsService = legalCollectionDocumentsService;
            _legalCollectionChangeCourseService = legalCollectionChangeCourseService;
            _legalCaseHttpService = legalCaseHttpService;
            _holidayService = holidayService;
            _contractExpensesService = contractExpensesService;
            _legalCollectionCreateService = legalCollectionCreateService;
        }

        [HttpPost("details")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(List<LegalCasesDetailsViewModel>))]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Get([FromBody] LegalCaseDetailsQuery query)
        {
            var result = await _legalCollectionDetailsService.GetDetailsAsync(query);
            return Ok(result);
        }
        
        [HttpPost("action-options")]
        public async Task<IActionResult> GetActionOptions([FromBody] LegalCaseActionOptionsQuery request)
        {
            var result = await _legalCollectionActionOptionsService.GetActionOptionsAsync(request);
            return Ok(result);
        }

        [HttpPost("filters")]
        [ProducesResponseType(typeof(PagedResponse<LegalCasesViewModel>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetLegalCases([FromBody] LegalCasesQuery query)
        {
            var result = await _legalCollectionsFilteringService.GetFilteredAsync(query);
            return Ok(result);
        }

        [HttpPost("update")]
        public async Task<IActionResult> Update([FromBody] UpdateLegalCaseCommand request)
        {
            var result = await _legalCollectionUpdateService.UpdateLegalCase(request);
            return Ok(result);
        }

        [HttpPost("uploadDocument")]
        public async Task<IActionResult> UploadLegalCaseDocument([FromBody] UploadLegalCaseDocumentCommand command)
        {
            var result = await _legalCollectionDocumentsService.UploadDocumentAsync(command);
            return Ok(result);
        }

        [HttpPost("deleteDocument")]
        [Authorize(Permissions.LegalCollectionDeleteDocument)]
        public async Task<IActionResult> DeleteLegalCaseDocument([FromBody] DeleteLegalCaseDocumentCommand command)
        {
            var result = await _legalCollectionDocumentsService.DeleteDocumentAsync(command);
            return Ok(result);
        }

        [HttpPost("printTemplateList")]
        public async Task<IActionResult> GetPrintTemplateList([FromBody] PrintTemplateRequestModel request)
        {
            var result = await _legalCollectionPrintTemplateService.GetListAsync(request.ContractId);
            return Ok(result);
        }

        [HttpPost("printTemplateCard")]
        public async Task<IActionResult> GetPrintTemplateCard([FromBody] LegalCasePrintTemplateQuery query)
        {
            var result = await _legalCollectionPrintTemplateService.GetAsync(query);
            return Ok(result);
        }
        
        [HttpPost("rollback")]
        public async Task<IActionResult> Rollback([FromBody] LegalCaseDetailsQuery query)
        {
            var result = await _legalCaseHttpService.RollbackLegalCaseHttpRequest(query.LegalCaseId);
            return Ok(result);
        }
        
        [HttpPost("change-course-various")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(List<ChangeCourseActionDto>))]
        public async Task<IActionResult> GetChangeCourseVarious([FromBody] LegalCaseChangeCourseVariousQuery request)
        {
            var result = await _legalCollectionChangeCourseService.GetChangeCourseVariousAsync(request.LegalCaseId);
            return Ok(result);
        }
        
        [HttpPost("change-course")]
        public async Task<IActionResult> ChangeCourse([FromBody] ChangeLegalCaseCourseCommand request)
        {
            var result = await _legalCollectionChangeCourseService.ChangeCourseAsync(request);
            return Ok(result);
        }
        
        [HttpGet("collateral-type")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(List<ChangeCourseActionDto>))]
        public async Task<IActionResult> GetCollateralTypes()
        {
            var result = _collateralTypeService.GetCollateralTypes();
            return Ok(result);
        }
        
        [HttpPost("regions")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(List<Region>))]
        public async Task<IActionResult> GetRegions()
        {
            var result = await _getRegionsService.GetListAsync();
            return Ok(result);
        }
        
        [HttpPost("range-holidays")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(bool))]
        public async Task<IActionResult> GetHoliday([FromBody] GetRangeHolidaysRequest request)
        {
            var holidays = await _holidayService.GetRangeHolidaysAsync(request.DateFrom, request.DateUntil);
            if (holidays == null || holidays.Count == 0)
            {
                return NotFound();
            }
            
            return Ok(holidays);
        }
        
        [HttpPost("contract-expenses")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(List<ContractExpensesViewModel>))]
        public async Task<IActionResult> GetContractExpenses([FromBody] ContractIdQuery request)
        {
            var result = await _contractExpensesService.GetContractAdditionalExpensesAsync(request.ContractId);
            return Ok(result);
        }
        
        [HttpPost("create")]
        public async Task<IActionResult> Create([FromBody] CreateLegalCaseCommand command)
        {
            var legalCaseId = await _legalCollectionCreateService.CreateAsync(command);
            return Ok(legalCaseId);
        }
    }
}