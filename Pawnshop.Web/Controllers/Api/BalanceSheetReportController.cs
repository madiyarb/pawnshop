using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using Pawnshop.Core;
using Pawnshop.Core.Exceptions;
using Pawnshop.Core.Queries;
using Pawnshop.Data.Access;
using Pawnshop.Data.Models.Audit;
using Pawnshop.Data.Models.Reports;
using Pawnshop.Services;
using Pawnshop.Services.Reports;

namespace Pawnshop.Web.Controllers.Api
{
    [Authorize(Permissions.BalanceSheetReportView)]
    public class BalanceSheetReportController : Controller
    {
        private readonly IBalanceSheetReportService _balanceSheetReportService;
        private readonly ReportLogsRepository _reportLogsRepository;
        private readonly ReportsRepository _reportsRepository;
        private readonly IEventLog _eventLog;
        private readonly ISessionContext _sessionContext;

        public BalanceSheetReportController(
            IBalanceSheetReportService balanceSheetReportService,
            ReportLogsRepository reportLogsRepository, 
            ReportsRepository reportsRepository, 
            IEventLog eventLog, 
            ISessionContext sessionContext)
        {
            _balanceSheetReportService = balanceSheetReportService;
            _reportLogsRepository = reportLogsRepository;
            _reportsRepository = reportsRepository;
            _eventLog = eventLog;
            _sessionContext = sessionContext;
        }

        /// <summary>Сформировать отчет "Оборотно-Сальдовая ведомость по счету"</summary>
        [HttpPost]
        public async Task<IActionResult> Generate([FromBody] ReportGenerateQuery query)
        {
            return Ok(new
            {
                list = await _balanceSheetReportService.GetBranches(query),
                total = await _balanceSheetReportService.GetTotals(query)
            });
        }

        [HttpPost]
        public async Task<IActionResult> GenerateClients([FromBody] ReportGenerateQuery query)
        {
            return Ok(await _balanceSheetReportService.GetClients(query));
        }

        [HttpPost]
        public async Task<IActionResult> GenerateContracts([FromBody] ReportGenerateQuery query)
        {
            return Ok(await _balanceSheetReportService.GetContracts(query));
        }

        [HttpPost]
        public async Task<IActionResult> Excel([FromBody] ReportGenerateQuery query)
        {
            var reportList = await _balanceSheetReportService.List(query);

            return GetExcelReport(reportList, query);
        }

        [HttpPost]
        public async Task<List<AccountSettings>> GetAccountSettings() => await Task.Run(() =>
            _balanceSheetReportService.GetAccountSettings());


        private FileContentResult GetExcelReport(List<BalanceSheetReport> reportList, ReportGenerateQuery query)
        {
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

            var report = _reportsRepository.Find(new { ReportCode = "RequestsToPkbReport" });
            var reportLog = new ReportLog()
            {
                AuthorId = _sessionContext.UserId,
                AuthorName = _sessionContext.UserName,
                ReportId = report.Id,
                Request = "reportList: " + JsonConvert.SerializeObject(reportList) + ", query: " + JsonConvert.SerializeObject(query),
                CreateDate = DateTime.Now,
                IsSuccessful = false,
            };

            try
            {
                using (var package = new ExcelPackage())
                {
                    var worksheet = package.Workbook.Worksheets.Add("Report");

                    CreateDefaultHeader(worksheet, query);
                    SetGroupRecordValues(worksheet, 6, reportList);

                    worksheet.Cells[worksheet.Dimension.Address].AutoFitColumns();

                    var branchList = reportList.Select(x => new { x.BranchId, x.BranchName })
                        .Distinct()
                        .OrderBy(x => x.BranchName);

                    // start position fill branch info
                    int branchLine = 7;

                    foreach (var branch in branchList)
                    {
                        var branchItems = reportList.Where(x => x.BranchId == branch.BranchId);

                        var collateralList = branchItems.Select(x => new { type = x.CollateralType, name = x.CollateralType.GetDisplayName() })
                            .Distinct()
                            .OrderBy(x => x.name);

                        // start position fill collateral info
                        var collateralLine = branchLine + 1;

                        foreach (var collateral in collateralList)
                        {
                            var collateralItems = branchItems.Where(x => x.CollateralType == collateral.type);

                            var clientList = collateralItems.Select(x => new { x.ClientName, x.ClientId, x.IdentityNumber })
                                .Distinct()
                                .OrderBy(x => x.ClientName);

                            // start position fill client info
                            int clientLine = collateralLine + 1;

                            foreach (var client in clientList)
                            {
                                var contracts = collateralItems.Where(x => x.ClientId == client.ClientId)
                                    .OrderBy(x => x.ClientName)
                                    .ToList();

                                // start position fill contract info
                                int contractLine = clientLine + 1;

                                contracts.ForEach(x =>
                                {
                                    worksheet.Cells[contractLine, 4].Value = x.ContractNumber;

                                    if (x.PlanIsActive)
                                        SetRecordValues(worksheet, contractLine, x.IncomingBalance, 0, x.DebitTurns, x.CreditTurns, x.OutgoingBalance, 0);
                                    else
                                        SetRecordValues(worksheet, contractLine, 0, x.IncomingBalance, x.DebitTurns, x.CreditTurns, 0, x.OutgoingBalance);

                                    contractLine++;
                                });

                                worksheet.Cells[clientLine, 3].Value = client.ClientName;
                                worksheet.Cells[clientLine, 3, clientLine, 4].Merge = true;
                                worksheet.Cells[clientLine, 5].Value = client.IdentityNumber;
                                SetGroupRecordValues(worksheet, clientLine, contracts);
                                SetCellsBackgroundColor(worksheet, clientLine, 3, clientLine, 11, "#DBDBDB");

                                clientLine += contracts.Count() + 1;
                            }

                            worksheet.Cells[collateralLine, 2].Value = collateral.name;
                            worksheet.Cells[collateralLine, 2, collateralLine, 5].Merge = true;
                            SetGroupRecordValues(worksheet, collateralLine, collateralItems);
                            SetCellsBackgroundColor(worksheet, collateralLine, 2, collateralLine, 11, "#DDEBF7");

                            collateralLine = clientLine;
                        }

                        worksheet.Cells[branchLine, 1].Value = branch.BranchName;
                        worksheet.Cells[branchLine, 1, branchLine, 5].Merge = true;
                        SetGroupRecordValues(worksheet, branchLine, branchItems);
                        SetCellsBackgroundColor(worksheet, branchLine, 1, branchLine, 11, "#B4C6E7");

                        branchLine = collateralLine;
                    }

                    worksheet.Cells[worksheet.Dimension.Address].AutoFitColumns();

                    var excelData = package.GetAsByteArray();
                    var contentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                    var fileName = "BalanceSheetReport.xlsx";

                    reportLog.IsSuccessful = true;
                    _reportLogsRepository.Log(reportLog);
                    return File(excelData, contentType, fileName);
                }
            }
            catch (PawnshopApplicationException ex)
            {
                _reportLogsRepository.Log(reportLog);
                throw;
            }
            catch (Exception ex)
            {
                _reportLogsRepository.Log(reportLog);
                _eventLog.Log(EventCode.ReportDownload, EventStatus.Failed, null, null, "reportList: " + JsonConvert.SerializeObject(reportList) + ", query: " + JsonConvert.SerializeObject(query), JsonConvert.SerializeObject(ex), null, _sessionContext.UserId);
                throw;
            }
        }

        private void CreateDefaultHeader(ExcelWorksheet worksheet, ReportGenerateQuery query)
        {
            worksheet.Cells[1, 1].Value = "Оборотно-сальдовая ведомость по счету c " + query.BeginDate.ToShortDateString() + " по " + query.EndDate.ToShortDateString();
            worksheet.Cells[1, 1, 1, 11].Merge = true;
            worksheet.Cells[1, 1, 1, 11].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

            worksheet.Cells[2, 1].Value = "Структурное подразделение";
            worksheet.Cells[2, 1, 2, 5].Merge = true;
            worksheet.Cells[2, 6].Value = "Сальдо на начало периода";
            worksheet.Cells[2, 6, 2, 7].Merge = true;
            worksheet.Cells[2, 8].Value = "Обороты за период";
            worksheet.Cells[2, 8, 2, 9].Merge = true;
            worksheet.Cells[2, 10].Value = "Сальдо на конец периода";
            worksheet.Cells[2, 10, 2, 11].Merge = true;

            worksheet.Cells[3, 2].Value = "Тип залога";
            worksheet.Cells[3, 2, 3, 5].Merge = true;

            worksheet.Cells[3, 6].Value = "Дебет";
            worksheet.Cells[3, 6, 5, 6].Merge = true;
            worksheet.Cells[3, 7].Value = "Кредит";
            worksheet.Cells[3, 7, 5, 7].Merge = true;
            worksheet.Cells[3, 8].Value = "Дебет";
            worksheet.Cells[3, 8, 5, 8].Merge = true;
            worksheet.Cells[3, 9].Value = "Кредит";
            worksheet.Cells[3, 9, 5, 9].Merge = true;
            worksheet.Cells[3, 10].Value = "Дебет";
            worksheet.Cells[3, 10, 5, 10].Merge = true;
            worksheet.Cells[3, 11].Value = "Кредит";
            worksheet.Cells[3, 11, 5, 11].Merge = true;

            worksheet.Cells[4, 3].Value = "Контрагенты";
            worksheet.Cells[4, 3, 4, 4].Merge = true;
            worksheet.Cells[4, 5].Value = "БИН/ИИН";

            worksheet.Cells[5, 4].Value = "Договоры";
            worksheet.Cells[5, 4, 5, 5].Merge = true;

            worksheet.Cells[6, 1].Value = "Итого";
            worksheet.Cells[6, 1, 6, 5].Merge = true;
            SetCellsBackgroundColor(worksheet, 6, 1, 6, 11, "#8EA9DB");
        }

        private void SetRecordValues(ExcelWorksheet worksheet, int line, decimal StartDebit, decimal StartCredit,
            decimal TurnoverDebit, decimal TurnoverCredit, decimal EndDebit, decimal EndCredit)
        {
            worksheet.Cells[line, 6, line, 11].Style.Numberformat.Format = "_(* #,##0.00_);_(* (#,##0.00);_(* \"-\"??_);_(@_)";
            worksheet.Cells[line, 6].Value = StartDebit;
            worksheet.Cells[line, 7].Value = StartCredit;
            worksheet.Cells[line, 8].Value = TurnoverDebit;
            worksheet.Cells[line, 9].Value = TurnoverCredit;
            worksheet.Cells[line, 10].Value = EndDebit;
            worksheet.Cells[line, 11].Value = EndCredit;
        }

        private void SetGroupRecordValues(ExcelWorksheet worksheet, int line, IEnumerable<BalanceSheetReport> reportList)
        {
            SetRecordValues(worksheet,
                line,
                reportList.Sum(x => x.PlanIsActive ? x.IncomingBalance : 0),
                reportList.Sum(x => x.PlanIsActive ? 0 : x.IncomingBalance),
                reportList.Sum(x => x.DebitTurns),
                reportList.Sum(x => x.CreditTurns),
                reportList.Sum(x => x.PlanIsActive ? x.OutgoingBalance : 0),
                reportList.Sum(x => x.PlanIsActive ? 0 : x.OutgoingBalance)
                );
        }

        private void SetCellsBackgroundColor(ExcelWorksheet worksheet, int fromRow, int fromColumn, int toRow, int toColumn, string hexColor)
        {
            worksheet.Cells[fromRow, fromColumn, toRow, toColumn].Style.Fill.PatternType = ExcelFillStyle.Solid;
            worksheet.Cells[fromRow, fromColumn, toRow, toColumn]
                .Style.Fill.BackgroundColor.SetColor(ColorTranslator.FromHtml(hexColor));
        }
    }
}