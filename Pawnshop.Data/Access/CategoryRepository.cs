using System;
using System.Collections.Generic;
using System.Linq;
using Pawnshop.Core;
using Pawnshop.Core.Impl;
using Pawnshop.Data.Models.Dictionaries;
using Dapper;
using Pawnshop.AccountingCore.Models;
using Pawnshop.Core.Queries;

namespace Pawnshop.Data.Access
{
    public class CategoryRepository : RepositoryBase, IRepository<Category>
    {
        public CategoryRepository(IUnitOfWork unitOfWork) : base(unitOfWork)
        {
        }

        public void Insert(Category entity)
        {
            using (var transaction = BeginTransaction())
            {
                entity.Id = UnitOfWork.Session.QuerySingleOrDefault<int>(@"
INSERT INTO Categories ( Name, CollateralType, DefaultParkingStatusId, IsDisabled, Code ) VALUES ( @Name, @CollateralType, @DefaultParkingStatusId, @IsDisabled, @Code )
SELECT SCOPE_IDENTITY()",
                    entity, UnitOfWork.Transaction);

                transaction.Commit();
            }
        }

        public void Update(Category entity)
        {
            using (var transaction = BeginTransaction())
            {
                UnitOfWork.Session.Execute(@"UPDATE Categories SET Name = @Name, CollateralType = @CollateralType, DefaultParkingStatusId=@DefaultParkingStatusId, IsDisabled=@IsDisabled Code=@Code WHERE Id = @Id", entity, UnitOfWork.Transaction);

                transaction.Commit();
            }
        }

        public void Delete(int id)
        {
            using (var transaction = BeginTransaction())
            {
                UnitOfWork.Session.Execute("DELETE FROM Categories WHERE Id = @id", new { id = id }, UnitOfWork.Transaction);
                transaction.Commit();
            }
        }

        public Category Get(int id)
        {
            return UnitOfWork.Session.QuerySingleOrDefault<Category>(@"
SELECT *
FROM Categories
WHERE Id = @id", new { id }, UnitOfWork.Transaction);
        }

        public Category Find(object query)
        {
            throw new NotImplementedException();
        }

        public List<Category> List(ListQuery listQuery, object query = null)
        {
            if (listQuery == null) throw new ArgumentNullException(nameof(listQuery));

            var collateralType = query?.Val<CollateralType?>("CollateralType");
            var name = query?.Val<string?>("Name");
            var pre = "isDisabled = 0";
            pre += collateralType.HasValue ? " AND CollateralType = @collateralType" : string.Empty;
            pre += !string.IsNullOrEmpty(name) ? " AND Name = @name" : string.Empty;
            var condition = listQuery.Like(pre, "Name");
            var order = listQuery.Order(string.Empty, new Sort
            {
                Name = "Name",
                Direction = SortDirection.Asc
            });
            var page = listQuery.Page();

            return UnitOfWork.Session.Query<Category>($@"
SELECT *
  FROM Categories
{condition} {order} {page}", new
            {
                collateralType,
                name,
                listQuery.Page?.Offset,
                listQuery.Page?.Limit,
                listQuery.Filter
            }, UnitOfWork.Transaction).ToList();
        }

        public int Count(ListQuery listQuery, object query = null)
        {
            if (listQuery == null) throw new ArgumentNullException(nameof(listQuery));

            var collateralType = query.Val<short?>("collateralType");
            var pre = collateralType.HasValue ? "CollateralType = @collateralType" : string.Empty;

            var condition = listQuery.Like(pre, "Name");

            return UnitOfWork.Session.ExecuteScalar<int>($@"
SELECT COUNT(*)
  FROM Categories
{condition}", new
            {
                collateralType,
                listQuery.Filter
            });
        }

        public int RelationCount(int categoryId)
        {
            return UnitOfWork.Session.ExecuteScalar<int>(@"
SELECT COUNT(*)
FROM ContractPositions
WHERE CategoryId = @categoryId", new { categoryId = categoryId });
        }
    }
}