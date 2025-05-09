using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Pawnshop.Core;
using Pawnshop.Data.Models.DebtorRegistry;
using Pawnshop.Data.Models.LegalCollection;
using Pawnshop.Services.DebtorRegisrty.Dtos;
using Pawnshop.Services.DebtorRegisrty.HttpService;
using Pawnshop.Services.DebtorRegisrty.Interfaces;

namespace Pawnshop.Web.Controllers.Api
{
    [Authorize(Permissions.LegalCollectionView)]
    [Route("api/debt-register")]
    public class DebtRegisterController : Controller
    {
        private readonly IFilteredDebtRegistryService _debtRegisterService;
        private readonly IDebtorRegisterDetailsService _debtorRegisterDetailsService;

        public DebtRegisterController(
            IFilteredDebtRegistryService debtRegisterService,
            IDebtorRegisterDetailsService debtorRegisterDetailsService
            )
        {
            _debtRegisterService = debtRegisterService;
            _debtorRegisterDetailsService = debtorRegisterDetailsService;
        }
        
        [HttpPost("filters")]
        [ProducesResponseType(typeof(PagedResponse<DebtRegistriesViewModel>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetDebtRegistry([FromBody] DebtorRegistriesQuery query)
        {
            var result = await _debtRegisterService.GetFilteringAsync(query);
            return Ok(result);
        }
        
        [HttpPost("details")]
        [ProducesResponseType(typeof(List<DebtorDetailsResponseDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> Details([FromQuery] string iin)
        {
            var result = await _debtorRegisterDetailsService.Details(iin);
            return Ok(result);
        }
    }
}