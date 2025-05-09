using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Pawnshop.Core;
using Pawnshop.Data.Access;
using Pawnshop.Data.Models.ApplicationOnlineFcbKdnPayment;
using Pawnshop.Services.Integrations.Fcb;
using Pawnshop.Services.Models.Contracts.Kdn;
using Pawnshop.Web.Models.FCBReport;
using System.Linq;
using System.Threading.Tasks;
using System;
using Pawnshop.Services.Clients;
using Pawnshop.Data.Models.Reports;
using Newtonsoft.Json;
using Pawnshop.Core.Exceptions;
using Pawnshop.Data.Models.Audit;
using Pawnshop.Services;

namespace Pawnshop.Web.Controllers.Api
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class FcbController : ControllerBase
    {
        private readonly ApplicationOnlineFcbKdnPaymentRepository _applicationOnlineFcbKdnPaymentRepository;
        private readonly ClientRepository _clientRepository;
        private readonly IFcb4Kdn _fcb;
        private readonly FcbReportRepository _fcbReportRepository;
        private readonly IManualCalculationClientExpenseService _manualCalculationClientExpenseService;
        private readonly ISessionContext _sessionContext;
        private readonly ReportLogsRepository _reportLog;
        private readonly ReportsRepository _reportsRepository;
        private readonly IEventLog _eventLog;

        public FcbController(
            ApplicationOnlineFcbKdnPaymentRepository applicationOnlineFcbKdnPaymentRepository,
            ClientRepository clientRepository,
            IFcb4Kdn fcb, 
            FcbReportRepository fcbReportRepository,
            IManualCalculationClientExpenseService manualCalculationClientExpenseService,
            ISessionContext sessionContext,
            ReportLogsRepository reportLogsRepository,
            ReportsRepository reportsRepository,
            IEventLog eventLog)
        {
            _applicationOnlineFcbKdnPaymentRepository = applicationOnlineFcbKdnPaymentRepository;
            _clientRepository = clientRepository;
            _fcb = fcb;
            _fcbReportRepository = fcbReportRepository;
            _manualCalculationClientExpenseService = manualCalculationClientExpenseService;
            _sessionContext = sessionContext;
            _reportLog = reportLogsRepository;
            _reportsRepository = reportsRepository;
            _eventLog = eventLog;
        }

        [HttpGet]
        [Route("GetFiles")]
        public async Task<IActionResult> GetFiles(int ClientId)
        {
            var entities = _fcbReportRepository.GetByClientId(ClientId);

            var response = entities.Select(x => new FCBReportView
            {
                AuthorId = x.AuthorId,
                AuthorName = x.Author?.Fullname,
                ClientId = ClientId,
                CreateDate = x.CreateDate,
                DeleteDate = x.DeleteDate,
                FolderName = x.FolderName,
                Id = x.Id,
                PdfFileLink = x.PdfFileLink,
                XmlFileLink = x.XmlFileLink
            });

            return Ok(response);
        }

        [HttpPost]
        [Route("GetKdn")]
        public async Task<IActionResult> GetKdn([FromBody] FcbKdnRequest request)
        {
            var fcbKdnResponse = GetFcbKdnFromLocalStorage(request?.IIN);
            if (fcbKdnResponse != null)
                return Ok(fcbKdnResponse);

            request.Author = _sessionContext.UserName;
            request.OrganizationId = 1;

            var result = await _fcb.StorekdnReqWithIncome(request);

            if (request.ApplicationOnlineId.HasValue)
            {
                _applicationOnlineFcbKdnPaymentRepository.Insert(new ApplicationOnlineFcbKdnPayment
                {
                    ApplicationOnlineId = request.ApplicationOnlineId.Value,
                    CreateBy = _sessionContext.UserId,
                    CreateDate = DateTime.Now,
                    PaymentAmount = result.Debt,
                    Success = result.ErrorCode == 0,
                });
            }

            return Ok(result);
        }

        [HttpPost]
        [Route("GetOurReport")]
        public async Task<IActionResult> GetOurReport([FromBody] FcbOurReportsRequest request)
        {
            var report = _reportsRepository.Find(new { ReportCode = "RequestsToPkbReport" });
            var reportLog = new ReportLog()
            {
                AuthorId = _sessionContext.UserId,
                AuthorName = _sessionContext.UserName,
                ReportId = report.Id,
                Request = JsonConvert.SerializeObject(request),
                CreateDate = DateTime.Now,
                IsSuccessful = false,
            };

            try
            {
                reportLog.IsSuccessful = true;
                _reportLog.Log(reportLog);
                return Ok(await _fcb.GetOurReport(request));
            }
            catch (PawnshopApplicationException ex)
            {
                _reportLog.Log(reportLog);
                throw;
            }
            catch (Exception ex)
            {
                _reportLog.Log(reportLog);
                _eventLog.Log(EventCode.ReportDownload, EventStatus.Failed, null, null, JsonConvert.SerializeObject(request), JsonConvert.SerializeObject(ex), null, _sessionContext.UserId);
                throw;
            }
        }

        [HttpPost]
        [Route("GetOurReport/Excel")]
        public async Task<IActionResult> GetOurReportExcel([FromBody] FcbOurReportsRequest request)
        {
            var report = _reportsRepository.Find(new { ReportCode = "RequestsToPkbReport" });
            var reportLog = new ReportLog()
            {
                AuthorId = _sessionContext.UserId,
                AuthorName = _sessionContext.UserName,
                ReportId = report.Id,
                Request = "Excel" + JsonConvert.SerializeObject(request),
                CreateDate = DateTime.Now,
                IsSuccessful = false,
            };

            try
            {
                reportLog.IsSuccessful = true;
                _reportLog.Log(reportLog);
                return File(await _fcb.GetOurReportExcel(request), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "PKBReport.xlsx");
            }
            catch (PawnshopApplicationException ex)
            {
                _reportLog.Log(reportLog);
                throw;
            }
            catch (Exception ex)
            {
                _reportLog.Log(reportLog);
                _eventLog.Log(EventCode.ReportDownload, EventStatus.Failed, null, null, JsonConvert.SerializeObject(request), JsonConvert.SerializeObject(ex), null, _sessionContext.UserId);
                throw;
            }
        }

        [HttpPost]
        [Route("GetReport")]
        public async Task<IActionResult> GetReport([FromBody] FcbReportRequest request)
        {
            request.Author = _sessionContext.UserName;
            request.AuthorId = _sessionContext.UserId;
            request.OrganizationId = 1;
            request.ReportType = Data.Models.Contracts.Kdn.FCBReportTypeCode.IndividualStandard;
            return Ok(await _fcb.GetReport(request));
        }


        private FcbKdnResponse GetFcbKdnFromLocalStorage(string iin)
        {
            var client = _clientRepository.FindByIdentityNumber(iin);

            if (client == null)
                return null;

            var manualCalculationClientExpense = _manualCalculationClientExpenseService.GetByClientId(client.Id);
            if (manualCalculationClientExpense is null)
                return null;

            return new FcbKdnResponse()
            {
                Debt = manualCalculationClientExpense.Debt
            };
        }
    }
}
