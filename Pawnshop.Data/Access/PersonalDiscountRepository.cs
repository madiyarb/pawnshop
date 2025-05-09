using System;
using System.Collections.Generic;
using System.Linq;
using Dapper;
using Pawnshop.Core;
using Pawnshop.Core.Impl;
using Pawnshop.Core.Queries;
using Pawnshop.Data.Models.Clients;
using Pawnshop.Data.Models.Contracts;
using Pawnshop.Data.Models.Dictionaries;
using Pawnshop.Data.Models.Membership;

namespace Pawnshop.Data.Access
{
    public class PersonalDiscountRepository : RepositoryBase, IRepository<PersonalDiscount>
    {
        public PersonalDiscountRepository(IUnitOfWork unitOfWork) : base(unitOfWork)
        {
        }

        public void Insert(PersonalDiscount entity)
        {
            using (var transaction = BeginTransaction())
            {
                entity.Id = UnitOfWork.Session.QuerySingleOrDefault<int>(@"
                    INSERT INTO PersonalDiscounts
                        (PercentAdjustment, PercentDiscount, OverduePercentDiscount, OverduePercentAdjustment, DebtPenaltyAdjustment, 
                        DebtPenaltyDiscount, PercentPenaltyAdjustment, PercentPenaltyDiscount,
                        CollateralType, Locked, ActionType, BlackoutId, AuthorId, CreateDate, Note)
                    VALUES
                        (@PercentAdjustment, @PercentDiscount, @OverduePercentDiscount, @OverduePercentAdjustment, @DebtPenaltyAdjustment, 
                        @DebtPenaltyDiscount, @PercentPenaltyAdjustment, @PercentPenaltyDiscount,
                        @CollateralType, @Locked, @ActionType, @BlackoutId, @AuthorId, @CreateDate, @Note)
                    SELECT SCOPE_IDENTITY()", entity, UnitOfWork.Transaction);

                transaction.Commit();
            }
        }

        public void Update(PersonalDiscount entity)
        {
            using (var transaction = BeginTransaction())
            {
                UnitOfWork.Session.Execute(@"
                    UPDATE PersonalDiscounts
                    SET 
                        PercentAdjustment = @PercentAdjustment,
                        PercentDiscount = @PercentDiscount,
                        OverduePercentDiscount = @OverduePercentDiscount,
                        OverduePercentAdjustment = @OverduePercentAdjustment,
                        DebtPenaltyAdjustment = @DebtPenaltyAdjustment,
                        DebtPenaltyDiscount = @DebtPenaltyDiscount,
                        PercentPenaltyAdjustment = @PercentPenaltyAdjustment,
                        PercentPenaltyDiscount = @PercentPenaltyDiscount,
                        CollateralType = @CollateralType,
                        Locked = @Locked,
                        ActionType = @ActionType,
                        BlackoutId = @BlackoutId,
                        AuthorId = @AuthorId,
                        CreateDate = @CreateDate,
                        Note = @Note
                    WHERE Id = @Id", entity, UnitOfWork.Transaction);

                transaction.Commit();
            }
        }

        public void Delete(int id)
        {
            throw new NotImplementedException();
        }

        public PersonalDiscount Get(int id)
        {
            return UnitOfWork.Session.Query<PersonalDiscount>(@"
SELECT *
FROM PersonalDiscounts 
WHERE Id = @id", new { id }, UnitOfWork.Transaction).FirstOrDefault();
        }

        public PersonalDiscount Find(object query)
        {
            throw new NotImplementedException();
        }

        public List<PersonalDiscount> List(ListQuery listQuery, object query = null)
        {
            if (listQuery == null) throw new ArgumentNullException(nameof(listQuery));

            var order = listQuery.Order(string.Empty, new Sort
            {
                Name = "Id",
                Direction = SortDirection.Desc
            });
            var page = listQuery.Page();

            return UnitOfWork.Session.Query<PersonalDiscount>($@"
SELECT *
FROM PersonalDiscounts
WHERE Locked = 0
{order}", UnitOfWork.Transaction).ToList();
        }

        public int Count(ListQuery listQuery, object query = null)
        {
            if (listQuery == null) throw new ArgumentNullException(nameof(listQuery));

            return UnitOfWork.Session.ExecuteScalar<int>($@"
SELECT COUNT(*)
FROM PersonalDiscounts
WHERE Locked = 0", UnitOfWork.Transaction);
        }
    }
}