using Pawnshop.AccountingCore.Models;
using Pawnshop.Core.Exceptions;
using Pawnshop.Core.Queries;
using Pawnshop.Data.Access;
using Pawnshop.Data.Models.Contracts.Actions;
using Pawnshop.Data.Models.Dictionaries;
using System;
using System.Collections.Generic;
using System.Text;

namespace Pawnshop.Services.Expenses
{
    public class ExpenseService : IExpenseService
    {
        private readonly ExpenseRepository _expenseRepository;
        public ExpenseService(ExpenseRepository expenseRepository)
        {
            _expenseRepository = expenseRepository;
        }

        public List<Expense> GetList(ListQuery listQuery = null)
        {
            if (listQuery == null)
                listQuery = new ListQuery { Page = null };

            return _expenseRepository.List(listQuery, null);
        }

        public int Count(ListQuery listQuery = null)
        {
            if (listQuery == null)
                listQuery = new ListQuery { Page = null };

            return _expenseRepository.Count(listQuery, null);
        }

        public Expense Get(int id)
        {
            Expense entity = _expenseRepository.Get(id);
            if (entity == null)
                throw new PawnshopApplicationException($"Тип расхода не найден по {nameof(Expense.Id)} {id}");

            return entity;
        }

        public List<Expense> FindExpenses(CollateralType collateralType, ContractActionType contractActionType) {
            return _expenseRepository.FindExpenses(new { collateralType, contractActionType });
        }

        public void Create(Expense expense)
        {
            if (expense == null)
                throw new ArgumentNullException(nameof(expense));

            _expenseRepository.Insert(expense);
        }

        public void Update(Expense expense)
        {
            if (expense == null)
                throw new ArgumentNullException(nameof(expense));

            if (expense.Id <= 0)
                throw new ArgumentException($"{nameof(expense.Id)} должен быть положительным числом", nameof(expense));

            Get(expense.Id);
            _expenseRepository.Update(expense);
        }
        public void Delete(int id)
        {
            if (id <= 0)
                throw new ArgumentOutOfRangeException($"{nameof(id)} должен быть положительным числом");

            Get(id);
            _expenseRepository.Delete(id);
        }
    }
}
