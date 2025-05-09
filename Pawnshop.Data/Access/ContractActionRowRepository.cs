using Dapper;
using Pawnshop.Core;
using Pawnshop.Core.Impl;
using Pawnshop.Core.Queries;
using Pawnshop.Data.Models.Contracts.Actions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Pawnshop.Data.Access
{
    public class ContractActionRowRepository : RepositoryBase, IRepository<ContractActionRow>
    {
        public ContractActionRowRepository(IUnitOfWork unitOfWork) : base(unitOfWork)
        {
        }

        public void Insert(ContractActionRow entity)
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));

            UnitOfWork.Session.ExecuteScalar<int>(@"
                INSERT INTO ContractActionRows
                       ( ActionId, PaymentType, Period, OriginalPercent, 
                        [Percent], Cost, DebitAccountId, CreditAccountId, 
                        OrderId, LoanSubjectId)
                VALUES ( @ActionId, @PaymentType, @Period, @OriginalPercent, 
                        @Percent, @Cost, @DebitAccountId, @CreditAccountId, 
                        @OrderId, @LoanSubjectId)
                SELECT SCOPE_IDENTITY()", entity, UnitOfWork.Transaction);
        }

        public void Update(ContractActionRow entity)
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));

            UnitOfWork.Session.Execute(@"
                UPDATE ContractActionRows 
                SET 
	                ActionId=@ActionId, 
	                LoanSubjectId=@LoanSubjectId, 
	                PaymentType=@PaymentType, 
	                Period=@Period, 
	                OriginalPercent=@OriginalPercent, 
	                [Percent]=@Percent, 
	                Cost=@Cost, 
	                OrderId=@OrderId, 
	                DebitAccountId=@DebitAccountId, 
	                CreditAccountId=@CreditAccountId
                WHERE Id = @Id", entity, UnitOfWork.Transaction);
        }

        public void Delete(int id)
        {
            throw new NotImplementedException("Функция удаления ContractActionRow недоступно");
        }

        public ContractActionRow Get(int id)
        {
            return UnitOfWork.Session.Query<ContractActionRow>(@"
                SELECT car.* FROM ContractActionRows car
                WHERE Id = @id", new { id }, UnitOfWork.Transaction).SingleOrDefault();
        }

        public List<ContractActionRow> List(ListQuery listQuery, object query)
        {
            throw new NotImplementedException("Функция получения списка ContractActionRow недоступна");
        }

        public int Count(ListQuery listQuery, object query)
        {
            throw new NotImplementedException("Функция получения количества ContractActionRow недоступна");
        }

        public ContractActionRow Find(object query)
        {
            throw new NotImplementedException("Функция поиска ContractActionRow недоступна");
        }

        public List<ContractActionRow> GetByContractActionId(int actionId)
        {
            return UnitOfWork.Session.Query<ContractActionRow>(@"
                SELECT car.* FROM ContractActionRows car
                WHERE ActionId = @actionId", new { actionId }, UnitOfWork.Transaction).ToList();
        }
    }
}
