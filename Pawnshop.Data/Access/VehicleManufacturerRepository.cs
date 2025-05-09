using System;
using System.Collections.Generic;
using System.Linq;
using Dapper;
using Pawnshop.Core;
using Pawnshop.Core.Impl;
using Pawnshop.Core.Queries;
using Pawnshop.Data.Models.Dictionaries;

namespace Pawnshop.Data.Access
{
    public class VehicleManufacturerRepository : RepositoryBase, IRepository<VehicleManufacturer>
    {
        public VehicleManufacturerRepository(IUnitOfWork unitOfWork) : base(unitOfWork)
        {
        }

        public int Count(ListQuery listQuery, object query = null)
        {
            if (listQuery == null) throw new ArgumentNullException(nameof(listQuery));

            var condition = listQuery.Like(string.Empty, "Name");

            return UnitOfWork.Session.ExecuteScalar<int>($@"
SELECT COUNT(*)
FROM VehicleManufacturers
{condition}", new
            {
                listQuery.Filter
            });
        }

        public void Delete(int id)
        {
            using (var transaction = UnitOfWork.BeginTransaction())
            {
                UnitOfWork.Session.Execute(@"UPDATE VehicleManufacturers SET DeleteDate = dbo.GETASTANADATE() WHERE Id = @id", new { id }, UnitOfWork.Transaction);
                transaction.Commit();
            }
        }

        public VehicleManufacturer Find(object query)
        {
            throw new NotImplementedException();
        }

        public VehicleManufacturer Get(int id)
        {
            return UnitOfWork.Session.QuerySingleOrDefault<VehicleManufacturer>(@"
SELECT * 
FROM VehicleManufacturers
WHERE Id=@id", new { id }, UnitOfWork.Transaction);
        }

        public void Insert(VehicleManufacturer entity)
        {
            using (var transaction = BeginTransaction())
            {
                entity.Id = UnitOfWork.Session.QuerySingleOrDefault<int>(@"
INSERT INTO VehicleManufacturers ( Name, Code )
VALUES ( @Name, @Code )
SELECT SCOPE_IDENTITY()", entity, UnitOfWork.Transaction);

                transaction.Commit();
            }
        }

        public List<VehicleManufacturer> List(ListQuery listQuery, object query = null)
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

            return UnitOfWork.Session.Query<VehicleManufacturer>($@"
SELECT *
FROM VehicleManufacturers
{condition} {order} {page}", new
            {
                listQuery.Page?.Offset,
                listQuery.Page?.Limit,
                listQuery.Filter
            }, UnitOfWork.Transaction).ToList();
        }
        
        public void Update(VehicleManufacturer entity)
        {
            using (var transaction = BeginTransaction())
            {
                UnitOfWork.Session.Execute(@"
UPDATE VehicleManufacturers
SET Name = @Name, Code = @Code
WHERE Id = @Id", entity, UnitOfWork.Transaction);

                transaction.Commit();
            }
        }

        public int RelationCount(int manufacturerId)
        {
            return UnitOfWork.Session.ExecuteScalar<int>(@"
SELECT COUNT(*) FROM VehicleWMIs
WHERE VehicleManufacturerId = @manufacturerId AND DeleteDate IS NULL", new { manufacturerId = manufacturerId });
        }
    }
}