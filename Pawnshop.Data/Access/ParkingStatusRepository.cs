using Dapper;
using Pawnshop.Core;
using Pawnshop.Core.Impl;
using Pawnshop.Core.Queries;
using Pawnshop.Data.Models.Dictionaries;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Pawnshop.Data.Access
{
    public class ParkingStatusRepository : RepositoryBase, IRepository<ParkingStatus>
    {
        public ParkingStatusRepository(IUnitOfWork unitOfWork) : base(unitOfWork)
        {
        }

        public void Insert(ParkingStatus entity)
        {
            using (var transaction = BeginTransaction())
            {
                entity.Id = UnitOfWork.Session.QuerySingleOrDefault<int>(@"
                    INSERT INTO ParkingStatuses (StatusName) VALUES (@StatusName)
                    SELECT SCOPE_IDENTITY()",
                    entity, UnitOfWork.Transaction);

                transaction.Commit();
            }
        }

        public void Update(ParkingStatus entity)
        {
            using (var transaction = BeginTransaction())
            {
                UnitOfWork.Session.Execute(@"
                    UPDATE ParkingStatuses SET StatusName=@StatusName
                    WHERE Id = @Id", entity, UnitOfWork.Transaction);

                transaction.Commit();
            }
        }

        public void Delete(int id)
        {
            UnitOfWork.Session.Execute(@"UPDATE ParkingStatuses SET DeleteDate = dbo.GETASTANADATE() WHERE Id = @id", new { id }, UnitOfWork.Transaction);
        }

        public ParkingStatus Find(object query)
        {
            throw new NotImplementedException();
        }

        public ParkingStatus Get(int id)
        {
            return UnitOfWork.Session.QuerySingleOrDefault<ParkingStatus>(@"
                SELECT *
                FROM ParkingStatuses
                WHERE Id = @id", new { id });
        }

        public List<ParkingStatus> List(ListQuery listQuery, object query = null)
        {
            if (listQuery == null) throw new ArgumentNullException(nameof(listQuery));

            var condition = listQuery.Like("DeleteDate IS NULL");
            var order = listQuery.Order(string.Empty, new Sort
            {
                Name = "StatusName",
                Direction = SortDirection.Asc
            });
            var page = listQuery.Page();

            return UnitOfWork.Session.Query<ParkingStatus>($@"
                SELECT *
                  FROM ParkingStatuses
                {condition} {order} {page}", new
            {
                listQuery.Page?.Offset,
                listQuery.Page?.Limit,
                listQuery.Filter
            }, UnitOfWork.Transaction).ToList();
        }

        public int Count(ListQuery listQuery, object query = null)
        {
            if (listQuery == null) throw new ArgumentNullException(nameof(listQuery));

            var condition = listQuery.Like("DeleteDate IS NULL", "StatusName");

            return UnitOfWork.Session.ExecuteScalar<int>($@"
                SELECT COUNT(*)
                  FROM ParkingStatuses
                {condition}", new
            {
                listQuery.Filter
            });
        }

        public int RelationCount(int parkingStatusId)
        {
            return UnitOfWork.Session.ExecuteScalar<int>(@"
                SELECT SUM(ParkingStatusCounts.ParkingStatusCount)
                FROM (
                    SELECT COUNT(*) as ParkingStatusCount
                    FROM Cars
                    WHERE ParkingStatusId = @parkingStatusId
                    UNION ALL
                    SELECT COUNT(*) asParkingStatusCount
                    FROM Machineries
                    WHERE ParkingStatusId = @parkingStatusId
                ) ParkingStatusCounts", new { parkingStatusId });
        }
    }
}
