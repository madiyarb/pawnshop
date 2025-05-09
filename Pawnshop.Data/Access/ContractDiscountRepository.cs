using System;
using System.Collections.Generic;
using System.Linq;
using Dapper;
using Pawnshop.Core;
using Pawnshop.Core.Impl;
using Pawnshop.Core.Queries;
using Pawnshop.Data.Models.Clients;
using Pawnshop.Data.Models.Contracts;
using Pawnshop.Data.Models.Contracts.Discounts;
using Pawnshop.Data.Models.Dictionaries;
using Pawnshop.Data.Models.Membership;

namespace Pawnshop.Data.Access
{
    public class ContractDiscountRepository : RepositoryBase, IRepository<ContractDiscount>
    {
        public ContractDiscountRepository(IUnitOfWork unitOfWork) : base(unitOfWork)
        {
        }

        public void Insert(ContractDiscount entity)
        {
            using (var transaction = BeginTransaction())
            {
                entity.Id = UnitOfWork.Session.QuerySingleOrDefault<int>(@"
                    INSERT INTO ContractDiscounts
                        (IsTypical, BeginDate, EndDate, PersonalDiscountId, 
                         ContractActionId,PercentDiscountSum, OverduePercentDiscountSum,
                         PercentPenaltyDiscountSum, DebtPenaltyDiscountSum,
                         Note, AuthorId, CreateDate, ContractId)
                    VALUES
                         (@IsTypical, @BeginDate, @EndDate, @PersonalDiscountId, 
                         @ContractActionId,@PercentDiscountSum, @OverduePercentDiscountSum,
                         @PercentPenaltyDiscountSum, @DebtPenaltyDiscountSum,
                         @Note, @AuthorId, @CreateDate, @ContractId)
                    SELECT SCOPE_IDENTITY()", entity, UnitOfWork.Transaction);

                transaction.Commit();
            }
        }

        public void Update(ContractDiscount entity)
        {
            using (var transaction = BeginTransaction())
            {
                UnitOfWork.Session.Execute(@"
                    UPDATE ContractDiscounts
                    SET 
                        IsTypical = @IsTypical, 
                        BeginDate = @BeginDate, 
                        EndDate = @EndDate, 
                        PersonalDiscountId = @PersonalDiscountId, 
                        ContractActionId = @ContractActionId,
                        PercentDiscountSum = @PercentDiscountSum, 
                        OverduePercentDiscountSum = @OverduePercentDiscountSum,
                        PercentPenaltyDiscountSum = @PercentPenaltyDiscountSum,
                        DebtPenaltyDiscountSum = @DebtPenaltyDiscountSum,
                        Note = @Note, 
                        AuthorId = @AuthorId,
                        CreateDate = @CreateDate,
                        ContractId = @ContractId,
                        DeleteDate = @DeleteDate
                    WHERE Id = @Id", entity, UnitOfWork.Transaction);

                transaction.Commit();
            }
        }

        public void Delete(int id)
        {
            UnitOfWork.Session.Execute("UPDATE ContractDiscounts SET DeleteDate = dbo.GETASTANADATE() WHERE Id = @id", new { id }, UnitOfWork.Transaction);
        }
        public void CancelAction(int actionId)
        {
            UnitOfWork.Session.Execute("UPDATE ContractDiscounts SET ContractActionId = NULL WHERE ContractActionId = @actionId", new { actionId }, UnitOfWork.Transaction);
        }

        public ContractDiscount GetOnlyDiscount(int id)
        {
            return UnitOfWork.Session.Query<ContractDiscount>(@"
                SELECT 
                    cd.*
                FROM ContractDiscounts cd
                WHERE cd.Id = @id 
                    AND cd.DeleteDate IS NULL",  new { id }, 
                    UnitOfWork.Transaction).FirstOrDefault();
        }

        public ContractDiscount Get(int id)
        {
            return UnitOfWork.Session.Query<ContractDiscount, PersonalDiscount, ContractDiscount>(@"
                SELECT 
                CASE WHEN cd.DeleteDate IS NOT NULL THEN 20
                WHEN cd.ContractActionId IS NOT NULL THEN 10
                WHEN dbo.GETASTANADATE() NOT BETWEEN cd.BeginDate AND cd.EndDate THEN 30
                ELSE 0 END AS Status,
                cd.*, pd.*
                FROM ContractDiscounts cd
                LEFT JOIN PersonalDiscounts pd ON cd.PersonalDiscountId=pd.Id
                WHERE cd.Id = @id AND cd.DeleteDate IS NULL", (cd, pd) =>
            {
                if (cd != null)
                {
                    if (pd != null)
                    {
                        cd.PersonalDiscount = pd;
                    }
                }
                return cd;
            }, new { id }, UnitOfWork.Transaction).FirstOrDefault();
        }

        public List<ContractDiscount> GetByContractActionId(int contractActionId)
        {
            return UnitOfWork.Session.Query<ContractDiscount>(@"
                SELECT cd.* 
                FROM ContractDiscounts cd
                WHERE cd.ContractActionId = @contractActionId 
                    AND DeleteDate IS NULL", new { contractActionId }, UnitOfWork.Transaction).ToList();
        }

        public List<ContractDiscount> GetByContractIds(List<int> contractIds)
        {
            return UnitOfWork.Session.Query<ContractDiscount, PersonalDiscount, ContractDiscount>(@"
                SELECT 
                CASE WHEN cd.DeleteDate IS NOT NULL THEN 20
                WHEN cd.ContractActionId IS NOT NULL THEN 10
                WHEN dbo.GETASTANADATE() NOT BETWEEN cd.BeginDate AND cd.EndDate THEN 30
                ELSE 0 END AS Status,
                cd.*, pd.*
                FROM ContractDiscounts cd
                LEFT JOIN PersonalDiscounts pd ON cd.PersonalDiscountId=pd.Id
                WHERE cd.ContractId in @contractIds AND cd.DeleteDate IS NULL", (cd, pd) =>
            {
                if (cd != null)
                {
                    if (pd != null)
                    {
                        cd.PersonalDiscount = pd;
                    }
                }
                return cd;
            }, new { contractIds }, UnitOfWork.Transaction).ToList();
        }

        public List<ContractDiscount> GetByContractId(int contractId)
        {
            return UnitOfWork.Session.Query<ContractDiscount, PersonalDiscount, ContractDiscount>(@"
                SELECT 
                CASE WHEN cd.DeleteDate IS NOT NULL THEN 20
                WHEN cd.ContractActionId IS NOT NULL THEN 10
                WHEN dbo.GETASTANADATE() NOT BETWEEN cd.BeginDate AND cd.EndDate THEN 30
                ELSE 0 END AS Status,
                cd.*, pd.*
                FROM ContractDiscounts cd
                LEFT JOIN PersonalDiscounts pd ON cd.PersonalDiscountId=pd.Id
                WHERE cd.ContractId = @contractId AND cd.DeleteDate IS NULL", (cd, pd) =>
            {
                if (cd != null)
                {
                    if (pd != null)
                    {
                        cd.PersonalDiscount = pd;
                    }
                }
                return cd;
            }, new { contractId }, UnitOfWork.Transaction).ToList();
        }
        public ContractDiscount Find(object query)
        {
            throw new NotImplementedException();
        }

        public List<ContractDiscount> List(ListQuery listQuery, object query = null)
        {
            throw new NotImplementedException();
        }

        public int Count(ListQuery listQuery, object query = null)
        {
            throw new NotImplementedException();
        }
    }
}