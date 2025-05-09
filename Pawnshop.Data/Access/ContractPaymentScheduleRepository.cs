using Dapper;
using Pawnshop.Core;
using Pawnshop.Core.Impl;
using Pawnshop.Core.Queries;
using Pawnshop.Data.Models.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Pawnshop.Data.Access
{
    public class ContractPaymentScheduleRepository : RepositoryBase, IRepository<ContractPaymentSchedule>
    {
        public ContractPaymentScheduleRepository(IUnitOfWork unitOfWork) : base(unitOfWork)
        {
        }

        public void Insert(ContractPaymentSchedule entity)
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));

            entity.CreateDate = DateTime.Now;
            entity.Id = UnitOfWork.Session.ExecuteScalar<int>(@"
                INSERT INTO ContractPaymentSchedule
                    (Revision, ContractId, Date, ActualDate, 
                    DebtLeft, DebtCost, PercentCost, PenaltyCost, 
                    CreateDate, DeleteDate, ActionId, Canceled, Period, 
                    Prolongated, NextWorkingDate)
                VALUES
                    (@Revision, @ContractId, @Date, @ActualDate, 
                    @DebtLeft, @DebtCost, @PercentCost, @PenaltyCost, 
                    @CreateDate, @DeleteDate, @ActionId, @Canceled, @Period, 
                    @Prolongated, @NextWorkingDate)
                SELECT SCOPE_IDENTITY()", entity, UnitOfWork.Transaction);
        }

        public async Task InsertAsync(ContractPaymentSchedule entity)
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));

            entity.CreateDate = DateTime.Now;
            entity.Id = await UnitOfWork.Session.ExecuteScalarAsync<int>(@"
                INSERT INTO ContractPaymentSchedule
                    (Revision, ContractId, Date, ActualDate, 
                    DebtLeft, DebtCost, PercentCost, PenaltyCost, 
                    CreateDate, DeleteDate, ActionId, Canceled, Period, 
                    Prolongated, NextWorkingDate)
                VALUES
                    (@Revision, @ContractId, @Date, @ActualDate, 
                    @DebtLeft, @DebtCost, @PercentCost, @PenaltyCost, 
                    @CreateDate, @DeleteDate, @ActionId, @Canceled, @Period, 
                    @Prolongated, @NextWorkingDate)
                SELECT SCOPE_IDENTITY()", entity, UnitOfWork.Transaction);
        }

        public void Update(ContractPaymentSchedule entity)
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));

            UnitOfWork.Session.Execute(@"
                UPDATE ContractPaymentSchedule 
                SET 
                    Revision = @Revision, 
                    ContractId = @ContractId, 
                    Date = @Date, 
                    ActualDate = @ActualDate, 
                    DebtLeft = @DebtLeft, 
                    DebtCost = @DebtCost, 
                    PercentCost = @PercentCost, 
                    PenaltyCost = @PenaltyCost, 
                    CreateDate = @CreateDate,
                    DeleteDate = @DeleteDate, 
                    ActionId = @ActionId, 
                    Canceled = @Canceled, 
                    Period = @Period, 
                    Prolongated = @Prolongated,
                    NextWorkingDate = @NextWorkingDate
                WHERE Id = @Id", entity, UnitOfWork.Transaction);
        }

        public async Task UpdateAsync(ContractPaymentSchedule entity)
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));

            await UnitOfWork.Session.ExecuteAsync(@"
                UPDATE ContractPaymentSchedule 
                SET 
                    Revision = @Revision, 
                    ContractId = @ContractId, 
                    Date = @Date, 
                    ActualDate = @ActualDate, 
                    DebtLeft = @DebtLeft, 
                    DebtCost = @DebtCost, 
                    PercentCost = @PercentCost, 
                    PenaltyCost = @PenaltyCost, 
                    CreateDate = @CreateDate,
                    DeleteDate = @DeleteDate, 
                    ActionId = @ActionId, 
                    Canceled = @Canceled, 
                    Period = @Period, 
                    Prolongated = @Prolongated,
                    NextWorkingDate = @NextWorkingDate
                WHERE Id = @Id", entity, UnitOfWork.Transaction);
        }

        public ContractPaymentScheduleRevision GetRevision(int id, int revision)
        {
            return UnitOfWork.Session.Query<ContractPaymentScheduleRevision>(@"
                    SELECT cpsr.* FROM ContractPaymentScheduleRevisions cpsr
                    WHERE cpsr.ContractPaymentScheduleId = @Id AND cpsr.Revision = @revision",
                    new { id, revision }, UnitOfWork.Transaction).FirstOrDefault();
        }

        public void Delete(int id)
        {
            DateTime deleteDate = DateTime.Now;
            UnitOfWork.Session.Query<ContractPaymentSchedule>(@"
                UPDATE FROM ContractPaymentSchedule
                SET DeleteDate = deleteDate
                WHERE cps.Id = @id",
            new { id, deleteDate }, UnitOfWork.Transaction).FirstOrDefault();
        }

        public void DeleteByContractId(int contractId)
        {
            UnitOfWork.Session.Query<ContractPaymentSchedule>(@"
                UPDATE FROM ContractPaymentSchedule
                SET DeleteDate = dbo.GetAstanaDate()
                WHERE cps.ContractId = @contractId",
            new { contractId }, UnitOfWork.Transaction).FirstOrDefault();
        }

        public ContractPaymentSchedule Get(int id)
        {
            return UnitOfWork.Session.Query<ContractPaymentSchedule>(@"
                    SELECT cps.* FROM ContractPaymentSchedule cps
                    WHERE cps.Id = @Id AND cps.DeleteDate IS NULL",
                    new { id }, UnitOfWork.Transaction).FirstOrDefault();
        }

        public ContractPaymentSchedule GetWithDeleted(int id)
        {
	        return UnitOfWork.Session.Query<ContractPaymentSchedule>(@"
                    SELECT cps.* FROM ContractPaymentSchedule cps
                    WHERE cps.Id = @Id",
		        new { id }, UnitOfWork.Transaction).FirstOrDefault();
        }

		public List<ContractPaymentSchedule> GetListByContractId(int contractId)
        {
            return UnitOfWork.Session.Query<ContractPaymentSchedule>(@"
                    SELECT cps.* FROM ContractPaymentSchedule cps
                    WHERE cps.ContractId = @contractId AND cps.DeleteDate IS NULL",
                new { contractId }, UnitOfWork.Transaction).ToList();
        }

        public async Task<IEnumerable<ContractPaymentSchedule>> GetListByContractIdAsync(int contractId)
        {
            return await UnitOfWork.Session.QueryAsync<ContractPaymentSchedule>(@"
                    SELECT cps.* FROM ContractPaymentSchedule cps
                    WHERE cps.ContractId = @contractId AND cps.DeleteDate IS NULL",
                new { contractId }, UnitOfWork.Transaction);
        }

        public async Task<IEnumerable<RestructuredContractPaymentSchedule>> GetUpdatedListByContractIdAsync(int contractId)
        {
            return await UnitOfWork.Session.QueryAsync<RestructuredContractPaymentSchedule>(@"
                    SELECT cps.* FROM ContractPaymentSchedule cps
                    WHERE cps.ContractId = @contractId AND cps.DeleteDate IS NULL",
                new { contractId }, UnitOfWork.Transaction);
        }

        public List<ContractPaymentSchedule> List(ListQuery listQuery, object query)
        {
            throw new NotImplementedException();
        }

        public ContractPaymentSchedule Find(object query)
        {
            throw new NotImplementedException();
        }

        public int Count(ListQuery listQuery, object query)
        {
            throw new NotImplementedException();
        }

        public void LogChanges(ContractPaymentSchedule entity, int userId)
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));

            var log = new ContractPaymentScheduleRevision
            {
                ContractPaymentScheduleId = entity.Id,
                Status = entity.Status,
                Revision = entity.Revision,
                ContractId = entity.ContractId,
                Date = entity.Date,
                ActualDate = entity.ActualDate,
                DebtLeft = entity.DebtLeft,
                DebtCost = entity.DebtCost,
                PercentCost = entity.PercentCost,
                PenaltyCost = entity.PenaltyCost,
                CreateDate = entity.CreateDate,
                DeleteDate = entity.DeleteDate,
                ActionId = entity.ActionId,
                Canceled = entity.Canceled,
                Period = entity.Period,
                Prolongated = entity.Prolongated,
                UpdatedByAuthorId = userId,
                UpdateDate = DateTime.Now,
                NextWorkingDate = entity.NextWorkingDate
            };

            UnitOfWork.Session.Execute(@"
                INSERT INTO ContractPaymentScheduleRevisions
                    (ContractPaymentScheduleId, Revision, ContractId, Date, 
                    ActualDate, DebtLeft, DebtCost, PercentCost, PenaltyCost, 
                    CreateDate, DeleteDate, ActionId, Canceled, Period, 
                    Prolongated, UpdatedByAuthorId, UpdateDate, NextWorkingDate)
                VALUES 
                    (@ContractPaymentScheduleId, @Revision, @ContractId, @Date, 
                    @ActualDate, @DebtLeft, @DebtCost, @PercentCost, @PenaltyCost, 
                    @CreateDate, @DeleteDate, @ActionId, @Canceled, @Period, 
                    @Prolongated, @UpdatedByAuthorId, @UpdateDate, @NextWorkingDate)
                SELECT SCOPE_IDENTITY()", log, UnitOfWork.Transaction);
        }

        public async Task<List<ContractPaymentScheduleVersion>> GetScheduleVersions(int ContractId)
        {
            return UnitOfWork.Session.Query<ContractPaymentScheduleVersion>(@"
                Select ROW_NUMBER() OVER(ORDER BY CreateDate ASC) AS Number, ContractId, ActionId, CreateDate as ScheduleDate, Id as HistoryId
                from ContractPaymentScheduleHistory where ContractId = @ContractId
                and  DeleteDate is null
                and Status = 20", new { ContractId },
                    UnitOfWork.Transaction).ToList();
        }

        public async Task<List<ContractPaymentScheduleVersion>> GetScheduleVersionsWithoutChangedControlDate(int ContractId)
        {
	        return UnitOfWork.Session.Query<ContractPaymentScheduleVersion>(@"
                 Select ROW_NUMBER() OVER(ORDER BY h.CreateDate ASC) AS Number, h.ContractId, h.ActionId, h.CreateDate as ScheduleDate, h.Id as HistoryId
                from ContractPaymentScheduleHistory h 
				left join ContractActions ca on ca.Id=h.ActionId
				where h.ContractId = @ContractId
                and  h.DeleteDate is null
                and h.Status = 20 and ca.ActionType!=190", new { ContractId },
		        UnitOfWork.Transaction).ToList();
        }

		public async Task<List<RestructuredContractPaymentSchedule>> GetScheduleByAction(int ActionId)
        {
            return UnitOfWork.Session.Query<RestructuredContractPaymentSchedule>(@"
                Select items.Id, items.Date,
                case when items.ActualDate is null then
                (select date from ContractActions ca
                inner join Contracts c on ca.ContractId = c.Id
                where ca.Id = @ActionId
                and c.PercentPaymentType != 20)
                else items.ActualDate end as ActualDate,
                items.DebtLeft, items.DebtCost, items.PercentCost, items.PenaltyCost, items.ActionType, items.DeleteDate, items.Status, 
                rcps.PaymentBalanceOfDefferedPercent, rcps.AmortizedBalanceOfDefferedPercent, rcps.PaymentBalanceOfOverduePercent, rcps.AmortizedBalanceOfOverduePercent, rcps.PaymentPenaltyOfOverdueDebt, rcps.AmortizedPenaltyOfOverdueDebt, rcps.PaymentPenaltyOfOverduePercent, rcps.AmortizedPenaltyOfOverduePercent, rcps.PenaltyOfOverduePaymentAmortizedPercent
                from ContractPaymentScheduleHistoryItems items
                inner join ContractPaymentScheduleHistory his on items.ContractPaymentScheduleHistoryId = his.Id
                LEFT JOIN RestructuredContractPaymentSchedule rcps on rcps.Id = items.ContractPaymentScheduleId
                where his.DeleteDate is null and his.ActionId = @ActionId
                order by items.Date, items.Id", new { ActionId },
                    UnitOfWork.Transaction).ToList();
        }

        public async Task<List<ContractPaymentSchedule>> GetScheduleByActionForChangeDate(int ActionId)
        {
	        return UnitOfWork.Session.Query<ContractPaymentSchedule>(@"
                Select items.ContractPaymentScheduleId as Id, items.Date, 
                case when items.ActualDate is null then
                (select date from ContractActions ca
                inner join Contracts c on ca.ContractId = c.Id
                where ca.Id = @ActionId
                and c.PercentPaymentType != 20)
                else items.ActualDate end as ActualDate,
                items.DebtLeft, items.DebtCost, items.PercentCost, items.PenaltyCost, items.ActionType, items.DeleteDate, items.Status
                from ContractPaymentScheduleHistoryItems items
                inner join ContractPaymentScheduleHistory his on items.ContractPaymentScheduleHistoryId = his.Id
                where his.DeleteDate is null and his.ActionId = @ActionId
                order by items.Date, items.Id", new { ActionId },
		        UnitOfWork.Transaction).ToList();
        }

		public async Task<List<ContractPaymentSchedule>> GetScheduleByHistory(int HistoryId)
		{
			return UnitOfWork.Session.Query<ContractPaymentSchedule>(@"
                Select items.Id, items.Date, items.ActualDate as ActualDate,
                items.DebtLeft, items.DebtCost, items.PercentCost, items.PenaltyCost, items.ActionType, items.DeleteDate, items.Status
                from ContractPaymentScheduleHistoryItems items
                inner join ContractPaymentScheduleHistory his on items.ContractPaymentScheduleHistoryId = his.Id
                where his.DeleteDate is null and his.Id = @HistoryId
                order by items.Date", new { HistoryId },
					UnitOfWork.Transaction).ToList();
		}

		public async Task<List<ContractPaymentSchedule>> GetScheduleRowsAfterPartialPayment(int ActionId, int ContractId)
        {
            return UnitOfWork.Session.Query<ContractPaymentSchedule>(@"
                Select * from ContractPaymentScheduleHistoryItems items
                inner join ContractPaymentScheduleHistory his on items.ContractPaymentScheduleHistoryId = his.Id
                where his.DeleteDate is null and his.ContractId = @ContractId and his.ActionId = @ActionId
                and items.Id > (Select top 1  Id from ContractPaymentScheduleHistoryItems where DeleteDate is null and
                ContractPaymentScheduleHistoryId = 
                (Select Id from ContractPaymentScheduleHistory where DeleteDate is null 
                and ActionType = 40 and ContractId = @ContractId and ActionId = @ActionId) order by Id desc)
                ", new { ActionId, ContractId },
                    UnitOfWork.Transaction).ToList();
        }

        public async Task<List<ContractPaymentSchedule>> GetScheduleRowsBeforePartialPayment(int ContractId)
        {
            return UnitOfWork.Session.Query<ContractPaymentSchedule>(@"
                Select * from ContractPaymentScheduleHistoryItems items
                inner join ContractPaymentScheduleHistory his on items.ContractPaymentScheduleHistoryId = his.Id
                where his.DeleteDate is null and his.ContractId = @ContractId
                ", new { ContractId },
                    UnitOfWork.Transaction).ToList();
        }

        public async Task RollbackScheduleToPreviousPartialPayment(int ContractId, int ActionId, decimal Cost)
        {
            UnitOfWork.Session.Execute(@"
                if(exists(Select * from ContractPaymentScheduleHistory where ContractId = @ContractId and ActionId = @ActionId))
                begin
                update ContractPaymentSchedule set DeleteDate = getDate() where ContractId = @ContractId and ActionId = @ActionId and DebtCost = @Cost;
                update ContractPaymentSchedule set DeleteDate = getDate() where ContractId = @ContractId and DeleteDate is null and ActionId is null;
                update s set
                s.Date = i.Date,
                s.ActualDate = i.ActualDate,
                s.DebtLeft = i.DebtLeft,
                s.DebtCost = i.DebtCost,
                s.PercentCost = i.PercentCost,
                s.PenaltyCost = i.PenaltyCost,
                s.ActionId = i.OldActionId,
                s.DeleteDate = null
                from ContractPaymentSchedule s
                inner join ContractPaymentScheduleHistoryItems i on i.ContractPaymentScheduleId = s.Id
                inner join ContractPaymentScheduleHistory h on i.ContractPaymentScheduleHistoryId = h.Id 
                where h.ContractId = @ContractId and h.ActionId = @ActionId and h.DeleteDate is null

                Update ContractPaymentScheduleHistory set DeleteDate = GetDate() where ContractId = @ContractId and ActionId = @ActionId;
                Update ContractPaymentScheduleHistoryItems set DeleteDate = GetDate() where ContractPaymentScheduleHistoryId = 
                (Select Id from ContractPaymentScheduleHistory where ContractId = @ContractId and ActionId = @ActionId)
                end", new { ActionId, ContractId, Cost }, UnitOfWork.Transaction);
        }

        public async Task UpdateActionIdForPartialPayment(int ActionId, DateTime ActionDate, int ContractId)
        {
            UnitOfWork.Session.Execute(@"
            DECLARE @UpdatedIDs table (ID int)
            Update ContractPaymentSchedule set ActionId = @ActionId
            OUTPUT inserted.Id INTO @UpdatedIDs
            where ActualDate = Convert(Date, @ActionDate) and Date = ActualDate And ActionId is null
            and ContractId = @ContractId;
            Update ContractPaymentScheduleRevisions set ActionId = @ActionId where
            ContractPaymentScheduleId = (Select top 1 ID from @UpdatedIDs)",
            new { ActionId, ActionDate, ContractId },
                    UnitOfWork.Transaction);
        }

        public async Task UpdateActionIdForPartialPaymentUnpaid(int ActionId, DateTime ActionDate, int ContractId, decimal penaltyCost, bool isEndPeriod)
        {
            string sql = "";
            if (isEndPeriod)
            {
                sql = @"With UpdateContractPaymentScheduleView as (
                Select top 1 * from ContractPaymentSchedule 
                where
                ActionId is null and Prolongated is null  and DeleteDate is null
                and ContractId = @ContractId
                and Date <= Convert(Date, @ActionDate) order by CreateDate) 
                Update UpdateContractPaymentScheduleView set 
                ActionId = @ActionId,
                ActualDate = @ActionDate,
                Prolongated = @ActionDate,
                DebtLeft = DebtCost, 
                DebtCost = 0,
                PenaltyCost = @penaltyCost";
            }
            else
            {
                sql = @"With UpdateContractPaymentScheduleView as (
                Select * from ContractPaymentSchedule 
                where
                ActionId is null and DeleteDate is null
                and ContractId = @ContractId
                and Date <= Convert(Date, @ActionDate)) 
                Update UpdateContractPaymentScheduleView set 
                ActionId = @ActionId,
                ActualDate = @ActionDate";
            }
            UnitOfWork.Session.Execute(sql,
            new { ActionId, ActionDate, ContractId, penaltyCost },
                    UnitOfWork.Transaction);
        }

        public async Task<ContractPaymentSchedule> GetUnpaidSchedule(int ContractId)
        {
            return UnitOfWork.Session.Query<ContractPaymentSchedule>(@"
            Select top 1 * from ContractPaymentSchedule where DeleteDate is null and ActionId is null
            and ContractId = @ContractId
            order by CreateDate desc",
            new { ContractId },
                    UnitOfWork.Transaction).FirstOrDefault();
        }

        public async Task<ContractPaymentSchedule> GetNextPaymentSchedule(int ContractId, bool nowPeriodPayment = false)
        {
            var predicate = "WHERE DeleteDate IS NULL AND ActionId IS NULL AND ContractId = @ContractId";

            if (nowPeriodPayment)
                predicate += " AND Date > CAST(dbo.getastanadate() AS DATE)";

            return UnitOfWork.Session.Query<ContractPaymentSchedule>($@"SELECT TOP 1 *
  FROM ContractPaymentSchedule
 {predicate}
 ORDER BY Date",
                new { ContractId }, UnitOfWork.Transaction).FirstOrDefault();
        }

        public async Task<int> InsertContractPaymentScheduleHistory(int ContractId, int ActionId, int Status)
        {
            return UnitOfWork.Session.ExecuteScalar<int>(@"
                Insert into ContractPaymentScheduleHistory values
                (@ContractId, @ActionId, GetDate(), @Status, null)
                Select Scope_Identity()", new { ContractId, ActionId, Status },
                UnitOfWork.Transaction);
        }

        public async Task InsertContractPaymentScheduleHistoryItems(int ContractPaymentScheduleHistoryId, ContractPaymentSchedule item)
        {
            UnitOfWork.Session.Execute(@"
                INSERT INTO ContractPaymentScheduleHistoryItems
                    (ContractPaymentScheduleHistoryId, Date, ActualDate, 
                    DebtLeft, DebtCost, PercentCost, PenaltyCost, ActionType, Status, ContractPaymentScheduleId, OldActionId)
                VALUES
                    (@ContractPaymentScheduleHistoryId, @Date, @ActualDate, 
                    @DebtLeft, @DebtCost, @PercentCost, @PenaltyCost, @ActionType, @Status, @Id, @ActionId)",
                    new
                    {
                        ContractPaymentScheduleHistoryId,
                        item.Date,
                        item.ActualDate,
                        item.DebtLeft,
                        item.DebtCost,
                        item.PercentCost,
                        item.PenaltyCost,
                        item.ActionType,
                        item.Status,
                        item.Id,
                        item.ActionId
                    }, UnitOfWork.Transaction);
        }

        public async Task UpdateContractPaymentScheduleHistoryStatus(int ContractId, int ActionId, int Status)
        {
            UnitOfWork.Session.Execute(@"
                Update ContractPaymentScheduleHistory set Status = @Status where ContractId = @ContractId and ActionId = @ActionId",
            new { ContractId, ActionId, Status }, UnitOfWork.Transaction);
        }

        public async Task DeleteContractPaymentScheduleHistory(int ContractId, int ActionId)
        {
	        UnitOfWork.Session.Execute(@"
                Update ContractPaymentScheduleHistory set DeleteDate = getdate() where ContractId = @ContractId and ActionId = @ActionId",
		        new { ContractId, ActionId }, UnitOfWork.Transaction);
        }

		public async Task<List<ContractPaymentSchedule>> GetScheduleRowsAfterLastPartialPayment(int ContractId, int ActionId)
        {
            return UnitOfWork.Session.Query<ContractPaymentSchedule>(@"
                Select * from ContractPaymentSchedule where Date > (
                Select top 1 cps.Date from ContractPaymentSchedule cps
                where cps.ActionId = @ActionId
                order by cps.Date desc, cps.Id desc)
                and contractId = @ContractId and DeleteDate is null
                ", new { ContractId, ActionId },
                    UnitOfWork.Transaction).ToList();
        }

        public async Task<ContractPaymentSchedule> GetLastPartialPaymentScheduleRow(int ContractId, int ActionId)
        {
            return UnitOfWork.Session.Query<ContractPaymentSchedule>(@"
                Select cps.* from ContractPaymentSchedule cps
                inner join ContractActions ca on cps.ActionId = ca.Id
                where ca.ActionType = 40 and Round(ca.Cost, -1) = Round(cps.DebtCost, -1)
                and cps.ContractId = @ContractId and cps.ActionId = @ActionId
                ", new { ContractId, ActionId },
                    UnitOfWork.Transaction).FirstOrDefault();
        }

        public async Task<ContractPaymentSchedule> GetLastPartialPaymentScheduleHistoryRow(int ContractId, int ActionId)
        {
            return UnitOfWork.Session.Query<ContractPaymentSchedule>(@"
                Select cps.* from ContractPaymentScheduleHistoryItems cps
                inner join ContractPaymentScheduleHistory c on c.id = cps.ContractPaymentScheduleHistoryId
                inner join ContractActions ca on c.ActionId = ca.Id
                where cps.ActionType = 40
                and c.ContractId = @ContractId and c.ActionId = @ActionId
                ", new { ContractId, ActionId },
                    UnitOfWork.Transaction).FirstOrDefault();
        }

        public async Task<decimal> GetAverageMonthlyPaymentAsync(int contractId)
        {
            return await UnitOfWork.Session.ExecuteScalarAsync<decimal>(@"SELECT SUM(DebtCost + PercentCost)/ count(1) AS AverageMonthlyPayment
  FROM ContractPaymentSchedule
 WHERE DeleteDate IS NULL
   AND ContractId = @contractId",
                new { contractId }, UnitOfWork.Transaction);
        }

        public async Task<IEnumerable<ContractPaymentSchedule>> GetContractPaymentSchedules(int contractId)
        {
            return await UnitOfWork.Session.QueryAsync<ContractPaymentSchedule>(@"SELECT *,
       CASE 
        WHEN ActionId IS NOT NULL 
			AND ((SELECT ActionType FROM ContractActions WHERE Id = ActionId) = 200 
				OR (SELECT ActionType FROM ContractActions WHERE Id = ActionId) = 201)
			THEN 1
         WHEN ActionId IS NOT NULL
              AND (SELECT ActionType FROM ContractActions WHERE Id = ActionId) = 40
              AND (SELECT ROUND(Cost, -1) FROM ContractActions Where Id = ACTIONID) = ROUND(DebtCost, -1)
           THEN 15
         WHEN ActionId IS NOT NULL
           THEN 10
         WHEN Canceled IS NOT NULL
           THEN 30	
         WHEN ISNULL(NextWorkingDate, Date) < CONVERT(DATE, dbo.GETASTANADATE())
           THEN 20
         ELSE 0
       END AS Status
  FROM ContractPaymentSchedule WITH(NOLOCK)
 WHERE DeleteDate IS NULL
   AND ContractId = @contractId
 ORDER BY Date, Id",
                new { contractId }, UnitOfWork.Transaction);
        }
    }
}
