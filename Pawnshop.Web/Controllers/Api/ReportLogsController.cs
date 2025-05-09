using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Pawnshop.Core;
using Pawnshop.Core.Queries;
using Pawnshop.Data.Access;
using Pawnshop.Web.Models.List;
using Pawnshop.Data.Models.Reports;
using System.Linq;

namespace Pawnshop.Web.Controllers.Api
{
    [Authorize(Permissions.ReportLogsView)]
    public class ReportLogsController : Controller
    {
        public readonly ReportsRepository _reportsRepository;
        public readonly ReportLogsRepository _reportLogsRepository;

        public ReportLogsController(ReportsRepository reportsRepository,
            ReportLogsRepository reportLogsRepository)
        {
            _reportsRepository = reportsRepository;
            _reportLogsRepository = reportLogsRepository;
        }

        [HttpPost("/api/reportLogs/list")]
        public ListModel<ReportLogModel> List([FromBody] ListQueryModel<ReportLogsListQueryModel> listQuery)
        {
            if (listQuery == null)
                listQuery = new ListQueryModel<ReportLogsListQueryModel>();
            if (listQuery.Model == null)
                listQuery.Model = new ReportLogsListQueryModel();

            var reports = _reportsRepository.List(new ListQuery() { Page = null });
            var reportTypes = _reportsRepository.ListReportTypes(new ListQuery() { Page = null });

            var count = _reportLogsRepository.Count(listQuery, listQuery.Model);
                
            var list = _reportLogsRepository.List(listQuery, listQuery.Model).Select(log => {
                var report = reports.Find(report => report.Id == log.ReportId);
                var type = reportTypes.Find(type => type.Id == report.ReportTypeId);

                return new ReportLogModel()
                {
                    CreateDate = log.CreateDate.Date,
                    ReportDate = log.CreateDate,
                    Request = log.Request,
                    ReportName = type.ReportTypeName + ". " + report.ReportName,
                    AuthorName = "ID: " + log.AuthorId + ". " + log.AuthorName,
                    IsSuccessful = log.IsSuccessful,
                };
            }).ToList();

            return new ListModel<ReportLogModel>
            {
                List = list,
                Count = count
            };
        }
    }
}
