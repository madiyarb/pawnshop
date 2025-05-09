using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Pawnshop.Core.Exceptions;
using Pawnshop.Core;
using Pawnshop.Data.Models.Clients;
using Pawnshop.Services.Clients;
using Pawnshop.Services.Models.Clients;
using Pawnshop.Web.Engine.Services.Interfaces;
using Pawnshop.Web.Engine;

namespace Pawnshop.Web.Controllers.Api
{
    [Route("api/clients/{clientId:int}/expense"), Authorize(Permissions.ClientView)]
    public class ClientExpenseController : Controller
    {
        private readonly IClientExpenseService _clientExpenseService;
        private readonly IClientQuestionnaireService _clientQuestionnaireService;
        public ClientExpenseController(IClientExpenseService clientExpenseService, IClientQuestionnaireService clientQuestionnaireService)
        {
            _clientExpenseService = clientExpenseService;
            _clientQuestionnaireService = clientQuestionnaireService;
        }

        /// <summary>
        /// Получает затраты клиента
        /// </summary>
        /// <param name="clientId">Идентификатор клиента</param>
        /// <returns></returns>
        [HttpPost("get"), ProducesResponseType(typeof(ClientExpenseDto), 200)]
        public IActionResult Get([FromRoute] int clientId, [FromServices] IClientIncomeService clientIncomeService)
        {
            ClientExpense expense = _clientExpenseService.Get(clientId);

            if (expense == null)
                return NoContent();

            var dependents = clientIncomeService.GetFamilyDebt(clientId);

            return Ok(new ClientExpenseDto
            {
                Loan = expense.Loan,
                AllLoan = expense.AllLoan,
                Other = expense.Other,
                Housing = expense.Housing,
                Family = expense.Family,
                Vehicle = expense.Vehicle,
                Dependents = dependents,
            });
        }

        /// <summary>
        /// Сохраняет затраты клиента
        /// </summary>
        /// <param name="clientId">Идентификатор клиента</param>
        /// <param name="request"></param>
        /// <returns></returns>
        [Authorize(Permissions.ClientManage)]
        [HttpPost("save"), ProducesResponseType(typeof(ClientExpenseDto), 200)]
        public IActionResult Save([FromRoute] int clientId, [FromBody] ClientExpenseDto request)
        {
            bool canFillQuestionnaire = _clientQuestionnaireService.CanFillQuestionnaire(clientId);
            if (!canFillQuestionnaire)
                throw new PawnshopApplicationException("Данному клиенту нельзя заполнять анкету");

            ModelState.Validate();
            ClientExpense expense = _clientExpenseService.Save(clientId, request);
            return Ok(new ClientExpenseDto
            {
                Loan = expense.Loan,
                AllLoan = expense.AllLoan,
                Other = expense.Other,
                Housing = expense.Housing,
                Family = expense.Family,
                Vehicle = expense.Vehicle
            });
        }
    }
}
