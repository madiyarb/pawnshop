using Microsoft.AspNetCore.Mvc;
using Pawnshop.Data.Models.AbsOnline;
using Pawnshop.Services.AbsOnline;
using System.Net;
using System.Threading.Tasks;

namespace Pawnshop.Web.Controllers.Api
{
    [Route("api/v2/absonline")]
    [ApiController]
    public class AbsOnlineContractsController : Controller
    {
        private readonly IAbsOnlineContractsService _absOnlineContractsService;

        public AbsOnlineContractsController(IAbsOnlineContractsService absOnlineContractsService)
        {
            _absOnlineContractsService = absOnlineContractsService;
        }

        /// <summary>
        /// Метод возвращает список займов и кредитных линий (контракт шины <b><u>mobile_get_contracts</u></b>)
        /// </summary>
        /// <param name="iin">ИИН субъекта</param>
        /// <param name="phone">Номер телефона</param>
        /// <returns>Список займов и кредитных линий</returns>
        [HttpGet("contracts")]
        public async Task<IActionResult> GetContracts([FromQuery] string iin, [FromQuery] string phone)
        {
            if (string.IsNullOrEmpty(iin))
                return BadRequest("Обязательные параметры не заполнены.");

            var contracts = await _absOnlineContractsService.GetContractsByIdentityNumberAsync(iin);

            return Ok(_absOnlineContractsService.GetViewListAsync(contracts));
        }

        /// <summary>
        /// Метод возвращает информацию о статусе залового автомобиля и историю ЧСИ траншей
        /// </summary>
        /// <param name="creditLineNumber">Номер кредитной линии</param>
        /// <returns>Информация о статусе залового автомобиля и историю ЧСИ траншей</returns>
        [HttpGet("creditline/parking-inscription-status")]
        public async Task<IActionResult> GetCreditLineParkingInscriptionStatusAsync(string creditLineNumber)
        {
            if (string.IsNullOrEmpty(creditLineNumber))
                return BadRequest("Номер кредитной линии не может быть пустым.");

            return Ok(await _absOnlineContractsService.GetCreditLineParkingInscriptionStatusAsync(creditLineNumber));
        }

        /// <summary>
        /// Метод возвращает список кредитных линий (контракт шины <b><u>get_credit_lines</u></b>)
        /// </summary>
        /// <param name="iin">ИИН субъекта</param>
        /// <param name="phone">Номер телефона</param>
        /// <returns>Список кредитных линий</returns>
        [HttpGet("creditlines")]
        public async Task<IActionResult> GetCreditLinesAsync([FromQuery] string iin, [FromQuery] string phone)
        {
            if (string.IsNullOrEmpty(iin))
                return BadRequest("Обязательные параметры не заполнены.");

            var contracts = await _absOnlineContractsService.GetContractsByIdentityNumberAsync(iin);

            return Ok(_absOnlineContractsService.GetCreditLineViewListAsync(contracts));
        }

        /// <summary>
        /// Метод шины mobile_contract_data
        /// </summary>
        [HttpGet("contracts/{contractnumber}")]
        [ProducesResponseType(typeof(AbsOnlineContractMobileView), 200)]
        [ProducesResponseType(typeof(ProblemDetails), 404)]
        public async Task<IActionResult> GetContractByContractNumber([FromRoute] string contractnumber)
        {
            var contractData = await _absOnlineContractsService.GetMobileContractData(contractnumber);

            if (contractData == null)
                return NotFound($"Займ с указанным номером {contractnumber} не найден.");

            return Ok(contractData);
        }

        /// <summary>
        /// Метод шины mobile_main_screen
        /// </summary>
        [HttpGet("contracts/{iin}/mobilemainscreenmodel")]
        [ProducesResponseType(typeof(MobileMainScreenView), 200)]
        [ProducesResponseType(typeof(ProblemDetails), 404)]
        public async Task<IActionResult> GetContractsMobileMainScreenModel([FromRoute] string iin, [FromQuery] string phone)
        {
            if (string.IsNullOrEmpty(iin))
                return BadRequest("ИИН обязателен к заполнению.");

            return Ok(await _absOnlineContractsService.GetMobileMainScreenView(iin));
        }
    }
}
