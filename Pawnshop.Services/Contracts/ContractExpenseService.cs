using Pawnshop.AccountingCore.Models;
using Pawnshop.Core;
using Pawnshop.Core.Exceptions;
using Pawnshop.Core.Queries;
using Pawnshop.Data.Access;
using Pawnshop.Data.Models.Audit;
using Pawnshop.Data.Models.CashOrders;
using Pawnshop.Data.Models.Contracts;
using Pawnshop.Data.Models.Contracts.Actions;
using Pawnshop.Data.Models.Contracts.Expenses;
using Pawnshop.Data.Models.Dictionaries;
using Pawnshop.Data.Models.Membership;
using Pawnshop.Services.AccountingCore;
using Pawnshop.Services.Crm;
using Pawnshop.Services.Expenses;
using Pawnshop.Services.Models.Filters;
using Pawnshop.Services.Models.List;
using Pawnshop.Services.Remittances;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pawnshop.Services.Contracts
{
    public class ContractExpenseService : IContractExpenseService
    {
        private readonly ContractExpenseRepository _contractExpenseRepository;
        private readonly ContractExpenseRowRepository _contractExpenseRowRepository;
        private readonly ContractExpenseRowOrderRepository _contractExpenseRowOrderRepository;
        private readonly ContractActionRepository _contractActionRepository;
        private readonly ICashOrderService _cashOrderService;

        public ContractExpenseService(ContractExpenseRepository contractExpenseRepository,
            ContractExpenseRowRepository contractExpenseRowRepository, ICashOrderService cashOrderService,
            ContractExpenseRowOrderRepository contractExpenseRowOrderRepository,
            ContractActionRepository contractActionRepository)
        {
            _contractExpenseRepository = contractExpenseRepository;
            _contractExpenseRowRepository = contractExpenseRowRepository;
            _cashOrderService = cashOrderService;
            _contractExpenseRowOrderRepository = contractExpenseRowOrderRepository;
            _contractActionRepository = contractActionRepository;
        }

        public void Delete(int id)
        {
            _contractExpenseRepository.Delete(id);
        }

        public async Task<ContractExpense> GetAsync(int id)
        {
            ContractExpense expense = _contractExpenseRepository.Get(id);
            if (expense == null)
                throw new PawnshopApplicationException($"Расход с идентификатором {id} не найден");

            expense.ContractExpenseRows = _contractExpenseRowRepository.GetByContractExpenseId(expense.Id);
            if (expense.ContractExpenseRows == null)
                throw new PawnshopApplicationException($"Свойство {nameof(expense)}.{nameof(expense.ContractExpenseRows)} не должно быть null");

            foreach (ContractExpenseRow contractExpenseRow in expense.ContractExpenseRows)
            {
                if (contractExpenseRow == null)
                    throw new PawnshopApplicationException($"Переменная {nameof(contractExpenseRow)} не должна быть null");

                contractExpenseRow.ContractExpenseRowOrders = _contractExpenseRowOrderRepository.GetByContractExpenseRowId(contractExpenseRow.Id);
            }

            return expense;
        }

        public ListModel<ContractExpense> List(ListQueryModel<ContractExpenseFilter> listQuery)
        {
            return new ListModel<ContractExpense>
            {
                Count = _contractExpenseRepository.Count(listQuery, listQuery.Model),
                List = _contractExpenseRepository.List(listQuery, listQuery.Model)
            };
        }

        public ListModel<ContractExpense> List(ListQuery listQuery)
        {
            return new ListModel<ContractExpense>
            {
                Count = _contractExpenseRepository.Count(listQuery),
                List = _contractExpenseRepository.List(listQuery)
            };
        }

        public ContractExpense Save(ContractExpense model)
        {
            if (model.Id > 0)
            {
                _contractExpenseRepository.Update(model);
            }
            else
            {
                _contractExpenseRepository.Insert(model);
            }

            return model;
        }

        public IDbTransaction BeginTransaction()
        {
            return _contractExpenseRepository.BeginTransaction();
        }
    }
}
