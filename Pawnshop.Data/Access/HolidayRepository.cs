using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using Pawnshop.Core;
using Pawnshop.Core.Impl;
using Pawnshop.Core.Queries;
using Pawnshop.Data.Models.Dictionaries;
using Pawnshop.Data.Models.Membership;

namespace Pawnshop.Data.Access
{
    public class HolidayRepository : RepositoryBase, IRepository<Holiday>
    {
        public HolidayRepository(IUnitOfWork unitOfWork) : base(unitOfWork)
        {
        }

        public void Insert(Holiday entity)
        {
            using (var transaction = BeginTransaction())
            {
                entity.Id = UnitOfWork.Session.QuerySingleOrDefault<int>(@"
INSERT INTO Holidays ( Date, PayDate, AuthorId, CreateDate ) VALUES ( @Date, @PayDate, @AuthorId, @CreateDate )
SELECT SCOPE_IDENTITY()",
                    entity, UnitOfWork.Transaction);

                transaction.Commit();
            }
        }

        public void Update(Holiday entity)
        {
            using (var transaction = BeginTransaction())
            {
                UnitOfWork.Session.Execute("UPDATE Holidays SET Date = @Date, PayDate = @PayDate WHERE Id = @id", entity, UnitOfWork.Transaction);
                transaction.Commit();
            }
        }

        public void Delete(int id)
        {
            using (var transaction = BeginTransaction())
            {
                UnitOfWork.Session.Execute("DELETE FROM Holidays WHERE Id = @id", new { id }, UnitOfWork.Transaction);
                transaction.Commit();
            }
        }

        public Holiday Get(int id)
        {
            return UnitOfWork.Session.Query<Holiday, User, Holiday>(@"
SELECT h.*, u.*
  FROM Holidays h
LEFT JOIN Users u ON u.Id = h.AuthorId
WHERE h.Id = @id", (h, u) =>
            {
                h.Author = u;
                return h;
            }, new { id }, UnitOfWork.Transaction).FirstOrDefault();
        }

        public Holiday Get(DateTime date)
        {
            date = date.Date;
            return UnitOfWork.Session.Query<Holiday>(@"
                SELECT h.*
                FROM Holidays h
                WHERE Date = @date",
                new { date }, UnitOfWork.Transaction).FirstOrDefault();
        }

        public async Task<List<Holiday>> GetRangeHolidaysAsync(DateTime dateFrom, DateTime dateUntil)
        {
            var sqlQuery = @"
                SELECT *
                    FROM Holidays
                    WHERE Date >= @DateFrom AND Date <= @DateUntil";
            
            var parameters = new { DateFrom = dateFrom, DateUntil = dateUntil};
            
            var result = await UnitOfWork.Session
                .QueryAsync<Holiday>(sqlQuery, parameters, UnitOfWork.Transaction);

            return result?.ToList();
        }

        public Holiday Find(object query)
        {
            throw new NotImplementedException();
        }

        public List<Holiday> List(ListQuery listQuery, object query = null)
        {
            if (listQuery == null) throw new ArgumentNullException(nameof(listQuery));

            var beginDate = query?.Val<DateTime?>("BeginDate");
            var endDate = query?.Val<DateTime?>("EndDate");
            var payBeginDate = query?.Val<DateTime?>("PayBeginDate");
            var payEndDate = query?.Val<DateTime?>("PayEndDate");
            var payDate = query?.Val<DateTime?>("PayDate");

            var pre = "h.Id>0";

            pre += beginDate.HasValue ? " AND h.Date >= @beginDate" : string.Empty;
            pre += endDate.HasValue ? " AND h.Date <= @endDate" : string.Empty;
            pre += payBeginDate.HasValue ? " AND h.PayDate >= @payBeginDate" : string.Empty;
            pre += payEndDate.HasValue ? " AND h.PayDate <= @payEndDate" : string.Empty;
            pre += payDate.HasValue ? " AND CAST(h.PayDate AS DATE) = CAST(@payDate AS DATE)" : string.Empty;

            var condition = listQuery.Like(pre);
            var order = listQuery.Order(string.Empty, new Sort
            {
                Name = "h.Date",
                Direction = SortDirection.Asc
            });
            var page = listQuery.Page();

            return UnitOfWork.Session.Query<Holiday, User, Holiday>($@"
SELECT h.*, u.*
  FROM Holidays h
LEFT JOIN Users u ON u.Id = h.AuthorId
{condition} {order} {page}", (h, u) =>
            {
                h.Author = u;
                return h;
            },new
            {
                listQuery.Page?.Offset,
                listQuery.Page?.Limit,
                listQuery.Filter,
                beginDate,
                endDate,
                payBeginDate,
                payEndDate,
                payDate
            }, UnitOfWork.Transaction).ToList();
        }

        public int Count(ListQuery listQuery, object query = null)
        {
            if (listQuery == null) throw new ArgumentNullException(nameof(listQuery));

            var beginDate = query?.Val<DateTime?>("BeginDate");
            var endDate = query?.Val<DateTime?>("EndDate");
            var payBeginDate = query?.Val<DateTime?>("PayBeginDate");
            var payEndDate = query?.Val<DateTime?>("PayEndDate");
            var payDate = query?.Val<DateTime?>("PayDate");

            var pre = "h.Id>0";

            pre += beginDate.HasValue ? " AND h.Date >= @beginDate" : string.Empty;
            pre += endDate.HasValue ? " AND h.Date <= @endDate" : string.Empty;
            pre += payBeginDate.HasValue ? " AND h.PayDate >= @payBeginDate" : string.Empty;
            pre += payEndDate.HasValue ? " AND h.PayDate <= @payEndDate" : string.Empty;
            pre += payDate.HasValue ? " AND CAST(h.PayDate AS DATE) = CAST(@payDate AS DATE)" : string.Empty;

            var condition = listQuery.Like(pre);

            return UnitOfWork.Session.ExecuteScalar<int>($@"
SELECT COUNT(*)
  FROM Holidays h
{condition}", new
            {
                listQuery.Filter,
                beginDate,
                endDate,
                payBeginDate,
                payEndDate,
                payDate
            }, UnitOfWork.Transaction);
        }

        public DateTime GetFirstPreviousHolidayFromDate(DateTime date)
        {
            var query = @"SELECT MIN(date) FROM dbo.Holidays
                          WHERE PayDate = @date";
            return UnitOfWork.Session.ExecuteScalar<DateTime>(query,
                new { date }, UnitOfWork.Transaction);
        }
    }
}