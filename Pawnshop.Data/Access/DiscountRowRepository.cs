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
    public class DiscountRowRepository : RepositoryBase, IRepository<DiscountRow>
    {
        public DiscountRowRepository(IUnitOfWork unitOfWork) : base(unitOfWork)
        { 
        }

        public void Insert(DiscountRow entity)
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));

            entity.Id = UnitOfWork.Session.ExecuteScalar<int>(@"
                INSERT INTO DiscountRows
                    (DiscountId, PaymentType, AddedCost, SubtractedCost,
                    OriginalCost, AddedDays, SubtractedDays, OriginalDays, 
                    PercentAdjustment, OrderId)
                VALUES 
                    (@DiscountId, @PaymentType, @AddedCost, @SubtractedCost, 
                    @OriginalCost, @AddedDays, @SubtractedDays, @OriginalDays, 
                    @PercentAdjustment, @OrderId)
                SELECT SCOPE_IDENTITY()", entity, transaction: UnitOfWork.Transaction);
        }

        public void Update(DiscountRow entity)
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));

            UnitOfWork.Session.Execute(@"
                UPDATE DiscountRows
                SET
                    DiscountId = @DiscountId,
                    PaymentType = @PaymentType,
                    AddedCost = @addedCost, 
                    SubtractedCost = @SubtractedCost,
                    OriginalCost = @OriginalCost, 
                    AddedDays = @AddedDays,
                    SubtractedDays,
                    OriginalDays = @OriginalDays,
                    PercentAdjustment = @PercentAdjustment,
                    OrderId = @OrderId
                WHERE Id = @Id", entity, UnitOfWork.Transaction);
        }

        public void Delete(int id)
        {
            throw new NotImplementedException("Функция удаления DiscountRow недоступно");
        }

        public DiscountRow Get(int id)
        {
            return UnitOfWork.Session.Query<DiscountRow>(@"
                SELECT dr.* FROM DiscountRows dr
                WHERE Id = @id", new { id }, UnitOfWork.Transaction).SingleOrDefault();
        }

        public List<DiscountRow> List(ListQuery listQuery, object query)
        {
            throw new NotImplementedException("Функция получения списка DiscountRow недоступна");
        }

        public int Count(ListQuery listQuery, object query)
        {
            throw new NotImplementedException("Функция получения количества DiscountRow недоступна");
        }

        public DiscountRow Find(object query)
        {
            throw new NotImplementedException("Функция поиска количества DiscountRow недоступна");
        }

        public List<DiscountRow> GetByDiscountId(int discountId)
        {
            return UnitOfWork.Session.Query<DiscountRow>(@"
                SELECT dr.* FROM DiscountRows dr
                WHERE DiscountId = @discountId", new { discountId }, UnitOfWork.Transaction).ToList();
        }
    }
}
