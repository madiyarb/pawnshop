using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Pawnshop.Core;
using Pawnshop.Data.Models.Contracts;
using Pawnshop.Data.Models.Contracts.LegalCollectionCalculations;
using Pawnshop.Services.Contracts;
using Pawnshop.Services.LegalCollectionCalculations;
using System.Linq;
using System.Threading.Tasks;

namespace Pawnshop.Web.Controllers.Api
{
    [Route("api/legalAmounts")]
    [ApiController]
    public class LegalAmountsController : Controller
    {
        private readonly ICalculationLegalCollectionAmountsService _calculationLegalCollectionAmounts;
        private readonly ISessionContext _sessionContext;
        private readonly IContractService _contractService;

        public LegalAmountsController(
            ICalculationLegalCollectionAmountsService calculationLegalCollectionAmounts,
            ISessionContext sessionContext,
            IContractService contractService)
        {
            _calculationLegalCollectionAmounts = calculationLegalCollectionAmounts;
            _sessionContext = sessionContext;
            _contractService = contractService;
        }

        [HttpGet("calculate-legal-amounts/{contractId}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(LegalAmountsViewModel))]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> CalculateLegalAmounts([FromRoute] int contractId)
        {
            var currentContract = await _contractService.GetOnlyContractAsync(contractId);
            if (currentContract == null)
            {
                return NotFound();
            }

            var result = await _calculationLegalCollectionAmounts.CalculateLegalAmounts(currentContract);

            return Ok(result);
        }
    }
}
