using Pawnshop.Core;
using Pawnshop.Core.Exceptions;
using Pawnshop.Data.Access;
using Pawnshop.Data.Models.Clients;
using Pawnshop.Services.Models.Clients;
using System;
using System.Collections.Generic;
using System.Data;
using System.Text;

namespace Pawnshop.Services.Clients
{
    public class ClientExpenseService : IClientExpenseService
    {
        private readonly ClientExpenseRepository _clientExpenseRepository;
        private readonly IClientService _clientService;
        private readonly ISessionContext _sessionContext;
        public ClientExpenseService(ClientExpenseRepository clientExpenseRepository, IClientService clientService, ISessionContext sessionContext)
        {
            _clientExpenseRepository = clientExpenseRepository;
            _sessionContext = sessionContext;
            _clientService = clientService;
        }

        public ClientExpense Get(int clientId)
        {
            _clientService.CheckClientExists(clientId);
            return _clientExpenseRepository.Get(clientId);
        }

        public ClientExpense Save(int clientId, ClientExpenseDto expense)
        {
            if (expense == null)
                throw new ArgumentNullException(nameof(expense));

            Validate(expense);
            ClientExpense expenseFromDB = Get(clientId);
            using (var transaction = _clientExpenseRepository.BeginTransaction())
            {
                if (expenseFromDB == null)
                {
                    expenseFromDB = new ClientExpense
                    {
                        ClientId = clientId,
                        Loan = Convert.ToInt32(expense.Loan),
                        AllLoan = (int?)expense.AllLoan,
                        AvgPaymentToday = Convert.ToInt32(expense.AvgPaymentToday),
                        Other = Convert.ToInt32(expense.Other),
                        Housing = Convert.ToInt32(expense.Housing),
                        Family = Convert.ToInt32(expense.Family),
                        Vehicle = Convert.ToInt32(expense.Vehicle),
                        AuthorId = _sessionContext.UserId
                    };
                    _clientExpenseRepository.Insert(expenseFromDB);
                    _clientExpenseRepository.LogChanges(expenseFromDB, _sessionContext.UserId, true);
                }
                else
                {
                    expenseFromDB.Loan = Convert.ToInt32(expense.Loan);
                    expenseFromDB.AllLoan = (int?)expense.AllLoan;
                    expenseFromDB.AvgPaymentToday = Convert.ToInt32(expense.AvgPaymentToday);
                    expenseFromDB.Other = Convert.ToInt32(expense.Other);
                    expenseFromDB.Housing = Convert.ToInt32(expense.Housing);
                    expenseFromDB.Family = Convert.ToInt32(expense.Family);
                    expenseFromDB.Vehicle = Convert.ToInt32(expense.Vehicle);
                    _clientExpenseRepository.Update(expenseFromDB);
                    _clientExpenseRepository.LogChanges(expenseFromDB, _sessionContext.UserId);
                }

                transaction.Commit();
                return expenseFromDB;
            }
        }

        public bool IsClientExpenseFilled(int clientId)
        {
            ClientExpense expense = Get(clientId);
            if (expense == null)
                return false;

            return expense.Family.HasValue && expense.Housing.HasValue
                && expense.Loan.HasValue && expense.Other.HasValue && expense.Vehicle.HasValue && expense.AllLoan.HasValue;
        }

        private void Validate(ClientExpenseDto expense)
        {
            if (expense == null)
                throw new ArgumentNullException(nameof(expense));

            var errors = new List<string>();
            if(expense.AllLoan < 0)
                errors.Add($"Поле 'Сумма задолжности по всем непогашенным кредитам' должно быть положительным числом");

            if (expense.Loan < 0)
                errors.Add($"Поле 'Расходы на кредиты' должно быть положительным числом");

            if (expense.Other < 0)
                errors.Add($"Поле 'Прочие расходы' должно быть положительным числом");

            if (expense.Housing < 0)
                errors.Add($"Поле 'Расходы на дом' должно быть положительным числом");

            if (expense.Family < 0)
                errors.Add($"Поле 'Расходы на семью' должно быть положительным числом");

            if (expense.Vehicle < 0)
                errors.Add($"Поле 'Расходы на машину' должно быть положительным числом");

            if (expense.AvgPaymentToday < 0)
                errors.Add($"Поле 'Среднемесячный платеж кредита оформленного в день расчета КДН' должно быть положительным числом");

            if (errors.Count > 0)
                throw new PawnshopApplicationException(errors.ToArray());
        }

        public IDbTransaction BeginClientExpenseTransaction()
        {
            return _clientExpenseRepository.BeginTransaction();
        }
    }
}
