using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Pawnshop.Services.Auction.Mapping.Interfaces;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Pawnshop.Core;

// todo удалить после успешного маппинга
namespace Pawnshop.Controllers
{
    [ApiController]
    [Authorize(Permissions.LegalCollectionView)]
    [Route("api/auction-mapping")]
    public class AuctionMappingController : ControllerBase
    {
        private readonly IAuctionMappingService _mappingService;

        public AuctionMappingController(IAuctionMappingService mapperService)
        {
            _mappingService = mapperService;
        }

        [HttpPost("car")]
        public async Task<IActionResult> MapCarFile(IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                return BadRequest("Файл не загружен.");
            }

            var isSuccess = await _mappingService.HandleCarFile(file);

            if (isSuccess)
            {
                return Ok("Файл обработан успешно. Данные сохранены.");
            }

            throw new System.Exception("Во время обработки файла произошла ошибка");
        }

        [HttpPost("expense")]
        public async Task<IActionResult> MapExpenseFile(IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                return BadRequest("Файл не загружен.");
            }

            var isSuccess = await _mappingService.HandleExpenseFile(file);

            if (isSuccess)
            {
                return Ok("Файл обработан успешно. Данные сохранены.");
            }

            throw new System.Exception("Во время обработки файла произошла ошибка");
        }
    }
}
