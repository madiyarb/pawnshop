using Pawnshop.Core;
using Pawnshop.Core.Exceptions;
using Pawnshop.Core.Queries;
using Pawnshop.Data.Access;
using Pawnshop.Data.Models.Dictionaries;
using Pawnshop.Services.Models.Filters;
using Pawnshop.Services.Models.List;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;
using System.Text.RegularExpressions;

namespace Pawnshop.Services.Clients
{
    public class ManualCalculationClientExpenseService : IManualCalculationClientExpenseService
    {
        private readonly ManualCalculationClientExpensesRepository _manualCalculationClientExpensesRepository;

        public ManualCalculationClientExpenseService(ManualCalculationClientExpensesRepository manualCalculationClientExpensesRepository)
        {
            _manualCalculationClientExpensesRepository = manualCalculationClientExpensesRepository;
        }

        public ListModel<ManualCalculationClientExpense> List(ListQuery listQuery)
        {
            return new ListModel<ManualCalculationClientExpense>
            {
                List = _manualCalculationClientExpensesRepository.List(listQuery),
                Count = _manualCalculationClientExpensesRepository.Count(listQuery)
            };
        }

        public ListModel<ManualCalculationClientExpense> List(ListQueryModel<ManualCalculationClientExpenseFilter> listQuery)
        {
            return new ListModel<ManualCalculationClientExpense>()
            {
                List = _manualCalculationClientExpensesRepository.List(listQuery, listQuery.Model),
                Count = _manualCalculationClientExpensesRepository.Count(listQuery, listQuery.Model)
            };
        }

        public ManualCalculationClientExpense Get(int id)
        {
            var clientExpense = _manualCalculationClientExpensesRepository.Get(id);
            if (clientExpense is null)
                throw new NullReferenceException($"Расходы клиента по прочим платежам с Id {id} не найден");
            return clientExpense;
        }

        public ManualCalculationClientExpense Save(ManualCalculationClientExpense clientExpense)
        {
            if (clientExpense.Id > 0) _manualCalculationClientExpensesRepository.Update(clientExpense);
            else _manualCalculationClientExpensesRepository.Insert(clientExpense);

            return clientExpense;
        }

        public void Delete(int id)
        {
            Get(id);
            _manualCalculationClientExpensesRepository.Delete(id);
        }

        public ManualCalculationClientExpense GetByClientId(int clientId)
        {
            var clientExpense = _manualCalculationClientExpensesRepository.GetByClientIdAndDate(clientId, DateTime.Today);
            return clientExpense;
        }
    }
}
