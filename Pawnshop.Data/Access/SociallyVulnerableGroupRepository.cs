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
    public class SociallyVulnerableGroupRepository : RepositoryBase, IRepository<SociallyVulnerableGroup>
    {
        public SociallyVulnerableGroupRepository(IUnitOfWork unitOfWork) : base(unitOfWork)
        {
        }

        public void Insert(SociallyVulnerableGroup entity)
        {
            using (var transaction = BeginTransaction())
            {
                entity.Id = UnitOfWork.Session.QuerySingleOrDefault<int>(@"
INSERT INTO SociallyVulnerableGroups ( Code, Name, Category  ) VALUES ( @Code, @Name, @Category )
SELECT SCOPE_IDENTITY()", entity, UnitOfWork.Transaction);

                transaction.Commit();
            }
        }

        public void Update(SociallyVulnerableGroup entity)
        {
            using (var transaction = BeginTransaction())
            {
                UnitOfWork.Session.Execute(@"UPDATE SociallyVulnerableGroups SET Code=@Code, Category=@Category WHERE Id = @Id", entity, UnitOfWork.Transaction);
                transaction.Commit();
            }
        }

        public void Delete(int id)
        {
            using (var transaction = BeginTransaction())
            {
                UnitOfWork.Session.Execute("UPDATE SociallyVulnerableGroups SET DeleteDate=dbo.GETASTANADATE() WHERE Id = @id", new { id }, UnitOfWork.Transaction);
                transaction.Commit();
            }
        }

        public SociallyVulnerableGroup Get(int id)
        {
            return UnitOfWork.Session.QuerySingleOrDefault<SociallyVulnerableGroup>(@"
SELECT *
FROM SociallyVulnerableGroups
WHERE Id = @id", new { id });
        }

        public SociallyVulnerableGroup Find(object query)
        {
            throw new System.NotImplementedException();
        }

        public List<SociallyVulnerableGroup> List(ListQuery listQuery, object query = null)
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

            return UnitOfWork.Session.Query<SociallyVulnerableGroup>($@"
SELECT *
FROM SociallyVulnerableGroups
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

            var pre = "DeleteDate IS NULL";
            var disabled = query?.Val<bool>("Disabled");
            if (disabled.HasValue) pre += " AND Disabled = @disabled";

            var condition = listQuery.Like(pre, "Name");

            return UnitOfWork.Session.ExecuteScalar<int>($@"
SELECT COUNT(*)
FROM SociallyVulnerableGroups
{condition}", new
            {
                listQuery.Filter
            });
        }
    }
}