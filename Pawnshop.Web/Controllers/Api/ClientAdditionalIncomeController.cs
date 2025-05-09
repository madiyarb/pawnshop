using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Pawnshop.Core;
using Pawnshop.Core.Exceptions;
using Pawnshop.Data.Models.Clients;
using Pawnshop.Services.Clients;
using Pawnshop.Web.Engine;
using Pawnshop.Web.Engine.Services.Interfaces;
using Pawnshop.Services.Models.Clients;
using System.Collections.Generic;
using System.Linq;
using Pawnshop.Data.Models.Clients.ClientIncomeHistory;
using System.Threading.Tasks;
using Pawnshop.Data.Models.Clients.ClientAdditionalIncomeHistory;


namespace Pawnshop.Web.Controllers.Api
{
    [Route("api/clients/{clientId:int}/additionalIncomes"), Authorize(Permissions.ClientView)]
    public class ClientAdditionalIncomeController : Controller
    {
        private readonly IClientAdditionalIncomeService _clientAdditionalIncomeService;
        private readonly IClientQuestionnaireService _clientQuestionnaireService;
        public ClientAdditionalIncomeController(IClientAdditionalIncomeService clientAdditionalIncomeService, IClientQuestionnaireService clientQuestionnaireService)
        {
            _clientAdditionalIncomeService = clientAdditionalIncomeService;
            _clientQuestionnaireService = clientQuestionnaireService;
        }

        /// <summary>
        /// Получает список дополнительных доходов клиента
        /// </summary>
        /// <param name="clientId">Идентификатор клиента</param>
        /// <returns></returns>
        [HttpPost("list"), ProducesResponseType(typeof(List<ClientAdditionalIncomeDto>), 200)]
        public IActionResult GetList([FromRoute] int clientId)
        {
            List<ClientAdditionalIncome> incomes = _clientAdditionalIncomeService.Get(clientId);
            return Ok(incomes.Select(income => new ClientAdditionalIncomeDto
            {
                Id = income.Id,
                TypeId = income.TypeId,
                Amount = income.Amount
            }));
        }

        /// <summary>
        /// Сохраняет список дополнительных доходов клиента
        /// </summary>
        /// <param name="clientId">Идентификатор клиента</param>
        /// <param name="request"></param>
        /// <returns></returns>
        [Authorize(Permissions.ClientManage)]
        [HttpPost("save"), ProducesResponseType(typeof(List<ClientAdditionalIncomeDto>), 200)]
        public IActionResult Save([FromRoute] int clientId, [FromBody] SaveClientAdditionalIncomesRequest request)
        {
            bool canFillQuestionnaire = _clientQuestionnaireService.CanFillQuestionnaire(clientId);
            if (!canFillQuestionnaire)
                throw new PawnshopApplicationException("Данному клиенту нельзя заполнять анкету");

            ModelState.Validate();
            List<ClientAdditionalIncome> incomes = _clientAdditionalIncomeService.Save(clientId, request.AdditionalIncomes);
            return Ok(incomes.Select(c => new ClientAdditionalIncomeDto
            {
                Id = c.Id,
                TypeId = c.TypeId,
                Amount = c.Amount
            }));
        }

        [HttpPost("filters")]
        public async Task<IActionResult> GetClientIncomeHistory([FromRoute] int clientId, [FromBody] ClientAdditionalIncomeHistoryQuery query)
        {
            var result = await _clientAdditionalIncomeService.GetHistoryFiltered(clientId, query);
            return Ok(result);
        }
    }
}
