using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Pawnshop.Core;
using Pawnshop.Core.Exceptions;
using Pawnshop.Data.Models.Clients;
using Pawnshop.Data.Models.Clients.ClientIncomeHistory;
using Pawnshop.Services.Clients;
using Pawnshop.Services.Models.Clients;
using Pawnshop.Web.Engine;
using Pawnshop.Web.Engine.Services.Interfaces;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Pawnshop.Web.Controllers.Api
{
    [Route("api/clients/{clientId:int}/clientIncomes"), Authorize(Permissions.ClientView)]
    public class ClientIncomeController : Controller
    {
        private readonly IClientIncomeService _clientIncomeService;
        private readonly IClientQuestionnaireService _clientQuestionnaireService;

        public ClientIncomeController(IClientIncomeService clientIncomeService,
                                      IClientQuestionnaireService clientQuestionnaireService
            )
        {
            _clientIncomeService = clientIncomeService;
            _clientQuestionnaireService = clientQuestionnaireService;
        }

        /// <summary>
        /// Получает список официальных доходов клиента
        /// </summary>
        /// <param name="clientId">Идентификатор клиента</param>
        /// <returns></returns>
        [HttpPost("incomeFormalList"), ProducesResponseType(typeof(List<ClientIncomeDto>), 200)]
        public IActionResult GetIncomeOfficialList([FromRoute] int clientId)
        {
            if (clientId == default)
                throw new PawnshopApplicationException("Выберите клиента");

            List<ClientIncome> incomes = _clientIncomeService.GetClientIncomes(clientId, (int)IncomeType.Formal);
            return Ok(incomes.Select(c => new ClientIncomeDto
            {
                Id = c.Id,
                ClientId = c.ClientId,
                IncomeType = c.IncomeType,
                ConfirmationDocumentTypeId = c.ConfirmationDocumentTypeId,
                FileRowId = c.FileRowId,
                FileRow = c.FileRow,
                IncomeTurns = c.IncomeTurns,
                MonthQuantity = c.MonthQuantity,
                IncomeAmount = c.IncomeAmount
            }));
        }

        /// <summary>
        /// Сохраняет официальные доходы клиента
        /// </summary>
        /// <param name="clientId">Идентификатор клиента</param>
        /// <param name="request"></param>
        /// <returns></returns>
        [Authorize(Permissions.ClientManage)]
        [HttpPost("incomeFormalSave"), ProducesResponseType(typeof(List<ClientIncomeDto>), 200)]
        public IActionResult IncomeOfficialSave([FromRoute] int clientId, [FromBody] SaveClientIncomesRequest request)
        {
            bool canFillQuestionnaire = _clientQuestionnaireService.CanFillQuestionnaire(clientId);
            if (!canFillQuestionnaire)
                throw new PawnshopApplicationException("Данному клиенту нельзя заполнять анкету");

            ModelState.Validate();
            List<ClientIncome> incomes = _clientIncomeService.Save(clientId, request.ClientIncomes, IncomeType.Formal);
            return Ok(incomes.Select(c => new ClientIncomeDto
            {
                Id = c.Id,
                ClientId = c.ClientId,
                IncomeType = c.IncomeType,
                ConfirmationDocumentTypeId = c.ConfirmationDocumentTypeId,
                FileRowId = c.FileRowId,
                IncomeTurns = c.IncomeTurns,
                MonthQuantity = c.MonthQuantity,
                IncomeAmount = c.IncomeAmount
            }));
        }

        //Раздел неофициальных доходов
        /// <summary>
        /// Получает список прочих подтверждаемых доходов клиента
        /// </summary>
        /// <param name="clientId">Идентификатор клиента</param>
        /// <returns></returns>
        [HttpPost("incomeAdditionalApprovedList"), ProducesResponseType(typeof(List<ClientIncomeDto>), 200)]
        public IActionResult GetIncomeAdditionalApprovedList([FromRoute] int clientId)
        {
            if (clientId == default)
                throw new PawnshopApplicationException("Выберите клиента");

            List<ClientIncome> incomes = _clientIncomeService.GetClientIncomes(clientId, (int)IncomeType.Informal);
            return Ok(incomes.Select(c => new ClientIncomeDto
            {
                Id = c.Id,
                ClientId = c.ClientId,
                IncomeType = c.IncomeType,
                ConfirmationDocumentTypeId = c.ConfirmationDocumentTypeId,
                FileRowId = c.FileRowId,
                FileRow = c.FileRow,
                IncomeTurns = c.IncomeTurns,
                MonthQuantity = c.MonthQuantity,
                IncomeAmount = c.IncomeAmount
            }));
        }

        [HttpPost("filters")]
        public async Task<IActionResult> GetClientIncomeHistory([FromRoute] int clientId, [FromBody] ClientIncomeHistoryQuery query)
        {
            var result = await _clientIncomeService.GetHistoryFiltered(clientId,query);
            return Ok(result);
        }


        /// <summary>
        /// Сохраняет прочие подтверждаемые доходы клиента
        /// </summary>
        /// <param name="clientId">Идентификатор клиента</param>
        /// <param name="request"></param>
        /// <returns></returns>
        [Authorize(Permissions.ClientManage)]
        [HttpPost("incomeAdditionalApprovedSave"), ProducesResponseType(typeof(List<ClientIncomeDto>), 200)]
        public IActionResult IncomeAdditionalApprovedSave([FromRoute] int clientId, [FromBody] SaveClientIncomesRequest request)
        {
            bool canFillQuestionnaire = _clientQuestionnaireService.CanFillQuestionnaire(clientId);
            if (!canFillQuestionnaire)
                throw new PawnshopApplicationException("Данному клиенту нельзя заполнять анкету");

            ModelState.Validate();
            List<ClientIncome> incomes = _clientIncomeService.Save(clientId, request.ClientIncomes, IncomeType.Informal);
            return Ok(incomes.Select(c => new ClientIncomeDto
            {
                Id = c.Id,
                ClientId = c.ClientId,
                IncomeType = c.IncomeType,
                ConfirmationDocumentTypeId = c.ConfirmationDocumentTypeId,
                FileRowId = c.FileRowId,
                IncomeTurns = c.IncomeTurns,
                MonthQuantity = c.MonthQuantity,
                IncomeAmount = c.IncomeAmount
            }));
        }

        /// <summary>
        /// Расчет суммы дохода от Вида дохода
        /// </summary>
        /// <param name="clientId">Идентификатор клиента</param>
        /// <param name="request"></param>
        /// <returns></returns>
        [Authorize(Permissions.ClientManage)]
        [HttpPost("calcIncomeAmount"), ProducesResponseType(typeof(ClientIncomeDto), 200)]
        public IActionResult CalcIncomeAmount([FromRoute] int clientId, [FromBody] ClientIncomeDto request)
        {
            if (clientId <= 0)
                return BadRequest();
            return Ok(_clientIncomeService.CalcIncomeAmount(request));
        }
    }
}
