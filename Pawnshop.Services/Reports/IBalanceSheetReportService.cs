using Pawnshop.Core.Queries;
using Pawnshop.Data.Models.CashOrders;
using Pawnshop.Data.Models.Reports;
using Pawnshop.Services.Models.List;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Pawnshop.Services.Reports
{
    public interface IBalanceSheetReportService
    {
        Task<List<BalanceSheetReport>> List(ReportGenerateQuery query);
        Task<int> GetTurnOrderCount(ReportGenerateQuery query);
        Task<BalanceSheetReportTotal> GetTotals(ReportGenerateQuery query);
        Task<List<BalanceSheetReport>> GetBranches(ReportGenerateQuery query);
        Task<List<BalanceSheetReport>> GetClients(ReportGenerateQuery query);
        Task<List<BalanceSheetReport>> GetContracts(ReportGenerateQuery query);
        Task<List<CashOrder>> GetCashOrderList(ReportGenerateQuery query);
        Task<ListModel<CashOrder>> GetBalanceSheetReportList(ReportGenerateQuery query);
        Task<List<AccountSettings>> GetAccountSettings();
    }
}
