using System;
using System.Collections.Generic;
using System.Linq;
using Dapper;
using Pawnshop.Core;
using Pawnshop.Core.Impl;
using Pawnshop.Core.Queries;
using Pawnshop.Data.Models.Contracts;
using Pawnshop.Data.Models.Dictionaries;

namespace Pawnshop.Data.Access
{
    public class PurityRepository : RepositoryBase, IRepository<Purity>
    {
        public PurityRepository(IUnitOfWork unitOfWork) : base(unitOfWork)
        {
        }

        public void Insert(Purity entity)
        {
            using (var transaction = BeginTransaction())
            {
                entity.Id = UnitOfWork.Session.QuerySingleOrDefault<int>(@"
INSERT INTO Purities ( Name ) VALUES ( @Name )
SELECT SCOPE_IDENTITY()", entity, UnitOfWork.Transaction);

                transaction.Commit();
            }
        }

        public void Update(Purity entity)
        {
            using (var transaction = BeginTransaction())
            {
                UnitOfWork.Session.Execute(@"UPDATE Purities SET Name = @Name WHERE Id = @Id", entity, UnitOfWork.Transaction);
                transaction.Commit();
            }
        }

        public void Delete(int id)
        {
            using (var transaction = BeginTransaction())
            {
                UnitOfWork.Session.Execute("DELETE FROM Purities WHERE Id = @id", new { id }, UnitOfWork.Transaction);
                transaction.Commit();
            }
        }

        public Purity Get(int id)
        {
            return UnitOfWork.Session.QuerySingleOrDefault<Purity>(@"
SELECT *
FROM Purities
WHERE Id = @id", new { id });
        }

        public Purity Find(object query)
        {
            throw new System.NotImplementedException();
        }

        public List<Purity> List(ListQuery listQuery, object query = null)
        {
            if (listQuery == null) throw new ArgumentNullException(nameof(listQuery));

            var condition = listQuery.Like(string.Empty, "Name");
            var order = listQuery.Order(string.Empty, new Sort
            {
                Name = "Name",
                Direction = SortDirection.Asc
            });
            var page = listQuery.Page();

            return UnitOfWork.Session.Query<Purity>($@"
SELECT *
FROM Purities
{condition} {order} {page}", new
            {
                listQuery.Page?.Offset,
                listQuery.Page?.Limit,
                listQuery.Filter
            }).ToList();
        }

        public int Count(ListQuery listQuery, object query = null)
        {
            if (listQuery == null) throw new ArgumentNullException(nameof(listQuery));

            var condition = listQuery.Like(string.Empty, "Name");

            return UnitOfWork.Session.ExecuteScalar<int>($@"
SELECT COUNT(*)
FROM Purities
{condition}", new
            {
                listQuery.Filter
            });
        }

        public int RelationCount(int purityId)
        {
            return UnitOfWork.Session.ExecuteScalar<int>(@"
SELECT SUM(pc.PositionCount)
FROM (
    SELECT COUNT(*) AS PurityCount
    FROM ContractPositions
    WHERE JSON_VALUE(PositionSpecific, '$.PurityId') = @purityId
    UNION ALL
    SELECT COUNT(*) AS PurityCount
    FROM Sellings
    WHERE JSON_VALUE(PositionSpecific, '$.PurityId') = @purityId
) AS pc", new { purityId });
        }
    }
}