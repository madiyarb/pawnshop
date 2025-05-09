
using Pawnshop.Services.Models.Contracts.Kdn;
using System.IO;
using System.Threading.Tasks;

namespace Pawnshop.Services.Integrations.Fcb
{
    public interface IFcb4Kdn
    {
        Task<FcbKdnResponse> StorekdnReqWithIncome(FcbKdnRequest fcbKdnRequest);

        Task<FcbReportResponse> GetReport(FcbReportRequest fcbReportRequest);

        Task<FcbOurReportResponse> GetOurReport(FcbOurReportsRequest request);

        Task<byte[]> GetOurReportExcel(FcbOurReportsRequest request);

        Task<bool> CheckOverdueClient(Stream xmlStream);

        Task<FCBChecksResult> FCBChecks(Stream xmlStream);

        Task<bool> CheckGamblerFeature(Stream xmlStream);

        bool ValidateReportResponse(FcbReportResponse fcbReportResponse);

        Task<bool> CheckStopCreditFromReport(Stream xmlStream);

        Task<bool> ValidateIsOverdueClient(int clientId);
    }
}
