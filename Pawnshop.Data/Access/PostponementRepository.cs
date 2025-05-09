using System;
using System.Collections.Generic;
using System.Linq;
using Dapper;
using Pawnshop.Core;
using Pawnshop.Core.Impl;
using Pawnshop.Core.Queries;
using Pawnshop.Data.Models.Contracts;
using Pawnshop.Data.Models.Dictionaries;
using Pawnshop.Data.Models.Postponements;

namespace Pawnshop.Data.Access
{
    public class PostponementRepository : RepositoryBase, IRepository<Postponement>
    {
        public PostponementRepository(IUnitOfWork unitOfWork) : base(unitOfWork)
        {
        }

        public void Insert(Postponement entity)
        {
            using (var transaction = BeginTransaction())
            {
                entity.Id = UnitOfWork.Session.QuerySingleOrDefault<int>(@"
INSERT INTO Postponements ( Name, Date ) VALUES ( @Name, @Date )
SELECT SCOPE_IDENTITY()", entity, UnitOfWork.Transaction);

                transaction.Commit();
            }
        }

        public void Update(Postponement entity)
        {
            using (var transaction = BeginTransaction())
            {
                UnitOfWork.Session.Execute(@"UPDATE Postponements SET Name = @Name, Date = @Date WHERE Id = @Id", entity, UnitOfWork.Transaction);
                transaction.Commit();
            }
        }

        public void Delete(int id)
        {
            using (var transaction = BeginTransaction())
            {
                UnitOfWork.Session.Execute("DELETE FROM Postponements WHERE Id = @id", new { id }, UnitOfWork.Transaction);
                transaction.Commit();
            }
        }

        public Postponement Get(int id)
        {
            return UnitOfWork.Session.QuerySingleOrDefault<Postponement>(@"
SELECT *
FROM Postponements
WHERE Id = @id", new { id });
        }

        public Postponement Find(object query)
        {
            throw new System.NotImplementedException();
        }

        public List<Postponement> List(ListQuery listQuery, object query = null)
        {
            if (listQuery == null) throw new ArgumentNullException(nameof(listQuery));

            var condition = listQuery.Like(string.Empty, "Name");
            var order = listQuery.Order(string.Empty, new Sort
            {
                Name = "Name",
                Direction = SortDirection.Asc
            });
            var page = listQuery.Page();

            return UnitOfWork.Session.Query<Postponement>($@"
SELECT *
FROM Postponements
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
FROM Postponements
{condition}", new
            {
                listQuery.Filter
            });
        }

        public int RelationCount(int PostponementId)
        {
            return UnitOfWork.Session.ExecuteScalar<int>(@"
SELECT SUM(pc.PostponementCount)
FROM (
    SELECT COUNT(*) AS PostponementCount
    FROM ContractPostponements
    WHERE PostponementId = @PostponementId
) AS pc", new { PostponementId });
        }
    }
}