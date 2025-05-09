using System;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Pawnshop.Core;
using Pawnshop.Core.Exceptions;
using Pawnshop.Data.Models.Auction;
using Pawnshop.Services.Auction.Interfaces;

namespace Pawnshop.Web.Controllers.Api
{
    [Authorize(Permissions.LegalCollectionView)]
    [Route("api/auction-expense")]
    public class AuctionExpenseController : Controller
    {
        private readonly IRegisterAuctionExpenseService _registerAuctionExpenseService;

        public AuctionExpenseController(IRegisterAuctionExpenseService registerAuctionExpenseService)
        {
            _registerAuctionExpenseService = registerAuctionExpenseService;
        }

        [HttpPost("create")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Create([FromBody] RegisterAuctionExpenseRequest request)
        {
            try
            {
                await _registerAuctionExpenseService.RegisterAsync(request);
                return NoContent();
            }
            catch (PawnshopApplicationException e)
            {
                return StatusCode(StatusCodes.Status422UnprocessableEntity, $"Не удалось обработать данные: {e.Message}");
            }
            catch (Exception e)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, $"При обработке данных произошла ошибка: {e.Message}");
            }
        }
    }
}