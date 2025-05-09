using Pawnshop.Core.Queries;
using Pawnshop.Data.Access;
using Pawnshop.Data.Models.CashOrders;
using Pawnshop.Data.Models.Reports;
using Pawnshop.Services.Models.List;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Pawnshop.Services.Reports
{
    public class BalanceSheetReportService : IBalanceSheetReportService
    {
        private readonly BalanceSheetReportRepository _balanceSheetReportRepository;

        public BalanceSheetReportService(BalanceSheetReportRepository balanceSheetReportRepository)
        {
            _balanceSheetReportRepository = balanceSheetReportRepository;
        }

        public async Task<List<BalanceSheetReport>> List(ReportGenerateQuery query)
        {
            return await _balanceSheetReportRepository.List(query);
        }

        public async Task<int> GetTurnOrderCount(ReportGenerateQuery query)
        {
            return await _balanceSheetReportRepository.GetTurnOrderCount(query);
        }

        public async Task<BalanceSheetReportTotal> GetTotals(ReportGenerateQuery query)
        {
            return await _balanceSheetReportRepository.GetTotals(query);
        }

        public async Task<List<AccountSettings>> GetAccountSettings()
        {
            return await _balanceSheetReportRepository.GetAccountSettings();
        }

        public async Task<List<BalanceSheetReport>> GetBranches(ReportGenerateQuery query)
        {
            return await _balanceSheetReportRepository.GetBranches(query);
        }

        public async Task<List<BalanceSheetReport>> GetClients(ReportGenerateQuery query)
        {
            return await _balanceSheetReportRepository.GetClients(query);
        }

        public async Task<List<BalanceSheetReport>> GetContracts(ReportGenerateQuery query)
        {
            return await _balanceSheetReportRepository.GetContracts(query);
        }

        public async Task<ListModel<CashOrder>> GetBalanceSheetReportList(ReportGenerateQuery listQuery)
        {
            return new ListModel<CashOrder>()
            {
                List = await _balanceSheetReportRepository.GetCashOrderList(listQuery),
                Count = await _balanceSheetReportRepository.GetTurnOrderCount(listQuery),
            };
        }

        public async Task<List<CashOrder>> GetCashOrderList(ReportGenerateQuery query)
        {
            return await _balanceSheetReportRepository.GetCashOrderList(query);
        }
    }
}
