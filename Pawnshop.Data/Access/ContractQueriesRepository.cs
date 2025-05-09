using System;
using System.Collections.Generic;
using Dapper;
using Pawnshop.AccountingCore.Models;
using System.Linq;
using System.Threading.Tasks;
using Pawnshop.Core;
using Pawnshop.Core.Impl;
using Pawnshop.Core.Queries;
using Pawnshop.Data.Models.Contracts;
using SRegex = System.Text.RegularExpressions;

namespace Pawnshop.Data.Access
{
    public sealed class ContractQueriesRepository : RepositoryBase
    {
        public ContractQueriesRepository(IUnitOfWork unitOfWork) : base(unitOfWork) 
        {
            
        }

        public async Task<List<ContractListInfo>> GetFilteredList(ListQuery listQuery, object query = null, bool withoutTranches = false)
        {
            var contracts =(await InternalGetFilteredList(listQuery, query)).ToList();

            if (withoutTranches)
                return contracts;

            var creditLineIds = contracts
                .Where(x => x.ContractClass == ContractClass.CreditLine)
                .Select(x => x.Id)
                .ToList();

            if (!creditLineIds.Any())
                return contracts;

            var tranches = await InternalGetFilteredList(listQuery, query, creditLineIds);

            contracts.ForEach(x => x.Tranches = tranches.Where(t => t.CreditLineId == x.Id).ToList());

            return contracts;
        }


        private async Task<IEnumerable<ContractListInfo>> InternalGetFilteredList(ListQuery listQuery, object query = null, IList<int> creditLineIds = null)
        {
            if (listQuery == null)
                throw new ArgumentNullException(nameof(listQuery));

            var identityNumber = query?.Val<string>("IdentityNumber");
            var beginDate = query?.Val<DateTime?>("BeginDate");
            var endDate = query?.Val<DateTime?>("EndDate");
            var collateralType = query?.Val<CollateralType?>("CollateralType");
            var displayStatus = query?.Val<ContractDisplayStatus?>("DisplayStatus");
            var carNumber = query?.Val<string>("CarNumber");
            var rca = query?.Val<string>("Rca");
            var isTransferred = query?.Val<bool?>("IsTransferred") ?? false;
            var ownerIds = query?.Val<int[]>("OwnerIds");
            var clientId = query?.Val<int?>("ClientId");
            var settingId = query?.Val<int?>("SettingId");
            var allBranches = query?.Val<bool?>("AllBranches");
            var createdInOnline = query?.Val<bool?>("CreatedInOnline");

            var from = "FROM Contracts c";
            var predicate = string.Empty;
            var offset = string.Empty;

            if (creditLineIds?.Any() ?? false)
            {
                predicate = "WHERE c.ContractClass = 3 AND t.CreditLineId IN @creditLineIds";
                offset = "OFFSET (0) ROWS FETCH NEXT 100 ROWS ONLY";
            }
            else
            {
                offset = "OFFSET (@offset) ROWS FETCH NEXT @limit ROWS ONLY";
                var result = GetContractsPredicate(listQuery.Filter, beginDate, endDate,
                    collateralType, displayStatus, isTransferred, clientId, settingId);

                predicate = result.Item1;

                if (!string.IsNullOrEmpty(result.Item2))
                    from = result.Item2;
            }

            var pre = string.Empty;
            if (!String.IsNullOrEmpty(identityNumber) || !String.IsNullOrEmpty(carNumber) || !String.IsNullOrEmpty(rca))
            {
                if (!String.IsNullOrEmpty(identityNumber))
                    pre += " AND cc.IdentityNumber = @identityNumber";
                if (!String.IsNullOrEmpty(carNumber))
                    pre += " AND cars.TransportNumber = @carNumber";
                if (!String.IsNullOrEmpty(rca))
                    pre += " AND realty.rca = @rca";
            }
            else if (!creditLineIds?.Any() ?? true)
            {
                pre += (!allBranches.HasValue || !allBranches.Value) && ownerIds != null && ownerIds.Length > 0 ? " AND c.BranchId IN @ownerIds" : string.Empty;
            }

            if (createdInOnline != null)
            {
                pre = " AND c.CreatedInOnline =@createdInOnline";
            }
            predicate += pre;

            string sql = $@"
DECLARE @current_date DATE = CONVERT(DATE, dbo.GETASTANADATE());
WITH contractsList AS (
  SELECT DISTINCT
         c.Id
         ,c.ContractNumber
         ,c.ContractClass
         ,c.ContractDate
         ,c.MaturityDate
         ,c.LoanCost
         ,c.LoanPeriod
         ,c.LoanPercent
         ,c.DeleteDate
         ,c.Status
         ,c.InscriptionId
         ,cps.NextPaymentDate
         ,c.ProlongDate
         ,c.BuyoutDate
         ,c.PercentPaymentType
         ,c.Locked
         ,c.SettingId
         ,cc.Id AS ClientId
         ,cc.FullName AS ClientFullName
         ,cc.MaidenName
         ,g.DisplayName AS BranchName
         ,u.FullName AS AuthorFullName
         ,lps.Name AS ProductName
         ,lps.IsFloatingDiscrete AS IsFloatingDiscrete
         ,lps.PaymentPeriodType
         ,(SELECT SUM(acc.Balance)
             FROM Accounts acc, AccountSettings accSetting
            WHERE acc.ContractId = c.Id
              AND acc.AccountSettingId = accSetting.Id
              AND accSetting.Code in ('PENY_ACCOUNT', 'PENY_PROFIT', 'PENY_ACCOUNT_OFFBALANCE', 'PENY_PROFIT_OFFBALANCE')
            GROUP BY acc.ContractId) AS allPenBal
         ,(SELECT CONCAT(' ', IIF(cars.Id IS NOT NULL, CONCAT_WS(' ', cars.Mark, cars.Model, REPLACE(cars.TransportNumber, ' ', '')), CONCAT(dv.Name,' ',realty.RCA)),',',CHAR(10)) 
	        FROM ContractPositions cpp 
	        LEFT JOIN Cars cars ON cars.Id = cpp.PositionId
	        LEFT JOIN Realties realty ON realty.Id = cpp.PositionId
            LEFT JOIN DomainValues dv ON dv.Id = realty.RealtyTypeId
	        WHERE cpp.ContractId = c.Id AND cpp.DeleteDate IS NULL 
	        order by cpp.Id DESC
	        FOR XML PATH('')) AS grnzCollat
         ,Encumbrance.Id AS EncumbranceId
         ,t.CreditLineId
         ,c.CollateralType
         ,c.CreatedInOnline
         ,i.Status AS InscriptionStatus
         ,ccs.CollectionStatusCode
         ,DATEDIFF(DAY,ccs.StartDelayDate,dbo.GetAstanaDate()) AS DelayDays
    {from}
    JOIN Clients cc on cc.Id = c.ClientId
    JOIN Groups g on g.Id = c.BranchId
    JOIN Users u on u.Id = c.AuthorId
    LEFT JOIN dogs.Tranches t on t.Id = c.Id
    LEFT JOIN dogs.Tranches t2 on t2.CreditLineId = c.Id
    LEFT JOIN Contracts c2 on c2.Id = t2.Id
    LEFT JOIN Inscriptions i ON i.Id = c.InscriptionId
    LEFT JOIN LoanPercentSettings lps on lps.Id = c.SettingId
    LEFT JOIN ContractPositions cpp ON cpp.ContractId = c.Id
    LEFT JOIN Cars cars ON cars.Id = cpp.PositionId
    LEFT JOIN Realties realty ON realty.Id = cpp.PositionId
    LEFT JOIN DomainValues dv ON dv.Id = realty.RealtyTypeId
    LEFT JOIN CollectionContractStatuses ccs ON ccs.ContractId = c.Id AND ccs.IsActive = 1 AND ccs.DeleteDate IS NULL
    LEFT JOIN CollectionContractStatuses ccs2 ON ccs2.ContractId = c2.Id AND ccs2.IsActive = 1 AND ccs2.DeleteDate IS NULL
    OUTER APPLY(
        SELECT ce.Id FROM ContractExpenses1 ce 
        LEFT JOIN Expenses e ON e.Id = ce.ExpenseId AND e.DeleteDate IS NULL
	    WHERE ce.ContractId = c.Id AND ce.DeleteDate IS NULL
	    AND e.ActionType = 50 AND e.Cost > 0) Encumbrance
   OUTER APPLY (SELECT TOP 1 ISNULL(cps.NextWorkingDate, cps.Date) as NextPaymentDate
               FROM ContractPaymentSchedule cps
              WHERE cps.ContractId = c.Id
                AND cps.DeleteDate IS NULL
                AND cps.ActualDate IS NULL
                AND cps.Canceled IS NULL
                AND cps.Date <= @current_date
              ORDER BY cps.Date) cps
   {predicate}
   GROUP BY c.Id, c.ContractNumber, c.ContractClass, c.ContractDate, c.MaturityDate, c.LoanCost, c.LoanPeriod, c.LoanPercent, c.DeleteDate, c.Status, c.InscriptionId, cps.NextPaymentDate,
            c.ProlongDate, c.BuyoutDate, c.PercentPaymentType, c.Locked, c.SettingId, cc.Id, cc.FullName, cc.MaidenName, g.DisplayName, u.FullName, lps.Name, lps.IsFloatingDiscrete,
            lps.PaymentPeriodType, t.CreditLineId, c.CollateralType, Encumbrance.Id, i.Status, ccs.CollectionStatusCode, DATEDIFF(DAY,ccs.StartDelayDate,dbo.GetAstanaDate()), g.Name, c.CreatedInOnline
   ORDER BY c.ContractDate DESC, c.ContractNumber Desc
  {offset}
)
SELECT *
       ,(CASE
           WHEN c.DeleteDate IS NOT NULL THEN 60
           WHEN c.Status = 0 THEN 0
           WHEN c.Status = 20 THEN 5
           WHEN c.Status = 24 THEN 24
           WHEN c.Status = 30 AND c.NextPaymentDate < CAST(dbo.GETASTANADATE() AS DATE) THEN 20
           WHEN c.Status = 30 AND c.allPenBal < 0 THEN 20
           WHEN c.Status = 30 AND c.MaturityDate >= CONVERT(DATE, dbo.GETASTANADATE()) AND c.ProlongDate IS NOT NULL THEN 30
           WHEN c.Status = 30 AND c.MaturityDate >= CONVERT(DATE, dbo.GETASTANADATE()) AND c.ProlongDate IS NULL THEN 10
           WHEN c.Status = 40 THEN 40
           WHEN c.Status = 50 THEN 50
           WHEN c.Status = 60 THEN 55
           ELSE 0
       END) AS DisplayStatus
       ,IIF(EncumbranceId > 0, 1, 0) AS HasEncumbrance
  FROM contractsList c
 ORDER BY c.ContractDate DESC, c.ContractNumber Desc";
            var returnList = await UnitOfWork.Session.QueryAsync<ContractListInfo>(sql,
                new
                {
                    listQuery.Filter,
                    listQuery.Page?.Offset,
                    listQuery.Page?.Limit,
                    identityNumber,
                    beginDate,
                    endDate,
                    collateralType,
                    carNumber,
                    rca,
                    creditLineIds,
                    ownerIds,
                    clientId,
                    settingId,
                    createdInOnline
                }, UnitOfWork.Transaction, commandTimeout: 1800);
            return returnList;
        }


        private (string, string) GetContractsPredicate(string value, DateTime? beginDate, DateTime? endDate, CollateralType? collateralType,
    ContractDisplayStatus? displayStatus, bool isTransferred, int? clientId, int? settingId)
        {

            var predicate = new List<string>
            {
                "WHERE c.ContractClass != 3"
            };

            if (collateralType.HasValue)
                predicate.Add("c.CollateralType = @collateralType");

            if (isTransferred)
                predicate.Add("EXISTS (SELECT * FROM ContractTransfers WHERE ContractId=c.Id AND BackTransferDate IS NULL)");
            else
                predicate.Add("NOT EXISTS (SELECT * FROM ContractTransfers WHERE ContractId=c.Id AND BackTransferDate IS NULL)");

            if (!string.IsNullOrEmpty(value))
            {

                if (value.LastIndexOf("-") is int index && index > 0 && SRegex.Regex.IsMatch(value.Substring(index, value.Length - index), "^-T\\d{3}$"))
                {
                    predicate.Add($"c.ContractNumber LIKE N'%{value.Substring(0, index)}%'");
                }
                else
                {
                    var fields = new List<string> { "c.ContractNumber", "cc.FullName", "cc.IdentityNumber", "cc.MobilePhone", "cars.TransportNumber" };
                    predicate.Add($"({string.Join("\r\nOR ", fields.Select(f => $"{f} LIKE N'%{value}%'").ToArray())})");
                }
            }

            if (clientId.HasValue)
                predicate.Add("c.ClientId = @clientId");

            if (settingId.HasValue)
                predicate.Add("c.SettingId = @settingId");

            if (displayStatus.HasValue)
            {
                if (displayStatus.Value != ContractDisplayStatus.Deleted)
                    predicate.Add("c.DeleteDate IS NULL");

                var result = GetContractsDisplayStatusPredicate(displayStatus.Value, beginDate, endDate);

                return (string.Join("\r\nAND ", predicate.Concat(result.Item1).ToArray()), result.Item2);
            }
            else
            {
                if (beginDate.HasValue)
                    predicate.Add("c.ContractDate >= @beginDate");

                if (endDate.HasValue)
                    predicate.Add("c.ContractDate <= @endDate");

                predicate.Add("c.DeleteDate IS NULL");
            }

            return (string.Join("\r\nAND ", predicate.ToArray()), string.Empty);
        }


        private (List<string>, string) GetContractsDisplayStatusPredicate(ContractDisplayStatus displayStatus, DateTime? beginDate, DateTime? endDate)
        {

            var fromStr = string.Empty;
            var predicate = new List<string>();

            var needContractActionsStatuses = new List<ContractDisplayStatus>
                {
                    ContractDisplayStatus.Prolong,
                    ContractDisplayStatus.BoughtOut,
                    ContractDisplayStatus.SoldOut,
                    ContractDisplayStatus.MonthlyPayment
                };

            if (needContractActionsStatuses.Contains(displayStatus))
            {
                fromStr = "FROM ContractActions ca\r\n  JOIN Contracts c ON ca.ContractId = c.Id";

                if (beginDate.HasValue)
                    predicate.Add("c.[ContractDate] >= @beginDate");

                if (endDate.HasValue)
                    predicate.Add("c.[ContractDate] <= @endDate");
            }
            else
            {
                if (beginDate.HasValue)
                    predicate.Add("c.ContractDate >= @beginDate");

                if (endDate.HasValue)
                    predicate.Add("c.ContractDate <= @endDate");
            }

            var statusPredicate = displayStatus switch
            {
                ContractDisplayStatus.AwaitForMoneySend => "c.Status = 20 AND c.MaturityDate >= CONVERT(DATE, dbo.GETASTANADATE()) AND c.ProlongDate IS NULL",
                ContractDisplayStatus.BoughtOut => "c.Status = 40 AND ca.ActionType IN (20, 30, 40, 90)",
                ContractDisplayStatus.MonthlyPayment => "c.Status = 30 AND ca.ActionType = 80 AND c.MaturityDate >= CONVERT(DATE, dbo.GETASTANADATE())",
                ContractDisplayStatus.New => "c.Status = 0",
                ContractDisplayStatus.Open => "c.Status = 30 AND c.MaturityDate >= CONVERT(DATE, dbo.GETASTANADATE()) AND c.ProlongDate IS NULL",
                ContractDisplayStatus.Overdue => "c.Status = 30 AND (c.NextPaymentDate < CONVERT(DATE, dbo.GETASTANADATE()))",
                ContractDisplayStatus.SoftCollection => $"(ccs.FincoreStatusId = {(int)ContractDisplayStatus.SoftCollection} OR ccs2.FincoreStatusId = {(int)ContractDisplayStatus.SoftCollection} )",
                ContractDisplayStatus.HardCollection => $"(ccs.FincoreStatusId = {(int)ContractDisplayStatus.HardCollection} OR ccs2.FincoreStatusId = {(int)ContractDisplayStatus.HardCollection} )",
                ContractDisplayStatus.LegalCollection => $"(ccs.FincoreStatusId =  {(int)ContractDisplayStatus.LegalCollection}  OR ccs2.FincoreStatusId =  {(int)ContractDisplayStatus.LegalCollection} )",
                ContractDisplayStatus.LegalHardCollection => $"(ccs.FincoreStatusId =  {(int)ContractDisplayStatus.LegalHardCollection}  OR ccs2.FincoreStatusId =  {(int)ContractDisplayStatus.LegalHardCollection} )",
                ContractDisplayStatus.Prolong => "c.Status = 30 AND ca.ActionType = 10 AND c.MaturityDate >= CONVERT(DATE, dbo.GETASTANADATE()) AND c.ProlongDate IS NOT NULL",
                ContractDisplayStatus.Signed => "c.Status = 30",
                ContractDisplayStatus.SoldOut => "c.Status = 50 AND ca.ActionType = 60",
                ContractDisplayStatus.Deleted => "c.DeleteDate IS NOT NULL",
                _ => string.Empty,
            };

            predicate.Add(statusPredicate);

            return (predicate, fromStr);
        }

        public async Task<int> GetFilteredListCount(ListQuery listQuery, object query = null)
        {
            if (listQuery == null)
                throw new ArgumentNullException(nameof(listQuery));

            var identityNumber = query?.Val<string>("IdentityNumber");
            var beginDate = query?.Val<DateTime?>("BeginDate");
            var endDate = query?.Val<DateTime?>("EndDate");
            var collateralType = query?.Val<CollateralType?>("CollateralType");
            var displayStatus = query?.Val<ContractDisplayStatus?>("DisplayStatus");
            var carNumber = query?.Val<string>("CarNumber");
            var rca = query?.Val<string>("Rca");
            var isTransferred = query?.Val<bool?>("IsTransferred") ?? false;
            var ownerIds = query?.Val<int[]>("OwnerIds");
            var clientId = query?.Val<int?>("ClientId");
            var settingId = query?.Val<int?>("SettingId");
            var allBranches = query?.Val<bool?>("AllBranches");
            var createdInOnline = query?.Val<bool?>("CreatedInOnline");

            var result = GetContractsPredicate(listQuery.Filter, beginDate, endDate,
                collateralType, displayStatus, isTransferred, clientId, settingId);

            var from = string.IsNullOrEmpty(result.Item2) ? "FROM Contracts c" : result.Item2;
            var predicate = result.Item1;

            var pre = string.Empty;
            if (!String.IsNullOrEmpty(identityNumber) || !String.IsNullOrEmpty(carNumber) || !String.IsNullOrEmpty(rca))
            {
                if (!String.IsNullOrEmpty(identityNumber))
                    pre += " AND cc.IdentityNumber = @identityNumber";
                if (!String.IsNullOrEmpty(carNumber))
                    pre += " AND cars.TransportNumber = @carNumber";
                if (!String.IsNullOrEmpty(rca))
                    pre += " AND realty.rca = @rca";
            }
            else
            {
                pre += (!allBranches.HasValue || !allBranches.Value) && ownerIds != null && ownerIds.Length > 0 ? " AND c.BranchId IN @ownerIds" : string.Empty;
            }

            if (createdInOnline != null)
            {
                pre = " AND c.CreatedInOnline =@createdInOnline";
            }
            predicate += pre;

            return await UnitOfWork.Session.ExecuteScalarAsync<int>($@"SELECT COUNT(DISTINCT c.Id)
  {from}
  JOIN Clients cc on cc.Id = c.ClientId
  JOIN Groups g on g.Id = c.BranchId
  JOIN Users u on u.Id = c.AuthorId
  LEFT JOIN dogs.Tranches t on t.Id = c.Id
  LEFT JOIN dogs.Tranches t2 on t2.CreditLineId = c.Id
  LEFT JOIN Contracts c2 on c2.Id = t2.Id  
  LEFT JOIN Inscriptions i ON i.Id = c.InscriptionId
  LEFT JOIN LoanPercentSettings lps on lps.Id = c.SettingId
  LEFT JOIN ContractPositions cpp ON cpp.ContractId = c.Id
  LEFT JOIN Cars cars ON cars.Id = cpp.PositionId
  LEFT JOIN Realties realty ON realty.Id = cpp.PositionId
  LEFT JOIN CollectionContractStatuses ccs ON ccs.ContractId = c.Id AND ccs.IsActive = 1 AND ccs.DeleteDate IS NULL
  LEFT JOIN CollectionContractStatuses ccs2 ON ccs2.ContractId = c2.Id AND ccs2.IsActive = 1 AND ccs2.DeleteDate IS NULL
 {predicate}",
                new
                {
                    listQuery.Filter,
                    listQuery.Page?.Offset,
                    listQuery.Page?.Limit,
                    identityNumber,
                    beginDate,
                    endDate,
                    collateralType,
                    carNumber,
                    rca,
                    ownerIds,
                    clientId,
                    settingId,
                    createdInOnline
                }, UnitOfWork.Transaction, commandTimeout: 1800);
        }
    }
}
