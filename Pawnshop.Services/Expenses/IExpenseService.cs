using System;
using System.Collections.Generic;
using System.Text;
using Pawnshop.AccountingCore.Models;
using Pawnshop.Core.Queries;
using Pawnshop.Data.Models.Contracts.Actions;
using Pawnshop.Data.Models.Dictionaries;

namespace Pawnshop.Services.Expenses
{
    public interface IExpenseService
    {
        List<Expense> GetList(ListQuery listQuery = null);
        int Count(ListQuery listQuery = null);
        Expense Get(int id);
        void Create(Expense expense);
        void Update(Expense expense);
        void Delete(int id);
        List<Expense> FindExpenses(CollateralType collateralType, ContractActionType contractActionType);
    }
}
