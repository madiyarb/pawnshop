using System.Diagnostics.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Dapper;
using Pawnshop.Core;
using Pawnshop.Data.Models.Crm;
using Pawnshop.Core.Impl;
using Pawnshop.Core.Queries;
using System.Reflection;

namespace Pawnshop.Data.Access
{
    public class CrmPaymentRepository : RepositoryBase, IRepository<CrmUploadPayment>
    {
        public CrmPaymentRepository(IUnitOfWork unitOfWork) : base(unitOfWork)
        {
        }

        public int Count(ListQuery listQuery, object query = null)
        {
            throw new NotImplementedException();
        }

        public void Delete(int id)
        {
            throw new NotImplementedException();
        }

        public CrmUploadPayment Find(object query)
        {
            throw new NotImplementedException();
        }

        public List<CrmUploadPayment> Find()
        {
            return UnitOfWork.Session.Query<CrmUploadPayment>($@"
SELECT cup.*
	FROM CrmUploadPayments cup
	JOIN Contracts c ON c.Id = cup.ContractId
WHERE UploadDate IS NULL
	AND (QueueDate IS NULL OR DATEDIFF(HOUR,QueueDate,dbo.GETASTANADATE())>1)
	AND CreateDate <= DATEADD(MINUTE, -2, dbo.GETASTANADATE())
	AND c.CollateralType <> 60
	AND c.CreatedInOnline = 0
	AND c.Status >= 30").ToList();
        }

        public CrmUploadPayment Get(int id)
        {
            return UnitOfWork.Session.Query<CrmUploadPayment>($@"
SELECT *
FROM CrmUploadPayments WHERE Id=@id", new {id}).FirstOrDefault();
        }

        public void Insert(CrmUploadPayment entity)
        {
            using (var transaction = BeginTransaction())
            {
                entity.Id = UnitOfWork.Session.Execute(@"
IF NOT EXISTS (SELECT * FROM CrmUploadPayments WHERE ContractId=@ContractId AND UploadDate IS NULL)
    BEGIN
        INSERT INTO CrmUploadPayments ( 
        ContractId,
        CreateDate )
        VALUES(
        @ContractId,
        @CreateDate)
        SELECT SCOPE_IDENTITY()
    END
ELSE
    BEGIN
        SELECT TOP 1 Id FROM CrmUploadPayments WHERE ContractId=@ContractId AND UploadDate IS NULL
    END", entity, UnitOfWork.Transaction);

                transaction.Commit();
            }
        }

        public void Insert(List<CrmUploadPayment> entities)
        {
            using (var transaction = BeginTransaction())
            {
                UnitOfWork.Session.Execute(@"
INSERT INTO CrmUploadPayments ( 
ContractId,
CreateDate,
UploadDate)
VALUES(
@ContractId,
@CreateDate,
@UploadDate)", entities, UnitOfWork.Transaction);

                transaction.Commit();
            }
        }

        public List<CrmUploadPayment> List(ListQuery listQuery, object query = null)
        {
            throw new NotImplementedException();
        }

        public void Update(CrmUploadPayment entity)
        {
            using (var transaction = BeginTransaction())
            {
                UnitOfWork.Session.Execute(@"
UPDATE CrmUploadPayments
SET UploadDate = @UploadDate,
QueueDate = @QueueDate
WHERE Id = @Id", entity, UnitOfWork.Transaction);

                transaction.Commit();
            }
        }

        public List<int> GeneratePaymentQueue()
        {
            return UnitOfWork.Session.Query<int>(@"
DECLARE @currentDate DATE = dbo.GETASTANADATE();
declare @beginDate datetime, @endDate datetime, @collateralType smallint
set @beginDate = @currentDate
set @endDate = DATEADD(ss, -1, DATEADD(dd, 2, @beginDate))
set @collateralType = 20

drop table if exists #tmp;
drop table if exists #final;
drop table if exists #result;
drop table if exists #tempAnnuity;

SELECT
	c.Id
into #tmp
FROM Contracts c
WHERE c.DeleteDate IS NULL
AND c.PercentPaymentType = 20
AND c.[Status] = 30
AND c.MaturityDate BETWEEN @beginDate AND @endDate
AND c.CollateralType = @collateralType
and c.CreatedInOnline = 0
union all
SELECT
	c.Id
FROM Contracts c
LEFT JOIN ContractPaymentSchedule cps on cps.ContractId=c.Id AND cps.deletedate is null AND cps.ActionId IS NULL
WHERE c.DeleteDate IS NULL
AND c.PercentPaymentType IN (30, 31, 32, 40)
AND c.[Status] = 30
AND cps.Date BETWEEN @beginDate AND @endDate
AND c.CollateralType = @collateralType
and c.CreatedInOnline = 0

SELECT t.Id
into #final
FROM #tmp t
OUTER APPLY (SELECT TOP 1 a.Id FROM Accounts a JOIN AccountSettings acs ON a.AccountSettingId = acs.Id WHERE acs.Code = 'OVERDUE_ACCOUNT' AND a.ContractId = t.Id) OverdueAccount
OUTER APPLY (SELECT TOP 1 a.Id FROM Accounts a JOIN AccountSettings acs ON a.AccountSettingId = acs.Id WHERE acs.Code = 'OVERDUE_PROFIT' AND a.ContractId = t.Id) OverdueProfit
OUTER APPLY (SELECT ISNULL(dbo.GetAccountBalanceById(OverdueAccount.Id, @endDate, default, default),0) bal) overdueAccountBalance
OUTER APPLY (SELECT ISNULL(dbo.GetAccountBalanceById(OverdueProfit.Id, @endDate, default, default),0) bal) overdueProfitBalance
where overdueAccountBalance.bal = 0
AND overdueProfitBalance.bal = 0;

SELECT c.Id
into #result
FROM Contracts c
WHERE c.DeleteDate IS NULL
AND c.[Status] IN (30, 50)
AND c.PercentPaymentType = 20
and c.CollateralType = @collateralType
AND DATEDIFF(DAY, c.MaturityDate, @currentDate) > 0
and DATEDIFF(DAY, IIF(c.locked = 1, c.MaturityDate, c.MaturityDate), @currentDate) between 1 and 16
union all
select 0 where 1 = 0;

--аннуитет
SELECT cps.ContractId
into #tempAnnuity
FROM ContractPaymentSchedule cps
INNER JOIN
	Contracts c
	ON c.Id = cps.ContractId
	AND c.PercentPaymentType != 20
WHERE CAST(Date AS date) < CAST(@currentDate AS DATE)
AND cps.ActionId IS NULL
AND cps.Canceled IS NULL
AND cps.DeleteDate IS NULL
and c.DeleteDate IS NULL
AND c.CollateralType = @collateralType
AND c.[Status] IN (30, 50)
GROUP BY cps.ContractId
having MAX(DATEDIFF(DAY, cps.[Date], CAST(@currentDate AS DATE))) between 1 and 16;

;with lastPayments
as
(
	SELECT
		cps.ContractId
	,	cps.[Date]
	,	row_number() over (partition by cps.ContractID order by cps.[Date] desc) as rn
	FROM ContractPaymentSchedule cps
	INNER JOIN
		Contracts c
		ON c.Id = cps.ContractId
		AND c.PercentPaymentType != 20
	where cps.[Date] < CAST(@currentDate AS DATE)
	AND cps.Canceled IS NULL
	AND cps.ActionId IS NOT NULL
	AND cps.DeleteDate IS NULL
	and c.DeleteDate IS NULL
	AND c.CollateralType = @collateralType
	AND c.[Status] IN (30, 50)
)
INSERT INTO #result with(TABLOCK) (Id)
SELECT
	cps.ContractId
FROM lastPayments cps
OUTER APPLY
(
	select SUM(abs(balance.bal)) AS Cost
	FROM Accounts a
	INNER JOIN
		AccountSettings acs
		ON acs.Id = a.AccountSettingId
		outer apply
	(
		SELECT TOP(1) ar.OutgoingBalance as bal
		FROM AccountRecords ar
		WHERE ar.AccountId = a.id
		AND ar.DeleteDate IS NULL
		AND ar.Date < DATEADD(DAY, 1, @currentDate)
		ORDER BY ar.Date DESC, ar.Id DESC
	) balance
	WHERE a.ContractId = cps.ContractId
	AND acs.Code IN ('PENY_ACCOUNT','PENY_PROFIT','PENY_ACCOUNT_OFFBALANCE','PENY_PROFIT_OFFBALANCE')
) peny
WHERE cps.rn = 1
and peny.Cost > 0
AND NOT EXISTS(SELECT* FROM #tempAnnuity t WHERE t.ContractId = cps.ContractId)
and DATEDIFF(DAY, cps.[Date], CAST(@currentDate AS DATE)) between 1 and 16;

INSERT INTO #result with(TABLOCK) (Id)
SELECT ContractId FROM #tempAnnuity;

select Id from #final
union
select Id from #result;
            ").ToList();
        }
    }
}
