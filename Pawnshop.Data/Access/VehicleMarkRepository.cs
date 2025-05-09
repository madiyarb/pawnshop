using Dapper;
using Pawnshop.Core.Impl;
using Pawnshop.Core.Queries;
using Pawnshop.Core;
using Pawnshop.Data.Models.Dictionaries;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System;

namespace Pawnshop.Data.Access
{
    public class VehicleMarkRepository : RepositoryBase, IRepository<VehicleMark>
    {
        public VehicleMarkRepository(IUnitOfWork unitOfWork) : base(unitOfWork)
        {
        }

        public int Count(ListQuery listQuery, object query = null)
        {
            if (listQuery == null) throw new ArgumentNullException(nameof(listQuery));

            var condition = listQuery.Like(string.Empty, "Name");

            return UnitOfWork.Session.ExecuteScalar<int>($@"
SELECT COUNT(*)
FROM VehicleMarks
{condition}", new
            {
                listQuery.Filter
            });
        }

        public void Delete(int id)
        {
            using (var transaction = UnitOfWork.BeginTransaction())
            {
                UnitOfWork.Session.Execute(@"UPDATE VehicleMarks SET DeleteDate = dbo.GETASTANADATE() WHERE Id = @id", new { id }, UnitOfWork.Transaction);
                transaction.Commit();
            }
        }

        public VehicleMark Find(object query)
        {
            throw new NotImplementedException();
        }

        public VehicleMark Get(int id)
        {
            return UnitOfWork.Session.QuerySingleOrDefault<VehicleMark>(@"
SELECT * 
FROM VehicleMarks
WHERE Id=@id", new { id }, UnitOfWork.Transaction);
        }

        public void Insert(VehicleMark entity)
        {
            using (var transaction = BeginTransaction())
            {
                entity.Id = UnitOfWork.Session.QuerySingleOrDefault<int>(@"
INSERT INTO VehicleMarks ( Name, Code, IsDisabled )
VALUES ( @Name, @Code, @IsDisabled )
SELECT SCOPE_IDENTITY()", entity, UnitOfWork.Transaction);

                transaction.Commit();
            }
        }

        public List<VehicleMark> List(ListQuery listQuery, object query = null)
        {
            if (listQuery == null) throw new ArgumentNullException(nameof(listQuery));

            var pre = "DeleteDate IS NULL";

            var condition = listQuery.Like(pre, "Name");
            var order = listQuery.Order(string.Empty, new Sort
            {
                Name = "Name",
                Direction = SortDirection.Asc
            });
            var page = listQuery.Page();

            return UnitOfWork.Session.Query<VehicleMark>($@"
SELECT *
FROM VehicleMarks
{condition} {order} {page}", new
            {
                listQuery.Page?.Offset,
                listQuery.Page?.Limit,
                listQuery.Filter
            }, UnitOfWork.Transaction).ToList();
        }

        public void Update(VehicleMark entity)
        {
            using (var transaction = BeginTransaction())
            {
                UnitOfWork.Session.Execute(@"
UPDATE VehicleMarks
SET Name = @Name, Code = @Code, IsDisabled = @IsDisabled
WHERE Id = @Id", entity, UnitOfWork.Transaction);

                transaction.Commit();
            }
        }

        public int RelationCount(int markId)
        {
            return UnitOfWork.Session.ExecuteScalar<int>(@"
SELECT SUM(m)
FROM (
SELECT COUNT(*) as m
FROM VehicleModels
WHERE VehicleMarkId = @markId AND DeleteDate IS NULL
UNION ALL
SELECT COUNT(*) as m
FROM Cars
WHERE VehicleMarkId = @markId
UNION ALL
SELECT COUNT(*) as m
FROM Machineries
WHERE VehicleMarkId = @markId
UNION ALL
SELECT COUNT(*) as m
FROM VehicleWMIs 
WHERE VehicleMarkId = @markId AND DeleteDate IS NULL) as t", new { markId = markId });
        }

        public List<VehicleMark> GetVehicleMarks()
        {
            return UnitOfWork.Session.Query<VehicleMark>($@"
            SELECT Id, Name
            FROM VehicleMarks
            WHERE IsDisabled = 0",
                UnitOfWork.Transaction).ToList();
        }

        public async Task<VehicleMark> FindByNameAsync(string name)
        {
            return await UnitOfWork.Session.QueryFirstOrDefaultAsync<VehicleMark>(
                "SELECT * FROM VehicleMarks WHERE Name = @name AND DeleteDate IS NULL AND IsDisabled = 0",
                new { name }, UnitOfWork.Transaction);
        }

        public async Task<VehicleMark> FindByNameOrCodeAsync(string value)
        {
            return await UnitOfWork.Session.QueryFirstOrDefaultAsync<VehicleMark>(@"SELECT *
  FROM VehicleMarks
 WHERE DeleteDate IS NULL
   AND (Name = @value OR Code = @value)
 ORDER BY Id DESC",
                new { value }, UnitOfWork.Transaction);
        }
    }
}