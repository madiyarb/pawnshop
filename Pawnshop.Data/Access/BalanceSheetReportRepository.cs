using Dapper;
using Pawnshop.Core.Queries;
using Pawnshop.Data.Models.Reports;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Data;
using System.Threading.Tasks;
using Pawnshop.Data.Models.CashOrders;
using System.Collections;
using Pawnshop.Data.Models.Clients;
using Pawnshop.Data.Models.Membership;
using Pawnshop.AccountingCore.Models;

namespace Pawnshop.Data.Access
{
    public class BalanceSheetReportRepository
    {
        public readonly ReportContext _reportContext;

        public BalanceSheetReportRepository(ReportContext reportContext)
        {
            _reportContext = reportContext;
        }

        public async Task<List<BalanceSheetReport>> List(ReportGenerateQuery query)
        {
            using (var connection = _reportContext.CreateConnection())
            {
                var list = await connection.QueryAsync<BalanceSheetReport>(
                    @"sld.AccountRecordsReport",
                    new
                    {
                        date_from = query.BeginDate,
                        date_till = query.EndDate,
                        accountSettigId = query.AccountSettingId,
                        termId = query.TermId,
                        contractIds = query.ContractIds,
                        clientIds = query.ClientIds,
                        branchIds = query.BranchIds,
                        collateralType = query.CollateralTypes,
                        rowCount = query.RowCount,
                        startRow = query.StartRow,
                    },
                    commandTimeout: 300000,
                    commandType: CommandType.StoredProcedure
                );

                return list.ToList();
            }
        }

        public async Task<int> GetTurnOrderCount(ReportGenerateQuery query)
        {
            using (var connection = _reportContext.CreateConnection())
            {
                var count = await connection.ExecuteScalarAsync<int>(
                    @$"SELECT sld.GetTurnOrderCount (@date_from, @date_till, @settingId, @termId, @contractIds, @clientIds, @branchIds, @collateralTypes, @isDebit)",
                    new
                    {
                        date_from = query.BeginDate,
                        date_till = query.EndDate,
                        settingId = query.AccountSettingId,
                        termId = query.TermId,
                        contractIds = query.ContractIds,
                        clientIds = query.ClientIds,
                        branchIds = query.BranchIds,
                        collateralTypes = query.CollateralTypes,
                        isDebit = query.IsDebit
                    },
                    commandTimeout: 300000
                );

                return count;
            }
        }

        public async Task<List<CashOrder>> GetCashOrderList(ReportGenerateQuery query)
        {
            using (var connection = _reportContext.CreateConnection())
            {
                var list = await connection.QueryAsync<CashOrder, Client, User, Account, Account, Group, User, CashOrder>(
                    @"DECLARE @result TABLE (OrderId INT)
                    INSERT INTO @result
                    EXEC [sld].GetTurnOrders @date_from, @date_till, @settingId, @termId, @contractIds, @clientIds, @branchIds, @collateralTypes, @isDebit, @startRow, @rowCount
                    SELECT co.*, op.Id AS TasOnlinePaymentId, uk.[Status] as UKassaStatus, c.*, u.*, da.*, ca.*, g.*, a.*
                    FROM dbo.CashOrders co
                    LEFT JOIN Clients c ON co.ClientId = c.Id
                    LEFT JOIN Users u ON co.UserId = u.Id
                    LEFT JOIN Accounts da ON co.DebitAccountId = da.Id
                    LEFT JOIN Accounts ca ON co.CreditAccountId = ca.Id
                    JOIN Groups g ON co.BranchId = g.Id
                    JOIN Users a ON co.AuthorId = a.Id
                    LEFT JOIN TasOnlinePayments op ON op.OrderId = co.Id
                    LEFT JOIN UKassaRequests uk on uk.CashOrderId = co.Id
                    WHERE co.Id IN (SELECT OrderId FROM @result)",
                    (co, c, u, da, ca, g, a) =>
                    {
                        co.Client = c;
                        co.User = u;
                        co.DebitAccount = da;
                        co.CreditAccount = ca;
                        co.Branch = g;
                        co.Author = a;
                        return co;
                    },
                    new
                    {
                        date_from = query.BeginDate,
                        date_till = query.EndDate,
                        settingId = query.AccountSettingId,
                        termId = query.TermId,
                        contractIds = query.ContractIds,
                        clientIds = query.ClientIds,
                        branchIds = query.BranchIds,
                        collateralTypes = query.CollateralTypes,
                        isDebit = query.IsDebit,
                        startRow = query.StartRow,
                        rowCount = query.RowCount,
                    },
                    commandTimeout: 300000
                );

                return list.ToList();
            }
        }

        public async Task<BalanceSheetReportTotal> GetTotals(ReportGenerateQuery query)
        {
            using (var connection = _reportContext.CreateConnection())
            {
                return await connection.QueryFirstOrDefaultAsync<BalanceSheetReportTotal>(
                    @"exec [sld].[GetTotals] @date_from, @date_till, @settingId, @termId, @contractId, @clientId, @BranchId, @collateralType",
                    new
                    {
                        date_from = query.BeginDate,
                        date_till = query.EndDate,
                        settingId = query.AccountSettingId,
                        termId = query.TermId,
                        contractId = query.ContractIds,
                        clientId = query.ClientIds,
                        branchId = query.BranchIds,
                        collateralType = query.CollateralTypes
                    },
                    commandTimeout: 300000
                );
            }
        }

        public async Task<List<BalanceSheetReport>> GetBranches(ReportGenerateQuery query)
        {
            using (var connection = _reportContext.CreateConnection())
            {
                var branches = await connection.QueryAsync<BalanceSheetReport>(
                    @"exec [sld].[GetBranchTotals] @date_from, @date_till, @settingId, @termId, @contractId, @clientId, @BranchId, @collateralType",
                    new
                    {
                        date_from = query.BeginDate,
                        date_till = query.EndDate,
                        settingId = query.AccountSettingId,
                        termId = query.TermId,
                        contractId = query.ContractIds,
                        clientId = query.ClientIds,
                        branchId = query.BranchIds,
                        collateralType = query.CollateralTypes
                    },
                    commandTimeout: 300000
                );

                return branches.ToList();
            }
        }

        public async Task<List<BalanceSheetReport>> GetClients(ReportGenerateQuery query)
        {
            using (var connection = _reportContext.CreateConnection())
            {
                var clients = await connection.QueryAsync<BalanceSheetReport>(
                    @"exec [sld].[GetClientTotals] @date_from, @date_till, @settingId, @termId, @contractId, @clientId, @BranchId, @collateralType, @startRow, @rowCount",
                    new
                    {
                        date_from = query.BeginDate,
                        date_till = query.EndDate,
                        settingId = query.AccountSettingId,
                        termId = query.TermId,
                        contractId = query.ContractIds,
                        clientId = query.ClientIds,
                        branchId = query.BranchIds,
                        collateralType = query.CollateralTypes,
                        startRow = query.StartRow,
                        rowCount = query.RowCount
                    },
                    commandTimeout: 300000
                );

                return clients.ToList();
            }
        }

        public async Task<List<BalanceSheetReport>> GetContracts(ReportGenerateQuery query)
        {
            using (var connection = _reportContext.CreateConnection())
            {
                var contracts = await connection.QueryAsync<BalanceSheetReport>(
                    @"exec [sld].[AccountRecordsReport] @date_from, @date_till, @settingId, @termId, @contractId, @clientId, @branchId, @collateralType, @startRow, @rowCount",
                    new
                    {
                        date_from = query.BeginDate,
                        date_till = query.EndDate,
                        settingId = query.AccountSettingId,
                        termId = query.TermId,
                        contractId = query.ContractIds,
                        clientId = query.ClientIds,
                        branchId = query.BranchIds,
                        collateralType = query.CollateralTypes,
                        startRow = query.StartRow,
                        rowCount = query.RowCount
                    },
                    commandTimeout: 300000
                );
                return contracts.ToList();
            }
        }

        public async Task<List<Models.Reports.AccountSettings>> GetAccountSettings()
        {
            using (var connection = _reportContext.CreateConnection())
            {
                var accountSettings = await connection.QueryAsync<Models.Reports.AccountSettings>(
                    @$"SELECT a.settingName + IIF(a.typeName IS NOT NULL, '. ' + a.typeName, '' ) as settingName, a.settingId, a.typeId
                    FROM sld.vw_AccountSettingFilter a
                    ORDER BY a.settingId, a.typeId"
                );

                return accountSettings.ToList();
            }
        }
    }
}
