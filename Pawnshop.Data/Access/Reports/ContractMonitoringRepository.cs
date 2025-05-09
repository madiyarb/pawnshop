using System;
using System.Collections.Generic;
using System.Linq;
using Dapper;
using Pawnshop.AccountingCore.Models;
using Pawnshop.Core;
using Pawnshop.Core.Impl;
using Pawnshop.Data.Models.Contracts;

namespace Pawnshop.Data.Access.Reports
{
    public class ContractMonitoringRepository : RepositoryBase
    {
        public ContractMonitoringRepository(IUnitOfWork unitOfWork) : base(unitOfWork)
        {
        }

        public List<dynamic> List(object query = null)
        {
            var beginDate = query?.Val<DateTime?>("BeginDate");
            var endDate = query?.Val<DateTime?>("EndDate");
            var collateralType = query?.Val<CollateralType?>("CollateralType");
            var branchId = query?.Val<int?>("BranchId");

            var prolongDayCount = query?.Val<object>("ProlongDayCount");
            var displayStatus = query?.Val<ContractDisplayStatus?>("DisplayStatus");
            var loanCost = query?.Val<object>("LoanCost");
            var isTransferred = query?.Val<bool?>("IsTransferred");

            if (!beginDate.HasValue) beginDate = DateTime.Now.Date;
            if (!endDate.HasValue) endDate = DateTime.Now.Date.AddHours(23).AddMinutes(59).AddSeconds(59);
            if (!collateralType.HasValue) throw new ArgumentNullException(nameof(collateralType));
            if (!branchId.HasValue) throw new ArgumentNullException(nameof(branchId));

            var condition = "WHERE c.DeleteDate IS NULL";
            condition += " AND c.CollateralType = @collateralType";
            condition += " AND c.BranchId = @branchId";

            condition += prolongDayCount != null
                ? $" AND DATEDIFF(DAY, c.ContractDate, CONVERT(DATE, dbo.GETASTANADATE())) {prolongDayCount.Val<string>("DisplayOperator")} @prolongDayCount"
                : string.Empty;

            condition += loanCost != null ? $" AND c.LoanCost {loanCost.Val<string>("DisplayOperator") ?? "="} @loanCost" : string.Empty;
            
            //if (isTransferred.HasValue && isTransferred.Value) condition += " AND EXISTS (SELECT * FROM ContractTransfers WHERE ContractId=c.Id AND BackTransferDate IS NULL)";
            //else condition += " AND EXISTS (SELECT * FROM ContractTransfers WHERE ContractId=c.Id AND BackTransferDate IS NULL)";

            var from = "FROM Contracts c";
            if (displayStatus.HasValue)
            {
                switch (displayStatus)
                {
                    case ContractDisplayStatus.New:
                        condition += " AND c.Status = 0";
                        condition += " AND c.ContractDate BETWEEN @beginDate AND @endDate";
                        break;
                    case ContractDisplayStatus.Open:
                        condition += " AND c.Status = 30 AND c.MaturityDate >= CONVERT(DATE, dbo.GETASTANADATE()) AND c.ProlongDate IS NULL";
                        condition += " AND c.ContractDate BETWEEN @beginDate AND @endDate";
                        break;
                    case ContractDisplayStatus.Overdue:
                        condition += @" AND c.Status = 30 AND IIF(c.PercentPaymentType=20,c.MaturityDate,DATEADD(MONTH, (
                                                SELECT COUNT(*)
                                                FROM ContractActions ca
                                                WHERE ca.ContractId = c.Id
                                                    AND ca.DeleteDate IS NULL
                                                    AND ca.ActionType = 80
                                            ) +1, c.ContractDate)) < CONVERT(DATE, dbo.GETASTANADATE())";
                        condition += " AND c.ContractDate BETWEEN @beginDate AND @endDate";
                        break;
                    case ContractDisplayStatus.Prolong:
                        condition += " AND c.Status = 30 AND c.MaturityDate >= CONVERT(DATE, dbo.GETASTANADATE()) AND c.ProlongDate IS NOT NULL";
                        condition += " AND ca.[Date] BETWEEN @beginDate AND @endDate";
                        condition += " AND ca.ActionType = 10";
                        from = "FROM ContractActions ca JOIN Contracts c ON ca.ContractId = c.Id";
                        break;
                    case ContractDisplayStatus.BoughtOut:
                        condition += " AND c.Status = 40";
                        condition += " AND ca.[Date] BETWEEN @beginDate AND @endDate";
                        condition += " AND ca.ActionType IN (20, 30, 40, 90)";
                        from = "FROM ContractActions ca JOIN Contracts c ON ca.ContractId = c.Id";
                        break;
                    case ContractDisplayStatus.SoldOut:
                        condition += " AND c.Status = 50";
                        condition += " AND ca.[Date] BETWEEN @beginDate AND @endDate";
                        condition += " AND ca.ActionType = 60";
                        from = "FROM ContractActions ca JOIN Contracts c ON ca.ContractId = c.Id";
                        break;
                    case ContractDisplayStatus.Signed:
                        condition += " AND c.Status = 30";
                        condition += " AND c.ContractDate BETWEEN @beginDate AND @endDate";
                        break;
                    default:
                        break;
                }
            }
            else
            {
                condition += " AND c.ContractDate BETWEEN @beginDate AND @endDate";                
            }

            return UnitOfWork.Session.Query<dynamic>($@"
WITH ContractPaged AS (
    SELECT c.Id
    {from}
    {condition}
    ORDER BY c.ContractDate DESC
    OFFSET (0) ROWS FETCH NEXT 100000 ROWS ONLY
)

SELECT DISTINCT
    c.ContractNumber,
    c.CollateralType,
    c.ContractDate,
    c.LoanCost,
    (CASE 
        WHEN c.DeleteDate IS NOT NULL THEN 60
        WHEN c.Status = 0 THEN 0
        WHEN c.Status = 30 AND IIF(c.PercentPaymentType=20,c.MaturityDate,DATEADD(MONTH, (
            SELECT COUNT(*)
            FROM ContractActions ca
            WHERE ca.ContractId = c.Id
                AND ca.DeleteDate IS NULL
                AND ca.ActionType = 80 
            ) + 1, c.ContractDate)) < CONVERT(DATE, dbo.GETASTANADATE()) THEN 20
		WHEN c.Status = 30 AND c.MaturityDate >= CONVERT(DATE, dbo.GETASTANADATE()) AND c.ProlongDate IS NULL THEN 10
        WHEN c.Status = 30 AND c.MaturityDate >= CONVERT(DATE, dbo.GETASTANADATE()) AND c.ProlongDate IS NOT NULL THEN 30
        WHEN c.Status = 40 THEN 40
        WHEN c.Status = 50 THEN 50
        ELSE 0
    END) AS DisplayStatus,
    c.MaturityDate,
	cl.FullName as ClientName,
    JSON_VALUE(c.ContractSpecific, '$.CollateralTotalWeight') as TotalWeight,
    JSON_VALUE(c.ContractSpecific, '$.CollateralSpecificWeight') as SpecificWeight,
    (
        SELECT TOP 1 p.Name
        FROM ContractPositions cp
        LEFT JOIN Purities p ON JSON_VALUE(cp.PositionSpecific, '$.PurityId') = p.Id
        WHERE cp.ContractId = c.Id
    ) as Purity,
    ISNULL((
        SELECT TOP 1 1
        FROM Insurances i
        WHERE i.ContractId = c.Id
    ), 0) as HasCasco,
    (
        SELECT TOP 1 ct.Name
        FROM ContractPositions cp
        JOIN Categories ct ON cp.CategoryId = ct.Id
        WHERE cp.ContractId = c.Id
    ) as CategoryName,
    (
        SELECT TOP 1 ca.Date
        FROM ContractActions ca
        WHERE ca.ContractId = c.Id AND ca.ActionType = 10
        ORDER BY ca.Date DESC
    ) as ProlongDate,
    (
        SELECT TOP 1 ca.TotalCost
        FROM ContractActions ca
        WHERE ca.ContractId = c.Id AND ca.ActionType = 10
        ORDER BY ca.Date DESC
    ) as ProlongCost,
    (
        SELECT TOP 1 ua.FullName
        FROM ContractActions ca
        JOIN Users ua ON ca.AuthorId = ua.Id
        WHERE ca.ContractId = c.Id AND ca.ActionType = 10
        ORDER BY ca.Date DESC
    ) as ActionAuthor,
    (
        SELECT TOP 1 ca.Date
        FROM ContractActions ca
        WHERE ca.ContractId = c.Id AND ca.ActionType IN (20, 30, 40)
        ORDER BY ca.Date DESC
    ) as BuyoutDate,
    u.FullName as ContractAuthor
FROM ContractPaged cp
JOIN Contracts c ON cp.Id = c.Id
JOIN Users u ON c.AuthorId = u.Id
JOIN Clients cl on c.ClientId = cl.Id",
            new
            {
                beginDate = beginDate,
                endDate = endDate,
                collateralType = collateralType,
                branchId = branchId,
                prolongDayCount = prolongDayCount?.Val<int?>("Value"),
                loanCost = loanCost?.Val<int?>("Value"),
            }).ToList();
        }
    }
}