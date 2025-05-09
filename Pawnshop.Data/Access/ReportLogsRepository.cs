using Dapper;
using Pawnshop.Core;
using Pawnshop.Core.Exceptions;
using Pawnshop.Core.Impl;
using Pawnshop.Core.Queries;
using Pawnshop.Data.Models.Reports;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Pawnshop.Data.Access
{
    public class ReportLogsRepository : RepositoryBase
    {
        public ReportLogsRepository(IUnitOfWork unitOfWork) : base(unitOfWork)
        {

        }

        public void Log(ReportLog entity)
        {
            if (entity == null)
                throw new ArgumentException(nameof(entity));

            using (var transaction = BeginTransaction())
            {
                var query = @"INSERT INTO ReportLogs (ReportId, AuthorId, AuthorName, Request, IsSuccessful, CreateDate) VALUES (@ReportId, @AuthorId, @AuthorName, @Request, @IsSuccessful, @CreateDate)";
                UnitOfWork.Session.QuerySingleOrDefault<int>(query, entity, UnitOfWork.Transaction);
                transaction.Commit();
            }
        }

        public ReportLog Get(int id)
        {
            var reportLog = UnitOfWork.Session.QuerySingleOrDefault<ReportLog>(@"
                SELECT *
                FROM ReportLogs
                WHERE Id = @id", new { id }, UnitOfWork.Transaction);

            return reportLog;
        }

        public List<ReportLog> List(ListQuery listQuery, object query = null)
        {
            if (listQuery == null)
                throw new ArgumentNullException(nameof(listQuery));

            var (condition, parameters) = GenerateCondition(listQuery, query);

            var order = listQuery.Order(string.Empty, new Sort
            {
                Name = "rl.Id",
                Direction = SortDirection.Desc
            });
            var page = listQuery.Page();

            return UnitOfWork.Session.Query<ReportLog>($@"
            SELECT *
            FROM ReportLogs rl
            JOIN Reports rep ON rl.ReportId = rep.Id
            {condition} {order} {page}",
                    parameters, UnitOfWork.Transaction).ToList();
        }

        public int Count(ListQuery listQuery, object query = null)
        {
            if (listQuery == null)
                throw new ArgumentNullException(nameof(listQuery));

            var (condition, parameters) = GenerateCondition(listQuery, query);

            return UnitOfWork.Session.Query<int>($@"
            SELECT count(*)
            FROM ReportLogs rl
            JOIN Reports rep ON rl.ReportId = rep.Id
            {condition}",
                    parameters, UnitOfWork.Transaction).FirstOrDefault();
        }

        private (string, object) GenerateCondition(ListQuery listQuery, object query)
        {
            var beginDate = query?.Val<DateTime?>("BeginDate");
            var endDate = query?.Val<DateTime?>("EndDate");

            string pre = "rl.DeleteDate is null";
            pre += beginDate.HasValue ? " AND rl.CreateDate >= @beginDate" : string.Empty;
            pre += endDate.HasValue ? " AND rl.CreateDate <= @endDate" : string.Empty;

            var condition = listQuery.Like(pre, new string[] { "rep.ReportName", "rl.AuthorName" });

            var parameters = new
            {
                listQuery.Page?.Offset,
                listQuery.Page?.Limit,
                listQuery.Filter,
                beginDate,
                endDate
            };

            return (condition, parameters);
        }
    }
}
