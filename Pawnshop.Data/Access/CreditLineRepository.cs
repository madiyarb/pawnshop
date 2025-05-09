using System;
using Dapper;
using Pawnshop.Core;
using Pawnshop.Core.Impl;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Pawnshop.Data.Models.CreditLines;


namespace Pawnshop.Data.Access
{
    public sealed class CreditLineRepository : RepositoryBase
    {
        public CreditLineRepository(IUnitOfWork unitOfWork) : base(unitOfWork) { }

        public List<int> GetContractListByCreditLineId(int creditLineId)
        {
            return UnitOfWork.Session.Query<int>($@"SELECT Contracts.Id FROM dogs.Tranches JOIN Contracts ON Tranches.Id = Contracts.Id WHERE Contracts.Status = 30 AND Tranches.CreditLineId = @creditLineId",
                new { creditLineId },
                UnitOfWork.Transaction).ToList();
        }
        // докинуть IsLiquidityOn, IsInsuranceAdditionalLimitOn в таблицы
        public (bool IsLiquidityOn, bool IsInsuranceAdditionalLimitOn) GetCreditLineSettings(int creditLineId)
        {
            return UnitOfWork.Session.Query<(bool IsLiquidityOn, bool IsInsuranceAdditionalLimitOn)>($@"SELECT IsLiquidityOn, IsInsuranceAdditionalLimitOn FROM dogs.CreditLines WHERE Id = @creditLineId",
                new { creditLineId },
                UnitOfWork.Transaction).ToList().FirstOrDefault();
        }

        public (bool IsLiquidityOn, bool IsInsuranceAdditionalLimitOn) GetTrancheSettings(int trancheId)
        {
            return UnitOfWork.Session.Query<(bool IsLiquidityOn, bool IsInsuranceAdditionalLimitOn)>($@"SELECT IsLiquidityOn, IsInsuranceAdditionalLimitOn FROM dogs.Tranches WHERE Id = @trancheId",
                new { trancheId },
                UnitOfWork.Transaction).ToList().FirstOrDefault();
        }

        public async Task<IList<ContractBalance>>  GetBalancesByContractIdsAsync(IList<int> contractIds, DateTime? date = null)
        {
            if (date == null)
            {
                date = DateTime.Now;
            }
            date = date.Value.Date.AddDays(1).AddTicks(-1);
            var dateOnly = date.Value.Date;
            if (!contractIds.Any())
                return new List<ContractBalance>();

            return UnitOfWork.Session.Query<ContractBalance>(@"WITH cteAccountRecords AS (
  SELECT ar.*
    FROM AccountRecords ar
    JOIN (SELECT MAX(ar.Id) AS Id,
                 ar.AccountId
            FROM AccountRecords ar
            JOIN (SELECT MAX(Date) AS Date,
                         AccountId
                    FROM AccountRecords
                   WHERE DeleteDate IS NULL
                     AND Date <= @date
                   GROUP BY AccountId
                 ) AS gar On gar.AccountId = ar.AccountId AND gar.Date = ar.Date
            WHERE ar.DeleteDate is null 
            GROUP BY ar.AccountId) AS g ON g.AccountId = ar.AccountId AND g.Id = ar.Id
)
,contractBalance AS (
  SELECT ContractId
         ,ISNULL([DEPO], 0) AS DEPO
         ,ISNULL([ACCOUNT], 0) AS ACCOUNT
         ,ISNULL([PROFIT], 0) AS PROFIT
         ,ISNULL([OVERDUE_ACCOUNT], 0) AS OVERDUE_ACCOUNT
         ,ISNULL([OVERDUE_PROFIT], 0) AS OVERDUE_PROFIT
         ,ISNULL([PENY_PROFIT], 0) AS PENY_PROFIT
         ,ISNULL([PENY_ACCOUNT], 0) AS PENY_ACCOUNT
         ,ISNULL([EXPENSE], 0) AS EXPENSE
         ,ISNULL(PROFIT_OFFBALANCE, 0) AS PROFIT_OFFBALANCE
         ,ISNULL(OVERDUE_PROFIT_OFFBALANCE, 0) AS OVERDUE_PROFIT_OFFBALANCE
         ,ISNULL(PENY_ACCOUNT_OFFBALANCE, 0) AS PENY_ACCOUNT_OFFBALANCE
         ,ISNULL(PENY_PROFIT_OFFBALANCE, 0) AS PENY_PROFIT_OFFBALANCE
		 ,ISNULL(DEFERMENT_PROFIT, 0) AS DEFERMENT_PROFIT
         ,ISNULL(AMORTIZED_PROFIT, 0) AS AMORTIZED_PROFIT
         ,ISNULL(AMORTIZED_PENY_ACCOUNT, 0) AS AMORTIZED_PENY_ACCOUNT
         ,ISNULL(AMORTIZED_PENY_PROFIT, 0) AS AMORTIZED_PENY_PROFIT
    FROM (SELECT a.ContractId
                 ,ISNULL(IIF(ap.IsActive = 1, ar.OutgoingBalance * -1, ar.OutgoingBalance), 0) AS Balance
                 ,acs.Code
            FROM Accounts a
            JOIN AccountSettings acs ON acs.Id = a.AccountSettingId
            JOIN AccountPlans ap ON ap.Id = a.AccountPlanId
            LEFT JOIN cteAccountRecords ar ON ar.AccountId = a.Id
           WHERE a.DeleteDate IS NULL
             AND acs.Code IN ('DEPO', 'ACCOUNT', 'PROFIT', 'OVERDUE_ACCOUNT', 'OVERDUE_PROFIT', 'PENY_PROFIT', 'PENY_ACCOUNT', 'EXPENSE', 'PROFIT_OFFBALANCE', 'OVERDUE_PROFIT_OFFBALANCE', 'PENY_ACCOUNT_OFFBALANCE', 'PENY_PROFIT_OFFBALANCE', 
		    'DEFERMENT_PROFIT', 'AMORTIZED_PROFIT', 'AMORTIZED_PENY_ACCOUNT', 'AMORTIZED_PENY_PROFIT')
             AND a.ContractId IN @contractIds
         ) AS cb
   PIVOT (
     MAX(cb.Balance)
     FOR cb.Code IN ([DEPO], [ACCOUNT], [PROFIT], [OVERDUE_ACCOUNT], [OVERDUE_PROFIT], [PENY_PROFIT], [PENY_ACCOUNT], [EXPENSE], [PROFIT_OFFBALANCE], [OVERDUE_PROFIT_OFFBALANCE], [PENY_ACCOUNT_OFFBALANCE], [PENY_PROFIT_OFFBALANCE],
		[DEFERMENT_PROFIT], [AMORTIZED_PROFIT], [AMORTIZED_PENY_ACCOUNT], [AMORTIZED_PENY_PROFIT])
   ) AS pivotTable
),
wrapperBalance AS (
SELECT cb.ContractId
       ,IIF(sch.DebtLeft IS NULL, 0, cb.ACCOUNT - ISNULL(sch.DebtLeft, 0)) AS RepaymentAccountAmount
       ,IIF(sch.DebtLeft IS NULL, 0, cb.PROFIT - NextPeriodAccrual.Amount) AS RepaymentProfitAmount
       ,cb.ACCOUNT AS AccountAmount
       ,cb.PROFIT AS ProfitAmount
       ,cb.OVERDUE_ACCOUNT AS OverdueAccountAmount
       ,cb.OVERDUE_PROFIT AS OverdueProfitAmount
       ,cb.PENY_ACCOUNT + cb.PENY_PROFIT AS PenyAmount
       ,cb.PENY_ACCOUNT as PenyAccount
       ,cb.PENY_PROFIT as PenyProfit
       ,cb.EXPENSE AS ExpenseAmount
       ,cb.DEPO AS PrepaymentBalance
       ,cb.OVERDUE_ACCOUNT + cb.OVERDUE_PROFIT + cb.PENY_ACCOUNT + cb.PENY_PROFIT + cb.EXPENSE AS RepaymentAmount
       ,cb.ACCOUNT + cb.PROFIT + cb.OVERDUE_ACCOUNT + cb.OVERDUE_PROFIT + cb.PENY_ACCOUNT + cb.PENY_PROFIT + cb.EXPENSE AS RedemptionAmount
       ,PROFIT_OFFBALANCE AS ProfitOffBalance
       ,[OVERDUE_PROFIT_OFFBALANCE] AS OverdueProfitOffBalance
       ,[PENY_ACCOUNT_OFFBALANCE] AS PenyAccountOffBalance
       ,[PENY_PROFIT_OFFBALANCE] AS PenyProfitOffBalance
	   ,[DEFERMENT_PROFIT] AS DefermentProfit
       ,[AMORTIZED_PROFIT] AS AmortizedProfit
       ,[AMORTIZED_PENY_ACCOUNT] AS AmortizedPenyAccount
       ,[AMORTIZED_PENY_PROFIT] AS AmortizedPenyProfit
  FROM contractBalance cb
 OUTER APPLY (SELECT cps.DebtLeft, cps.Date, cps.NextWorkingDate
                FROM ContractPaymentSchedule cps
               WHERE ContractId = cb.ContractId
                 AND @dateOnly BETWEEN cps.Date AND ISNULL(cps.NextWorkingDate, cps.Date)
                 AND ActualDate IS NULL
                 AND DeleteDate IS NULL
       ) sch
  OUTER APPLY (SELECT TOP 1 cps.PercentCost, cps.Period
                 FROM ContractPaymentSchedule cps
                WHERE cps.ContractId = cb.ContractId
                  AND cps.DeleteDate IS NULL
                  AND cps.ActualDate IS NULL
                  AND cps.Date > @dateOnly
                  AND sch.NextWorkingDate IS NOT NULL
                ORDER BY Date
                ) nextSchedule
  OUTER APPLY (SELECT MAX(OrderDate) AS OperationDate
                 FROM dbo.CashOrders co JOIN BusinessOperationSettings bos ON co.BusinessOperationSettingId = bos.Id
               WHERE sch.NextWorkingDate IS NOT NULL
                 AND co.ContractId = cb.ContractId
                 AND co.DeleteDate IS NULL
                 AND co.ApproveStatus = 10
                 AND co.OrderDate <= @date
                 AND bos.Code IN ('INTEREST_ACCRUAL', 'INTEREST_ACCRUAL_MIGRATION', 'INTEREST_ACCRUAL_OVERDUEDEBT', 'INTEREST_ACCRUAL_ON_HOLIDAYS')
  ) AS lastAccrual
  OUTER APPLY (SELECT IIF(sch.NextWorkingDate IS NOT NULL AND ISNULL(nextSchedule.PercentCost,0) > 0 AND ISNULL(lastAccrual.OperationDate, sch.Date) > sch.Date,
                          nextSchedule.PercentCost * DATEDIFF(DAY,sch.Date, lastAccrual.OperationDate) / IIF(ISNULL(nextSchedule.Period, 0) = 0, 30, nextSchedule.Period),
                          0) AS Amount

  ) AS NextPeriodAccrual
)
SELECT ContractId
       ,ContractNumber
       ,IsOffBalance
       ,AccountAmount
       ,ProfitAmount
       ,OverdueAccountAmount
       ,OverdueProfitAmount
       ,PenyAccount
       ,PenyAmount
       ,PenyProfit
       ,AccountAmount + OverdueAccountAmount AS TotalAcountAmount
       ,ProfitAmount + OverdueProfitAmount AS TotalProfitAmount
       ,ExpenseAmount
       ,PrepaymentBalance
       ,RepaymentAmount
       ,RepaymentAccountAmount
       ,RepaymentProfitAmount
       ,RepaymentAmount + RepaymentAccountAmount + RepaymentProfitAmount AS CurrentDebt
       ,IIF((RepaymentAmount - PrepaymentBalance + RepaymentAccountAmount + RepaymentProfitAmount) < 0, 0, RepaymentAmount - PrepaymentBalance + RepaymentAccountAmount + RepaymentProfitAmount) AS TotalRepaymentAmount
       ,IIF(RedemptionAmount - PrepaymentBalance < 0, 0, RedemptionAmount - PrepaymentBalance) AS TotalRedemptionAmount
       ,ProfitOffBalance
       ,OverdueProfitOffBalance
       ,PenyAccountOffBalance
       ,PenyProfitOffBalance
	   ,DefermentProfit
	   ,AmortizedProfit
	   ,AmortizedPenyAccount
	   ,AmortizedPenyProfit
  FROM wrapperBalance
  JOIN Contracts contr ON wrapperBalance.ContractId = contr.Id",
            new { contractIds, date, dateOnly }, UnitOfWork.Transaction).ToList();
        }

        public async Task<int?> GetLastAttractionChannelId(int creditLineId)
        {
            var builder = new SqlBuilder();
            builder.Select("AttractionChannelId");
            builder.LeftJoin(
                "Contracts ON Contracts.id = dogs.Tranches.Id");
            builder.Where("Tranches.CreditLineId = @creditLineId",
                new { creditLineId });
            builder.Where("AttractionChannelId IS NOT NULL");
            builder.OrderBy("Contracts.id desc");
            var builderTemplate = builder.AddTemplate("Select TOP 1 /**select**/ from dogs.Tranches /**leftjoin**/ /**where**/ /**orderby**/");
            return await UnitOfWork.Session.QueryFirstOrDefaultAsync<int?>(builderTemplate.RawSql,
                builderTemplate.Parameters);
        }

        public async Task<int> GetActiveTranchesCount(int creditLineId)
        {
            var builder = new SqlBuilder();
            builder.Select("Count(*)");
            builder.LeftJoin("Contracts ON Contracts.Id = dogs.Tranches.Id");
            builder.Where("dogs.Tranches.CreditLineId = @creditLineId", new { creditLineId = creditLineId });
            builder.Where("Contracts.Status = 30");
            builder.Where("Contracts.DeleteDate IS NULL");

            var builderTemplate =
                builder.AddTemplate("SELECT /**select**/ FROM dogs.Tranches  /**leftjoin**/ /**where**/");
            return await UnitOfWork.Session.QueryFirstOrDefaultAsync<int>(builderTemplate.RawSql, builderTemplate.Parameters, UnitOfWork.Transaction);
        }
    }
}
