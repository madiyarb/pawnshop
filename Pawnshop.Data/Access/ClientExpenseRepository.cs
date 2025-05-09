using Dapper;
using Pawnshop.Core;
using Pawnshop.Core.Exceptions;
using Pawnshop.Core.Impl;
using Pawnshop.Core.Queries;
using Pawnshop.Data.Models.Clients;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Pawnshop.Data.Access
{
    public class ClientExpenseRepository : RepositoryBase
    {
        public ClientExpenseRepository(IUnitOfWork unitOfWork) : base(unitOfWork)
        {
        }

        public void Insert(ClientExpense entity)
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));

            entity.CreateDate = DateTime.Now;
            UnitOfWork.Session.Execute(@"
                INSERT INTO ClientExpenses 
                    (ClientId, AllLoan, Loan, Other, Housing, Family, Vehicle, AuthorId, CreateDate, AvgPaymentToday)
                VALUES 
                    (@ClientId, @AllLoan, @Loan, @Other, @Housing, @Family, @Vehicle, @AuthorId, @CreateDate, @AvgPaymentToday)
                SELECT SCOPE_IDENTITY()", entity, UnitOfWork.Transaction);
        }

        public void Update(ClientExpense entity)
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));

            UnitOfWork.Session.Execute(@"
                UPDATE ClientExpenses
                SET 
                    AllLoan = @AllLoan,
                    Loan = @Loan, 
                    AvgPaymentToday = @AvgPaymentToday,
                    Other = @Other, 
                    Housing = @Housing, 
                    Family = @Family, 
                    Vehicle = @Vehicle, 
                    AuthorId = @AuthorId, 
                    CreateDate = @CreateDate
                WHERE ClientId = @ClientId", entity, UnitOfWork.Transaction);
        }

        public ClientExpense Get(int clientId)
        {
            return UnitOfWork.Session.Query<ClientExpense>(@"
                SELECT ce.*
                FROM ClientExpenses ce
                WHERE ce.ClientId = @clientId",
            new { clientId }, UnitOfWork.Transaction).FirstOrDefault();
        }

        public void LogChanges(ClientExpense entity, int userId, bool isNew = false)
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));

            var log = new ClientExpenseLog
            {
                ClientId = entity.ClientId,
                AllLoan = entity.AllLoan,
                Loan = entity.Loan,
                AvgPaymentToday = entity.AvgPaymentToday,
                Other = entity.Other,
                Housing = entity.Housing,
                Family = entity.Family,
                Vehicle = entity.Vehicle,
                AuthorId = entity.AuthorId,
                CreateDate = entity.CreateDate,
                UpdatedByAuthorId = !isNew ? userId : default(int?),
                UpdateDate = DateTime.Now
            };
            UnitOfWork.Session.Execute(@"
                INSERT INTO ClientExpenseLogs 
                    (ClientId, AllLoan, Loan, AvgPaymentToday, Other, Housing, Family, Vehicle, 
                     AuthorId, CreateDate, UpdatedByAuthorId, UpdateDate)
                VALUES 
                    (@ClientId, @AllLoan, @Loan, @AvgPaymentToday, @Other, @Housing, @Family, @Vehicle, 
                     @AuthorId, @CreateDate, @UpdatedByAuthorId, @UpdateDate)", log, UnitOfWork.Transaction);
        }
    }
}
