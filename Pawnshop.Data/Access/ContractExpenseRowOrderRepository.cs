using System;
using System.Collections.Generic;
using System.Text;
using Pawnshop.Core;
using Pawnshop.Core.Impl;
using Pawnshop.Core.Queries;
using Pawnshop.Data.Models.Contracts.Expenses;
using Dapper;
using System.Linq;
using Pawnshop.Data.Models.CashOrders;
using Pawnshop.Data.Models.AccountingCore;

namespace Pawnshop.Data.Access
{
    public class ContractExpenseRowOrderRepository : RepositoryBase, IRepository<ContractExpenseRowOrder>
    {
        public ContractExpenseRowOrderRepository(IUnitOfWork unitOfWork) : base(unitOfWork)
        {
        }

        public int Count(ListQuery listQuery, object query = null)
        {
            throw new NotImplementedException();
        }

        public void Delete(int id)
        {
            using (var transaction = BeginTransaction())
            {
                UnitOfWork.Session.Execute(@"
                    UPDATE ContractExpenseRowOrders SET
                        DeleteDate = dbo.GETASTANADATE()
                    WHERE Id = @id", new { id }, UnitOfWork.Transaction);

                transaction.Commit();
            }
        }

        public ContractExpenseRowOrder Find(object query)
        {
            throw new NotImplementedException();
        }

        public ContractExpenseRowOrder Get(int id)
        {
            return UnitOfWork.Session.QuerySingleOrDefault<ContractExpenseRowOrder>(@"
                SELECT *
                FROM ContractExpenseRowOrders
                WHERE Id = @id and DeleteDate IS NULL", new { id }, UnitOfWork.Transaction);
        }

        public void Insert(ContractExpenseRowOrder entity)
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));

            using (var transaction = BeginTransaction())
            {
                entity.Id = UnitOfWork.Session.QuerySingleOrDefault<int>(@"
                    INSERT INTO ContractExpenseRowOrders 
                        (ContractExpenseRowId, OrderId, AuthorId, CreateDate, DeleteDate)
                    VALUES 
                        (@ContractExpenseRowId, @OrderId, @AuthorId, @CreateDate, @DeleteDate)
                    SELECT SCOPE_IDENTITY()", entity, UnitOfWork.Transaction);

                transaction.Commit();
            }
        }

        public List<ContractExpenseRowOrder> GetByContractExpenseRowId(int contractExpenseRowId)
        {
            return UnitOfWork.Session.Query<ContractExpenseRowOrder, CashOrder, Account, Account, ContractExpenseRowOrder>($@"
                SELECT cero.*, co.*, ca.*, da.*
                FROM ContractExpenseRowOrders cero
                LEFT JOIN CashOrders co on co.Id = cero.OrderId
                LEFT JOIN Accounts ca on co.CreditAccountId = ca.Id
                LEFT JOIN Accounts da on co.DebitAccountId = da.Id
                WHERE cero.ContractExpenseRowId = @contractExpenseRowId 
                AND cero.DeleteDate IS NULL", (cero, co, ca, da) =>
                {
                    if (co != null)
                    {
                        co.CreditAccount = ca;
                        co.DebitAccount = da;
                    }

                    cero.Order = co;
                    return cero;
                }, new { contractExpenseRowId }, UnitOfWork.Transaction).ToList();
        }

        public ContractExpenseRowOrder GetByOrderId(int orderId)
        {
            return UnitOfWork.Session.Query<ContractExpenseRowOrder>($@"
                SELECT cero.*
                FROM ContractExpenseRowOrders cero
                WHERE cero.OrderId = @orderId
                AND cero.DeleteDate IS NULL", new { orderId }, UnitOfWork.Transaction).FirstOrDefault();
        }

        public List<ContractExpenseRowOrder> List(ListQuery listQuery, object query = null)
        {
            throw new NotImplementedException();
        }

        public void Update(ContractExpenseRowOrder entity)
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));

            using (var transaction = BeginTransaction())
            {
                UnitOfWork.Session.Execute(@"
                    UPDATE ContractExpenseRowOrders
                    SET 
                        ContractExpenseRowId = @ContractExpenseRowId
                        OrderId = @OrderId,
                        AuthorId = @AuthorId,
                        CreateDate = @CreateDate,
                        DeleteDate = @DeleteDate
                    WHERE Id = @Id", entity, UnitOfWork.Transaction);

                transaction.Commit();
            }
        }
    }
}
