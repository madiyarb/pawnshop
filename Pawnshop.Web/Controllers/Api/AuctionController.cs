using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Pawnshop.Core;
using Pawnshop.Data.Models.Auction;
using Pawnshop.Data.Models.Auction.Dtos.Amounts;
using Pawnshop.Data.Models.Auction.HttpRequestModels;
using Pawnshop.Services.Auction.HttpServices.Interfaces;
using Pawnshop.Services.Auction.Interfaces;

namespace Pawnshop.Web.Controllers.Api
{
    [Authorize(Permissions.AuctionManage)]
    [Route("api/auction")]
    [ApiController]
    public class AuctionController : Controller
    {
        private readonly ICalculationAuctionAmountsService _calculationAuctionAmounts;
        private readonly ICreateAuctionService _createAuctionService;
        private readonly IGetAuctionAmountsService _getAuctionAmountsService;
        private readonly IGetAuctionDebtAmounts _auctionDebtAmountsService;

        private readonly IAuctionOperationHttpService _auctionOperationHttpService;

        public AuctionController(
            ICalculationAuctionAmountsService calculationAuctionAmounts,
            ICreateAuctionService createAuctionService,
            IGetAuctionAmountsService getAuctionAmountsService,
            IAuctionOperationHttpService auctionOperationHttpService,
            IGetAuctionDebtAmounts auctionDebtAmountsService)
        {
            _calculationAuctionAmounts = calculationAuctionAmounts;
            _getAuctionAmountsService = getAuctionAmountsService;
            _auctionOperationHttpService = auctionOperationHttpService;
            _auctionDebtAmountsService = auctionDebtAmountsService;
            _createAuctionService = createAuctionService;
        }

        [HttpGet("get-amounts/{contractId:int}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(AuctionAmountsCompositeViewModel))]
        public async Task<IActionResult> GetContractAmounts(int contractId)
        {
            var result = await _getAuctionAmountsService.GetAmounts(contractId);
            return Ok(result);
        }

        [HttpPost("create")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(AuctionAmountsCompositeViewModel))]
        public async Task<IActionResult> Create([FromBody] CreateAuctionCommand request)
        {
            await _createAuctionService.Create(request);
            return Ok();
        }

        [HttpPost("calculate-amounts")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(AuctionAmountsCompositeViewModel))]
        public async Task<IActionResult> GetContractAmounts([FromBody] AuctionAmountsRequest request)
        {
            request.InputAmount ??= await _auctionDebtAmountsService.GetDebtAmount(request.ContractId);
            if (request.InputAmount <= 0)
            {
                return BadRequest(new { message = "Сумма ДКП должна быть положительным числом" });
            }
            var result = await _calculationAuctionAmounts.GetCalculatedAmounts(request);
            return Ok(result);
        }
        
        [HttpPost("approve-auction")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(AuctionAmountsCompositeViewModel))]
        public async Task<IActionResult> AuctionApprove([FromBody] ApproveAuctionCommand request, CancellationToken cancellationToken)
        {
            var result = await _auctionOperationHttpService.Approve(request, cancellationToken);
            return Ok(result);
        }
    }
}