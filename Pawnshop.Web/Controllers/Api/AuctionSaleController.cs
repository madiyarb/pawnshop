using System;
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
    [Route("api/auction-sale")]
    public class AuctionSaleController : Controller
    {
        private readonly IRegisterAuctionSaleService _auctionSaleService;

        public AuctionSaleController(IRegisterAuctionSaleService auctionSaleService)
        {
            _auctionSaleService = auctionSaleService;
        }
        
        [HttpPost("register")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Create([FromBody] RegisterAuctionSaleRequest request)
        {
            try
            {
                await _auctionSaleService.RegisterAsync(request);
                return NoContent();
            }
            catch (PawnshopApplicationException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
            catch (ArgumentNullException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "Произошла непредвиденная ошибка", details = ex.Message });
            }
        }
    }
}