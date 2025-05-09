using System;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Pawnshop.Core;
using Pawnshop.Data.Access;
using Pawnshop.Data.Models.Dictionaries;
using Pawnshop.Web.Engine;
using Pawnshop.Web.Models.Dictionary;
using Pawnshop.Web.Models.List;
using System.Collections.Generic;
using Pawnshop.Web.Models.Membership;
using Pawnshop.Services.Expenses;

namespace Pawnshop.Web.Controllers.Api
{
    [Authorize(Permissions.ExpenseView)]
    public class ExpenseController : Controller
    {
        private readonly IExpenseService _expenseService;
        private readonly ISessionContext _sessionContext;

        public ExpenseController(ISessionContext sessionContext, IExpenseService expenseService)
        {
            _expenseService = expenseService;
            _sessionContext = sessionContext;
        }

        [HttpPost]
        public ListModel<Expense> List([FromBody] ListQueryModel<ExpenseTypeListQueryModel> listQuery)
        {
            if (listQuery == null) listQuery = new ListQueryModel<ExpenseTypeListQueryModel>();
            if (listQuery.Model == null) listQuery.Model = new ExpenseTypeListQueryModel();
            
            return new ListModel<Expense>
            {
                List = _expenseService.GetList(listQuery),
                Count = _expenseService.Count(listQuery)
            };
        }

        [HttpPost]
        public Expense Card([FromBody] int id)
        {
            if (id <= 0) throw new ArgumentOutOfRangeException(nameof(id));

            var model = _expenseService.Get(id);
            if (model == null) throw new InvalidOperationException();

            return model;
        }

        [HttpPost]
        public List<Expense> Find([FromBody] ExpensesQueryModel query)
        {
            if (query == null) throw new ArgumentNullException(nameof(query));

            return _expenseService.FindExpenses(query.CollateralType, query.ActionType);
        }

        [HttpPost, Authorize(Permissions.ExpenseManage)]
        public Expense Save([FromBody] Expense model)
        {
            ModelState.Validate();

            if (model.Id > 0)
            {
                _expenseService.Update(model);
            }
            else
            {
                model.CreatedBy = _sessionContext.UserId;
                model.CreateDate = DateTime.Now;
                _expenseService.Create(model);
            }

            return model;
        }

        [HttpPost, Authorize(Permissions.ExpenseTypeManage)]
        public IActionResult Delete([FromBody] int id)
        {
            if (id <= 0) throw new ArgumentOutOfRangeException(nameof(id));

            _expenseService.Delete(id);
            return Ok();
        }
    }
}