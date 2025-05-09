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
    public class DiscountRepository : RepositoryBase, IRepository<Discount>
    {
        public DiscountRepository(IUnitOfWork unitOfWork) : base(unitOfWork)
        {
        }

        public void Insert(Discount entity)
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));

            entity.Id = UnitOfWork.Session.ExecuteScalar<int>(@"
                INSERT INTO Discounts
                    (ActionId, BlackoutId, ContractDiscountId)
                VALUES 
                    (@ActionId, @BlackoutId, @ContractDiscountId)
                SELECT SCOPE_IDENTITY()", entity, UnitOfWork.Transaction);
        }

        public void Update(Discount entity)
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));

            UnitOfWork.Session.Execute(@"
                UPDATE Discounts
                SET
                    ActionId = @ActionId,
                    BlackoutId = @BlackoutId, 
                    ContractDiscountId = @ContractDiscountId
                WHERE Id = @Id", entity, UnitOfWork.Transaction);
        }

        public void Delete(int id)
        {
            throw new NotImplementedException("Функция удаления Discount недоступна");
        }

        public Discount Get(int id)
        {
            return UnitOfWork.Session.Query<Discount>(@"
                SELECT d.* FROM Discounts d
                WHERE Id = @id", new { id }, UnitOfWork.Transaction).SingleOrDefault();
        }

        public List<Discount> GetListByContractActionId(int contractActionId)
        {
            return UnitOfWork.Session.Query<Discount>(@"
                SELECT d.* FROM Discounts d
                WHERE d.ActionId = @contractActionId",
                    new { contractActionId }, UnitOfWork.Transaction).ToList();
        }

        public List<Discount> List(ListQuery listQuery, object query)
        {
            throw new NotImplementedException("Функция получения списка Discount недоступна");
        }

        public int Count(ListQuery listQuery, object query)
        {
            throw new NotImplementedException("Функция получения количества Discount недоступна");
        }

        public Discount Find(object query)
        {
            throw new NotImplementedException("Функция поиска Discount недоступна");
        }

        public List<Discount> GetByActionId(int actionId)
        {
            return UnitOfWork.Session.Query<Discount>(@"
                SELECT d.* FROM Discounts d
                WHERE ActionId = @actionId", new { actionId }, UnitOfWork.Transaction).ToList();
        }
    }
}
