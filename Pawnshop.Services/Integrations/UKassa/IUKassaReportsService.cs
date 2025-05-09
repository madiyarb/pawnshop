using Pawnshop.Core.Queries;
using Pawnshop.Services.Models.List;
using Pawnshop.Services.Models.UKassa;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Pawnshop.Services.Integrations.UKassa
{
    public interface IUKassaReportsService
    {
        Task<List<UKassaReconciliationReport>> GetReport(int branchId, int? shiftId, DateTime date);
        Task<ListModel<CashOrderUKassaReportDto>> GetOperations(int branchId, int shiftId, DateTime date, Page page, string filter, int? status);
        Task<List<Shift>> GetShifts(int branchId, DateTime date);
    }
}
