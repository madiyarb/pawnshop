using System;
using System.Collections.Generic;
using Pawnshop.Core;
using Pawnshop.Core.Impl;
using Pawnshop.Data.Models.Dictionaries;
using Dapper;
using Pawnshop.Core.Queries;
using System.Linq;
using Pawnshop.AccountingCore.Models;
using Pawnshop.Data.Models.Contracts;

namespace Pawnshop.Data.Access
{
    public class GoldRepository : RepositoryBase, IRepository<Position>
    {
        public GoldRepository(IUnitOfWork unitOfWork) : base(unitOfWork)
        {
        }

        public void Insert(Position entity)
        {
            using (var transaction = BeginTransaction())
            {
                entity.Id = UnitOfWork.Session.QuerySingleOrDefault<int>(@"
INSERT INTO Positions ( Name, CollateralType ) VALUES ( @Name, @CollateralType )
SELECT SCOPE_IDENTITY()",
                    new
                    {
                        Name = entity.Name,
                        CollateralType = CollateralType.Gold,
                    }, UnitOfWork.Transaction);

                transaction.Commit();
            }
        }

        public void Update(Position entity)
        {
            using (var transaction = BeginTransaction())
            {
                UnitOfWork.Session.Execute(@"UPDATE Positions SET Name = @Name WHERE Id = @Id", entity, UnitOfWork.Transaction);

                transaction.Commit();
            }
        }

        public void Delete(int id)
        {
            using (var transaction = BeginTransaction())
            {
                UnitOfWork.Session.Execute("DELETE FROM Positions WHERE Id = @id", new { id = id }, UnitOfWork.Transaction);
                transaction.Commit();
            }
        }

        public Position Get(int id)
        {
            return UnitOfWork.Session.QuerySingleOrDefault<Position>(@"
SELECT *
FROM Positions
WHERE Id = @id", new { id = id });
        }

        public Position Find(object query)
        {
            throw new NotImplementedException();
        }

        public List<Position> List(ListQuery listQuery, object query = null)
        {
            if (listQuery == null) throw new ArgumentNullException(nameof(listQuery));

            var pre = "CollateralType = @collateralType";
            var condition = listQuery.Like(pre, "Name");
            var order = listQuery.Order(string.Empty, new Sort
            {
                Name = "Name",
                Direction = SortDirection.Asc
            });
            var page = listQuery.Page();

            return UnitOfWork.Session.Query<Position>($@"
SELECT *
  FROM Positions
{condition} {order} {page}", new
            {
                collateralType = (short)CollateralType.Gold,
                listQuery.Page?.Offset,
                listQuery.Page?.Limit,
                listQuery.Filter
            }).ToList();
        }

        public int Count(ListQuery listQuery, object query = null)
        {
            if (listQuery == null) throw new ArgumentNullException(nameof(listQuery));

            var pre = "CollateralType = @collateralType";
            var condition = listQuery.Like(pre, "Name");

            return UnitOfWork.Session.ExecuteScalar<int>($@"
SELECT COUNT(*)
  FROM Positions
{condition}", new
            {
                collateralType = (short)CollateralType.Gold,
                listQuery.Filter
            });
        }

        public int RelationCount(int positionId)
        {
            return UnitOfWork.Session.ExecuteScalar<int>(@"
SELECT COUNT(*)
FROM ContractPositions
WHERE PositionId = @positionId", new { positionId = positionId });
        }
    }
}