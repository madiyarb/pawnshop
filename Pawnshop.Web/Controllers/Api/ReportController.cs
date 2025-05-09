using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Pawnshop.Core;
using Pawnshop.Core.Exceptions;
using Pawnshop.Core.Options;
using Pawnshop.Data.Access;
using Pawnshop.Data.Access.AccountingCore;
using Pawnshop.Data.Access.Reports;
using Pawnshop.Data.Models.CashOrders;
using Pawnshop.Data.Models.Files;
using Pawnshop.Data.Models.Membership;
using Pawnshop.Web.Engine;
using Pawnshop.Web.Engine.Export.Reports;
using Pawnshop.Services.Storage;
using Pawnshop.Web.Models.List;
using Pawnshop.Web.Models.Reports;
using Pawnshop.Web.Models.Reports.AccountAnalysis;
using Pawnshop.Web.Models.Reports.AccountCard;
using Pawnshop.Web.Models.Reports.AccountCycle;
using Pawnshop.Web.Models.Reports.CashBook;
using Pawnshop.Web.Models.Reports.CashReport;
using Pawnshop.Web.Models.Reports.ConsolidateProfitReport;
using Pawnshop.Web.Models.Reports.ConsolidateReport;
using Pawnshop.Web.Models.Reports.ContractMonitoring;
using Pawnshop.Web.Models.Reports.DailyReport;
using Pawnshop.Web.Models.Reports.DelayReport;
using Pawnshop.Web.Models.Reports.DiscountReport;
using Pawnshop.Web.Models.Reports.ExpenseReports;
using Pawnshop.Web.Models.Reports.GoldPrice;
using Pawnshop.Web.Models.Reports.InsuranceReport;
using Pawnshop.Web.Models.Reports.OperationalReport;
using Pawnshop.Web.Models.Reports.OrderRegister;
using Pawnshop.Web.Models.Reports.PaymentReport;
using Pawnshop.Web.Models.Reports.ProfitReport;
using Pawnshop.Web.Models.Reports.ReconciliationReport;
using Pawnshop.Web.Models.Reports.SellingReport;
using Pawnshop.Web.Models.Reports.SplitProfits;
using Pawnshop.Web.Models.Reports.IssuanceReport;
using Pawnshop.Web.Models.Reports.ReinforceAndWithdrawReport;
using Pawnshop.Web.Models.Reports.AccountableReport;
using Pawnshop.Web.Models.Reports.NotificationReport;
using Pawnshop.Web.Models.Reports.RegionMonitoringReport;
using Pawnshop.Web.Models.Reports.StatisticsReport;
using Pawnshop.Web.Models.Reports.AccountMfoReport;
using Pawnshop.Web.Models.Reports.SoftCollectionReport;
using Pawnshop.Web.Models.Reports.SfkTransferedReport;
using Stimulsoft.Report;
using Stimulsoft.Report.Dictionary;
using Stimulsoft.Report.Mvc;
using Pawnshop.Web.Engine.Middleware;
using Pawnshop.Data.Models.Audit;
using Pawnshop.Web.Models.Reports.ClientExpensesReport;
using Pawnshop.Web.Models.Reports.CollateralReport;
using Pawnshop.Web.Models.Reports.VintageAnalysis;
using Pawnshop.Web.Models.Reports.TransferredContractsReport;
using Pawnshop.Web.Models.Reports.AccountMonitoringReport;
using Pawnshop.Web.Models.Reports.ConsolidateIssuanceReport;
using Pawnshop.Web.Models.Reports.AnalysisIssuanceReport;
using Pawnshop.Web.Models.Reports.AttractionChannelReport;
using Pawnshop.Web.Models.Reports.CarsParkingStatusReport;
using Pawnshop.Web.Models.Reports.CarParkingStatusReportForCARTAS;
using Pawnshop.Web.Models.Reports.UserPermissionsReport;
using Pawnshop.Web.Models.Reports.PosTerminalBookReport;
using Pawnshop.Web.Models.Reports.CashInTransitReport;
using Pawnshop.Web.Models.Reports.WithoutDrivingLicense;
using Pawnshop.Web.Models.Reports.BuyoutContractsWithInscriptionReport;
using Pawnshop.Web.Models.Reports.OnlinePaymentManageReport;
using Pawnshop.Web.Models.Reports.PaymentsMoreThanTenReport;
using Pawnshop.Web.Models.Reports.PhotoReport;
using Pawnshop.Web.Models.Reports.SMSSenderReport;
using Pawnshop.Web.Models.AuditOnPledgesReport;
using Pawnshop.Web.Models.Reports.ContractDiscountReport;
using Pawnshop.Web.Models.Reports.TasOnlineReport;
using Pawnshop.Web.Models.Reports.TmfReport;
using Pawnshop.Web.Models.Reports.InsurancePolicyReport;
using Pawnshop.Web.Models.Reports.InsurancePolicyActReport;
using Pawnshop.Web.Models.Reports.PrepaymentMonitoringReport;
using Pawnshop.Web.Models.Reports.DelayReportForSoftCollection;
using Pawnshop.Web.Models.Reports.BuyoutContractsReport;
using Pawnshop.Web.Models.Reports.KdnFailureStatisticsReport;
using Pawnshop.Web.Models.Reports.CollectionReport;
using Pawnshop.Web.Models.Reports.CustomerInteractionReport;
using Pawnshop.Web.Models.Reports.ApplicationsReport;
using Pawnshop.Data.Models.Reports;
using Pawnshop.Services;
using Pawnshop.Core.Queries;

namespace Pawnshop.Web.Controllers.Api
{
    public class ReportController : Controller
    {
        private readonly MemberRepository _memberRepository;
        private readonly ContractMonitoringRepository _contractMonitoringRepository;
        private readonly AccountAnalysisRepository _accountAnalysisRepository;

        private readonly ContractMonitoringExcelBuilder _contractMonitoringExcelBuilder;
        private readonly AccountAnalysisExcelBuilder _accountAnalysisExcelBuilder;

        private readonly IStorage _storage;
        private readonly ISessionContext _sessionContext;
        private readonly BranchContext _branchContext;
        private readonly EnviromentAccessOptions _options;

        private readonly AccountPlanRepository _accountPlanRepository;
        private readonly ReportLogsRepository _reportLog;
        private readonly ReportsRepository _reportsRepository;
        private readonly IEventLog _eventLog;

        public ReportController(MemberRepository memberRepository,
            ContractMonitoringRepository contractMonitoringRepository,
            AccountAnalysisRepository accountAnalysisRepository,
            ContractMonitoringExcelBuilder contractMonitoringExcelBuilder,
            AccountAnalysisExcelBuilder accountAnalysisExcelBuilder, IEventLog eventLog,
            IStorage storage, ISessionContext sessionContext, BranchContext branchContext, IOptions<EnviromentAccessOptions> options,
            AccountPlanRepository accountPlanRepository, ReportLogsRepository reportLogsRepository, ReportsRepository reportsRepository)
        {
            _memberRepository = memberRepository;
            _contractMonitoringRepository = contractMonitoringRepository;
            _accountAnalysisRepository = accountAnalysisRepository;

            _contractMonitoringExcelBuilder = contractMonitoringExcelBuilder;
            _accountAnalysisExcelBuilder = accountAnalysisExcelBuilder;
            _eventLog = eventLog;

            _storage = storage;
            _sessionContext = sessionContext;
            _branchContext = branchContext;
            _options = options.Value;

            _accountPlanRepository = accountPlanRepository;
            _reportLog = reportLogsRepository;
            _reportsRepository = reportsRepository;
        }

        [HttpPost]
        [Authorize(Permissions.ContractMonitoringView)]
        public ContractMonitoringModel ContractMonitoring([FromBody] ListQueryModel<ContractMonitoringQueryModel> listQuery)
        {
            if (listQuery == null) listQuery = new ListQueryModel<ContractMonitoringQueryModel>();
            if (listQuery.Model == null) listQuery.Model = new ContractMonitoringQueryModel
            {
                BranchId = _branchContext.Branch.Id
            };

            if (listQuery.Model.BeginDate == DateTime.MinValue) listQuery.Model.BeginDate = DateTime.Now.Date;
            if (listQuery.Model.EndDate == DateTime.MinValue) listQuery.Model.EndDate = DateTime.Now.Date.AddHours(23).AddMinutes(59).AddSeconds(59);

            listQuery.Model.BeginDate = listQuery.Model.BeginDate.Date;
            listQuery.Model.EndDate = listQuery.Model.EndDate.Date.AddHours(23).AddMinutes(59).AddSeconds(59);

            var model = new ContractMonitoringModel
            {
                BeginDate = listQuery.Model.BeginDate,
                EndDate = listQuery.Model.EndDate.Date,
                BranchName = _memberRepository.Groups(_sessionContext.UserId, null)
                    .FirstOrDefault(g => g.Type == GroupType.Branch && g.Id == listQuery.Model.BranchId)?.DisplayName,
                ProlongDayCount = listQuery.Model.ProlongDayCount,
                CollateralType = listQuery.Model.CollateralType,
                DisplayStatus = listQuery.Model.DisplayStatus,
                LoanCost = listQuery.Model.LoanCost,
                List = _contractMonitoringRepository.List(listQuery.Model).ToList(),
            };

            return model;
        }

        [HttpPost]
        [Authorize(Permissions.ContractMonitoringView)]
        public async Task<IActionResult> ExportContractMonitoring([FromBody] ListQueryModel<ContractMonitoringQueryModel> listQuery)
        {
            if (listQuery == null) listQuery = new ListQueryModel<ContractMonitoringQueryModel>();
            if (listQuery.Model == null) listQuery.Model = new ContractMonitoringQueryModel
            {
                BranchId = _branchContext.Branch.Id
            };

            if (listQuery.Model.BeginDate == DateTime.MinValue) listQuery.Model.BeginDate = DateTime.Now.Date;
            if (listQuery.Model.EndDate == DateTime.MinValue) listQuery.Model.EndDate = DateTime.Now.Date.AddHours(23).AddMinutes(59).AddSeconds(59);

            listQuery.Model.BeginDate = listQuery.Model.BeginDate.Date;
            listQuery.Model.EndDate = listQuery.Model.EndDate.Date.AddHours(23).AddMinutes(59).AddSeconds(59);

            var model = new ContractMonitoringModel
            {
                BeginDate = listQuery.Model.BeginDate,
                EndDate = listQuery.Model.EndDate,
                BranchName = _memberRepository.Groups(_sessionContext.UserId, null)
                    .FirstOrDefault(g => g.Type == GroupType.Branch && g.Id == listQuery.Model.BranchId)?.DisplayName,
                ProlongDayCount = listQuery.Model.ProlongDayCount,
                CollateralType = listQuery.Model.CollateralType,
                DisplayStatus = listQuery.Model.DisplayStatus,
                LoanCost = listQuery.Model.LoanCost,
                List = _contractMonitoringRepository.List(listQuery.Model).ToList(),
            };

            using (var stream = _contractMonitoringExcelBuilder.Build(model))
            {
                var fileName = await _storage.Save(stream, ContainerName.Temp, "contractMonitoring.xlsx");
                string contentType;
                new FileExtensionContentTypeProvider().TryGetContentType(fileName, out contentType);

                var fileRow = new FileRow
                {
                    CreateDate = DateTime.Now,
                    ContentType = contentType ?? "application/octet-stream",
                    FileName = fileName,
                    FilePath = fileName
                };
                return Ok(fileRow);
            }
        }

        [HttpPost]
        [Authorize(Permissions.AccountAnalysisView)]
        public AccountAnalysisModel AccountAnalysis([FromBody] ListQueryModel<AccountAnalysisQueryModel> listQuery)
        {
            if (listQuery == null) throw new ArgumentNullException(nameof(listQuery));
            if (listQuery.Model == null) listQuery.Model = new AccountAnalysisQueryModel
            {
                Month = DateTime.Now.Month,
                Year = DateTime.Now.Year,
                BranchId = _branchContext.Branch.Id
            };

            var report = _reportsRepository.Find(new { ReportCode = "AccountAnalysis" });

            if (report == null)
                throw new PawnshopApplicationException("Отчет не найден!");

            var reportLog = new ReportLog()
            {
                AuthorId = _sessionContext.UserId,
                AuthorName = _sessionContext.UserName,
                ReportId = report.Id,
                Request = JsonConvert.SerializeObject(listQuery),
                CreateDate = DateTime.Now,
                IsSuccessful = false,
            };

            try
            {
                var query = new
                {
                    BeginDate = new DateTime(listQuery.Model.Year, listQuery.Model.Month, 1),
                    EndDate = new DateTime(listQuery.Model.Year, listQuery.Model.Month, 1, 23, 59, 59).AddMonths(1).AddDays(-1),
                    BranchId = listQuery.Model.BranchId,
                    AccountId = listQuery.Model.AccountPlanId
                };

                var model = new AccountAnalysisModel
                {
                    BeginDate = query.BeginDate,
                    EndDate = query.EndDate,
                    BranchId = listQuery.Model.BranchId,
                    BranchName = _memberRepository.Groups(_sessionContext.UserId, null)
                        .FirstOrDefault(g => g.Type == GroupType.Branch && g.Id == listQuery.Model.BranchId)?.DisplayName,
                    AccountId = listQuery.Model.AccountPlanId,
                    AccountCode = _accountPlanRepository.Get(listQuery.Model.AccountPlanId)?.Code,
                    List = _accountAnalysisRepository.List(query).ToList(),
                    Group = _accountAnalysisRepository.Group(query),
                };

                reportLog.IsSuccessful = true;
                _reportLog.Log(reportLog);

                return model;
            }
            catch (Exception ex)
            {
                _reportLog.Log(reportLog);
                _eventLog.Log(EventCode.ReportDownload, EventStatus.Failed, null, null, JsonConvert.SerializeObject(listQuery), JsonConvert.SerializeObject(ex), null, _sessionContext.UserId);
                throw;
            }
        }

        public ListModel<dynamic> AccountAnalysisList([FromBody] ListQueryModel<AccountAnalysisQueryModel> listQuery)
        {
            if (listQuery == null) throw new ArgumentNullException(nameof(listQuery));
            if (listQuery.Model == null) listQuery.Model = new AccountAnalysisQueryModel
            {
                Month = DateTime.Now.Month,
                Year = DateTime.Now.Year,
                BranchId = _branchContext.Branch.Id
            };

            var report = _reportsRepository.Find(new { ReportCode = "AccountAnalysis" });

            if (report == null)
                throw new PawnshopApplicationException("Отчет не найден!");

            var reportLog = new ReportLog()
            {
                AuthorId = _sessionContext.UserId,
                AuthorName = _sessionContext.UserName,
                ReportId = report.Id,
                Request = JsonConvert.SerializeObject(listQuery),
                CreateDate = DateTime.Now,
                IsSuccessful = false,
            };

            try
            {
                var query = new
                {
                    BeginDate = new DateTime(listQuery.Model.Year, listQuery.Model.Month, 1),
                    EndDate = new DateTime(listQuery.Model.Year, listQuery.Model.Month, 1, 23, 59, 59).AddMonths(1).AddDays(-1),
                    BranchId = listQuery.Model.BranchId,
                    AccountId = listQuery.Model.AccountPlanId
                };

                var result = new ListModel<dynamic>
                {
                    List = _accountAnalysisRepository.List(query).ToList(),
                    Count = _accountAnalysisRepository.List(query).ToList().Count
                };

                reportLog.IsSuccessful = true;
                _reportLog.Log(reportLog);

                return result;
            }
            catch (Exception ex)
            {
                _reportLog.Log(reportLog);
                _eventLog.Log(EventCode.ReportDownload, EventStatus.Failed, null, null, JsonConvert.SerializeObject(listQuery), JsonConvert.SerializeObject(ex), null, _sessionContext.UserId);
                throw;
            }
        }

        [HttpPost]
        public ListModel<CashOrder> CashOrders([FromBody] ListQueryModel<CashOrdersQueryModel> listQuery)
        {
            if (listQuery == null) throw new ArgumentNullException(nameof(listQuery));
            if (listQuery.Model == null) listQuery.Model = new CashOrdersQueryModel
            {
                Month = DateTime.Now.Month,
                Year = DateTime.Now.Year,
                BranchId = _branchContext.Branch.Id
            };

            try
            {
                var query = new
                {
                    BeginDate = new DateTime(listQuery.Model.Year, listQuery.Model.Month, 1),
                    EndDate = new DateTime(listQuery.Model.Year, listQuery.Model.Month, 1, 23, 59, 59).AddMonths(1).AddDays(-1),
                    BranchId = listQuery.Model.BranchId,
                    DebitAccountId = listQuery.Model.DebitAccountId,
                    CreditAccountId = listQuery.Model.CreditAccountId
                };

                return new ListModel<CashOrder>
                {
                    List = _accountAnalysisRepository.CashOrders(query).ToList(),
                };
            }
            catch (Exception ex)
            {
                _eventLog.Log(EventCode.ReportDownload, EventStatus.Failed, null, null, JsonConvert.SerializeObject(listQuery), JsonConvert.SerializeObject(ex), null, _sessionContext.UserId);
                throw;
            }
        }

        [HttpPost]
        [Authorize(Permissions.AccountAnalysisView)]
        public async Task<IActionResult> ExportAccountAnalysis([FromBody] ListQueryModel<AccountAnalysisQueryModel> listQuery)
        {
            if (listQuery == null) throw new ArgumentNullException(nameof(listQuery));
            if (listQuery.Model == null) listQuery.Model = new AccountAnalysisQueryModel
            {
                Month = DateTime.Now.Month,
                Year = DateTime.Now.Year,
                BranchId = _branchContext.Branch.Id
            };

            var report = _reportsRepository.Find(new { ReportCode = "AccountAnalysis" });

            if (report == null)
                throw new PawnshopApplicationException("Отчет не найден!");

            var reportLog = new ReportLog()
            {
                AuthorId = _sessionContext.UserId,
                AuthorName = _sessionContext.UserName,
                ReportId = report.Id,
                Request = JsonConvert.SerializeObject(listQuery),
                CreateDate = DateTime.Now,
                IsSuccessful = false,
            };

            try
            {
                var query = new
                {
                    BeginDate = new DateTime(listQuery.Model.Year, listQuery.Model.Month, 1),
                    EndDate = new DateTime(listQuery.Model.Year, listQuery.Model.Month, 1, 23, 59, 59).AddMonths(1).AddDays(-1),
                    BranchId = listQuery.Model.BranchId,
                    AccountPlanId = listQuery.Model.AccountPlanId
                };

                var model = new AccountAnalysisModel
                {
                    BeginDate = query.BeginDate,
                    EndDate = query.EndDate,
                    BranchId = listQuery.Model.BranchId,
                    BranchName = _memberRepository.Groups(_sessionContext.UserId, null)
                        .FirstOrDefault(g => g.Type == GroupType.Branch && g.Id == listQuery.Model.BranchId)?.DisplayName,
                    AccountId = listQuery.Model.AccountPlanId,
                    AccountCode = _accountPlanRepository.Get(listQuery.Model.AccountPlanId)?.Code,
                    List = _accountAnalysisRepository.List(query).ToList(),
                    Group = _accountAnalysisRepository.Group(query),
                };

                using (var stream = _accountAnalysisExcelBuilder.Build(model))
                {
                    var fileName = await _storage.Save(stream, ContainerName.Temp, "accountAnalysis.xlsx");
                    string contentType;
                    new FileExtensionContentTypeProvider().TryGetContentType(fileName, out contentType);

                    var fileRow = new FileRow
                    {
                        CreateDate = DateTime.Now,
                        ContentType = contentType ?? "application/octet-stream",
                        FileName = fileName,
                        FilePath = fileName
                    };

                    reportLog.IsSuccessful = true;
                    _reportLog.Log(reportLog);

                    return Ok(fileRow);
                }
            }
            catch (Exception ex)
            {
                _reportLog.Log(reportLog);
                _eventLog.Log(EventCode.ReportDownload, EventStatus.Failed, null, null, JsonConvert.SerializeObject(listQuery), JsonConvert.SerializeObject(ex), null, _sessionContext.UserId);
                throw;
            }
        }

        [HttpPost]
        [Authorize]
        [Event(EventCode.ReportDownload, EventMode = EventMode.Request, EntityType = EntityType.None)]
        public string Report([FromBody] ReportQueryModel queryModel)
        {
            if (queryModel == null) 
                throw new ArgumentNullException(nameof(queryModel));
            if (string.IsNullOrWhiteSpace(queryModel.ReportName)) 
                throw new ArgumentNullException(nameof(queryModel.ReportName));

            var report = _reportsRepository.Find(new { ReportCode = queryModel.ReportName });

            if (report == null) 
                throw new PawnshopApplicationException("Отчет не найден!");

            var reportLog = new ReportLog()
            {
                AuthorId = _sessionContext.UserId,
                AuthorName = _sessionContext.UserName,
                ReportId = report.Id,
                Request = JsonConvert.SerializeObject(queryModel),
                CreateDate = DateTime.Now,
                IsSuccessful = false,
            };

            var reportResult = new StiReport();

            try
            {
                var notEnoughPermissionMessage = "У вас недостаточно прав для просмотра отчета, обратитесь к aдминистратору";
                if (queryModel.ReportName == "CashBookReport")
                {
                    if (!_sessionContext.HasPermission(Permissions.CashBookView)) throw new PawnshopApplicationException(notEnoughPermissionMessage);

                    var model = JsonConvert.DeserializeObject<CashBookQueryModel>(queryModel.ReportQuery);
                    reportResult.Load(StiNetCoreHelper.MapPath(this, "Reports/CashBookReport.mrt"));
                    reportResult["branchId"] = model.BranchId;
                    reportResult["beginDate"] = model.BeginDate.Date;
                    reportResult["endDate"] = model.EndDate.Date.AddHours(23).AddMinutes(59).AddSeconds(59);
                    reportResult["chiefAccountantName"] = _branchContext.Configuration.LegalSettings.ChiefAccountantName;
                    reportResult["cashierName"] = _branchContext.Configuration.LegalSettings.CashierName;
                }
                else if (queryModel.ReportName == "CashBalanceReport")
                {
                    if (!_sessionContext.HasPermission(Permissions.CashBalanceView)) throw new PawnshopApplicationException(notEnoughPermissionMessage);

                    var model = JsonConvert.DeserializeObject<CashBalanceQueryModel>(queryModel.ReportQuery);
                    reportResult.Load(StiNetCoreHelper.MapPath(this, "Reports/CashBalanceReport.mrt"));
                    reportResult["userId"] = _sessionContext.UserId;
                    reportResult["beginDate"] = model.CurrentDate.Date;
                    reportResult["endDate"] = model.CurrentDate.Date.AddHours(23).AddMinutes(59).AddSeconds(59);
                }
                else if (queryModel.ReportName == "CashReport")
                {
                    if (!_sessionContext.HasPermission(Permissions.CashReportView)) throw new PawnshopApplicationException(notEnoughPermissionMessage);

                    var model = JsonConvert.DeserializeObject<CashReportQueryModel>(queryModel.ReportQuery);
                    reportResult.Load(StiNetCoreHelper.MapPath(this, "Reports/CashReport.mrt"));
                    reportResult["branchId"] = model.BranchId;
                    reportResult["beginDate"] = new DateTime(model.Year, model.Month, 1);
                    reportResult["endDate"] = new DateTime(model.Year, model.Month, 1, 23, 59, 59).AddMonths(1).AddDays(-1);
                }
                else if (queryModel.ReportName == "SellingReport")
                {
                    if (!_sessionContext.HasPermission(Permissions.SellingReportView)) throw new PawnshopApplicationException(notEnoughPermissionMessage);

                    var model = JsonConvert.DeserializeObject<SellingReportQueryModel>(queryModel.ReportQuery);
                    reportResult.Load(StiNetCoreHelper.MapPath(this, "Reports/SellingReport.mrt"));
                    reportResult["branchIds"] = string.Join(",", model.BranchIds);
                    reportResult["beginDate"] = model.BeginDate.Date;
                    reportResult["endDate"] = model.EndDate.Date.AddHours(23).AddMinutes(59).AddSeconds(59);
                    reportResult["status"] = model.Status ?? 0;
                }
                else if (queryModel.ReportName == "DelayReport")
                {
                    if (!_sessionContext.HasPermission(Permissions.DelayReportView)) throw new PawnshopApplicationException(notEnoughPermissionMessage);

                    var model = JsonConvert.DeserializeObject<DelayReportQueryModel>(queryModel.ReportQuery);
                    reportResult.Load(StiNetCoreHelper.MapPath(this, "Reports/DelayReport.mrt"));
                    reportResult["beginDelayCount"] = model.BeginDelayCount;
                    reportResult["endDelayCount"] = model.EndDelayCount;
                    reportResult["collateralType"] = model.CollateralType;
                    reportResult["branchIds"] = string.Join(",", model.BranchIds);
                    reportResult["inscriptionVisible"] =
                        _sessionContext.Permissions.Any(x => x.Contains(Permissions.InscriptionManage));
                }
                else if (queryModel.ReportName == "DelayNewReport")
                {
                    if (!_sessionContext.HasPermission(Permissions.DelayReportView)) throw new PawnshopApplicationException(notEnoughPermissionMessage);

                    var model = JsonConvert.DeserializeObject<DelayReportQueryModel>(queryModel.ReportQuery);
                    reportResult.Load(StiNetCoreHelper.MapPath(this, "Reports/DelayNewReport.mrt"));
                    reportResult["beginDelayCount"] = model.BeginDelayCount;
                    reportResult["endDelayCount"] = model.EndDelayCount;
                    reportResult["collateralType"] = model.CollateralType;
                    reportResult["branchIds"] = string.Join(",", model.BranchIds);
                    reportResult["inscriptionVisible"] =
                        _sessionContext.Permissions.Any(x => x.Contains(Permissions.InscriptionManage));
                }
                else if (queryModel.ReportName == "DelayNewShortReport")
                {
                    if (!_sessionContext.HasPermission(Permissions.DelayReportTasOnlineView)) throw new PawnshopApplicationException(notEnoughPermissionMessage);

                    var model = JsonConvert.DeserializeObject<DelayReportQueryModel>(queryModel.ReportQuery);
                    reportResult.Load(StiNetCoreHelper.MapPath(this, "Reports/DelayNewShortReport.mrt"));
                    reportResult["beginDelayCount"] = model.BeginDelayCount;
                    reportResult["endDelayCount"] = model.EndDelayCount;
                    reportResult["collateralType"] = model.CollateralType;
                    reportResult["branchIds"] = string.Join(",", model.BranchIds);
                    reportResult["inscriptionVisible"] =
                        _sessionContext.Permissions.Any(x => x.Contains(Permissions.InscriptionManage));
                }
                else if (queryModel.ReportName == "TransferredContractsReport")
                {
                    if (!_sessionContext.HasPermission(Permissions.TransferredContractsReportView)) throw new PawnshopApplicationException(notEnoughPermissionMessage);

                    var model = JsonConvert.DeserializeObject<TransferredContractsReportQueryModel>(queryModel.ReportQuery);
                    reportResult.Load(StiNetCoreHelper.MapPath(this, "Reports/TransferredContractsReport.mrt"));
                    reportResult["branchIds"] = string.Join(",", model.BranchIds);
                    reportResult["ReportDate"] = model.ReportDate.Date;
                }
                else if (queryModel.ReportName == "DailyReport")
                {
                    if (!_sessionContext.HasPermission(Permissions.DailyReportView)) throw new PawnshopApplicationException(notEnoughPermissionMessage);

                    var model = JsonConvert.DeserializeObject<DailyReportQueryModel>(queryModel.ReportQuery);
                    reportResult.Load(StiNetCoreHelper.MapPath(this, "Reports/DailyReport.mrt"));
                    reportResult["branchId"] = model.BranchId;
                    reportResult["beginDate"] = model.CurrentDate.Date;
                    reportResult["endDate"] = model.CurrentDate.Date.AddHours(23).AddMinutes(59).AddSeconds(59);
                    reportResult["accountId"] = _branchContext.Configuration.CashOrderSettings.CashAccountId;
                }
                else if (queryModel.ReportName == "ConsolidateReport")
                {
                    if (!_sessionContext.HasPermission(Permissions.ConsolidateReportView)) throw new PawnshopApplicationException(notEnoughPermissionMessage);

                    var model = JsonConvert.DeserializeObject<ConsolidateReportQueryModel>(queryModel.ReportQuery);
                    reportResult.Load(StiNetCoreHelper.MapPath(this, "Reports/ConsolidateReport.mrt"));
                    reportResult["branchId"] = model.BranchId;
                    reportResult["beginDate"] = model.BeginDate.Date;
                    reportResult["endDate"] = model.EndDate.Date.AddHours(23).AddMinutes(59).AddSeconds(59);
                    reportResult["cashAccountId"] = _branchContext.Configuration.CashOrderSettings.CashAccountId;
                    reportResult["goldAccountId"] = _branchContext.Configuration.CashOrderSettings.GoldCollateralSettings.SellingSettings.CreditId ?? 0;
                    reportResult["autoAccountId"] = _branchContext.Configuration.CashOrderSettings.CarCollateralSettings.SellingSettings.CreditId ?? 0;
                    reportResult["goodsAccountId"] = _branchContext.Configuration.CashOrderSettings.GoodCollateralSettings.SellingSettings.CreditId ?? 0;
                    reportResult["machineryAccountId"] = _branchContext.Configuration.CashOrderSettings.MachineryCollateralSettings.SellingSettings.CreditId ?? 0;
                    reportResult["unsecuredAccountId"] = _branchContext.Configuration.CashOrderSettings.UnsecuredCollateralSettings.SupplySettings.DebitId ?? 0;
                    reportResult["userId"] = _sessionContext.UserId;
                }
                else if (queryModel.ReportName == "SfkConsolidateReport")
                {
                    if (!_sessionContext.HasPermission(Permissions.ConsolidateReportView)) throw new PawnshopApplicationException(notEnoughPermissionMessage);

                    var model = JsonConvert.DeserializeObject<ConsolidateReportQueryModel>(queryModel.ReportQuery);
                    reportResult.Load(StiNetCoreHelper.MapPath(this, "Reports/SfkConsolidateReport.mrt"));
                    reportResult["branchId"] = model.BranchId;
                    if (model.IsPeriod == true)
                    {
                        reportResult["beginDate"] = model.BeginDate.Date;
                        reportResult["endDate"] = model.EndDate.Date.AddHours(23).AddMinutes(59).AddSeconds(59);
                    }
                    else
                    {
                        reportResult["beginDate"] = model.BeginDate.Date.AddHours(23).AddMinutes(59).AddSeconds(59);
                    }
                    reportResult["userId"] = _sessionContext.UserId;
                    reportResult["isPeriod"] = model.IsPeriod;
                }
                else if (queryModel.ReportName == "DiscountReport")
                {
                    if (!_sessionContext.HasPermission(Permissions.DiscountReportView)) throw new PawnshopApplicationException(notEnoughPermissionMessage);

                    var model = JsonConvert.DeserializeObject<DiscountReportQueryModel>(queryModel.ReportQuery);
                    reportResult.Load(StiNetCoreHelper.MapPath(this, "Reports/DiscountReport.mrt"));
                    reportResult["branchId"] = model.BranchId;
                    if (model.Month == 0)
                    {
                        reportResult["beginDate"] = new DateTime(model.Year, 1, 1);
                        reportResult["endDate"] = new DateTime(model.Year, 12, 31, 23, 59, 59);
                    }
                    else
                    {
                        reportResult["beginDate"] = new DateTime(model.Year, model.Month, 1);
                        reportResult["endDate"] = new DateTime(model.Year, model.Month, 1, 23, 59, 59).AddMonths(1).AddDays(-1);
                    }
                }
                else if (queryModel.ReportName == "AccountCardReport")
                {
                    if (!_sessionContext.HasPermission(Permissions.AccountCardView)) throw new PawnshopApplicationException(notEnoughPermissionMessage);

                    var model = JsonConvert.DeserializeObject<AccountCardQueryModel>(queryModel.ReportQuery);
                    reportResult.Load(StiNetCoreHelper.MapPath(this, "Reports/AccountCardReport.mrt"));
                    reportResult["branchId"] = model.BranchId;
                    reportResult["beginDate"] = model.BeginDate.Date;
                    reportResult["endDate"] = model.EndDate.Date.AddHours(23).AddMinutes(59).AddSeconds(59);
                    reportResult["accountId"] = model.AccountId;
                }
                else if (queryModel.ReportName == "AccountCycleReport")
                {
                    if (!_sessionContext.HasPermission(Permissions.AccountCycleView)) throw new PawnshopApplicationException(notEnoughPermissionMessage);

                    var model = JsonConvert.DeserializeObject<AccountCycleQueryModel>(queryModel.ReportQuery);
                    reportResult.Load(StiNetCoreHelper.MapPath(this, "Reports/AccountCycleReport.mrt"));
                    reportResult["branchId"] = model.BranchId;
                    reportResult["beginDate"] = model.BeginDate.Date;
                    reportResult["endDate"] = model.EndDate.Date.AddHours(23).AddMinutes(59).AddSeconds(59);
                }
                else if (queryModel.ReportName == "GoldPriceReport")
                {
                    if (!_sessionContext.HasPermission(Permissions.GoldPriceView)) throw new PawnshopApplicationException(notEnoughPermissionMessage);

                    var model = JsonConvert.DeserializeObject<GoldPriceQueryModel>(queryModel.ReportQuery);
                    reportResult.Load(StiNetCoreHelper.MapPath(this, "Reports/GoldPriceReport.mrt"));
                    reportResult["branchId"] = model.BranchId;
                    reportResult["beginDate"] = model.BeginDate.Date;
                    reportResult["endDate"] = model.EndDate.Date.AddHours(23).AddMinutes(59).AddSeconds(59);
                    reportResult["queryPrice"] = model.QueryPrice;
                }
                else if (queryModel.ReportName == "ExpenseMonthReport")
                {
                    if (!_sessionContext.HasPermission(Permissions.ExpenseMonthReportView)) throw new PawnshopApplicationException(notEnoughPermissionMessage);

                    var model = JsonConvert.DeserializeObject<ExpenseMonthReportQueryModel>(queryModel.ReportQuery);
                    reportResult.Load(StiNetCoreHelper.MapPath(this, "Reports/ExpenseMonthReport.mrt"));
                    reportResult["branchId"] = model.BranchId;
                    reportResult["beginDate"] = new DateTime(model.Year, model.Month, 1);
                    reportResult["endDate"] = new DateTime(model.Year, model.Month, 1, 23, 59, 59).AddMonths(1).AddDays(-1);
                    reportResult["userId"] = _sessionContext.UserId;
                }
                else if (queryModel.ReportName == "ExpenseYearReport")
                {
                    if (!_sessionContext.HasPermission(Permissions.ExpenseYearReportView)) throw new PawnshopApplicationException(notEnoughPermissionMessage);

                    var model = JsonConvert.DeserializeObject<ExpenseYearReportQueryModel>(queryModel.ReportQuery);
                    reportResult.Load(StiNetCoreHelper.MapPath(this, "Reports/ExpenseYearReport.mrt"));
                    reportResult["branchId"] = model.BranchId;
                    reportResult["beginDate"] = new DateTime(model.Year, 1, 1);
                    reportResult["endDate"] = new DateTime(model.Year, 12, 31, 23, 59, 59);
                    reportResult["userId"] = _sessionContext.UserId;
                }
                else if (queryModel.ReportName == "OperationalReport")
                {
                    if (!_sessionContext.HasPermission(Permissions.OperationalReportView)) throw new PawnshopApplicationException(notEnoughPermissionMessage);

                    var model = JsonConvert.DeserializeObject<OperationalReportQueryModel>(queryModel.ReportQuery);
                    reportResult.Load(StiNetCoreHelper.MapPath(this, "Reports/OperationalReport.mrt"));
                    reportResult["beginDate"] = model.BeginDate;
                    reportResult["endDate"] = model.EndDate.Date.AddHours(23).AddMinutes(59).AddSeconds(59);
                    reportResult["userId"] = _sessionContext.UserId;
                    reportResult["cashAccountId"] = _branchContext.Configuration.CashOrderSettings.CashAccountId;
                    reportResult["goldAccountId"] = _branchContext.Configuration.CashOrderSettings.GoldCollateralSettings.SellingSettings.CreditId;
                    reportResult["autoAccountId"] = _branchContext.Configuration.CashOrderSettings.CarCollateralSettings.SellingSettings.CreditId;
                    reportResult["goodsAccountId"] = _branchContext.Configuration.CashOrderSettings.GoodCollateralSettings.SellingSettings.CreditId;
                    reportResult["machineryAccountId"] = _branchContext.Configuration.CashOrderSettings.MachineryCollateralSettings.SellingSettings.CreditId;
                    reportResult["sfkTransferAccountId"] = _branchContext.Configuration.CashOrderSettings.CarTransferSettings.SupplyDebtSettings.DebitId;
                    reportResult["sfkBuyoutAccountId"] = _branchContext.Configuration.CashOrderSettings.CarTransferSettings.DebtSettings.CreditId;
                }
                else if (queryModel.ReportName == "SplitProfitReport")
                {
                    if (!_sessionContext.HasPermission(Permissions.SplitProfitReportView)) throw new PawnshopApplicationException(notEnoughPermissionMessage);

                    var model = JsonConvert.DeserializeObject<SplitProfitReportQueryModel>(queryModel.ReportQuery);
                    reportResult.Load(StiNetCoreHelper.MapPath(this, "Reports/SplitProfitReport.mrt"));
                    reportResult["branchId"] = model.BranchId;
                    reportResult["endDate"] = model.EndDate.Date.AddHours(23).AddMinutes(59).AddSeconds(59);
                    reportResult["userId"] = _sessionContext.UserId;
                }
                else if (queryModel.ReportName == "OrderRegisterReport")
                {
                    if (!_sessionContext.HasPermission(Permissions.OrderRegisterView)) throw new PawnshopApplicationException(notEnoughPermissionMessage);

                    var model = JsonConvert.DeserializeObject<OrderRegisterQueryModel>(queryModel.ReportQuery);
                    reportResult.Load(StiNetCoreHelper.MapPath(this, "Reports/OrderRegisterReport.mrt"));
                    reportResult["branchId"] = model.BranchId;
                    reportResult["beginDate"] = model.BeginDate.Date;
                    reportResult["endDate"] = model.EndDate.Date.AddHours(23).AddMinutes(59).AddSeconds(59);
                    reportResult["userId"] = _sessionContext.UserId;
                    reportResult["accountType"] = model.AccountType;
                    reportResult["accountPlanId"] = model.AccountPlanId;
                    reportResult["processingType"] = model.ProcessingType;
                }
                else if (queryModel.ReportName == "ProfitReport")
                {
                    if (!_sessionContext.HasPermission(Permissions.ProfitReportView)) throw new PawnshopApplicationException(notEnoughPermissionMessage);

                    var model = JsonConvert.DeserializeObject<ProfitReportQueryModel>(queryModel.ReportQuery);
                    reportResult.Load(StiNetCoreHelper.MapPath(this, "Reports/ProfitReport.mrt"));
                    reportResult["branchIds"] = string.Join(",", model.BranchIds);
                    reportResult["beginDate"] = model.BeginDate.Date;
                    reportResult["endDate"] = model.EndDate.Date.AddHours(23).AddMinutes(59).AddSeconds(59);
                    reportResult["collateralType"] = model.CollateralType;
                }
                else if (queryModel.ReportName == "SfkProfitReport")
                {
                    if (!_sessionContext.HasPermission(Permissions.ProfitReportView)) throw new PawnshopApplicationException(notEnoughPermissionMessage);

                    var model = JsonConvert.DeserializeObject<ProfitReportQueryModel>(queryModel.ReportQuery);
                    reportResult.Load(StiNetCoreHelper.MapPath(this, "Reports/SfkProfitReport.mrt"));
                    reportResult["branchIds"] = string.Join(",", model.BranchIds);
                    reportResult["beginDate"] = model.BeginDate.Date;
                    reportResult["endDate"] = model.EndDate.Date.AddHours(23).AddMinutes(59).AddSeconds(59);
                    reportResult["userId"] = _sessionContext.UserId;
                    reportResult["collateralType"] = model.CollateralType;
                }
                else if (queryModel.ReportName == "ReconciliationReport")
                {
                    if (!_sessionContext.HasPermission(Permissions.ReconciliationReportView)) throw new PawnshopApplicationException(notEnoughPermissionMessage);

                    var model = JsonConvert.DeserializeObject<ReconciliationReportQueryModel>(queryModel.ReportQuery);
                    reportResult.Load(StiNetCoreHelper.MapPath(this, "Reports/ReconciliationReport.mrt"));
                    reportResult["beginDate"] = model.BeginDate.Date;
                    reportResult["endDate"] = model.EndDate.Date.AddHours(23).AddMinutes(59).AddSeconds(59);
                }
                else if (queryModel.ReportName == "ConsolidateProfitReport")
                {
                    if (!_sessionContext.HasPermission(Permissions.ProfitReportView)) throw new PawnshopApplicationException(notEnoughPermissionMessage);

                    var model = JsonConvert.DeserializeObject<ConsolidateProfitReportQueryModel>(queryModel.ReportQuery);
                    reportResult.Load(StiNetCoreHelper.MapPath(this, "Reports/ConsolidateProfitReport.mrt"));
                    reportResult["beginDate"] = model.BeginDate.Date;
                    reportResult["endDate"] = model.EndDate.Date.AddHours(23).AddMinutes(59).AddSeconds(59);
                    reportResult["collateralType"] = model.CollateralType;
                }
                else if (queryModel.ReportName == "SfkConsolidateProfitReport")
                {
                    if (!_sessionContext.HasPermission(Permissions.ProfitReportView)) throw new PawnshopApplicationException(notEnoughPermissionMessage);

                    var model = JsonConvert.DeserializeObject<ConsolidateProfitReportQueryModel>(queryModel.ReportQuery);
                    reportResult.Load(StiNetCoreHelper.MapPath(this, "Reports/SfkConsolidateProfitReport.mrt"));
                    reportResult["beginDate"] = model.BeginDate.Date;
                    reportResult["endDate"] = model.EndDate.Date.AddHours(23).AddMinutes(59).AddSeconds(59);
                    reportResult["collateralType"] = model.CollateralType;
                }
                else if (queryModel.ReportName == "PaymentReport")
                {
                    if (!_sessionContext.HasPermission(Permissions.PaymentReportView)) throw new PawnshopApplicationException(notEnoughPermissionMessage);

                    var model = JsonConvert.DeserializeObject<PaymentReportQueryModel>(queryModel.ReportQuery);
                    reportResult.Load(StiNetCoreHelper.MapPath(this, "Reports/PaymentReport.mrt"));
                    reportResult["branchIds"] = string.Join(",", model.BranchIds);
                    reportResult["beginDate"] = model.CurrentDate.Date;
                    reportResult["endDate"] = model.CurrentDate.Date.AddHours(23).AddMinutes(59).AddSeconds(59);
                    reportResult["collateralType"] = model.CollateralType;
                }
                else if (queryModel.ReportName == "IssuanceReport")
                {
                    if (!_sessionContext.HasPermission(Permissions.IssuanceReportView)) throw new PawnshopApplicationException(notEnoughPermissionMessage);

                    var model = JsonConvert.DeserializeObject<IssuanceReportQueryModel>(queryModel.ReportQuery);
                    reportResult.Load(StiNetCoreHelper.MapPath(this, "Reports/IssuanceReport.mrt"));
                    reportResult["branchId"] = model.BranchId;
                    reportResult["beginDate"] = model.BeginDate.Date;
                    reportResult["endDate"] = model.EndDate.Date.AddHours(23).AddMinutes(59).AddSeconds(59);
                    reportResult["userId"] = _sessionContext.UserId;
                }
                else if (queryModel.ReportName == "ReinforceAndWithdrawReport")
                {
                    if (!_sessionContext.HasPermission(Permissions.ReinforceAndWithdrawReportView)) throw new PawnshopApplicationException(notEnoughPermissionMessage);

                    var model = JsonConvert.DeserializeObject<ReinforceAndWithdrawReportQueryModel>(queryModel.ReportQuery);
                    reportResult.Load(StiNetCoreHelper.MapPath(this, "Reports/ReinforceAndWithdrawReport.mrt"));
                    reportResult["branchId"] = model.BranchId;
                    reportResult["beginDate"] = model.BeginDate.Date;
                    reportResult["endDate"] = model.EndDate.Date.AddHours(23).AddMinutes(59).AddSeconds(59);
                }
                else if (queryModel.ReportName == "SfkTransferedReport")
                {
                    if (!_sessionContext.HasPermission(Permissions.ConsolidateReportView)) throw new PawnshopApplicationException(notEnoughPermissionMessage);

                    reportResult.Load(StiNetCoreHelper.MapPath(this, "Reports/SfkTransferedReport.mrt"));
                    var model = JsonConvert.DeserializeObject<SfkTransferedReportQueryModel>(queryModel.ReportQuery);
                    reportResult["endDate"] = model.EndDate.Date;
                    reportResult["poolNumber"] = model.PoolNumber;
                    reportResult["contractStatus"] = model.ContractStatus;
                }
                else if (queryModel.ReportName == "AccountableReport")
                {
                    if (!_sessionContext.HasPermission(Permissions.AccountableReportView)) throw new PawnshopApplicationException(notEnoughPermissionMessage);

                    reportResult.Load(StiNetCoreHelper.MapPath(this, "Reports/AccountableReport.mrt"));
                    var model = JsonConvert.DeserializeObject<AccountableReportQueryModel>(queryModel.ReportQuery);
                    reportResult["accountPlanIds"] = string.Join(",", model.AccountPlanIds);
                    reportResult["branchIds"] = string.Join(",", model.BranchIds);
                    reportResult["beginDate"] = model.BeginDate.Date;
                    reportResult["endDate"] = model.EndDate.Date.AddHours(23).AddMinutes(59).AddSeconds(59);
                    reportResult["userId"] = _sessionContext.UserId;
                    reportResult["isExpense"] = model.IsExpense;
                }
                else if (queryModel.ReportName == "NotificationReport")
                {
                    if (!_sessionContext.HasPermission(Permissions.NotificationReportView)) throw new PawnshopApplicationException(notEnoughPermissionMessage);

                    reportResult.Load(StiNetCoreHelper.MapPath(this, "Reports/NotificationReport.mrt"));
                    var model = JsonConvert.DeserializeObject<NotificationReportQueryModel>(queryModel.ReportQuery);
                    reportResult["branchId"] = model.BranchId;
                    reportResult["beginDate"] = model.BeginDate.Date;
                    reportResult["endDate"] = model.BeginDate.Date.AddHours(23).AddMinutes(59).AddSeconds(59);
                    reportResult["userId"] = _sessionContext.UserId;
                }
                else if (queryModel.ReportName == "RegionMonitoringReport")
                {
                    if (!_sessionContext.HasPermission(Permissions.RegionMonitoringReportView)) throw new PawnshopApplicationException(notEnoughPermissionMessage);

                    reportResult.Load(StiNetCoreHelper.MapPath(this, "Reports/RegionMonitoringReport.mrt"));
                    var model = JsonConvert.DeserializeObject<RegionMonitoringReportQueryModel>(queryModel.ReportQuery);
                    reportResult["branchIds"] = string.Join(",", model.BranchIds);
                    reportResult["beginDate"] = model.BeginDate.Date.AddHours(23).AddMinutes(59).AddSeconds(59);
                }
                else if (queryModel.ReportName == "StatisticsReport")
                {
                    if (!_sessionContext.HasPermission(Permissions.StatisticsReportView)) throw new PawnshopApplicationException(notEnoughPermissionMessage);

                    reportResult.Load(StiNetCoreHelper.MapPath(this, "Reports/StatisticsReport.mrt"));
                    var model = JsonConvert.DeserializeObject<StatisticsReportQueryModel>(queryModel.ReportQuery);
                    reportResult["branchIds"] = string.Join(",", model.BranchIds);
                    reportResult["beginDate"] = model.BeginDate.Date;
                    reportResult["endDate"] = model.EndDate.Date.AddHours(23).AddMinutes(59).AddSeconds(59);
                }
                else if (queryModel.ReportName == "PrepaymentUsedReport")
                {
                    if (!_sessionContext.HasPermission(Permissions.PrepaymentUsedReportView)) throw new PawnshopApplicationException(notEnoughPermissionMessage);

                    reportResult.Load(StiNetCoreHelper.MapPath(this, "Reports/PrepaymentUsedReport.mrt"));
                    var model = JsonConvert.DeserializeObject<StatisticsReportQueryModel>(queryModel.ReportQuery);
                    reportResult["branchIds"] = string.Join(",", model.BranchIds);
                    reportResult["beginDate"] = model.BeginDate.Date;
                    reportResult["endDate"] = model.EndDate.Date.AddHours(23).AddMinutes(59).AddSeconds(59);
                }
                else if (queryModel.ReportName == "AccountMfoReport")
                {
                    if (!_sessionContext.HasPermission(Permissions.AccountMfoReportView)) throw new PawnshopApplicationException(notEnoughPermissionMessage);

                    reportResult.Load(StiNetCoreHelper.MapPath(this, "Reports/AccountMfoReport.mrt"));
                    var model = JsonConvert.DeserializeObject<AccountMfoReportQueryModel>(queryModel.ReportQuery);
                    reportResult["beginDate"] = model.BeginDate.Date;
                    reportResult["endDate"] = model.EndDate.Date.AddHours(23).AddMinutes(59).AddSeconds(59);
                    reportResult["poolNumber"] = model.PoolNumber;
                }
                else if (queryModel.ReportName == "SoftCollectionReport")
                {
                    if (!_sessionContext.HasPermission(Permissions.SoftCollectionReportView)) throw new PawnshopApplicationException(notEnoughPermissionMessage);

                    reportResult.Load(StiNetCoreHelper.MapPath(this, "Reports/SoftCollectionReport.mrt"));
                    var model = JsonConvert.DeserializeObject<SoftCollectionReportQueryModel>(queryModel.ReportQuery);
                    reportResult["beginDate"] = model.BeginDate.Date;
                    reportResult["endDate"] = model.EndDate.Date.AddHours(23).AddMinutes(59).AddSeconds(59);
                    reportResult["branchIds"] = string.Join(",", model.BranchIds);
                    reportResult["collateralType"] = model.CollateralType;
                }
                else if (queryModel.ReportName == "ConsolidateSoftCollectionReport")
                {
                    if (!_sessionContext.HasPermission(Permissions.SoftCollectionReportView)) throw new PawnshopApplicationException(notEnoughPermissionMessage);

                    reportResult.Load(StiNetCoreHelper.MapPath(this, "Reports/ConsolidateSoftCollectionReport.mrt"));
                    var model = JsonConvert.DeserializeObject<SoftCollectionReportQueryModel>(queryModel.ReportQuery);
                    reportResult["beginDate"] = model.BeginDate.Date;
                    reportResult["endDate"] = model.EndDate.Date.AddHours(23).AddMinutes(59).AddSeconds(59);
                    reportResult["branchIds"] = string.Join(",", model.BranchIds);
                    reportResult["collateralType"] = model.CollateralType;
                }
                else if (queryModel.ReportName == "ClientExpensesReport")
                {
                    if (!_sessionContext.HasPermission(Permissions.СlientExpensesReportView)) throw new PawnshopApplicationException(notEnoughPermissionMessage);

                    reportResult.Load(StiNetCoreHelper.MapPath(this, "Reports/ClientExpensesReport.mrt"));
                    var model = JsonConvert.DeserializeObject<ClientExpensesReportQueryModel>(queryModel.ReportQuery);
                    reportResult["branchIds"] = string.Join(",", model.BranchIds);
                    reportResult["expenseType"] = model.ExpenseType;
                    reportResult["beginDate"] = model.BeginDate.Date;
                    reportResult["endDate"] = model.EndDate.Date.AddHours(23).AddMinutes(59).AddSeconds(59);
                }
                else if (queryModel.ReportName == "AccountMonitoringReport")
                {
                    if (!_sessionContext.HasPermission(Permissions.AccountMonitoringReportView)) throw new PawnshopApplicationException(notEnoughPermissionMessage);

                    reportResult.Load(StiNetCoreHelper.MapPath(this, "Reports/AccountMonitoringReport.mrt"));
                    var model = JsonConvert.DeserializeObject<AccountMonitoringReportQueryModel>(queryModel.ReportQuery);
                    reportResult["branchIds"] = string.Join(",", model.BranchIds);
                    reportResult["beginDate"] = model.BeginDate.Date;
                    reportResult["endDate"] = model.EndDate.Date.AddHours(23).AddMinutes(59).AddSeconds(59);
                    reportResult["accountPlanId"] = model.AccountPlanId;
                    reportResult["accountType"] = model.AccountType;
                }
                else if (queryModel.ReportName == "ConsolidateIssuanceReport")
                {
                    if (!_sessionContext.HasPermission(Permissions.ConsolidateIssuanceReportView)) throw new PawnshopApplicationException(notEnoughPermissionMessage);

                    var model = JsonConvert.DeserializeObject<ConsolidateIssuanceQueryModel>(queryModel.ReportQuery);
                    reportResult.Load(StiNetCoreHelper.MapPath(this, "Reports/ConsolidateIssuanceReport.mrt"));
                    reportResult["branchIds"] = string.Join(",", model.BranchIds);
                    reportResult["beginDate"] = model.BeginDate.Date;
                    reportResult["endDate"] = model.EndDate.Date.AddHours(23).AddMinutes(59).AddSeconds(59);
                }
                else if (queryModel.ReportName == "SoftCollectionKPIReport")
                {
                    if (!_sessionContext.HasPermission(Permissions.SoftCollectionReportView)) throw new PawnshopApplicationException(notEnoughPermissionMessage);

                    var model = JsonConvert.DeserializeObject<SoftCollectionKPIReportQueryModel>(queryModel.ReportQuery);
                    reportResult.Load(StiNetCoreHelper.MapPath(this, "Reports/SoftCollectionKPIReport.mrt"));
                    reportResult["branchIds"] = string.Join(",", model.BranchIds);
                    reportResult["months"] = model.Month;
                    reportResult["year"] = model.Year;
                    reportResult["fromBegining"] = model.FromBegining;
                }
                else if (queryModel.ReportName == "SoftCollectionKPIEightTempReport")
                {
                    if (!_sessionContext.HasPermission(Permissions.SoftCollectionReportView)) throw new PawnshopApplicationException("У вас недостаточно прав для просмотра отчета, обратитесь к администратору");

                    var model = JsonConvert.DeserializeObject<SoftCollectionKPIReportQueryModel>(queryModel.ReportQuery);
                    reportResult.Load(StiNetCoreHelper.MapPath(this, "Reports/SoftCollectionKPIEightTempReport.mrt"));
                    reportResult["branchIds"] = string.Join(",", model.BranchIds);
                    reportResult["months"] = model.Month;
                    reportResult["year"] = model.Year;
                    reportResult["fromBegining"] = model.FromBegining;
                }
                else if (queryModel.ReportName == "SoftCollectionKPIFifteenTempReport")
                {
                    if (!_sessionContext.HasPermission(Permissions.SoftCollectionReportView)) throw new PawnshopApplicationException("У вас недостаточно прав для просмотра отчета, обратитесь к администратору");

                    var model = JsonConvert.DeserializeObject<SoftCollectionKPIReportQueryModel>(queryModel.ReportQuery);
                    reportResult.Load(StiNetCoreHelper.MapPath(this, "Reports/SoftCollectionKPIFifteenTempReport.mrt"));
                    reportResult["branchIds"] = string.Join(",", model.BranchIds);
                    reportResult["months"] = model.Month;
                    reportResult["year"] = model.Year;
                    reportResult["fromBegining"] = model.FromBegining;
                }
                else if (queryModel.ReportName == "AnalysisIssuanceReport")
                {
                    if (!_sessionContext.HasPermission(Permissions.AnalysisIssuanceReportView)) throw new PawnshopApplicationException(notEnoughPermissionMessage);

                    var model = JsonConvert.DeserializeObject<AnalysisIssuanceReportQueryModel>(queryModel.ReportQuery);
                    reportResult.Load(StiNetCoreHelper.MapPath(this, "Reports/AnalysisIssuanceReport.mrt"));
                    reportResult["branchIds"] = string.Join(",", model.BranchIds);
                    reportResult["beginDate"] = model.BeginDate.Date;
                    reportResult["endDate"] = model.EndDate.Date.AddHours(23).AddMinutes(59).AddSeconds(59);
                }
                else if (queryModel.ReportName == "AttractionChannelReport")
                {
                    if (!_sessionContext.HasPermission(Permissions.AttractionChannelReportView)) throw new PawnshopApplicationException(notEnoughPermissionMessage);

                    var model = JsonConvert.DeserializeObject<AttractionChannelReportQueryModel>(queryModel.ReportQuery);
                    reportResult.Load(StiNetCoreHelper.MapPath(this, "Reports/AttractionChannelReport.mrt"));
                    reportResult["branchIds"] = string.Join(",", model.BranchIds);
                    reportResult["beginDate"] = model.BeginDate.Date;
                    reportResult["endDate"] = model.EndDate.Date.AddHours(23).AddMinutes(59).AddSeconds(59);
                    reportResult["collateralType"] = model.CollateralType;
                }
                else if (queryModel.ReportName == "EmloyeeContractReport")
                {
                    if (!_sessionContext.HasPermission(Permissions.EmloyeeContractReportView)) throw new PawnshopApplicationException(notEnoughPermissionMessage);

                    reportResult.Load(StiNetCoreHelper.MapPath(this, "Reports/EmloyeeContractReport.mrt"));
                }
                else if (queryModel.ReportName == "CarParkingStatusReport")
                {
                    if (!_sessionContext.HasPermission(Permissions.CarParkingStatusReportView)) throw new PawnshopApplicationException(notEnoughPermissionMessage);

                    var model = JsonConvert.DeserializeObject<CarParkingStatusReportQueryModel>(queryModel.ReportQuery);
                    reportResult.Load(StiNetCoreHelper.MapPath(this, "Reports/CarParkingStatusReport.mrt"));
                    reportResult["branchIds"] = string.Join(",", model.BranchIds);
                    reportResult["beginDate"] = model.BeginDate.Date;
                }
                else if (queryModel.ReportName == "CarParkingStatusReportForCARTAS")
                {
                    if (!_sessionContext.HasPermission(Permissions.CarParkingStatusReportForCARTASView)) throw new PawnshopApplicationException(notEnoughPermissionMessage);

                    var model = JsonConvert.DeserializeObject<CarParkingStatusReportForCARTASQueryModel>(queryModel.ReportQuery);
                    reportResult.Load(StiNetCoreHelper.MapPath(this, "Reports/CarParkingStatusReportForCARTAS.mrt"));
                    reportResult["branchIds"] = string.Join(",", model.BranchIds);
                    reportResult["beginDate"] = model.BeginDate.Date;
                }
                else if (queryModel.ReportName == "UserPermissionsReport")
                {
                    if (!_sessionContext.HasPermission(Permissions.UserPermissionsReportView)) throw new PawnshopApplicationException(notEnoughPermissionMessage);

                    var model = JsonConvert.DeserializeObject<UserPermissionsReportQueryModel>(queryModel.ReportQuery);
                    reportResult.Load(StiNetCoreHelper.MapPath(this, "Reports/UserPermissionsReport.mrt"));

                    reportResult["userIds"] = string.Join(",", model.UserIds);
                    reportResult["roleIds"] = string.Join(",", model.RoleIds);

                    var members = _memberRepository.RolesAndUsers(model.UserIds.ToArray(), model.RoleIds.ToArray());
                    members.ForEach(x =>
                    {
                        x.PermissionDisplayName =
                            Permissions.All?.FirstOrDefault(p => p.Name == x.PermissionName)?.DisplayName ?? "NOT FOUND";
                    });
                    reportResult.RegBusinessObject("Users", members);
                }
                else if (queryModel.ReportName == "PosTerminalBookReport")
                {
                    if (!_sessionContext.HasPermission(Permissions.PosTerminalBookReportView)) throw new PawnshopApplicationException(notEnoughPermissionMessage);

                    var model = JsonConvert.DeserializeObject<PosTerminalBookReportQueryModel>(queryModel.ReportQuery);
                    reportResult.Load(StiNetCoreHelper.MapPath(this, "Reports/PosTerminalBookReport.mrt"));
                    reportResult["branchId"] = model.BranchId;
                    reportResult["beginDate"] = model.BeginDate.Date;
                    reportResult["endDate"] = model.EndDate.Date.AddHours(23).AddMinutes(59).AddSeconds(59);
                    reportResult["chiefAccountantName"] = _branchContext.Configuration.LegalSettings.ChiefAccountantName;
                    reportResult["cashierName"] = _branchContext.Configuration.LegalSettings.CashierName;
                }
                else if (queryModel.ReportName == "CashInTransitReport")
                {
                    if (!_sessionContext.HasPermission(Permissions.CashInTransitReportView)) throw new PawnshopApplicationException(notEnoughPermissionMessage);

                    var model = JsonConvert.DeserializeObject<CashInTransitReportQueryModel>(queryModel.ReportQuery);
                    reportResult.Load(StiNetCoreHelper.MapPath(this, "Reports/CashInTransitReport.mrt"));
                    reportResult["branchId"] = model.BranchId;
                    reportResult["beginDate"] = model.BeginDate.Date;
                    reportResult["endDate"] = model.EndDate.Date.AddHours(23).AddMinutes(59).AddSeconds(59);
                    reportResult["orderType"] = model.OrderType;
                }
                else if (queryModel.ReportName == "WithoutDriveLicenseReport")
                {
                    if (!_sessionContext.HasPermission(Permissions.WithoutDriveLicenseReportView)) throw new PawnshopApplicationException(notEnoughPermissionMessage);

                    var model = JsonConvert.DeserializeObject<WithoutDriveLicenseReportQueryModel>(queryModel.ReportQuery);
                    reportResult.Load(StiNetCoreHelper.MapPath(this, "Reports/WithoutDriveLicenseReport.mrt"));
                    reportResult["branchIds"] = string.Join(",", model.BranchIds);
                    reportResult["beginDate"] = model.BeginDate.Date;
                    reportResult["endDate"] = model.EndDate.Date.AddHours(23).AddMinutes(59).AddSeconds(59);
                }
                else if (queryModel.ReportName == "BuyoutContractsWithInscriptionReport")
                {
                    if (!_sessionContext.HasPermission(Permissions.BuyoutContractsWithInscriptionReportView)) throw new PawnshopApplicationException(notEnoughPermissionMessage);

                    var model = JsonConvert.DeserializeObject<BuyoutContractsWithInscriptionReportQueryModel>(queryModel.ReportQuery);
                    reportResult.Load(StiNetCoreHelper.MapPath(this, "Reports/BuyoutContractsWithInscriptionReport.mrt"));
                    reportResult["branchIds"] = string.Join(",", model.BranchIds);
                    reportResult["beginDate"] = model.BeginDate.Date;
                    reportResult["endDate"] = model.EndDate.Date.AddHours(23).AddMinutes(59).AddSeconds(59);
                }
                else if (queryModel.ReportName == "PhotoReport")
                {
                    if (!_sessionContext.HasPermission(Permissions.PhotoReportView)) throw new PawnshopApplicationException(notEnoughPermissionMessage);

                    var model = JsonConvert.DeserializeObject<PhotoReportQueryModel>(queryModel.ReportQuery);
                    reportResult.Load(StiNetCoreHelper.MapPath(this, "Reports/PhotoReport.mrt"));
                    reportResult["branchIds"] = string.Join(",", model.BranchIds);
                    reportResult["beginDate"] = model.BeginDate.Date;
                    reportResult["endDate"] = model.EndDate.Date.AddHours(23).AddMinutes(59).AddSeconds(59);
                }
                else if (queryModel.ReportName == "OnlinePaymentManageReport")
                {
                    if (!_sessionContext.HasPermission(Permissions.OnlinePaymentManageReportView)) throw new PawnshopApplicationException(notEnoughPermissionMessage);

                    var model = JsonConvert.DeserializeObject<OnlinePaymentManageReportQueryModel>(queryModel.ReportQuery);
                    reportResult.Load(StiNetCoreHelper.MapPath(this, "Reports/OnlinePaymentManageReport.mrt"));
                    reportResult["beginDate"] = model.BeginDate.Date;
                    reportResult["endDate"] = model.EndDate.Date.AddHours(23).AddMinutes(59).AddSeconds(59);
                    reportResult["status"] = model.Status;
                    reportResult["processingType"] = model.ProcessingType;
                }
                else if (queryModel.ReportName == "SMSSenderReport")
                {
                    if (!_sessionContext.HasPermission(Permissions.SMSSenderReportView)) throw new PawnshopApplicationException(notEnoughPermissionMessage);

                    var model = JsonConvert.DeserializeObject<SMSSenderReportQueryModel>(queryModel.ReportQuery);
                    reportResult.Load(StiNetCoreHelper.MapPath(this, "Reports/SMSSenderReport.mrt"));
                    reportResult["branchIds"] = string.Join(",", model.BranchIds);
                    reportResult["beginDate"] = model.BeginDate.Date;
                    reportResult["endDate"] = model.EndDate.Date.AddHours(23).AddMinutes(59).AddSeconds(59);
                }
                else if (queryModel.ReportName == "PaymentsMoreThanTenReport")
                {
                    if (!_sessionContext.HasPermission(Permissions.PaymentsMoreThanTenReportView)) throw new PawnshopApplicationException(notEnoughPermissionMessage);

                    var model = JsonConvert.DeserializeObject<PaymentsMoreThanTenReportQueryModel>(queryModel.ReportQuery);
                    reportResult.Load(StiNetCoreHelper.MapPath(this, "Reports/PaymentsMoreThanTenReport.mrt"));
                    reportResult["branchIds"] = string.Join(",", model.BranchIds);
                    reportResult["beginDate"] = model.BeginDate.Date;
                    reportResult["endDate"] = model.EndDate.Date.AddHours(23).AddMinutes(59).AddSeconds(59);
                }
                else if (queryModel.ReportName == "AuditOnPledgesReport")
                {
                    if (!_sessionContext.HasPermission(Permissions.AuditOnPledgesReportView)) throw new PawnshopApplicationException(notEnoughPermissionMessage);

                    reportResult.Load(StiNetCoreHelper.MapPath(this, "Reports/AuditOnPledgesReport.mrt"));
                    var model = JsonConvert.DeserializeObject<AuditOnPledgesReportQueryModel>(queryModel.ReportQuery);
                    reportResult["branchIds"] = string.Join(",", model.BranchIds);
                    reportResult["beginDate"] = model.BeginDate.Date.AddHours(23).AddMinutes(59).AddSeconds(59);
                }
                else if (queryModel.ReportName == "DelayNewHCReport")
                {
                    if (!_sessionContext.HasPermission(Permissions.DelayReportView)) throw new PawnshopApplicationException(notEnoughPermissionMessage);

                    var model = JsonConvert.DeserializeObject<DelayReportQueryModel>(queryModel.ReportQuery);
                    reportResult.Load(StiNetCoreHelper.MapPath(this, "Reports/DelayNewHCReport.mrt"));
                    reportResult["beginDelayCount"] = model.BeginDelayCount;
                    reportResult["endDelayCount"] = model.EndDelayCount;
                    reportResult["collateralType"] = model.CollateralType;
                    reportResult["branchIds"] = string.Join(",", model.BranchIds);
                    reportResult["inscriptionVisible"] =
                        _sessionContext.Permissions.Any(x => x.Contains(Permissions.InscriptionManage));
                }
                else if (queryModel.ReportName == "DelayNewInWorkHCReport")
                {
                    if (!_sessionContext.HasPermission(Permissions.DelayReportView)) throw new PawnshopApplicationException(notEnoughPermissionMessage);

                    var model = JsonConvert.DeserializeObject<DelayReportQueryModel>(queryModel.ReportQuery);
                    reportResult.Load(StiNetCoreHelper.MapPath(this, "Reports/DelayNewInWorkHCReport.mrt"));
                    reportResult["beginDelayCount"] = model.BeginDelayCount;
                    reportResult["endDelayCount"] = model.EndDelayCount;
                    reportResult["collateralType"] = model.CollateralType;
                    reportResult["branchIds"] = string.Join(",", model.BranchIds);
                    reportResult["inscriptionVisible"] =
                        _sessionContext.Permissions.Any(x => x.Contains(Permissions.InscriptionManage));
                }
                else if (queryModel.ReportName == "ContractDiscountReport")
                {
                    if (!_sessionContext.HasPermission(Permissions.ContractDiscountReportView)) throw new PawnshopApplicationException(notEnoughPermissionMessage);

                    var model = JsonConvert.DeserializeObject<ContractDiscountReportQueryModel>(queryModel.ReportQuery);
                    reportResult.Load(StiNetCoreHelper.MapPath(this, "Reports/ContractDiscountReport.mrt"));
                    reportResult["beginDate"] = model.BeginDate.Date;
                    reportResult["endDate"] = model.EndDate.Date.AddHours(23).AddMinutes(59).AddSeconds(59);
                }
                else if (queryModel.ReportName == "ArrestedAndDeadReport")
                {
                    if (!_sessionContext.HasPermission(Permissions.ArrestedAndDeadReportView)) throw new PawnshopApplicationException(notEnoughPermissionMessage);

                    var model = JsonConvert.DeserializeObject<ArrestedAndDeadReportQueryModel>(queryModel.ReportQuery);
                    reportResult.Load(StiNetCoreHelper.MapPath(this, "Reports/ArrestedAndDeadReport.mrt"));
                    reportResult["endDelayCount"] = model.EndDelayCount;
                    reportResult["collateralType"] = model.CollateralType;
                    reportResult["branchIds"] = string.Join(",", model.BranchIds);
                }
                else if (queryModel.ReportName == "TasOnlineReport")
                {
                    if (!_sessionContext.HasPermission(Permissions.TasOnlineReportView)) throw new PawnshopApplicationException(notEnoughPermissionMessage);

                    var model = JsonConvert.DeserializeObject<TasOnlineReportQueryModel>(queryModel.ReportQuery);
                    reportResult.Load(StiNetCoreHelper.MapPath(this, "Reports/TasOnlineReport.mrt"));
                    reportResult["beginDate"] = model.BeginDate.Date;
                    reportResult["endDate"] = model.EndDate.Date.AddHours(23).AddMinutes(59).AddSeconds(59);
                }
                else if (queryModel.ReportName == "InsurancePolicyReport")
                {
                    if (!_sessionContext.HasPermission(Permissions.InsurancePolicyReportView)) throw new PawnshopApplicationException(notEnoughPermissionMessage);

                    var model = JsonConvert.DeserializeObject<InsurancePolicyReportQueryModel>(queryModel.ReportQuery);
                    reportResult.Load(StiNetCoreHelper.MapPath(this, "Reports/InsurancePolicyReport.mrt"));
                    reportResult["branchIds"] = string.Join(",", model.BranchIds);
                    reportResult["beginDate"] = model.BeginDate.Date;
                    reportResult["endDate"] = model.EndDate.Date.AddHours(23).AddMinutes(59).AddSeconds(59);
                }
                else if (queryModel.ReportName == "InsurancePolicyActReport")
                {
                    if (!_sessionContext.HasPermission(Permissions.InsurancePolicyActReportView)) throw new PawnshopApplicationException(notEnoughPermissionMessage);

                    var model = JsonConvert.DeserializeObject<InsurancePolicyActReportQueryModel>(queryModel.ReportQuery);
                    reportResult.Load(StiNetCoreHelper.MapPath(this, "Reports/InsurancePolicyActReport.mrt"));
                    reportResult["beginDate"] = model.BeginDate.Date;
                    reportResult["endDate"] = model.EndDate.Date.AddHours(23).AddMinutes(59).AddSeconds(59);
                }
                else if (queryModel.ReportName == "PrepaymentMonitoringReport")
                {
                    if (!_sessionContext.HasPermission(Permissions.PrepaymentMonitoringReportView)) throw new PawnshopApplicationException(notEnoughPermissionMessage);

                    var model = JsonConvert.DeserializeObject<PrepaymentMonitoringReportQueryModel>(queryModel.ReportQuery);
                    reportResult.Load(StiNetCoreHelper.MapPath(this, "Reports/PrepaymentMonitoringReport.mrt"));
                    reportResult["beginDate"] = model.BeginDate.Date;
                    reportResult["endDate"] = model.EndDate.Date.AddHours(23).AddMinutes(59).AddSeconds(59);
                    reportResult["branchIds"] = string.Join(",", model.BranchIds);
                }
                else if (queryModel.ReportName == "DelayReportForSoftCollection")
                {
                    if (!_sessionContext.HasPermission(Permissions.DelayReportForSoftCollectionView)) throw new PawnshopApplicationException(notEnoughPermissionMessage);

                    var model = JsonConvert.DeserializeObject<DelayReportForSoftCollectionQueryModel>(queryModel.ReportQuery);
                    reportResult.Load(StiNetCoreHelper.MapPath(this, "Reports/DelayReportForSoftCollection.mrt"));
                    reportResult["beginDelayCount"] = model.BeginDelayCount;
                    reportResult["endDelayCount"] = model.EndDelayCount;
                    reportResult["collateralType"] = model.CollateralType;
                    reportResult["branchIds"] = string.Join(",", model.BranchIds);
                    reportResult["inscriptionVisible"] =
                        _sessionContext.Permissions.Any(x => x.Contains(Permissions.DelayReportForSoftCollectionView));
                }
                else if (queryModel.ReportName == "DelayReportForSoftCollection3dParty")
                {
                    if (!_sessionContext.HasPermission(Permissions.DelayReportForSoftCollectionView)) throw new PawnshopApplicationException(notEnoughPermissionMessage);

                    var model = JsonConvert.DeserializeObject<DelayReportForSoftCollectionQueryModel>(queryModel.ReportQuery);
                    reportResult.Load(StiNetCoreHelper.MapPath(this, "Reports/DelayReportForSoftCollection3dParty.mrt"));
                    reportResult["beginDelayCount"] = model.BeginDelayCount;
                    reportResult["endDelayCount"] = model.EndDelayCount;
                    reportResult["collateralType"] = model.CollateralType;
                    reportResult["branchIds"] = string.Join(",", model.BranchIds);
                    reportResult["inscriptionVisible"] =
                        _sessionContext.Permissions.Any(x => x.Contains(Permissions.DelayReportForSoftCollectionView));
                }
                else if (queryModel.ReportName == "BuyoutContractsReport")
                {
                    if (!_sessionContext.HasPermission(Permissions.BuyoutContractsReportView)) throw new PawnshopApplicationException(notEnoughPermissionMessage);

                    //TODO сделать свой QueryModel
                    var model = JsonConvert.DeserializeObject<BuyoutContractsReportQueryModel>(queryModel.ReportQuery);
                    reportResult.Load(StiNetCoreHelper.MapPath(this, "Reports/BuyoutContractsReport.mrt"));
                    reportResult["branchIds"] = string.Join(",", model.BranchIds);
                    reportResult["beginDate"] = model.BeginDate.Date;
                    reportResult["endDate"] = model.EndDate.Date.AddHours(23).AddMinutes(59).AddSeconds(59);
                }
                else if (queryModel.ReportName == "BuyoutGprReport")
                {
                    if (!_sessionContext.HasPermission(Permissions.BuyoutGprReportView)) throw new PawnshopApplicationException(notEnoughPermissionMessage);

                    reportResult.Load(StiNetCoreHelper.MapPath(this, "Reports/BuyoutGprReport.mrt"));
                }
                else if (queryModel.ReportName == "RefinanceGprReport")
                {
                    if (!_sessionContext.HasPermission(Permissions.RefinanceGrpReportView)) throw new PawnshopApplicationException(notEnoughPermissionMessage);

                    reportResult.Load(StiNetCoreHelper.MapPath(this, "Reports/RefinanceGprReport.mrt"));
                }
                else if (queryModel.ReportName == "KdnFailureStatisticsReport")
                {
                    if (!_sessionContext.HasPermission(Permissions.KdnFailureStatisticsReportView)) throw new PawnshopApplicationException(notEnoughPermissionMessage);

                    var model = JsonConvert.DeserializeObject<KdnFailureStatisticsReportQueryModel>(queryModel.ReportQuery);
                    reportResult.Load(StiNetCoreHelper.MapPath(this, "Reports/KdnFailureStatisticsReport.mrt"));
                    reportResult["beginDate"] = model.BeginDate.Date;
                    reportResult["endDate"] = model.EndDate.Date.AddHours(23).AddMinutes(59).AddSeconds(59);
                }
                else if (queryModel.ReportName == "CollateralReport")
                {
                    if (!_sessionContext.HasPermission(Permissions.CollateralReportView)) throw new PawnshopApplicationException(notEnoughPermissionMessage);

                    var model = JsonConvert.DeserializeObject<CollateralReportQueryModel>(queryModel.ReportQuery);
                    reportResult.Load(StiNetCoreHelper.MapPath(this, "Reports/CollateralReport.mrt"));
                    reportResult["branchIds"] = string.Join(",", model.BranchIds);
                    reportResult["ReportDate"] = model.ReportDate.Date;
                }
                else if (queryModel.ReportName == "VintageAnalysis")
                {
                    if (!_sessionContext.HasPermission(Permissions.VintageAnalysisView)) throw new PawnshopApplicationException(notEnoughPermissionMessage);

                    var model = JsonConvert.DeserializeObject<VintageAnalysisQueryModel>(queryModel.ReportQuery);
                    reportResult.Load(StiNetCoreHelper.MapPath(this, "Reports/VintageAnalysis.mrt"));
                    reportResult["beginDate"] = model.BeginDate.Date;
                    reportResult["endDate"] = model.EndDate.Date.AddHours(23).AddMinutes(59).AddSeconds(59);
                    reportResult["contractStartDate"] = model.ContractStartDate.Date;
                    reportResult["contractEndDate"] = model.ContractEndDate.Date.AddHours(23).AddMinutes(59).AddSeconds(59);
                    reportResult["beginDelayCount"] = model.BeginDelayCount;
                    reportResult["endDelayCount"] = model.EndDelayCount;
                    reportResult["collateralType"] = model.CollateralType;
                    reportResult["branchIds"] = string.Join(",", model.BranchIds);
                }
                else if (queryModel.ReportName == "TmfReport")
                {
                    if (!_sessionContext.HasPermission(Permissions.TmfReportView)) throw new PawnshopApplicationException(notEnoughPermissionMessage);

                    var model = JsonConvert.DeserializeObject<TmfReportQueryModel>(queryModel.ReportQuery);
                    reportResult.Load(StiNetCoreHelper.MapPath(this, "Reports/TmfReport.mrt"));
                    reportResult["beginDate"] = model.BeginDate.Date;
                    reportResult["endDate"] = model.EndDate.Date.AddHours(23).AddMinutes(59).AddSeconds(59);
                }
                else if (queryModel.ReportName == "LoanPortfolioReport")
                {
                    if (!_sessionContext.HasPermission(Permissions.LoanPortfolioReportView)) throw new PawnshopApplicationException(notEnoughPermissionMessage);

                    var model = JsonConvert.DeserializeObject<LoanPortfolioReportQueryModel>(queryModel.ReportQuery);
                    reportResult.Load(StiNetCoreHelper.MapPath(this, "Reports/LoanPortfolioReport.mrt"));
                    reportResult["ReportDate"] = model.ReportDate.Date;
                }
                else if (queryModel.ReportName == "CurrentAccountDebtAndBalanceReport")
                {
                    if (!_sessionContext.HasPermission(Permissions.CurrentAccountDebtAndBalanceReportView)) throw new PawnshopApplicationException(notEnoughPermissionMessage);

                    var model = JsonConvert.DeserializeObject<CurrentAccountDebtAndBalanceReportQueryModel>(queryModel.ReportQuery);
                    reportResult.Load(StiNetCoreHelper.MapPath(this, "Reports/CurrentAccountDebtAndBalanceReport.mrt"));
                    reportResult["ReportDate"] = model.ReportDate.Date;
                }
                else if (queryModel.ReportName == "CallingCustomersWithCurrentPaymentDateReport")
                {
                    if (!_sessionContext.HasPermission(Permissions.CallingCustomersWithCurrentPaymentDateReportView)) throw new PawnshopApplicationException(notEnoughPermissionMessage);

                    var model = JsonConvert.DeserializeObject<CallingCustomersWithCurrentPaymentDateReportQueryModel>(queryModel.ReportQuery);
                    reportResult.Load(StiNetCoreHelper.MapPath(this, "Reports/CallingCustomersWithCurrentPaymentDateReport.mrt"));
                    reportResult["ReportDate"] = model.ReportDate.Date;
                }
                else if (queryModel.ReportName == "SecurityServiceReport")
                {
                    if (!_sessionContext.HasPermission(Permissions.SecurityServiceReportView)) throw new PawnshopApplicationException(notEnoughPermissionMessage);

                    var model = JsonConvert.DeserializeObject<SecurityServiceReportQueryModel>(queryModel.ReportQuery);
                    reportResult.Load(StiNetCoreHelper.MapPath(this, "Reports/SecurityServiceReport.mrt"));
                    reportResult["ReportDate"] = model.ReportDate.Date;
                    reportResult["UserId"] = _sessionContext.UserId;
                }
                else if (queryModel.ReportName == "WithdrawalFromPledgeReport")
                {
                    if (!_sessionContext.HasPermission(Permissions.WithdrawalFromPledgeReportView)) throw new PawnshopApplicationException(notEnoughPermissionMessage);

                    var model = JsonConvert.DeserializeObject<WithdrawalFromPledgeReportQueryModel>(queryModel.ReportQuery);
                    reportResult.Load(StiNetCoreHelper.MapPath(this, "Reports/WithdrawalFromPledgeReport.mrt"));
                    reportResult["beginDate"] = model.BeginDate.Date;
                    reportResult["endDate"] = model.EndDate.Date;
                }
                else if (queryModel.ReportName == "SecurityServiceEffectivenessReport")
                {
                    if (!_sessionContext.HasPermission(Permissions.SecurityServiceEffectivenessReportView)) throw new PawnshopApplicationException(notEnoughPermissionMessage);

                    var model = JsonConvert.DeserializeObject<SecurityServiceEffectivenessReportQueryModel>(queryModel.ReportQuery);
                    reportResult.Load(StiNetCoreHelper.MapPath(this, "Reports/SecurityServiceEffectivenessReport.mrt"));
                    reportResult["beginDate"] = model.BeginDate.Date;
                    reportResult["endDate"] = model.EndDate.Date;
                }
                else if (queryModel.ReportName == "CollectionReport")
                {
                    if (!_sessionContext.HasPermission(Permissions.CollectionReportView)) throw new PawnshopApplicationException(notEnoughPermissionMessage);

                    var model = JsonConvert.DeserializeObject<CollectionReportQueryModel>(queryModel.ReportQuery);
                    reportResult.Load(StiNetCoreHelper.MapPath(this, "Reports/CollectionReport.mrt"));
                    reportResult["branchIds"] = string.Join(",", model.BranchIds);
                    reportResult["overdueStatus"] = model.OverdueStatus;
                    reportResult["collateralType"] = model.CollateralType;
                }
                else if (queryModel.ReportName == "CustomerInteractionReport")
                {
                    if (!_sessionContext.HasPermission(Permissions.OnlineReportsView)) throw new PawnshopApplicationException(notEnoughPermissionMessage);

                    var model = JsonConvert.DeserializeObject<CustomerInteractionReportQueryModel>(queryModel.ReportQuery);
                    reportResult.Load(StiNetCoreHelper.MapPath(this, "Reports/CustomerInteractionReport.mrt"));
                    reportResult["beginDate"] = model.BeginDate.Date;
                    reportResult["endDate"] = model.EndDate.Date.AddHours(23).AddMinutes(59).AddSeconds(59);
                }
                else if (queryModel.ReportName == "ApplicationsReport")
                {
                    if (!_sessionContext.HasPermission(Permissions.OnlineReportsView)) throw new PawnshopApplicationException(notEnoughPermissionMessage);

                    var model = JsonConvert.DeserializeObject<ApplicationsReportQueryModel>(queryModel.ReportQuery);
                    reportResult.Load(StiNetCoreHelper.MapPath(this, "Reports/ApplicationsReport.mrt"));
                    reportResult["beginDate"] = model.BeginDate.Date;
                    reportResult["endDate"] = model.EndDate.Date.AddHours(23).AddMinutes(59).AddSeconds(59);
                }

                var sqlDatabase = (reportResult.Dictionary.Databases["ReportConnection"] as StiSqlDatabase) ?? ((StiSqlDatabase)reportResult.Dictionary.Databases[0]);
                sqlDatabase.ConnectionString = _options.ReportDatabaseConnectionString;
                reportResult.Render();

                reportLog.IsSuccessful = true;
                _reportLog.Log(reportLog);
            }
            catch (PawnshopApplicationException ex)
            {
                _reportLog.Log(reportLog);
                throw;
            }
            catch (Exception ex)
            {
                _reportLog.Log(reportLog);
                _eventLog.Log(EventCode.ReportDownload, EventStatus.Failed, null, null, JsonConvert.SerializeObject(queryModel), JsonConvert.SerializeObject(ex), null, _sessionContext.UserId);
                throw;
            }

            return reportResult.SaveDocumentJsonToString();
        }
    }
}