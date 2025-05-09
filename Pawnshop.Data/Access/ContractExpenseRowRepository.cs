using System;
using System.Collections.Generic;
using System.Text;
using Pawnshop.Core;
using Pawnshop.Core.Impl;
using Pawnshop.Core.Queries;
using Pawnshop.Data.Models.Contracts.Expenses;
using Dapper;
using System.Linq;

namespace Pawnshop.Data.Access
{
    public class ContractExpenseRowRepository : RepositoryBase, IRepository<ContractExpenseRow>
    {
        public ContractExpenseRowRepository(IUnitOfWork unitOfWork) : base(unitOfWork)
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
                    UPDATE ContractExpenseRows SET
                        DeleteDate = dbo.GETASTANADATE()
                    WHERE Id = @id", new { id }, UnitOfWork.Transaction);

                transaction.Commit();
            }
        }

        public ContractExpenseRow Find(object query)
        {
            throw new NotImplementedException();
        }

        public ContractExpenseRow Get(int id)
        {
            return UnitOfWork.Session.QuerySingleOrDefault<ContractExpenseRow>(@"
                SELECT *
                FROM ContractExpenseRows
                WHERE Id = @id and DeleteDate IS NULL", new { id }, UnitOfWork.Transaction);
        }

        public void Insert(ContractExpenseRow entity)
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));

            using (var transaction = BeginTransaction())
            {
                entity.Id = UnitOfWork.Session.QuerySingleOrDefault<int>(@"
                    INSERT INTO ContractExpenseRows (ContractExpenseId, ActionId, ExpensePaymentType, Cost, AuthorId, CreateDate, DeleteDate)
                    VALUES (@ContractExpenseId, @ActionId, @ExpensePaymentType, @Cost, @AuthorId, @CreateDate, @DeleteDate)
                    SELECT SCOPE_IDENTITY()", entity, UnitOfWork.Transaction);

                transaction.Commit();
            }
        }

        public List<ContractExpenseRow> GetByContractExpenseId(int contractExpenseId)
        {
            return UnitOfWork.Session.Query<ContractExpenseRow>($@"
                SELECT cer.*
                FROM ContractExpenseRows cer
                WHERE cer.ContractExpenseId = @contractExpenseId AND cer.DeleteDate IS NULL", new { contractExpenseId }, UnitOfWork.Transaction).ToList();
        }

        public List<ContractExpenseRow> List(ListQuery listQuery, object query = null)
        {
            throw new NotImplementedException();
        }

        public void Update(ContractExpenseRow entity)
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));

            using (var transaction = BeginTransaction())
            {
                UnitOfWork.Session.Execute(@"
                    UPDATE ContractExpenseRows
                    SET 
                        ContractExpenseId = @ContractExpenseId,
                        ActionId = @ActionId,
                        ExpensePaymentType = @ExpensePaymentType,
                        Cost = @Cost, 
                        AuthorId = @AuthorId,
                        CreateDate = @CreateDate,
                        DeleteDate = @DeleteDate
                    WHERE Id = @Id", entity, UnitOfWork.Transaction);
                
                transaction.Commit();
            }
        }
    }
}
