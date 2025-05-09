using System;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Pawnshop.Core;
using Pawnshop.Core.Queries;
using Pawnshop.Data.Access;
using Pawnshop.Data.Models.Audit;
using Pawnshop.Data.Models.Dictionaries;
using Pawnshop.Services.Clients;
using Pawnshop.Services.Models.Filters;
using Pawnshop.Services.Models.List;
using Pawnshop.Web.Engine;
using Pawnshop.Web.Engine.Middleware;

namespace Pawnshop.Web.Controllers.Api
{
    public class ManualCalculationClientExpenseController : Controller
    {
        private readonly IManualCalculationClientExpenseService _manualCalculationClientExpenseService;

        public ManualCalculationClientExpenseController(IManualCalculationClientExpenseService manualCalculationClientExpenseService)
        {
            _manualCalculationClientExpenseService = manualCalculationClientExpenseService;
        }

        [HttpPost]
        public ListModel<ManualCalculationClientExpense> List([FromBody] ListQueryModel<ManualCalculationClientExpenseFilter> listQuery)
        {
            return _manualCalculationClientExpenseService.List(listQuery);
        }

        [HttpPost]
        public ManualCalculationClientExpense Card([FromBody] int id)
        {
            if (id <= 0) throw new ArgumentOutOfRangeException(nameof(id));
            return _manualCalculationClientExpenseService.Get(id);
        }

        [HttpPost]
        [Event(EventCode.DictManualCalculationClientExpenseSaved, EventMode = EventMode.Response)]
        public ManualCalculationClientExpense Save([FromBody] ManualCalculationClientExpense clientExpense)
        {
            return _manualCalculationClientExpenseService.Save(clientExpense);
        }

        [HttpPost]
        [Event(EventCode.DictManualCalculationClientExpenseDeleted, EventMode = EventMode.Request)]
        public IActionResult Delete([FromBody] int id)
        {
            _manualCalculationClientExpenseService.Delete(id);
            return Ok();
        }
    }
}