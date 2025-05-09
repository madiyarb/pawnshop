using Pawnshop.Services.Models.UKassa;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Pawnshop.Services.Integrations.UKassa
{
    public interface IUKassaHttpService
    {
        void Login();
        void Logout(string token);
        (bool, string) SendRequest(string idempKey, string url, string requestData);
        List<Shift> GetShiftReports(int kassaId, DateTime dateFrom, DateTime dateTo);
        Shift GetActiveShift(int kassaId);
        UKassaReportResponse GetZReport(int shiftId);
        UKassaReportResponse GetXReport(int kassaId);
        List<UKassaOperation> GetShiftOperations(int shiftId);
    }
}
