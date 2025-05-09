using Dapper;
using Pawnshop.Core;
using Pawnshop.Core.Impl;
using Pawnshop.Data.Models.Contracts;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Transactions;

namespace Pawnshop.Data.Access
{
    public class RestructuredContractPaymentScheduleRepository : RepositoryBase
    {
        public RestructuredContractPaymentScheduleRepository(IUnitOfWork unitOfWork) : base(unitOfWork)
        {
        }

        public void Insert(RestructuredContractPaymentSchedule entity)
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));

            using (var transaction = BeginTransaction())
            {
                entity.Id = UnitOfWork.Session.ExecuteScalar<int>(@"
                INSERT INTO RestructuredContractPaymentSchedule
                    (Id, PaymentBalanceOfDefferedPercent, AmortizedBalanceOfDefferedPercent, PaymentBalanceOfOverduePercent, AmortizedBalanceOfOverduePercent, 
                    PaymentPenaltyOfOverdueDebt, AmortizedPenaltyOfOverdueDebt, PaymentPenaltyOfOverduePercent, AmortizedPenaltyOfOverduePercent, 
                    PenaltyOfOverduePaymentAmortizedPercent, RestructuredCreateDate)
                VALUES
                    (@Id, @PaymentBalanceOfDefferedPercent, @AmortizedBalanceOfDefferedPercent, @PaymentBalanceOfOverduePercent, @AmortizedBalanceOfOverduePercent, 
                    @PaymentPenaltyOfOverdueDebt, @AmortizedPenaltyOfOverdueDebt, @PaymentPenaltyOfOverduePercent, @AmortizedPenaltyOfOverduePercent, 
                    @PenaltyOfOverduePaymentAmortizedPercent, @RestructuredCreateDate)
                SELECT SCOPE_IDENTITY()", entity, UnitOfWork.Transaction);
                transaction.Commit();
            }
        }

        public async Task InsertAsync(RestructuredContractPaymentSchedule entity)
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));

            using (var transaction = BeginTransaction())
            {
                entity.RestructuredCreateDate = DateTime.Now;
                entity.Id = await UnitOfWork.Session.ExecuteScalarAsync<int>(@"
                INSERT INTO RestructuredContractPaymentSchedule
                    (Id, PaymentBalanceOfDefferedPercent, AmortizedBalanceOfDefferedPercent, PaymentBalanceOfOverduePercent, AmortizedBalanceOfOverduePercent, 
                    PaymentPenaltyOfOverdueDebt, AmortizedPenaltyOfOverdueDebt, PaymentPenaltyOfOverduePercent, AmortizedPenaltyOfOverduePercent, 
                    PenaltyOfOverduePaymentAmortizedPercent, RestructuredCreateDate)
                VALUES
                    (@Id, @PaymentBalanceOfDefferedPercent, @AmortizedBalanceOfDefferedPercent, @PaymentBalanceOfOverduePercent, @AmortizedBalanceOfOverduePercent, 
                    @PaymentPenaltyOfOverdueDebt, @AmortizedPenaltyOfOverdueDebt, @PaymentPenaltyOfOverduePercent, @AmortizedPenaltyOfOverduePercent, 
                    @PenaltyOfOverduePaymentAmortizedPercent, @RestructuredCreateDate)
                SELECT SCOPE_IDENTITY()", entity, UnitOfWork.Transaction);
                transaction.Commit();
            }
        }

        public RestructuredContractPaymentSchedule Get(int id)
        {
            return UnitOfWork.Session.ExecuteScalar<RestructuredContractPaymentSchedule>(@"
                SELECT * FROM RestructuredContractPaymentSchedule
                WHERE Id = @id", new { id });
        }

        public bool IsRestructuredScheduleItemExists(int id)
        {
            return UnitOfWork.Session.ExecuteScalar<bool>(@"
                SELECT 1 FROM RestructuredContractPaymentSchedule
                WHERE Id = @id", new { id });
        }

        public void Update(RestructuredContractPaymentSchedule entity)
        {
            using (var transaction = BeginTransaction())
            {
                UnitOfWork.Session.Execute(@"
UPDATE RestructuredContractPaymentSchedule
SET PaymentBalanceOfDefferedPercent = @PaymentBalanceOfDefferedPercent, AmortizedBalanceOfDefferedPercent = @AmortizedBalanceOfDefferedPercent, PaymentBalanceOfOverduePercent = @PaymentBalanceOfOverduePercent, 
AmortizedBalanceOfOverduePercent = @AmortizedBalanceOfOverduePercent, PaymentPenaltyOfOverdueDebt = @PaymentPenaltyOfOverdueDebt, AmortizedPenaltyOfOverdueDebt = @AmortizedPenaltyOfOverdueDebt, 
PaymentPenaltyOfOverduePercent = @PaymentPenaltyOfOverduePercent, AmortizedPenaltyOfOverduePercent = @AmortizedPenaltyOfOverduePercent, 
PenaltyOfOverduePaymentAmortizedPercent = @PenaltyOfOverduePaymentAmortizedPercent, RestructuredDeleteDate = @RestructuredDeleteDate, RestructuredCreateDate = @RestructuredCreateDate
WHERE Id = @Id", entity, UnitOfWork.Transaction);

                transaction.Commit();
            }
        }

        public async Task<IEnumerable<RestructuredContractPaymentSchedule>> GetListByContractId(int contractId)
        {
            return await UnitOfWork.Session.QueryAsync<RestructuredContractPaymentSchedule>(@"
                SELECT *,
                CASE 
                    WHEN ActionId IS NOT NULL AND ((SELECT ActionType FROM ContractActions WHERE Id = ActionId) = 200 
				    OR (SELECT ActionType FROM ContractActions WHERE Id = ActionId) = 201) THEN 1
                    WHEN ActionId IS NULL AND EXISTS (SELECT 1 FROM ClientDeferments cd WHERE cd.ContractId = cps.ContractId AND cd.StartDate < cps.Date AND Status = 10) THEN 2
                    WHEN ActionId IS NOT NULL AND (Select ActionType From ContractActions Where Id = ActionId) = 40 THEN 15
                    WHEN ActionId IS NOT NULL THEN 10
                    WHEN Canceled IS NOT NULL THEN 30	
                    WHEN ISNULL(NextWorkingDate, Date) < CONVERT(DATE, dbo.GETASTANADATE()) THEN 20
                    ELSE 0
                END AS Status
                FROM ContractPaymentSchedule cps WITH(NOLOCK)
	            LEFT JOIN RestructuredContractPaymentSchedule rcps on rcps.Id = cps.Id
                WHERE ContractId = @id AND DeleteDate IS NULL
                ORDER BY Date, cps.Id", new
                { id = contractId
                }, UnitOfWork.Transaction);
        }
    }
}
