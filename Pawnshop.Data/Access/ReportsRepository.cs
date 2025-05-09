using Dapper;
using Pawnshop.Core;
using Pawnshop.Core.Impl;
using Pawnshop.Core.Queries;
using System.Collections.Generic;
using System;
using System.Linq;
using Pawnshop.Data.Models.Reports;

namespace Pawnshop.Data.Access
{
    public class ReportsRepository : RepositoryBase
    {
        public ReportsRepository(IUnitOfWork unitOfWork) : base(unitOfWork)
        {

        }

        public Report Get(int id)
        {
            var report = UnitOfWork.Session.QuerySingleOrDefault<Report>(@"
                SELECT *
                FROM Reports
                WHERE DeleteDate IS NULL AND Id = @id", new { id }, UnitOfWork.Transaction);

            return report;
        }

        public Report Find(object query)
        {
            var reportName = query?.Val<string>("ReportName");
            var reportCode = query?.Val<string>("ReportCode");
            var reportTypeId = query?.Val<int>("ReportTypeId");
            var condition = "WHERE DeleteDate IS NULL";

            condition += (reportName != null && reportName.Length > 0) ? " AND ReportName = @ReportName" : string.Empty;
            condition += (reportCode != null && reportCode.Length > 0) ? " AND ReportCode = @ReportCode" : string.Empty;
            condition += (reportTypeId.HasValue && reportTypeId.Value > 0) ? " AND ReportTypeId = @ReportTypeId" : string.Empty;

            return UnitOfWork.Session.Query<Report>($@"
                SELECT *
                FROM Reports
                {condition}", new
                {
                    reportName,
                    reportCode,
                    reportTypeId
            }, UnitOfWork.Transaction).FirstOrDefault();
        }

        public List<Report> List(ListQuery listQuery)
        {
            if (listQuery == null)
                throw new ArgumentNullException(nameof(listQuery));

            var condition = listQuery.Like(string.Empty);
            var order = listQuery.Order(string.Empty, new Sort
            {
                Name = "Id",
                Direction = SortDirection.Desc
            });
            var page = listQuery.Page();

            return UnitOfWork.Session.Query<Report>($@"
                SELECT *
                FROM Reports
                {condition} {order} {page}",
                new
                {
                    listQuery.Page?.Offset,
                    listQuery.Page?.Limit,
                    listQuery.Filter
                }, UnitOfWork.Transaction).ToList();
        }

        public List<ReportType> ListReportTypes(ListQuery listQuery)
        {
            if (listQuery == null)
                throw new ArgumentNullException(nameof(listQuery));

            var condition = listQuery.Like(string.Empty);
            var order = listQuery.Order(string.Empty, new Sort
            {
                Name = "Id",
                Direction = SortDirection.Desc
            });
            var page = listQuery.Page();

            return UnitOfWork.Session.Query<ReportType>($@"
                SELECT *
                FROM ReportTypes
                {condition} {order} {page}",
                new
                {
                    listQuery.Page?.Offset,
                    listQuery.Page?.Limit,
                    listQuery.Filter
                }, UnitOfWork.Transaction).ToList();
        }
    }
}
