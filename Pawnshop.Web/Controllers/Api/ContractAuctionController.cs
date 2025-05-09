using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Pawnshop.Data.Models;
using System.Threading.Tasks;
using Pawnshop.Services.Auction.Interfaces;

namespace Pawnshop.Web.Controllers.Api
{
    [Route("api/contract-auction")]
    public class ContractAuctionController : Controller
    {
        private readonly ICarAuctionService _carAuctionService;

        public ContractAuctionController(ICarAuctionService carAuctionService)
        {
            _carAuctionService = carAuctionService;
        }

        [HttpPost("create")]
        public async Task<IActionResult> Create([FromBody] CarAuction carAuction)
        {
            var id = await _carAuctionService.CreateAsync(carAuction);
            return Ok(id);
        }
    }

}