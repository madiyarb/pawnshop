using Microsoft.AspNetCore.Mvc;
using Pawnshop.Services.AbsOnline;
using System.Threading.Tasks;

namespace Pawnshop.Web.Controllers.Api
{
    [Route("api/v2/absonline")]
    [ApiController]
    public class AbsOnlineClientsController : Controller
    {
        private readonly IAbsOnlineClientsService _absOnlineClientsService;

        public AbsOnlineClientsController(IAbsOnlineClientsService absOnlineClientsService)
        {
            _absOnlineClientsService = absOnlineClientsService;
        }

        /// <summary>
        /// Метод возвращает активные залоговые объекты клиента и закрытые не более трех месяцев назад
        /// </summary>
        /// <param name="iin">ИИН клиента</param>
        /// <returns>Список залоговых объектов</returns>
        [HttpGet("clients/{iin}/positions")]
        public async Task<IActionResult> GetClientPositions(string iin)
        {
            if (string.IsNullOrEmpty(iin))
                return BadRequest("Обязательные параметры не заполнены.");

            return Ok(await _absOnlineClientsService.GetClientPositionsAsync(iin));
        }
    }
}
